using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor3D : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;

    [Header("Salto/Gravedad")]
    public float jumpHeight = 1.4f;
    public float gravity = -20f;

    [Header("Cámara")]
    public Camera cam;              // asigna Main Camera
    public float rotateLerp = 12f;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        // Suelo con CharacterController
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f) velocity.y = -2f;

        // Entradas
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f);

        // Direcciones de cámara en el plano
        Vector3 camFwd = cam.transform.forward; camFwd.y = 0; camFwd.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0; camRight.Normalize();
        Vector3 moveDir = camFwd * input.z + camRight * input.x;

        // Velocidad
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        // Movimiento XZ
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            controller.Move(moveDir * (speed * Time.deltaTime));
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateLerp * Time.deltaTime);
        }

        // Salto
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
