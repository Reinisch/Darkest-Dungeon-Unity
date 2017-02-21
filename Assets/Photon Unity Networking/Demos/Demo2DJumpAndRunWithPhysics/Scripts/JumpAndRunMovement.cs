using UnityEngine;
using System.Collections;

public class JumpAndRunMovement : MonoBehaviour
{
    public float Speed;
    public float JumpForce;

    Animator m_Animator;
    Rigidbody2D m_Body;
    PhotonView m_PhotonView;

    bool m_IsGrounded;

    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        UpdateIsGrounded();
        UpdateIsRunning();
        UpdateFacingDirection();
    }

    void FixedUpdate()
    {
        if( m_PhotonView.isMine == false )
        {
            return;
        }

        UpdateMovement();
        UpdateJumping();
    }

    void UpdateFacingDirection()
    {
        if( m_Body.velocity.x > 0.2f )
        {
            transform.localScale = new Vector3( 1, 1, 1 );
        }
        else if( m_Body.velocity.x < -0.2f )
        {
            transform.localScale = new Vector3( -1, 1, 1 );
        }
    }

    void UpdateJumping()
    {
        if (Input.GetButton("Jump") && m_IsGrounded)
        {
            m_Animator.SetTrigger("IsJumping");
            m_Body.AddForce(Vector2.up * JumpForce);
            m_PhotonView.RPC("DoJump", PhotonTargets.Others);
        }
    }

    [PunRPC]
    void DoJump()
    {
        m_Animator.SetTrigger( "IsJumping" );
    }

    void UpdateMovement()
    {
        Vector2 movementVelocity = m_Body.velocity;

        if( Input.GetAxisRaw( "Horizontal" ) > 0.5f )
        {
            movementVelocity.x = Speed;

        }
        else if( Input.GetAxisRaw( "Horizontal" ) < -0.5f )
        {
            movementVelocity.x = -Speed;
        }
        else
        {
            movementVelocity.x = 0;
        }

        m_Body.velocity = movementVelocity;
    }

    void UpdateIsRunning()
    {
        m_Animator.SetBool( "IsRunning", Mathf.Abs( m_Body.velocity.x ) > 0.1f );
    }

    void UpdateIsGrounded()
    {
        Vector2 position = new Vector2( transform.position.x, transform.position.y );

        //RaycastHit2D hit = Physics2D.Raycast( position, -Vector2.up, 0.1f, 1 << LayerMask.NameToLayer( "Ground" ) );
        RaycastHit2D hit = Physics2D.Raycast(position, -Vector2.up, 0.1f);

        m_IsGrounded = hit.collider != null;
        m_Animator.SetBool( "IsGrounded", m_IsGrounded );
    }
}
