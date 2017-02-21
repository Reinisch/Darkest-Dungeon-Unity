using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/// <summary>
/// This component can be used to move objects along a path. Optionally, the path can be updated by 
/// </summary>
public class DragToMove : MonoBehaviour
{
    public float speed = 5.0f;
    public Transform[] cubes;
    public List<Vector3> PositionsQueue = new List<Vector3>(20);

    private Vector3[] cubeStartPositions;
    private int nextPosIndex = 0;
    private float lerpTime = 0.0f;
    
    private bool recording;


    /// <summary>
    /// Initializes the positioning of the objects-to-move.
    /// </summary>
    public void Start()
    {
        // per object-to-move, we get the initial (position) offset, so we can move each object to a slightly separate position on the path.
        this.cubeStartPositions = new Vector3[this.cubes.Length];
        for (int i = 0; i < this.cubes.Length; i++)
        {
            Transform cube = this.cubes[i];
            this.cubeStartPositions[i] = cube.position;
        }
    }


    /// <summary>
    /// This actually moves the objects along a defined path (unless a new path is being recorded).
    /// </summary>
    public void Update()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return; // only the Master Client can define the path
        }

        if (recording)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            StartCoroutine("RecordMouse");
            return;
        }

        if (PositionsQueue.Count == 0)
        {
            return;
        }

        
        Vector3 targetPos = PositionsQueue[nextPosIndex];
        
        int prevPosIndex = this.nextPosIndex > 0 ? this.nextPosIndex - 1 : PositionsQueue.Count-1;
        Vector3 prevPos = PositionsQueue[prevPosIndex];
        lerpTime = lerpTime+Time.deltaTime * speed;

        for (int i = 0; i < this.cubes.Length; i++)
        {
            Transform cube = this.cubes[i];
            
            Vector3 cubeTargetPos = targetPos + this.cubeStartPositions[i];
            Vector3 cubePrevPos = prevPos +  this.cubeStartPositions[i];

            cube.transform.position = Vector3.Lerp(cubePrevPos, cubeTargetPos, lerpTime);
        }
        if (lerpTime > 1.0f)
        {
            nextPosIndex = (nextPosIndex + 1) % this.PositionsQueue.Count;
            lerpTime = 0.0f;
        }
    }

    /// <summary>
    /// Coroutine to record a path. Either records mouse- or touch-positions while (button) pressed.
    /// </summary>
    public IEnumerator RecordMouse()
    {
        recording = true;
        PositionsQueue.Clear();

        while (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            yield return new WaitForSeconds(0.1f);
            

            Vector3 v3 = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                // if we have touch input, we use that instead of mouse
                v3 = Input.GetTouch(0).position;
            }


            Ray inputRay = Camera.main.ScreenPointToRay(v3);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                PositionsQueue.Add(hit.point);
            }
        }
        
        nextPosIndex = 0;
        recording = false;
    }

}
