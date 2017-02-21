using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMODUnity
{
    public class OneshotList
    {
        List<FMOD.Studio.EventInstance> instances = new List<FMOD.Studio.EventInstance>();

        public void Add(FMOD.Studio.EventInstance instance)
        {
            instances.Add(instance);
        }

        public void Update(FMOD.ATTRIBUTES_3D attributes)
        {
            // Cull finished instances
            FMOD.Studio.PLAYBACK_STATE state;
            var finishedInstances = instances.FindAll((x) => { x.getPlaybackState(out state); return state == FMOD.Studio.PLAYBACK_STATE.STOPPED; });
            foreach (var instance in finishedInstances)
            {
                instance.release();
            }
            instances.RemoveAll((x) => !x.isValid());
            
            // Update 3D attributes
            foreach(var instance in instances)
            {
                instance.set3DAttributes(attributes);
            }
        }

        public void SetParameterValue(string name, float value)
        {
            foreach (var instance in instances)
            {
                instance.setParameterValue(name, value);
            }
        }

        public void StopAll(FMOD.Studio.STOP_MODE stopMode)
        {
            foreach (var instance in instances)
            {
                instance.stop(stopMode);
                instance.release();
            }
            instances.Clear();
        }
    }
}
