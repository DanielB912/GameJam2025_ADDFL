using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor3D : MonoBehaviour
{
    [Header("Control")]
    public bool inputEnabled = true;   // ← habilita/deshabilita input

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
        // Si el control está deshabilitado (p.ej. puzzle abierto),
        // mantenemos al player “pegado” al suelo y sin mover/rotar.
        if (!inputEnabled)
        {
            // Fijar leve empuje hacia abajo para mantener grounded con CC
            if (controller.isGrounded && velocity.y < 0f)
                velocity.y = -2f;
            else
                velocity.y += gravity * Time.deltaTime;

            // Solo aplicamos la componente vertical (mínima)
            controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);
            return;
        }

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

        // Movimiento XZ + rotación hacia la dirección de avance
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

    // Útil si prefieres no tocar la variable directamente
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (enabled == false)
        {
            // Reinicia horizontal y estabiliza vertical cuando se bloquea
            velocity.x = 0f;
            velocity.z = 0f;
        }
    }
}
