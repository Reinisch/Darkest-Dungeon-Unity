using UnityEngine;

public class OnClickRightDestroy : MonoBehaviour
{
    public void OnPressRight()
    {
        Debug.Log("RightClick Destroy");
        PhotonNetwork.Destroy(gameObject);
    }
}