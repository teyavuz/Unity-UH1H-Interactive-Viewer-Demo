using UnityEngine;

public class OrbitCameraController : MonoBehaviour
{
    public enum ControlMode
    {
        Freelook,
        AnchorInspect
    }

    [Header("Target")]
    public Transform target;      // HelicopterRoot (freelook pivot)
    public Transform cam;         // Main Camera transform

    [Header("Orbit")]
    public float rotateSpeed = 180f;
    public float pitchMin = -20f;
    public float pitchMax = 70f;

    [Header("Zoom")]
    public float zoomSpeed = 3f;
    public float minDistance = 2f;
    public float maxDistance = 6f;

    [Header("Smoothing")]
    public float rotationLerp = 15f;
    public float zoomLerp = 15f;

    [Header("Mode (Debug)")]
    [SerializeField] private ControlMode mode = ControlMode.Freelook;

    // Anchor inspect limits (runtime)
    float anchorBaseYaw;
    float anchorYawLimit = 25f;
    float anchorMinDist = 1.5f;
    float anchorMaxDist = 3.5f;
    bool anchorLockPitch = true;
    float anchorFixedPitch = 10f;

    float yaw;
    float pitch;
    float targetDistance;

    bool isDragging;
    Vector2 lastPointerPos;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("[OrbitCameraController] Target not set.");
            enabled = false;
            return;
        }

        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (cam == null)
        {
            Debug.LogError("[OrbitCameraController] Camera transform not set.");
            enabled = false;
            return;
        }

        var e = transform.rotation.eulerAngles;
        yaw = e.y;
        pitch = NormalizePitch(e.x);

        targetDistance = Mathf.Clamp(-cam.localPosition.z, minDistance, maxDistance);

        transform.position = target.position;
    }

    void Update()
    {
        // Freelook'ta pivot target, Inspect'te pivot kendi pozisyonunda kalacak (dışarıdan set edilecek)
        if (mode == ControlMode.Freelook && target != null)
            transform.position = target.position;

        HandlePointerDrag();
        HandleZoom();

        ApplyConstraints();

        var desiredRot = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationLerp);

        var desiredLocalPos = new Vector3(0f, 0f, -targetDistance);
        cam.localPosition = Vector3.Lerp(cam.localPosition, desiredLocalPos, Time.deltaTime * zoomLerp);
    }

    void HandlePointerDrag()
    {
        // Mouse
        if (Input.mousePresent)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastPointerPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                Vector2 cur = Input.mousePosition;
                Vector2 delta = cur - lastPointerPos;
                lastPointerPos = cur;

                yaw += (delta.x / Screen.width) * rotateSpeed;

                // Inspect modda pitch kilitli/kapalı olacak
                if (mode == ControlMode.Freelook)
                    pitch -= (delta.y / Screen.height) * rotateSpeed;
            }

            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
                targetDistance -= wheel * zoomSpeed * 0.2f;
        }

        // Touch
        if (Input.touchCount == 1)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastPointerPos = t.position;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
            else if (isDragging && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
            {
                Vector2 cur = t.position;
                Vector2 delta = cur - lastPointerPos;
                lastPointerPos = cur;

                yaw += (delta.x / Screen.width) * rotateSpeed;

                if (mode == ControlMode.Freelook)
                    pitch -= (delta.y / Screen.height) * rotateSpeed;
            }
        }
    }

    void HandleZoom()
    {
        // Pinch zoom
        if (Input.touchCount == 2)
        {
            var t0 = Input.GetTouch(0);
            var t1 = Input.GetTouch(1);

            Vector2 p0Prev = t0.position - t0.deltaPosition;
            Vector2 p1Prev = t1.position - t1.deltaPosition;

            float prevMag = (p0Prev - p1Prev).magnitude;
            float curMag = (t0.position - t1.position).magnitude;
            float diff = curMag - prevMag;

            targetDistance -= diff * (zoomSpeed / Screen.dpi);
        }
    }

    void ApplyConstraints()
    {
        if (mode == ControlMode.Freelook)
        {
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            return;
        }

        // AnchorInspect
        float minYaw = anchorBaseYaw - anchorYawLimit;
        float maxYaw = anchorBaseYaw + anchorYawLimit;
        yaw = ClampAngle(yaw, minYaw, maxYaw);

        if (anchorLockPitch)
            pitch = anchorFixedPitch;

        targetDistance = Mathf.Clamp(targetDistance, anchorMinDist, anchorMaxDist);
    }

    public void SetFreelook()
    {
        mode = ControlMode.Freelook;
        // Freelook limitlerini kullanır (minDistance/maxDistance vs.)
    }

    public void SetAnchorInspect(CameraAnchor anchor)
    {
        mode = ControlMode.AnchorInspect;
    
        // Baz açıları ANCHOR'dan al (rigin o anki rotasyonundan değil)
        var anchorEuler = anchor.transform.rotation.eulerAngles;

        anchorBaseYaw = anchorEuler.y;

        anchorYawLimit = Mathf.Max(0f, anchor.yawLimit);
        anchorMinDist = anchor.minDistance;
        anchorMaxDist = anchor.maxDistance;
        anchorLockPitch = anchor.lockPitch;

        // Eğer pitch kilitliyse, fixedPitch'i anchor'ın pitch'ine eşitle
        anchorFixedPitch = NormalizePitch(anchorEuler.x);

        // Controller iç değerlerini anchor'a snap'le
        yaw = anchorBaseYaw;

        if (anchorLockPitch)
        pitch = anchorFixedPitch;
        else
            pitch = NormalizePitch(anchorEuler.x); // serbestse de anchor pitch'inden başla

        targetDistance = Mathf.Clamp(targetDistance, anchorMinDist, anchorMaxDist);
    }   

    public void SnapRotationTo(Quaternion rot)
    {
        // Anchor'a giderken rotasyonu buna ayarlamak için
        var e = rot.eulerAngles;
        yaw = e.y;
        pitch = NormalizePitch(e.x);
        transform.rotation = rot;
    }

    public void SnapDistance(float distance)
    {
        targetDistance = distance;
        cam.localPosition = new Vector3(0f, 0f, -targetDistance);
    }

    static float NormalizePitch(float x)
    {
        if (x > 180f) x -= 360f;
        return x;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
        min = Mathf.Repeat(min + 180f, 360f) - 180f;
        max = Mathf.Repeat(max + 180f, 360f) - 180f;

        // Eğer aralık wrap'liyorsa basit clamp yetmez. Pratik çözüm: en yakın sınır.
        if (min <= max)
            return Mathf.Clamp(angle, min, max);

        // wrap durumunda: angle min..180 veya -180..max içinde olmalı
        bool inRange = (angle >= min && angle <= 180f) || (angle >= -180f && angle <= max);
        if (inRange) return angle;

        float distToMin = Mathf.Abs(Mathf.DeltaAngle(angle, min));
        float distToMax = Mathf.Abs(Mathf.DeltaAngle(angle, max));
        return distToMin < distToMax ? min : max;
    }
}
