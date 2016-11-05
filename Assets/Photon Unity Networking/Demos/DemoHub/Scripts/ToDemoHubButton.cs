// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToDemoHubButton.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Demos
// </copyright>
// <summary>
//  Present a button on all launched demos from hub to allow getting back to the demo hub.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------



#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3) || UNITY_6
#define UNITY_MIN_5_4
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExitGames.Demos
{
	/// <summary>
	/// Present a button on all launched demos from hub to allow getting back to the demo hub.
	/// </summary>
	public class ToDemoHubButton : MonoBehaviour
	{

		private static ToDemoHubButton instance;


		CanvasGroup _canvasGroup;

		public static ToDemoHubButton Instance
	    {
	        get
	        {
	            if (instance == null)
	            {
					instance = FindObjectOfType(typeof (ToDemoHubButton)) as ToDemoHubButton;
	            }

	            return instance;
	        }
	    }

	    public void Awake()
	    {
	        if (Instance != null && Instance != this)
	        {
	            Destroy(gameObject);
	        }
	    }

	    // Use this for initialization
	    public void Start()
	    {
	        DontDestroyOnLoad(gameObject);

			_canvasGroup = GetComponent<CanvasGroup>();


			#if UNITY_MIN_5_4
			// Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
			{
				this.CalledOnLevelWasLoaded(scene.buildIndex);
			};
			#endif


	    }

		#if !UNITY_MIN_5_4
		/// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
		void OnLevelWasLoaded(int level)
		{
			this.CalledOnLevelWasLoaded(level);
		}
		#endif
		

		void CalledOnLevelWasLoaded(int level)
		{
			Debug.Log("CalledOnLevelWasLoaded");
			if (EventSystem.current == null)
			{
				Debug.LogError("no eventSystem");
			}
		}


	    public void Update()
	    {
	        bool sceneZeroLoaded = false;

	        #if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
	        sceneZeroLoaded = SceneManager.GetActiveScene().buildIndex == 0;
	        #else
	        sceneZeroLoaded = Application.loadedLevel == 0;
	        #endif

			if (sceneZeroLoaded && _canvasGroup.alpha!= 0f)
			{
				_canvasGroup.alpha = 0f;
				_canvasGroup.interactable = false;
			}

			if (!sceneZeroLoaded && _canvasGroup.alpha!= 1f)
			{
				_canvasGroup.alpha = 1f;
				_canvasGroup.interactable = true;
			}
	       
	    }

		public void BackToHub()
		{
			PhotonNetwork.Disconnect();
			SceneManager.LoadScene(0);
		}

	}
}