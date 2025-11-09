using UnityEngine;

public class CameraOrbitController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;  // el jugador
    public float distance = 5f;
    public float height = 2f;

    [Header("Rotación")]
    public float sensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Suavizado")]
    public float followSmooth = 10f;

    private float yaw = 0f;
    private float pitch = 20f;

    void Start()
    {
        if (!target)
        {
            Debug.LogWarning("[CameraOrbitController] No se asignó target (jugador).");
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        // Rotar solo con click derecho
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Zoom opcional con la rueda
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * 3f;
            distance = Mathf.Clamp(distance, 2f, 10f);
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPos = target.position - rotation * Vector3.forward * distance + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSmooth);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * followSmooth);
    }
}
