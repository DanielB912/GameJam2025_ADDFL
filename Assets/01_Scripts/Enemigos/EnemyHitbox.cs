using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyHitbox : MonoBehaviour
{
    [Header("References")]
    public MonoBehaviour owner; // puede ser EnemyChase o EnemyPatrol

    [Header("Settings")]
    public string playerTag = "Player";
    public bool destroyOnHit = true;

    [Header("Damage (para futuro sistema de vida)")]
    public int damage = 1; // <-- aún no se usa

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        var rb = GetComponent<Rigidbody>();
        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Debug.Log($"[EnemyHitbox] Player hit by {owner?.name ?? "Unknown enemy"}");

        // 🩸 Buscar componente PlayerHealth en el jugador
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsAlive())
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"[EnemyHitbox] Hizo daño: {damage}");
        }

        // 💥 Destruir enemigo si está configurado así
        if (destroyOnHit)
        {
            if (owner != null)
                Destroy(owner.gameObject);
            else
                Destroy(transform.root.gameObject);
        }
    }
}
