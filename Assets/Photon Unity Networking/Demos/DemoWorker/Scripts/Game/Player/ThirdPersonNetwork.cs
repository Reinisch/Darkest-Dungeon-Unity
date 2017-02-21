using UnityEngine;
using System.Collections;

public class ThirdPersonNetwork : Photon.MonoBehaviour
{
    ThirdPersonCamera cameraScript;
    ThirdPersonController controllerScript;


	bool firstTake = false;

	void OnEnable()
	{
	 	firstTake = true;	 
	}

    void Awake()
    {
        cameraScript = GetComponent<ThirdPersonCamera>();
        controllerScript = GetComponent<ThirdPersonController>();

         if (photonView.isMine)
        {
            //MINE: local player, simply enable the local scripts
            cameraScript.enabled = true;
            controllerScript.enabled = true;
        }
        else
        {           
            cameraScript.enabled = false;

            controllerScript.enabled = true;
            controllerScript.isControllable = false;
        }

        gameObject.name = gameObject.name + photonView.viewID;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext((int)controllerScript._characterState);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation); 
        }
        else
        {
            //Network player, receive data
            controllerScript._characterState = (CharacterState)(int)stream.ReceiveNext();
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();

			// avoids lerping the character from "center" to the "current" position when this client joins
			if (firstTake)
			{
				firstTake = false;
				this.transform.position = correctPlayerPos;
				transform.rotation = correctPlayerRot;
			}

        }
    }

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
        }
    }

}