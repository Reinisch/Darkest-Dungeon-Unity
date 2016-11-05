using UnityEngine;
using System.Collections;

/// <summary>
/// Simple script to forward a sub-objects OnTriggerEnter to the parent's PickupItem-component.
/// </summary>
/// <remarks>
/// This is useful when a physcial object has a collider but also needs a trigger.
/// </remarks>
public class PickupTriggerForward : MonoBehaviour {

    

    public void OnTriggerEnter(Collider other)
    {
        PickupItem parentPickupItem = this.transform.parent.GetComponent<PickupItem>();
        if (parentPickupItem != null)
        {
           parentPickupItem.OnTriggerEnter(other);
        }

        // by enabling this log, you can see that more triggers are forwarded than necessary.
        // collisions with any collider will trigger, which is not perfect but OK in the demo.
        //Debug.Log("Triggered. Called parent: " + ((parentPickupItem != null)));
    }
}
