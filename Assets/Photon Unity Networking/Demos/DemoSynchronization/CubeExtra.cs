// 
// This script calculates how fast the object moved between any two consecutive updates. 
// Assuming that the movement stays about the same, we can move the local copy of a cube fairly well.
//
// Note that CubeExtra doesn't lag as much as Interpolation. 
// Based on what we know as given, we move our cube to a guessed (!) position where it probably is.
// As consequence, you will notice that this "simulation" does overshoot and speed might be higher/lower 
// than in the original.
//
// This extrapolation script uses only position updates. This is very lean. 
// The controlling client could calculate speed and send that, but it's not needed!
// 


using UnityEngine;

[RequireComponent(typeof (PhotonView))]
public class CubeExtra : Photon.MonoBehaviour, IPunObservable
{
    [Range(0.9f, 1.1f)]
    public float Factor = 0.98f;    // this factor makes the extrapolated movement a bit slower. the idea is to compensate some of the lag-dependent variance.

    // some internal values. see comments below
    private Vector3 latestCorrectPos = Vector3.zero;
    private Vector3 movementVector = Vector3.zero;
    private Vector3 errorVector = Vector3.zero;
    private double lastTime = 0;


    public void Awake()
    {
        this.latestCorrectPos = transform.position;
    }


    // this method is called by PUN when this script is being "observed" by a PhotonView (setup in inspector)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // the controlling player of a cube sends only the position
            Vector3 pos = transform.localPosition;
            stream.Serialize(ref pos);
        }
        else
        {
            // other players (not controlling this cube) will read the position and timing and calculate everything else based on this
            Vector3 updatedLocalPos = Vector3.zero;
            stream.Serialize(ref updatedLocalPos);

            double timeDiffOfUpdates = info.timestamp - this.lastTime;  // the time that passed after the sender sent it's previous update
            this.lastTime = info.timestamp;


            // the movementVector calculates how far the "original" cube moved since it sent the last update.
            // we calculate this based on the sender's timing, so we exclude network lag. that makes our movement smoother.
            this.movementVector = (updatedLocalPos - this.latestCorrectPos) / (float)timeDiffOfUpdates;

            // the errorVector is how far our cube is away from the incoming position update. using this corrects errors somewhat, until the next update arrives.
            // with this, we don't have to correct our cube's position with a new update (which introduces visible, hard stuttering).
            this.errorVector = (updatedLocalPos - transform.localPosition) / (float)timeDiffOfUpdates;

            
            // next time we get an update, we need this update's position:
            this.latestCorrectPos = updatedLocalPos;
        }
    }


    public void Update()
    {
        if (photonView.isMine)
        {
            return; // if this object is under our control, we don't need to apply received position-updates 
        }

        // we move the object, based on the movement it did between the last two updates.
        // in addition, we factor in the error between the last correct update we got and our extrapolated position at that time.
        transform.localPosition += (this.movementVector + this.errorVector) * this.Factor * Time.deltaTime;
    }
}