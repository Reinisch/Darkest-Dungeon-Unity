using UnityEngine;
using System.Collections;


public enum PickupCharacterState
{
    Idle = 0,
    Walking = 1,
    Trotting = 2,
    Running = 3,
    Jumping = 4,
}

[RequireComponent(typeof(CharacterController))]
public class PickupController : MonoBehaviour, IPunObservable
{

    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip jumpPoseAnimation;

    public float walkMaxAnimationSpeed = 0.75f;
    public float trotMaxAnimationSpeed = 1.0f;
    public float runMaxAnimationSpeed = 1.0f;
    public float jumpAnimationSpeed = 1.15f;
    public float landAnimationSpeed = 1.0f;

    private Animation _animation;



    public PickupCharacterState _characterState;

    // The speed when walking
    public float walkSpeed = 2.0f;
    // after trotAfterSeconds of walking we trot with trotSpeed
    public float trotSpeed = 4.0f;
    // when pressing "Fire3" button (cmd) we start running
    public float runSpeed = 6.0f;

    public float inAirControlAcceleration = 3.0f;

    // How high do we jump when pressing jump and letting go immediately
    public float jumpHeight = 0.5f;

    // The gravity for the character
    public float gravity = 20.0f;
    // The gravity in controlled descent mode
    public float speedSmoothing = 10.0f;
    public float rotateSpeed = 500.0f;
    public float trotAfterSeconds = 3.0f;

    public bool canJump = false;

    private float jumpRepeatTime = 0.05f;
    private float jumpTimeout = 0.15f;
    private float groundedTimeout = 0.25f;

    // The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
    private float lockCameraTimer = 0.0f;

    // The current move direction in x-z
    private Vector3 moveDirection = Vector3.zero;
    // The current vertical speed
    private float verticalSpeed = 0.0f;
    // The current x-z move speed
    private float moveSpeed = 0.0f;

    // The last collision flags returned from controller.Move
    private CollisionFlags collisionFlags;

    // Are we jumping? (Initiated with jump button and not grounded yet)
    private bool jumping = false;
    private bool jumpingReachedApex = false;

    // Are we moving backwards (This locks the camera to not do a 180 degree spin)
    private bool movingBack = false;
    // Is the user pressing any keys?
    private bool isMoving = false;
    // When did the user start walking (Used for going into trot after a while)
    private float walkTimeStart = 0.0f;
    // Last time the jump button was clicked down
    private float lastJumpButtonTime = -10.0f;
    // Last time we performed a jump
    private float lastJumpTime = -1.0f;
    // the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
    //private float lastJumpStartHeight = 0.0f;
    private Vector3 inAirVelocity = Vector3.zero;

    private float lastGroundedTime = 0.0f;
    Vector3 velocity = Vector3.zero;
    private Vector3 lastPos;
    private Vector3 remotePosition;

    public bool isControllable = false;
    public bool DoRotate = true;
    public float RemoteSmoothing = 5;
    public bool AssignAsTagObject = true;

    void Awake()
    {
        // PUN: automatically determine isControllable, if this GO has a PhotonView
        PhotonView pv = this.gameObject.GetComponent<PhotonView>();
        if (pv != null)
        {
            isControllable = pv.isMine;

            // The pickup demo assigns this GameObject as the PhotonPlayer.TagObject. This way, we can access this character (controller, position, etc) easily
            if (this.AssignAsTagObject)
            {
                pv.owner.TagObject = this.gameObject;
            }

            // please note: we change this setting on ANY PickupController if "DoRotate" is off. not only locally when it's "our" GameObject!
            if (pv.observed is Transform && !DoRotate)
            {
                pv.onSerializeTransformOption = OnSerializeTransform.OnlyPosition;
            }
        }


        moveDirection = transform.TransformDirection(Vector3.forward);

        _animation = GetComponent<Animation>();
        if (!_animation)
            Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");

        if (!idleAnimation)
        {
            _animation = null;
            Debug.Log("No idle animation found. Turning off animations.");
        }
        if (!walkAnimation)
        {
            _animation = null;
            Debug.Log("No walk animation found. Turning off animations.");
        }
        if (!runAnimation)
        {
            _animation = null;
            Debug.Log("No run animation found. Turning off animations.");
        }
        if (!jumpPoseAnimation && canJump)
        {
            _animation = null;
            Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
        }
    }

