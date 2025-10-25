using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public EnemyChase owner;  // opcional, se autolocaliza si está vacío

    void Awake()
    {
        if (!owner)
            owner = GetComponentInParent<EnemyChase>();
    }

    void OnTriggerEnter(Collider other)
    {
        // necesitamos encontrar al Player
        var rb = other.attachedRigidbody;
        if (rb && rb.CompareTag("Player"))
        {
            owner?.OnHitPlayer(rb.gameObject);
            return;
        }
        if (other.CompareTag("Player"))
        {
            owner?.OnHitPlayer(other.gameObject);
        }
    }
}
