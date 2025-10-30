using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 2f;
    public float distance = 3f;
    public bool horizontal = true;

    [Header("Giro visual")]
    public Transform model;
    public float yawDerecha = 90f;
    public float yawIzquierda = -90f;
    public bool flipOnTurn = true;

    // 🆕 NUEVO: referencia opcional al hitbox
    [Header("Hitbox (opcional)")]
    public EnemyHitbox hitbox;  // arrástralo o se autolocaliza

    private Vector3 startPos;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;
        if (!model) model = transform;

        // 🆕 Intentar conectar hitbox automáticamente
        if (!hitbox)
            hitbox = GetComponentInChildren<EnemyHitbox>(true);

        if (hitbox)
            hitbox.owner = this;

        AplicarGiro();
    }

    void Update()
    {
        Vector3 delta = (horizontal ? Vector3.right : Vector3.forward) * direction * speed * Time.deltaTime;
        transform.position += delta;

        float desplazamiento = horizontal ? (transform.position.x - startPos.x)
                                          : (transform.position.z - startPos.z);

        if (Mathf.Abs(desplazamiento) >= distance)
        {
            direction *= -1;
            AplicarGiro();
        }
    }

    void AplicarGiro()
    {
        if (!flipOnTurn || !model) return;
        float yaw = (direction > 0) ? yawDerecha : yawIzquierda;
        model.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