    void Update()
    {        
        if (isControllable)
        {
            if (Input.GetButtonDown("Jump"))
            {
                lastJumpButtonTime = Time.time;
            }

            UpdateSmoothedMovementDirection();

            // Apply gravity
            // - extra power jump modifies gravity
            // - controlledDescent mode modifies gravity
            ApplyGravity();

            // Apply jumping logic
            ApplyJumping();


            // Calculate actual motion
            Vector3 movement = moveDirection * moveSpeed + new Vector3(0, verticalSpeed, 0) + inAirVelocity;
            movement *= Time.deltaTime;

            //Debug.Log(movement.x.ToString("0.000") + ":" + movement.z.ToString("0.000"));

            // Move the controller
            CharacterController controller = GetComponent<CharacterController>();
            collisionFlags = controller.Move(movement);

        }

        // PUN: if a remote position is known, we smooth-move to it (being late(r) but smoother)
        if (this.remotePosition != Vector3.zero)
        {
            transform.position = Vector3.Lerp(transform.position, this.remotePosition, Time.deltaTime * this.RemoteSmoothing);
        }

        velocity = (transform.position - lastPos)*25;

        // ANIMATION sector
        if (_animation)
        {
            if (_characterState == PickupCharacterState.Jumping)
            {
                if (!jumpingReachedApex)
                {
                    _animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
                    _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    _animation.CrossFade(jumpPoseAnimation.name);
                }
                else
                {
                    _animation[jumpPoseAnimation.name].speed = -landAnimationSpeed;
                    _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    _animation.CrossFade(jumpPoseAnimation.name);
                }
            }
            else
            {
                if (_characterState == PickupCharacterState.Idle)
                {
                    _animation.CrossFade(idleAnimation.name);
                }
                else if (_characterState == PickupCharacterState.Running)
                {
                    _animation[runAnimation.name].speed = runMaxAnimationSpeed;
                    if (this.isControllable)
                    {
                        _animation[runAnimation.name].speed = Mathf.Clamp(velocity.magnitude, 0.0f, runMaxAnimationSpeed);
                    }
                    _animation.CrossFade(runAnimation.name);
                }
                else if (_characterState == PickupCharacterState.Trotting)
                {
                    _animation[walkAnimation.name].speed = trotMaxAnimationSpeed;
                    if (this.isControllable)
                    {
                        _animation[walkAnimation.name].speed = Mathf.Clamp(velocity.magnitude, 0.0f, trotMaxAnimationSpeed);
                    }
                    _animation.CrossFade(walkAnimation.name);
                }
                else if (_characterState == PickupCharacterState.Walking)
                {
                    _animation[walkAnimation.name].speed = walkMaxAnimationSpeed;
                    if (this.isControllable)
                    {
                        _animation[walkAnimation.name].speed = Mathf.Clamp(velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
                    }
                    _animation.CrossFade(walkAnimation.name);
                }
                
                if (_characterState != PickupCharacterState.Running)
                {
                    _animation[runAnimation.name].time = 0.0f;
                }
            }
        }
        // ANIMATION sector

        // Set rotation to the move direction
        if (IsGrounded())
        {
            // a specialty of this controller: you can disable rotation!
            if (DoRotate)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
        else
        {
            /* This causes choppy behaviour when colliding with SIDES
             * Vector3 xzMove = velocity;
            xzMove.y = 0;
            if (xzMove.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(xzMove);
            }*/
        }

        // We are in jump mode but just became grounded
        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
            inAirVelocity = Vector3.zero;
            if (jumping)
            {
                jumping = false;
                SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
            }
        }

        lastPos = transform.position;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(this.transform.position);
            stream.SendNext((byte)this._characterState);
        }
        else
        {
            bool initialRemotePosition = (remotePosition == Vector3.zero);
            
            remotePosition = (Vector3)stream.ReceiveNext();
            this._characterState = (PickupCharacterState)((byte)stream.ReceiveNext());

            if (initialRemotePosition)
            {
                // avoids lerping the character from "center" to the "current" position when this client joins
                this.transform.position = remotePosition;
            }
        }
    }

