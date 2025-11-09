using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public enum OutOfRangeMode { ReturnToStart, IdleInPlace, KeepLastFacing, KeepLookingAtPlayer }

    [Header("Targets & Ranges")]
    public Transform player;
    public float chaseSpeed = 3f;
    public float chaseRange = 8f;
    public float stopRange = 10f;

    [Header("Out-of-range behaviour")]
    public OutOfRangeMode outOfRangeMode = OutOfRangeMode.ReturnToStart;
    public float returnSpeedFactor = 0.5f;

    [Header("Rotation")]
    public bool lockYRotation = true;
    public float turnSmoothing = 10f;

    [Header("Contact")]
    public bool destroyOnContact = true;   // destroy this enemy on touching the player

    private Vector3 startPos;
    private bool chasing = false;
    public bool IsChasing => chasing;

    void Start()
    {
        startPos = transform.position;
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (chasing)
        {
            if (distance > stopRange) chasing = false;
            else { ChasePlayer(); return; }
        }

        switch (outOfRangeMode)
        {
            case OutOfRangeMode.ReturnToStart: ReturnToStart(false); break;
            case OutOfRangeMode.IdleInPlace: break;
            case OutOfRangeMode.KeepLastFacing: break;
            case OutOfRangeMode.KeepLookingAtPlayer: ReturnToStart(true); break;
        }

        if (!chasing && distance < chaseRange) chasing = true;
    }

    void ChasePlayer()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        transform.position += toPlayer * chaseSpeed * Time.deltaTime;

        Vector3 lookTarget = lockYRotation
            ? new Vector3(player.position.x, transform.position.y, player.position.z)
            : player.position;

        Quaternion targetRot = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSmoothing * Time.deltaTime);
    }

    void ReturnToStart(bool keepLookAtPlayer)
    {
        Vector3 delta = startPos - transform.position;
        if (delta.magnitude > 0.05f)
        {
            transform.position += delta.normalized * (chaseSpeed * returnSpeedFactor) * Time.deltaTime;

            Vector3 lookTarget = keepLookAtPlayer && player
                ? (lockYRotation ? new Vector3(player.position.x, transform.position.y, player.position.z) : player.position)
                : (lockYRotation ? new Vector3(startPos.x, transform.position.y, startPos.z) : startPos);

            Quaternion targetRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSmoothing * Time.deltaTime);
        }
    }

    // 🔴 Physical contact with the player (uses MeshCollider)
    /*void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        // Future damage to player:
        // collision.collider.GetComponent<PlayerHealth>()?.TakeDamage(damageAmount);

        if (destroyOnContact) Destroy(gameObject);
    }
    */
    // En EnemyChase.cs
    public void OnHitPlayer(GameObject playerGO)
    {
        // TODO: cuando tengas vida del jugador, descomenta:
        // var hp = playerGO.GetComponent<PlayerHealth>();
        // if (hp) hp.TakeDamage(damageAmount);

        Destroy(gameObject); // destruir al enemigo al contacto real
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = chasing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chasing ? stopRange : chaseRange);
    }
}
