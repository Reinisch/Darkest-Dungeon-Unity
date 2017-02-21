using UnityEngine;
using System;
using System.Collections.Generic;

namespace FMODUnity
{
    [AddComponentMenu("FMOD Studio/FMOD Studio Event Emitter")]
    public class StudioEventEmitter : MonoBehaviour
    {
        [EventRef]
        public String Event;
        public EmitterGameEvent PlayEvent;
        public EmitterGameEvent StopEvent;
        public String CollisionTag;
        public bool AllowFadeout = true;
        public bool TriggerOnce = false;

        public ParamRef[] Params;
        
        private FMOD.Studio.EventDescription eventDescription;
        private FMOD.Studio.EventInstance instance;
        private bool hasTriggered;
        private bool isQuitting;

        void Start() 
        {
            RuntimeUtils.EnforceLibraryOrder();
            HandleGameEvent(EmitterGameEvent.LevelStart);
        }

        void OnApplicationQuit()
        {
            isQuitting = true;
        }

        void OnDestroy()
        {
            if (!isQuitting)
            {
                HandleGameEvent(EmitterGameEvent.LevelEnd);
                if (instance != null && instance.isValid())
                {
                    RuntimeManager.DetachInstanceFromGameObject(instance);
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (String.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
            {
                HandleGameEvent(EmitterGameEvent.TriggerEnter);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (String.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
            {
                HandleGameEvent(EmitterGameEvent.TriggerExit);
            }
        }

        void OnCollisionEnter()
        {
            HandleGameEvent(EmitterGameEvent.CollisionEnter);
        }

        void OnCollisionExit()
        {
            HandleGameEvent(EmitterGameEvent.CollisionExit);
        }

        void HandleGameEvent(EmitterGameEvent gameEvent)
        {
            if (PlayEvent == gameEvent)
            {
                Play();
            }
            if (StopEvent == gameEvent)
            {
                Stop();
            }
        }

        void Lookup()
        {
            eventDescription = RuntimeManager.GetEventDescription(Event);
        }

        public void Play()
        {
            if (TriggerOnce && hasTriggered)
            {
                return;
            }

            if (String.IsNullOrEmpty(Event))
            {
                return;
            }

            if (eventDescription == null)
            {
                Lookup();
            }

            bool isOneshot = false;
            if (!Event.StartsWith("snapshot", StringComparison.CurrentCultureIgnoreCase))
            {
                eventDescription.isOneshot(out isOneshot);
            }
            bool is3D;
            eventDescription.is3D(out is3D);

            if (instance != null && !instance.isValid())
            {
                instance = null;
            }

            // Let previous oneshot instances play out
            if (isOneshot && instance != null)
            {
                instance.release();
                instance = null;
            }

            if (instance == null)
            {
                eventDescription.createInstance(out instance);

                // Only want to update if we need to set 3D attributes
                if (is3D)
                {
                    var rigidBody = GetComponent<Rigidbody>();
                    var transform = GetComponent<Transform>();
                    instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject, rigidBody));
                    RuntimeManager.AttachInstanceToGameObject(instance, transform, rigidBody);
                }
            }

            foreach(var param in Params)
            {
                instance.setParameterValue(param.Name, param.Value);
            }

            instance.start();

            hasTriggered = true;

        }

        public void Stop()
        {
            if (instance != null)
            {
                instance.stop(AllowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
                instance = null;
            }
        }
        
        public void SetParameter(string name, float value)
        {
            if (instance != null)
            {
                instance.setParameterValue(name, value);
            }
        }
        
        public bool IsPlaying()
        {
            if (instance != null && instance.isValid())
            {
                FMOD.Studio.PLAYBACK_STATE playbackState;
                instance.getPlaybackState(out playbackState);
                return (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED);
            }
            return false;
        }        
    }
}