    void UpdateSmoothedMovementDirection()
    {
        Transform cameraTransform = Camera.main.transform;
        bool grounded = IsGrounded();

        // Forward vector relative to the camera along the x-z plane	
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;

        // Right vector relative to the camera
        // Always orthogonal to the forward vector
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // Are we moving backwards or looking backwards
        if (v < -0.2f)
            movingBack = true;
        else
            movingBack = false;

        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        // Target direction relative to the camera
        Vector3 targetDirection = h * right + v * forward;
        // Debug.Log("targetDirection " + targetDirection);

        // Grounded controls
        if (grounded)
        {
            // Lock camera for short period when transitioning moving & standing still
            lockCameraTimer += Time.deltaTime;
            if (isMoving != wasMoving)
                lockCameraTimer = 0.0f;

            // We store speed and direction seperately,
            // so that when the character stands still we still have a valid forward direction
            // moveDirection is always normalized, and we only update it if there is user input.
            if (targetDirection != Vector3.zero)
            {
                // If we are really slow, just snap to the target direction
                if (moveSpeed < walkSpeed * 0.9f && grounded)
                {
                    moveDirection = targetDirection.normalized;
                }
                // Otherwise smoothly turn towards it
                else
                {
                    moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);

                    moveDirection = moveDirection.normalized;
                }
            }

            // Smooth the speed based on the current target direction
            float curSmooth = speedSmoothing * Time.deltaTime;

            // Choose target speed
            //* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

            _characterState = PickupCharacterState.Idle;

            // Pick speed modifier
            if ((Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift)) && isMoving)
            {
                targetSpeed *= runSpeed;
                _characterState = PickupCharacterState.Running;
            }
            else if (Time.time - trotAfterSeconds > walkTimeStart)
            {
                targetSpeed *= trotSpeed;
                _characterState = PickupCharacterState.Trotting;
            }
            else if (isMoving)
            {
                targetSpeed *= walkSpeed;
                _characterState = PickupCharacterState.Walking;
            }
        
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);

            // Reset walk time start when we slow down
            if (moveSpeed < walkSpeed * 0.3f)
                walkTimeStart = Time.time;
        }
        // In air controls
        else
        {
            // Lock camera while in air
            if (jumping)
                lockCameraTimer = 0.0f;

            if (isMoving)
                inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
        }
    }

    void ApplyJumping()
    {
        // Prevent jumping too fast after each other
        if (lastJumpTime + jumpRepeatTime > Time.time)
            return;

        if (IsGrounded())
        {
            // Jump
            // - Only when pressing the button down
            // - With a timeout so you can press the button slightly before landing		
            if (canJump && Time.time < lastJumpButtonTime + jumpTimeout)
            {
                verticalSpeed = CalculateJumpVerticalSpeed(jumpHeight);
                SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void ApplyGravity()
    {
        if (isControllable)	// don't move player at all if not controllable.
        {
            // Apply gravity
            //bool jumpButton = Input.GetButton("Jump");
            
            // When we reach the apex of the jump we send out a message
            if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0f)
            {
                jumpingReachedApex = true;
                SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
            }

            if (IsGrounded())
                verticalSpeed = 0.0f;
            else
                verticalSpeed -= gravity * Time.deltaTime;
        }
    }

    float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * targetJumpHeight * gravity);
    }

    void DidJump()
    {
        jumping = true;
        jumpingReachedApex = false;
        lastJumpTime = Time.time;
        //lastJumpStartHeight = transform.position.y;
        lastJumpButtonTime = -10;

        _characterState = PickupCharacterState.Jumping;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //	Debug.DrawRay(hit.point, hit.normal);
        if (hit.moveDirection.y > 0.01f)
            return;
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }

    public bool IsJumping()
    {
        return jumping;
    }

    public bool IsGrounded()
    {
        return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
    }

    public Vector3 GetDirection()
    {
        return moveDirection;
    }

    public bool IsMovingBackwards()
    {
        return movingBack;
    }

    public float GetLockCameraTimer()
    {
        return lockCameraTimer;
    }

    public bool IsMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
    }

    public bool HasJumpReachedApex()
    {
        return jumpingReachedApex;
    }

    public bool IsGroundedWithTimeout()
    {
        return lastGroundedTime + groundedTimeout > Time.time;
    }

    public void Reset()
    {
        gameObject.tag = "Player";
    }
}