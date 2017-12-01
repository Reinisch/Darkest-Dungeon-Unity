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
        PhotonView pv = gameObject.GetComponent<PhotonView>();
        if (pv != null)
        {
            this.isControllable = pv.isMine;

            // The pickup demo assigns this GameObject as the PhotonPlayer.TagObject. This way, we can access this character (controller, position, etc) easily
            if (this.AssignAsTagObject)
            {
                pv.owner.TagObject = gameObject;
            }

            // please note: we change this setting on ANY PickupController if "DoRotate" is off. not only locally when it's "our" GameObject!
            if (!this.DoRotate && pv.ObservedComponents != null)
            {
                for (int i = 0; i < pv.ObservedComponents.Count; ++i)
                {
                    if (pv.ObservedComponents[i] is Transform)
                    {
                        pv.onSerializeTransformOption = OnSerializeTransform.OnlyPosition;
                        break;
                    }
                }
            }
        }


        this.moveDirection = transform.TransformDirection(Vector3.forward);

        this._animation = GetComponent<Animation>();
        if (!this._animation)
            Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");

        if (!this.idleAnimation)
        {
            this._animation = null;
            Debug.Log("No idle animation found. Turning off animations.");
        }
        if (!this.walkAnimation)
        {
            this._animation = null;
            Debug.Log("No walk animation found. Turning off animations.");
        }
        if (!this.runAnimation)
        {
            this._animation = null;
            Debug.Log("No run animation found. Turning off animations.");
        }
        if (!this.jumpPoseAnimation && this.canJump)
        {
            this._animation = null;
            Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
        }
    }

    void Update()
    {        
        if (this.isControllable)
        {
            if (Input.GetButtonDown("Jump"))
            {
                this.lastJumpButtonTime = Time.time;
            }

            this.UpdateSmoothedMovementDirection();

            // Apply gravity
            // - extra power jump modifies gravity
            // - controlledDescent mode modifies gravity
            this.ApplyGravity();

            // Apply jumping logic
            this.ApplyJumping();


            // Calculate actual motion
            Vector3 movement = this.moveDirection *this.moveSpeed + new Vector3(0, this.verticalSpeed, 0) + this.inAirVelocity;
            movement *= Time.deltaTime;

            //Debug.Log(movement.x.ToString("0.000") + ":" + movement.z.ToString("0.000"));

            // Move the controller
            CharacterController controller = GetComponent<CharacterController>();
            this.collisionFlags = controller.Move(movement);

        }

        // PUN: if a remote position is known, we smooth-move to it (being late(r) but smoother)
        if (this.remotePosition != Vector3.zero)
        {
            transform.position = Vector3.Lerp(transform.position, this.remotePosition, Time.deltaTime * this.RemoteSmoothing);
        }

        this.velocity = (transform.position - this.lastPos)*25;

        // ANIMATION sector
        if (this._animation)
        {
            if (this._characterState == PickupCharacterState.Jumping)
            {
                if (!this.jumpingReachedApex)
                {
                    this._animation[this.jumpPoseAnimation.name].speed = this.jumpAnimationSpeed;
                    this._animation[this.jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    this._animation.CrossFade(this.jumpPoseAnimation.name);
                }
                else
                {
                    this._animation[this.jumpPoseAnimation.name].speed = -this.landAnimationSpeed;
                    this._animation[this.jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    this._animation.CrossFade(this.jumpPoseAnimation.name);
                }
            }
            else
            {
                if (this._characterState == PickupCharacterState.Idle)
                {
                    this._animation.CrossFade(this.idleAnimation.name);
                }
                else if (this._characterState == PickupCharacterState.Running)
                {
                    this._animation[this.runAnimation.name].speed = this.runMaxAnimationSpeed;
                    if (this.isControllable)
                    {
                        this._animation[this.runAnimation.name].speed = Mathf.Clamp(this.velocity.magnitude, 0.0f, this.runMaxAnimationSpeed);
                    }
                    this._animation.CrossFade(this.runAnimation.name);
                }
                else if (this._characterState == PickupCharacterState.Trotting)
                {
                    this._animation[this.walkAnimation.name].speed = this.trotMaxAnimationSpeed;
                    if (this.isControllable)
                    {
                        this._animation[this.walkAnimation.name].speed = Mathf.Clamp(this.velocity.magnitude, 0.0f, this.trotMaxAnimationSpeed);
                    }
                    this._animation.CrossFade(this.walkAnimation.name);
                }
                else if (this._characterState == PickupCharacterState.Walking)
                {
                    this._animation[this.walkAnimation.name].speed = this.walkMaxAnimationSpeed;
                    if (this.isControllable)
                    {
                        this._animation[this.walkAnimation.name].speed = Mathf.Clamp(this.velocity.magnitude, 0.0f, this.walkMaxAnimationSpeed);
                    }
                    this._animation.CrossFade(this.walkAnimation.name);
                }
                
                if (this._characterState != PickupCharacterState.Running)
                {
                    this._animation[this.runAnimation.name].time = 0.0f;
                }
            }
        }
        // ANIMATION sector

        // Set rotation to the move direction
        if (this.IsGrounded())
        {
            // a specialty of this controller: you can disable rotation!
            if (this.DoRotate)
            {
                transform.rotation = Quaternion.LookRotation(this.moveDirection);
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
        if (this.IsGrounded())
        {
            this.lastGroundedTime = Time.time;
            this.inAirVelocity = Vector3.zero;
            if (this.jumping)
            {
                this.jumping = false;
                SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
            }
        }

        this.lastPos = transform.position;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext((byte)this._characterState);
        }
        else
        {
            bool initialRemotePosition = (this.remotePosition == Vector3.zero);

            this.remotePosition = (Vector3)stream.ReceiveNext();
            this._characterState = (PickupCharacterState)((byte)stream.ReceiveNext());

            if (initialRemotePosition)
            {
                // avoids lerping the character from "center" to the "current" position when this client joins
                transform.position = this.remotePosition;
            }
        }
    }

    void UpdateSmoothedMovementDirection()
    {
        Transform cameraTransform = Camera.main.transform;
        bool grounded = this.IsGrounded();

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
            this.movingBack = true;
        else
            this.movingBack = false;

        bool wasMoving = this.isMoving;
        this.isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        // Target direction relative to the camera
        Vector3 targetDirection = h * right + v * forward;
        // Debug.Log("targetDirection " + targetDirection);

        // Grounded controls
        if (grounded)
        {
            // Lock camera for short period when transitioning moving & standing still
            this.lockCameraTimer += Time.deltaTime;
            if (this.isMoving != wasMoving)
                this.lockCameraTimer = 0.0f;

            // We store speed and direction seperately,
            // so that when the character stands still we still have a valid forward direction
            // moveDirection is always normalized, and we only update it if there is user input.
            if (targetDirection != Vector3.zero)
            {
                // If we are really slow, just snap to the target direction
                if (this.moveSpeed < this.walkSpeed * 0.9f && grounded)
                {
                    this.moveDirection = targetDirection.normalized;
                }
                // Otherwise smoothly turn towards it
                else
                {
                    this.moveDirection = Vector3.RotateTowards(this.moveDirection, targetDirection, this.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);

                    this.moveDirection = this.moveDirection.normalized;
                }
            }

            // Smooth the speed based on the current target direction
            float curSmooth = this.speedSmoothing * Time.deltaTime;

            // Choose target speed
            //* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

            this._characterState = PickupCharacterState.Idle;

            // Pick speed modifier
            if ((Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift)) && this.isMoving)
            {
                targetSpeed *= this.runSpeed;
                this._characterState = PickupCharacterState.Running;
            }
            else if (Time.time - this.trotAfterSeconds > this.walkTimeStart)
            {
                targetSpeed *= this.trotSpeed;
                this._characterState = PickupCharacterState.Trotting;
            }
            else if (this.isMoving)
            {
                targetSpeed *= this.walkSpeed;
                this._characterState = PickupCharacterState.Walking;
            }

            this.moveSpeed = Mathf.Lerp(this.moveSpeed, targetSpeed, curSmooth);

            // Reset walk time start when we slow down
            if (this.moveSpeed < this.walkSpeed * 0.3f)
                this.walkTimeStart = Time.time;
        }
        // In air controls
        else
        {
            // Lock camera while in air
            if (this.jumping)
                this.lockCameraTimer = 0.0f;

            if (this.isMoving)
                this.inAirVelocity += targetDirection.normalized * Time.deltaTime *this.inAirControlAcceleration;
        }
    }

    void ApplyJumping()
    {
        // Prevent jumping too fast after each other
        if (this.lastJumpTime + this.jumpRepeatTime > Time.time)
            return;

        if (this.IsGrounded())
        {
            // Jump
            // - Only when pressing the button down
            // - With a timeout so you can press the button slightly before landing		
            if (this.canJump && Time.time < this.lastJumpButtonTime + this.jumpTimeout)
            {
                this.verticalSpeed = this.CalculateJumpVerticalSpeed(this.jumpHeight);
                SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void ApplyGravity()
    {
        if (this.isControllable)	// don't move player at all if not controllable.
        {
            // Apply gravity
            //bool jumpButton = Input.GetButton("Jump");
            
            // When we reach the apex of the jump we send out a message
            if (this.jumping && !this.jumpingReachedApex && this.verticalSpeed <= 0.0f)
            {
                this.jumpingReachedApex = true;
                SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
            }

            if (this.IsGrounded())
                this.verticalSpeed = 0.0f;
            else
                this.verticalSpeed -= this.gravity * Time.deltaTime;
        }
    }

    float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * targetJumpHeight *this.gravity);
    }

    void DidJump()
    {
        this.jumping = true;
        this.jumpingReachedApex = false;
        this.lastJumpTime = Time.time;
        //lastJumpStartHeight = transform.position.y;
        this.lastJumpButtonTime = -10;

        this._characterState = PickupCharacterState.Jumping;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //	Debug.DrawRay(hit.point, hit.normal);
        if (hit.moveDirection.y > 0.01f)
            return;
    }

    public float GetSpeed()
    {
        return this.moveSpeed;
    }

    public bool IsJumping()
    {
        return this.jumping;
    }

    public bool IsGrounded()
    {
        return (this.collisionFlags & CollisionFlags.CollidedBelow) != 0;
    }

    public Vector3 GetDirection()
    {
        return this.moveDirection;
    }

    public bool IsMovingBackwards()
    {
        return this.movingBack;
    }

    public float GetLockCameraTimer()
    {
        return this.lockCameraTimer;
    }

    public bool IsMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
    }

    public bool HasJumpReachedApex()
    {
        return this.jumpingReachedApex;
    }

    public bool IsGroundedWithTimeout()
    {
        return this.lastGroundedTime + this.groundedTimeout > Time.time;
    }

    public void Reset()
    {
        gameObject.tag = "Player";
    }
}