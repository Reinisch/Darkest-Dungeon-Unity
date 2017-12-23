using UnityEngine;
using UnityStandardAssets.ImageEffects;

public enum CameraMode { Static, Follow }

public class RaidPartyCamera : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float smoothTime = 0.7F;
    [SerializeField]
    private Light raidLight;
    [SerializeField]
    private CameraMode mode;

    [SerializeField]
    private BlurOptimized blur;
    [SerializeField]
    private Camera blurCamera;

    private bool zooming;
    private float velocityFOV;
    private float frustumDistanceTarget;
    private float frustumTargetWidth = 252.2945f;
    private Vector3 velocity = Vector3.zero;

    public float StandardFOV { get; set; }
    public float TargetFOV { get; set; }
    public float SmoothTimeFOV { get; set; }

    public Camera Camera { get; set; }
    public Transform Transform { get; set; }
    public Transform Target { get { return target; } set { target = value; } }
    public CameraMode Mode { get { return mode; } set { mode = value; } }

    private void Awake()
    {
        Camera = GetComponent<Camera>();
        Transform = GetComponent<Transform>();
    }

    private void Start()
    {
        SmoothTimeFOV = 0.1f;
        StandardFOV = 60;
        TargetFOV = 60;

        Vector3 defaultRoomCameraPosition = new Vector3(-1069.303f, 0, -300);
        frustumDistanceTarget = Vector3.Distance(defaultRoomCameraPosition,
            RaidSceneManager.RoomView.RaidRoom.RectTransform.position);
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(Camera.fieldOfView, TargetFOV))
        {
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, TargetFOV, ref velocityFOV, SmoothTimeFOV);
            blurCamera.fieldOfView = Camera.fieldOfView;
        }
        else if (zooming)
        {
            if (Mathf.Approximately(TargetFOV, StandardFOV))
                zooming = false;
        }
        else
        {
            var frustumHeight = 2.0f * frustumDistanceTarget * Mathf.Tan(Camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var frustumWidth = frustumHeight * Camera.aspect;

            if (Mathf.Approximately(StandardFOV, TargetFOV) && !zooming && !Mathf.Approximately(Mathf.Round(frustumWidth), Mathf.Round(frustumTargetWidth)))
            {
                // change field of view to fit in default room/corridor view in every aspect ratio
                StandardFOV = 2.0f * Mathf.Atan(frustumTargetWidth / Camera.aspect * 0.5f / frustumDistanceTarget) * Mathf.Rad2Deg;
                TargetFOV = StandardFOV;
            }
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
        zooming = true;
        TargetFOV = targetFOV;
        SmoothTimeFOV = time;
    }

    public void SwitchBlur(bool activate)
    {
#if !(UNITY_ANDROID || UNITY_IOS)
        blur.enabled = activate;
#endif
    }
}