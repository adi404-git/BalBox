using UnityEngine;
public class CameraOrbit : MonoBehaviour
{
    [Header("Target")]
    public Transform pivot;

    [Header("Radius")]
    public float baseRadius      = 10f;
    public float expansionFactor = 0.3f;
    public float minRadius       = 7f;
    public float maxRadius       = 26f;
    public float radiusSmoothTime = 0.6f;

    [Header("Orbit Speed")]
    public float keyOrbitSpeed = 75f; 
    public float mouseOrbitSpeed = 0.3f;

    [Header("Free-Look (LMB drag)")]
    public float lookSpeedX   = 0.25f;
    public float lookSpeedY   = 0.2f;
    public float maxLookYaw   = 70f;
    public float maxLookPitch = 40f;

    [Header("Reset")]
    public float resetSmoothTime = 0.2f;

    [Header("LMB click threshold")]
    public float dragThreshold = 5f;

    // orbit angle around pivot
    private float theta = 180f;
    private float currentRadius;
    private float radiusVelocity;

    // free-look offsets
    private float lookYaw;
    private float lookPitch;
    private float yawVel;
    private float pitchVel;
    private bool  isResetting;
    private bool    lmbDrag;
    private Vector2 lmbDownPos;
    public bool LMBIsDrag => lmbDrag;

    void Start()
    {
        currentRadius = baseRadius;
        if (pivot != null)
        {
            Vector3 off = transform.position - pivot.position;
            theta = Mathf.Atan2(off.x, off.z) * Mathf.Rad2Deg;
        }
    }

    void LateUpdate()
    {
        if (pivot == null) return;
        TrackLMB();
        HandleInputs();
        Apply();
    }

    void TrackLMB()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lmbDownPos = Input.mousePosition;
            lmbDrag    = false;
        }
        if (Input.GetMouseButton(0) && !lmbDrag)
            if (Vector2.Distance(Input.mousePosition, lmbDownPos) > dragThreshold)
                lmbDrag = true;
        if (Input.GetMouseButtonUp(0))
            lmbDrag = false;
    }

    void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.R))
            isResetting = true;

        if (isResetting)
        {
            lookYaw   = Mathf.SmoothDampAngle(lookYaw,   0f, ref yawVel,   resetSmoothTime);
            lookPitch = Mathf.SmoothDampAngle(lookPitch, 0f, ref pitchVel, resetSmoothTime);
            if (Mathf.Abs(lookYaw) < 0.1f && Mathf.Abs(lookPitch) < 0.1f)
            {
                lookYaw = lookPitch = 0f;
                isResetting = false;
            }
            // Still allow orbit while resetting
        }
        else if (lmbDrag && Input.GetMouseButton(0))
        {
            // Free-look: move view, not position
            lookYaw   += Input.GetAxis("Mouse X") * lookSpeedX * 100f * Time.deltaTime;
            lookPitch -= Input.GetAxis("Mouse Y") * lookSpeedY * 100f * Time.deltaTime;
            lookYaw    = Mathf.Clamp(lookYaw,   -maxLookYaw,   maxLookYaw);
            lookPitch  = Mathf.Clamp(lookPitch, -maxLookPitch, maxLookPitch);
        }

        float orbitDir = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  orbitDir -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) orbitDir += 1f;
        theta += orbitDir * keyOrbitSpeed * Time.deltaTime;
        //rmb dragging implementation:
        if (Input.GetMouseButton(1))
            theta += Input.GetAxis("Mouse X") * mouseOrbitSpeed * 100f * Time.deltaTime;
    }

    void Apply()
    {
        float targetR = Mathf.Clamp(baseRadius + pivot.position.y * expansionFactor, minRadius, maxRadius);
        currentRadius = Mathf.SmoothDamp(currentRadius, targetR, ref radiusVelocity, radiusSmoothTime);

        float rad = theta * Mathf.Deg2Rad;
        transform.position = pivot.position + new Vector3(
            Mathf.Sin(rad) * currentRadius, 0f, Mathf.Cos(rad) * currentRadius);

        Vector3    dir      = (pivot.position - transform.position).normalized;
        Quaternion baseLook = Quaternion.LookRotation(dir);
        Quaternion offset   = Quaternion.Euler(lookPitch, lookYaw, 0f);
        transform.rotation  = baseLook * offset;
    }
}
