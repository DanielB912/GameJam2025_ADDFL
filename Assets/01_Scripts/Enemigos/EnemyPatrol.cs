using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float distance = 3f;
    public bool horizontal = true;      // X if true, Z if false

    [Header("Visual Rotation")]
    public Transform model;             // drag the mesh/child here
    public float yawRight = 90f;
    public float yawLeft = -90f;
    public bool flipOnTurn = true;

    private Vector3 startPos;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;
        if (!model) model = transform; // fallback
        ApplyRotation(); // initial facing
    }

    void Update()
    {
        Vector3 delta = (horizontal ? Vector3.right : Vector3.forward) * direction * speed * Time.deltaTime;
        transform.position += delta;

        float offset = horizontal ? (transform.position.x - startPos.x)
                                  : (transform.position.z - startPos.z);

        if (Mathf.Abs(offset) >= distance)
        {
            direction *= -1;
            ApplyRotation();
        }
    }

    void ApplyRotation()
    {
        if (!flipOnTurn || !model) return;
        float yaw = (direction > 0) ? yawRight : yawLeft;
        model.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ⚠️ Damage player here in the future:
            // other.GetComponent<PlayerHealth>()?.TakeDamage(damageAmount);

            Destroy(gameObject); // destroy this enemy
        }
    }
}
