using UnityEngine;

public class CameraZooming : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float minXBound = -50f;
    [SerializeField] private float maxXBound = 50f;


    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.fieldOfView -= scroll * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
        }

        HandleCameraMovement();
    }

    private void HandleCameraMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput != 0f)
        {
            Vector3 newPosition = cam.transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;
            newPosition.x = Mathf.Clamp(newPosition.x, minXBound, maxXBound);
            cam.transform.position = newPosition;
        }
    }
}
