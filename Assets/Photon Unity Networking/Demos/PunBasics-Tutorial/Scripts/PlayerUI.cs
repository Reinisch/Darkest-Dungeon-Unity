// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerUI.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in DemoAnimator to deal with the networked player instance UI display tha follows a given player to show its health and name
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


using UnityEngine;
using UnityEngine.UI;

using System.Collections;

namespace ExitGames.Demos.DemoAnimator
{
	/// <summary>
	/// Player UI. Constraint the UI to follow a PlayerManager GameObject in the world,
	/// Affect a slider and text to display Player's name and health
	/// </summary>
	public class PlayerUI : MonoBehaviour {

		#region Public Properties

		[Tooltip("Pixel offset from the player target")]
		public Vector3 ScreenOffset = new Vector3(0f,30f,0f);

		[Tooltip("UI Text to display Player's Name")]
		public Text PlayerNameText;

		[Tooltip("UI Slider to display Player's Health")]
		public Slider PlayerHealthSlider;

		#endregion

		#region Private Properties

		PlayerManager _target;

		float _characterControllerHeight = 0f;

		Transform _targetTransform;

		Renderer _targetRenderer;

		Vector3 _targetPosition;

		#endregion

		#region MonoBehaviour Messages
		
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase
		/// </summary>
		void Awake(){

			this.GetComponent<Transform>().SetParent (GameObject.Find("Canvas").GetComponent<Transform>());
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// update the health slider to reflect the Player's health
		/// </summary>
		void Update()
		{
			// Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
			if (_target == null) {
				Destroy(this.gameObject);
				return;
			}


			// Reflect the Player Health
			if (PlayerHealthSlider != null) {
				PlayerHealthSlider.value = _target.Health;
			}
		}

		/// <summary>
		/// MonoBehaviour method called after all Update functions have been called. This is useful to order script execution.
		/// In our case since we are following a moving GameObject, we need to proceed after the player was moved during a particular frame.
		/// </summary>
		void LateUpdate () {

			// Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
			if (_targetRenderer!=null) {
				this.gameObject.SetActive(_targetRenderer.isVisible);
			}
			
			// #Critical
			// Follow the Target GameObject on screen.
			if (_targetTransform!=null)
			{
				_targetPosition = _targetTransform.position;
				_targetPosition.y += _characterControllerHeight;
				
				this.transform.position = Camera.main.WorldToScreenPoint (_targetPosition) + ScreenOffset;
			}

		}




		#endregion

		#region Public Methods

		/// <summary>
		/// Assigns a Player Target to Follow and represent.
		/// </summary>
		/// <param name="target">Target.</param>
		public void SetTarget(PlayerManager target){

			if (target == null) {
				Debug.LogError("<Color=Red><b>Missing</b></Color> PlayMakerManager target for PlayerUI.SetTarget.",this);
				return;
			}

			// Cache references for efficiency because we are going to reuse them.
			_target = target;
			_targetTransform = _target.GetComponent<Transform>();
			_targetRenderer = _target.GetComponent<Renderer>();


			CharacterController _characterController = _target.GetComponent<CharacterController> ();

			// Get data from the Player that won't change during the lifetime of this Component
			if (_characterController != null){
				_characterControllerHeight = _characterController.height;
			}

			if (PlayerNameText != null) {
				PlayerNameText.text = _target.photonView.owner.name;
			}
		}

		#endregion

	}
}