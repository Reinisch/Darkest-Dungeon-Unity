using System;
using UnityEngine;

namespace FMODUnity
{
    [Serializable]
    public class EmitterRef
    {
        public StudioEventEmitter Target;
        public ParamRef[] Params;
    }

    [AddComponentMenu("FMOD Studio/FMOD Studio Parameter Trigger")]
    public class StudioParameterTrigger: MonoBehaviour
    {                        
        public EmitterRef[] Emitters;
        public EmitterGameEvent TriggerEvent;
        public string CollisionTag;
        
        private bool firstUpdate = true;

        void OnEnable()
        {
        }

        void OnDestroy()
        {
            HandleGameEvent(EmitterGameEvent.LevelEnd);
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
            if (TriggerEvent == gameEvent)
            {
                TriggerParameters();
            }
        }
        void Update()
        {
            if (firstUpdate)
            {
                HandleGameEvent(EmitterGameEvent.LevelStart);
                firstUpdate = false;
                enabled = false;
            }
        }
        public void TriggerParameters()
        {
            for (int i = 0; i < Emitters.Length; i++)
            {
                var emitterRef = Emitters[i];
                if (emitterRef.Target != null)
                {
                    for (int j = 0; j < Emitters[i].Params.Length; j++)
                    {
                        emitterRef.Target.SetParameter(Emitters[i].Params[j].Name, Emitters[i].Params[j].Value);
                    }
                }
            }
        }
    }
}
