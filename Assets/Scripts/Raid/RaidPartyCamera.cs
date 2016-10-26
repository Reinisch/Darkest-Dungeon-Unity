using UnityEngine;
using System.Diagnostics;
using UnityStandardAssets.ImageEffects;

public enum CameraMode { Static, Follow }

public class RaidPartyCamera : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.7F;
    public BlurOptimized blur;
    public Camera blurCamera;
    public Light raidLight;
    private Vector3 velocity = Vector3.zero;

    public RaidPartyController controller;

    public float StandardFOV { get; set; }
    public float TargetFOV { get; set; }
    public float SmoothTimeFOV { get; set; }
    float velocityFOV = 0;

    float frustumDistanceTarget = 0;
    float frustumTargetWidth = 252.2945f;

    public Camera Camera { get; set; }

    public Stopwatch StopWatch { get; set; }
    public CameraMode mode;

    public Transform Transform { get; set; }
    public Vector3 ActionPosition
    {
        get
        {
            if(mode == CameraMode.Follow)
                return new Vector3(RaidSceneManager.DungeonCamera.target.position.x,
                    RaidSceneManager.DungeonCamera.Transform.position.y,
                    RaidSceneManager.DungeonCamera.Transform.position.z);
            else
                return new Vector3(RaidSceneManager.DungeonCamera.Transform.position.x,
                    RaidSceneManager.DungeonCamera.Transform.position.y,
                    RaidSceneManager.DungeonCamera.Transform.position.z);
        }
    }

    void Awake()
    {
        Camera = GetComponent<Camera>();
        Transform = GetComponent<Transform>();
        StopWatch = new Stopwatch();
    }

    void Start()
    {
        SmoothTimeFOV = 0.1f;
        StandardFOV = 60;
        TargetFOV = 60;

        Vector3 defaultRoomCameraPosition = new Vector3(-1069.303f, 0, -300);
        frustumDistanceTarget = Vector3.Distance(defaultRoomCameraPosition,
            RaidSceneManager.RoomView.raidRoom.RectTransform.position);
    }

    void LateUpdate()
    {
        if (!Mathf.Approximately(Camera.fieldOfView, TargetFOV))
        {
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, TargetFOV, ref velocityFOV, SmoothTimeFOV);
            blurCamera.fieldOfView = Camera.fieldOfView;
            return;
        }

        var frustumHeight = 2.0f * frustumDistanceTarget * Mathf.Tan(Camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * Camera.aspect;

        if (StandardFOV == TargetFOV && !Mathf.Approximately(frustumWidth, frustumTargetWidth))
        {
            // change field of view to fit in default room/corridor view in every aspect ratio
            StandardFOV = 2.0f * Mathf.Atan(frustumTargetWidth / Camera.aspect * 0.5f / frustumDistanceTarget) * Mathf.Rad2Deg;
            TargetFOV = StandardFOV;
        }

        if (mode == CameraMode.Static)
        {
            return;
        }

        if (mode == CameraMode.Follow)
        {
            Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

    public void SetCampingLight()
    {
        raidLight.color = new Color(0.8f, 0, 0, 1);
        raidLight.type = LightType.Spot;
        raidLight.spotAngle = 60;
        raidLight.range = 150;
        raidLight.intensity = 5;
    }
    public void SetRaidingLight(TorchRangeType torchRange)
    {
        raidLight.color = Color.white;
        raidLight.type = LightType.Point;
        raidLight.range = 150;
        raidLight.intensity = 7;
    }
    public void Zoom(float targetFOV, float time)
    {
        TargetFOV = targetFOV;
        SmoothTimeFOV = time;
    }
}