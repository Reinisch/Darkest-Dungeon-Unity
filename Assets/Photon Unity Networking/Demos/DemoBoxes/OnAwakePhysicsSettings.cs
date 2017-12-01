using UnityEngine;

/// <summary>
/// Sets isKinematic to true, GameObject is owned by a another player (PhotonView.isMine == false).
/// For Rigidbody and Rigidbody2D.
/// </summary>
[RequireComponent(typeof (PhotonView))]
public class OnAwakePhysicsSettings : Photon.MonoBehaviour
{
    public void Awake()
    {
        if (!photonView.isMine)
        {
            Rigidbody attachedRigidbody = GetComponent<Rigidbody>();
            if (attachedRigidbody != null)
            {
                attachedRigidbody.isKinematic = true;
            }
            else
            {
                Rigidbody2D attachedRigidbody2d = GetComponent<Rigidbody2D>();
                if (attachedRigidbody2d != null)
                {
                    attachedRigidbody2d.isKinematic = true;
                }
            }
        }
    }
}
