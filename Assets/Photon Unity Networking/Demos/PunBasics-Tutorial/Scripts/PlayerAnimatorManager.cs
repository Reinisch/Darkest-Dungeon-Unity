// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerAnimatorManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in DemoAnimator to cdeal with the networked player Animator Component controls.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


using UnityEngine;
using System.Collections;

namespace ExitGames.Demos.DemoAnimator
{
	public class PlayerAnimatorManager : Photon.MonoBehaviour 
	{
		#region PUBLIC PROPERTIES

		public float DirectionDampTime = 0.25f;

		#endregion

		#region PRIVATE PROPERTIES

		Animator animator;
	//	PhotonAnimatorView animatorView;

		#endregion

		#region MONOBEHAVIOUR MESSAGES

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
	    void Start () 
	    {
	        animator = GetComponent<Animator>();
	//		animatorView = GetComponent<PhotonAnimatorView>();
	    }
	        
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// </summary>
	    void Update () 
	    {

			// Prevent control is connected to Photon and represent the localPlayer
	        if( photonView.isMine == false && PhotonNetwork.connected == true )
	        {
	            return;
	        }

			// failSafe is missing Animator component on GameObject
	        if (!animator)
	        {
				return;
			}

			// deal with Jumping
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);			

			// only allow jumping if we are running.
            if (stateInfo.IsName("Base Layer.Run"))
            {
				// When using trigger parameter
				if (Input.GetButtonDown("Fire2")) animator.SetTrigger("Jump"); 
			}
           
			// deal with movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

			// prevent negative Speed.
            if( v < 0 )
            {
                v = 0;
            }

			// set the Animator Parameters
            animator.SetFloat( "Speed", h*h+v*v );
            animator.SetFloat( "Direction", h, DirectionDampTime, Time.deltaTime );
	    }

		#endregion

	}
}