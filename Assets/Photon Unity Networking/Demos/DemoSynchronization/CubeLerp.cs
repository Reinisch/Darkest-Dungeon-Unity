//
// A (very) simple network interpolation script, using Lerp().
//
// This will lag-behind, compared to the moving cube on the controlling client.
// Actually, we deliberately lag behing a bit more, to avoid stops, if updates arrive late.
//
// This script does not hide loss very well and might stop the local cube.
//

using UnityEngine;


[RequireComponent(typeof (PhotonView))]
public class CubeLerp : Photon.MonoBehaviour, IPunObservable
{
    private Vector3 latestCorrectPos;
    private Vector3 onUpdatePos;
    private float fraction;


    public void Start()
    {
        this.latestCorrectPos = transform.position;
        this.onUpdatePos = transform.position;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
        }
        else
        {
            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            this.latestCorrectPos = pos;                // save this to move towards it in FixedUpdate()
            this.onUpdatePos = transform.localPosition; // we interpolate from here to latestCorrectPos
            this.fraction = 0;                          // reset the fraction we alreay moved. see Update()

            transform.localRotation = rot;              // this sample doesn't smooth rotation
        }
    }


    public void Update()
    {
        if (this.photonView.isMine)
        {
            return;     // if this object is under our control, we don't need to apply received position-updates 
        }

        // We get 10 updates per sec. Sometimes a few less or one or two more, depending on variation of lag.
        // Due to that we want to reach the correct position in a little over 100ms. We get a new update then.
        // This way, we can usually avoid a stop of our interpolated cube movement.
        //
        // Lerp() gets a fraction value between 0 and 1. This is how far we went from A to B.
        //
        // So in 100 ms, we want to move from our previous position to the latest known. 
        // Our fraction variable should reach 1 in 100ms, so we should multiply deltaTime by 10.
        // We want it to take a bit longer, so we multiply with 9 instead!

        this.fraction = this.fraction + Time.deltaTime * 9;
        transform.localPosition = Vector3.Lerp(this.onUpdatePos, this.latestCorrectPos, this.fraction); // set our pos between A and B
    }
}