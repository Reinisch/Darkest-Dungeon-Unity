using UnityEngine;

public class OnDoubleclickDestroy : Photon.MonoBehaviour
{
    private float timeOfLastClick;
    private const float ClickDeltaForDoubleclick = 0.2f;


    // called by InputToEvent.
    // we use a short timeout to detect double clicks.
    // on double click, the networked object gets destroyed (on all clients).
    private void OnClick()
    {
        if (!this.photonView.isMine)
        {
            // this networkView (provided by Photon.MonoBehaviour) says the object is not ours.
            // so this client can't destroy it.
            return;
        }

        if (Time.time - this.timeOfLastClick < ClickDeltaForDoubleclick)
        {
            // double click => destory in network
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            this.timeOfLastClick = Time.time;
        }
    }
}