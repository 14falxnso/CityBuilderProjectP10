using UnityEngine;
using UnityEngine.EventSystems;

public class PlanetZooCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundHeight = 0f;
    [SerializeField] private bool followTerrainHeight = true;
    [SerializeField] private float minCameraHeight = 1.5f;

    [Header("Pan")]
    [SerializeField] private float minPanSpeed = 6f;
    [SerializeField] private float maxPanSpeed = 40f;
    [SerializeField] private float fastMultiplier = 2.5f;
    [SerializeField] private float panSmoothTime = 0.12f;
    [SerializeField] private bool edgeScrolling = false;
    [SerializeField] private float edgeSize = 10f;

    [Header("Rotate / Tilt")]
    [SerializeField] private float keyRotateSpeed = 90f;
    [SerializeField] private float keyTiltSpeed = 45f;
    [SerializeField] private float dragRotateSpeed = 0.25f;
    [SerializeField] private float dragTiltSpeed = 0.2f;
    [SerializeField] private float minPitch = 10f;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float rotateSmoothTime = 0.08f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 80f;
    [SerializeField] private float zoomSmoothTime = 0.12f;
    [SerializeField] private bool zoomToCursor = true;

    [Header("Bounds (focus point)")]
    [SerializeField] private float minXBound = -50f;
    [SerializeField] private float maxXBound = 50f;
    [SerializeField] private float minZBound = -50f;
    [SerializeField] private float maxZBound = 50f;

    [Header("Input")]
    [SerializeField] private bool ignoreWhenPointerOverUI = true;

    private Vector3 targetPivot, currentPivot, pivotVelocity;
    private float targetYaw, currentYaw, yawVelocity;
    private float targetPitch, currentPitch, pitchVelocity;
    private float targetDistance, currentDistance, distanceVelocity;

    private Vector3 lastMousePos;


    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        cam.orthographic = false;

        Vector3 euler = cam.transform.eulerAngles;
        targetYaw = currentYaw = euler.y;
        targetPitch = currentPitch = Mathf.Clamp(NormalizeAngle(euler.x), minPitch, maxPitch);

        Ray center = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 focus = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f)).Raycast(center, out float e)
            ? center.GetPoint(e)
            : cam.transform.position + cam.transform.forward * 20f;

        targetPivot = currentPivot = focus;
        targetDistance = currentDistance = Mathf.Clamp(Vector3.Distance(cam.transform.position, focus), minDistance, maxDistance);

        lastMousePos = Input.mousePosition;
        ApplyTransform();
    }


    private void Update()
    {
        Vector3 mouseDelta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        HandleKeyboardPan();
        HandleEdgeScroll();
        HandleKeyRotate();
        HandleMouseOrbit(mouseDelta);
        HandleMousePan(mouseDelta);
        HandleZoom();

        targetPivot.x = Mathf.Clamp(targetPivot.x, minXBound, maxXBound);
        targetPivot.z = Mathf.Clamp(targetPivot.z, minZBound, maxZBound);
        targetPivot.y = SampleGroundHeight(targetPivot);
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }

    private void LateUpdate()
    {
        currentPivot = Vector3.SmoothDamp(currentPivot, targetPivot, ref pivotVelocity, panSmoothTime);
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotateSmoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, rotateSmoothTime);
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, zoomSmoothTime);

        ApplyTransform();
    }


    private void HandleKeyboardPan()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h == 0f && v == 0f) return;

        PanByInput(new Vector2(h, v));
    }

    private void HandleEdgeScroll()
    {
        if (!edgeScrolling || !Application.isFocused) return;

        Vector3 m = Input.mousePosition;
        if (m.x < 0 || m.y < 0 || m.x > Screen.width || m.y > Screen.height) return;

        Vector2 dir = Vector2.zero;
        if (m.x <= edgeSize) dir.x = -1f;
        else if (m.x >= Screen.width - edgeSize) dir.x = 1f;
        if (m.y <= edgeSize) dir.y = -1f;
        else if (m.y >= Screen.height - edgeSize) dir.y = 1f;

        if (dir != Vector2.zero) PanByInput(dir);
    }

    private void PanByInput(Vector2 input)
    {
        if (input.sqrMagnitude > 1f) input.Normalize();

        float t = Mathf.InverseLerp(minDistance, maxDistance, targetDistance);
        float speed = Mathf.Lerp(minPanSpeed, maxPanSpeed, t);
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) speed *= fastMultiplier;

        Quaternion yaw = Quaternion.Euler(0f, targetYaw, 0f);
        Vector3 move = (yaw * Vector3.right * input.x + yaw * Vector3.forward * input.y) * (speed * Time.deltaTime);
        targetPivot += move;
    }

    private void HandleMousePan(Vector3 mouseDelta)
    {
        if (!Input.GetMouseButton(2) || mouseDelta == Vector3.zero) return;
        if (Input.GetMouseButtonDown(2) && IsPointerOverUI()) return;

        float unitsPerPixel = 2f * targetDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;

        float foreshorten = Mathf.Max(0.25f, Mathf.Sin(targetPitch * Mathf.Deg2Rad));

        Quaternion yaw = Quaternion.Euler(0f, targetYaw, 0f);
        Vector3 move = yaw * Vector3.right * mouseDelta.x + yaw * Vector3.forward * (mouseDelta.y / foreshorten);

        targetPivot -= move * unitsPerPixel;
    }

    private void HandleKeyRotate()
    {
        if (Input.GetKey(KeyCode.Q)) targetYaw -= keyRotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) targetYaw += keyRotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.R)) targetPitch += keyTiltSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.F)) targetPitch -= keyTiltSpeed * Time.deltaTime;
    }

    private void HandleMouseOrbit(Vector3 mouseDelta)
    {
        if (!Input.GetMouseButton(1) || mouseDelta == Vector3.zero) return;
        if (Input.GetMouseButtonDown(1) && IsPointerOverUI()) return;

        targetYaw += mouseDelta.x * dragRotateSpeed;
        targetPitch -= mouseDelta.y * dragTiltSpeed;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f || IsPointerOverUI()) return;

        float oldDistance = targetDistance;

        float factor = Mathf.Clamp(1f - scroll * zoomSpeed, 0.1f, 10f);
        targetDistance = Mathf.Clamp(oldDistance * factor, minDistance, maxDistance);

        if (zoomToCursor && TryGetCursorGroundPoint(out Vector3 cursorPoint))
        {
            Vector3 toCursor = cursorPoint - targetPivot;
            toCursor.y = 0f;

            float reach = maxDistance * 1.5f; // guard against rays near the horizon
            if (toCursor.magnitude > reach) toCursor = toCursor.normalized * reach;

            float t = 1f - targetDistance / oldDistance;
            targetPivot += toCursor * t;
        }
    }


    private void ApplyTransform()
    {
        float minPitchForHeight = Mathf.Asin(Mathf.Clamp01(minCameraHeight / Mathf.Max(currentDistance, 0.01f))) * Mathf.Rad2Deg;
        float pitch = Mathf.Min(Mathf.Max(currentPitch, minPitchForHeight), maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0f);
        cam.transform.rotation = rotation;
        cam.transform.position = currentPivot - rotation * Vector3.forward * currentDistance;
    }

    private bool TryGetCursorGroundPoint(out Vector3 point)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, groundMask))
        {
            point = hit.point;
            return true;
        }

        Plane plane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }

        point = default;
        return false;
    }

    private float SampleGroundHeight(Vector3 position)
    {
        if (!followTerrainHeight) return groundHeight;

        Vector3 origin = new Vector3(position.x, 500f, position.z);
        return Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000f, groundMask)
            ? hit.point.y
            : groundHeight;
    }

    private bool IsPointerOverUI()
    {
        return ignoreWhenPointerOverUI
            && EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject();
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private void OnValidate()
    {
        minDistance = Mathf.Max(0.1f, minDistance);
        maxDistance = Mathf.Max(minDistance, maxDistance);
        minPitch = Mathf.Clamp(minPitch, 1f, 89f);
        maxPitch = Mathf.Clamp(maxPitch, minPitch, 89f);
    }
}