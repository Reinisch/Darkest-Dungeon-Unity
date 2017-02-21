using System;
using UnityEngine;
using System.Collections;

namespace FMODUnity
{
    [AddComponentMenu("FMOD Studio/FMOD Studio Listener")]
    public class StudioListener : MonoBehaviour
    {
        Rigidbody rigidBody;

        void OnEnable()
        {
            RuntimeUtils.EnforceLibraryOrder();
            rigidBody = gameObject.GetComponent<Rigidbody>();
            RuntimeManager.HasListener = true;
            RuntimeManager.SetListenerLocation(gameObject, rigidBody);
        }

        void OnDisable()
        {
            RuntimeManager.HasListener = false;
        }

        void Update()
        {
            RuntimeManager.SetListenerLocation(gameObject, rigidBody);
        }
    }
}
