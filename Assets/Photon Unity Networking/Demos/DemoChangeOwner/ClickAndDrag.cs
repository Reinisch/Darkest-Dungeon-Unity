using UnityEngine;
using System.Collections;

public class ClickAndDrag : Photon.MonoBehaviour
{
    private Vector3 camOnPress;
    private bool following;
    private float factor = -0.1f;

	// Update is called once per frame
	void Update ()
	{
        if (!photonView.isMine)
        {
            return;
        }

	    InputToEvent input = Camera.main.GetComponent<InputToEvent>();
	    if (input == null) return;
        if (!following)
        {
            if (input.Dragging)
            {
                camOnPress = this.transform.position;
                following = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            if (input.Dragging)
            {
                Vector3 target = camOnPress - (new Vector3(input.DragVector.x, 0, input.DragVector.y) * factor);
                this.transform.position = Vector3.Lerp(this.transform.position, target, Time.deltaTime*.5f);
            }
            else
            {
                camOnPress = Vector3.zero;
                following = false;
            }
        }
	}
}
