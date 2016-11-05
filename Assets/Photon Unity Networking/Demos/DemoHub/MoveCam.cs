using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class MoveCam : MonoBehaviour
{
    private Vector3 originalPos;
    private Vector3 randomPos;
    private Transform camTransform;
    public Transform lookAt;

	// Use this for initialization
	void Start () 
    {
        camTransform = GetComponent<Camera>().transform;
	    originalPos = camTransform.position;

        randomPos = originalPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-1, 1));
    }
	
	// Update is called once per frame
    private void Update()
    {
        camTransform.position = Vector3.Slerp(camTransform.position, randomPos, Time.deltaTime);
        camTransform.LookAt(lookAt);
        if (Vector3.Distance(camTransform.position, randomPos) < 0.5f)
        {
            randomPos = originalPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-1, 1));
        }
    }
}
