using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_AI_NAVIGATION || UNITY_2019_1_OR_NEWER
using UnityEngine.AI;
#endif

[RequireComponent(typeof(Collider))]
public class BossAI : MonoBehaviour
{
    public enum BossState { Patrol, Chase, Cooldown }

    [Header("Movimiento")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float turnSpeed = 8f;
    public float stoppingDistance = 0.4f;

    [Tooltip("Puntos de patrulla en el techo (en orden).")]
    public List<Transform> waypoints = new List<Transform>();
    public bool loopPatrol = true;

    [Header("Detección")]
    public Transform player;                          // se autoasigna por tag "Player" si está vacío
    public float detectRadius = 10f;
    public float loseRadius = 16f;                    // sale de persecución si se aleja más que esto
    public LayerMask sightMask = ~0;                  // opcional, por si quieres raycast/obstáculos
    public bool requireLineOfSight = false;
    public float sightHeight = 1.0f;                  // altura desde donde “mira” el boss

    [Header("Onda de daño")]
    public float shockInterval = 4.0f;                // cada cuánto emite una onda
    public float shockGrowSeconds = 0.75f;            // cuánto tarda en expandirse
    public float shockMaxRadius = 6f;                 // radio máximo de la onda
    public int shockDamage = 2;                     // daño por onda
    public float shockTickEvery = 0.1f;               // granularidad de chequeo
    public GameObject shockVfxPrefab;                 // opcional: un anillo/quad que se escala

    [Header("Gizmos/VFX")]
    public Color detectColor = new Color(0, 1, 1, 0.2f);
    public Color loseColor = new Color(1, 0.5f, 0, 0.15f);
    public Color shockColor = new Color(0.2f, 0.8f, 1f, 0.15f);

    [Header("Debug (solo lectura)")]
    public BossState state = BossState.Patrol;
    public int currentWp = 0;

#if UNITY_AI_NAVIGATION || UNITY_2019_1_OR_NEWER
    NavMeshAgent agent;
#endif
    Collider bodyCol;
    float shockTimer;
    HashSet<Transform> hitThisWave = new HashSet<Transform>();

    void Awake()
    {
        bodyCol = GetComponent<Collider>();
        bodyCol.isTrigger = false; // el boss colisiona (ajústalo si lo prefieres gatillo)

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

#if UNITY_AI_NAVIGATION || UNITY_2019_1_OR_NEWER
        agent = GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent.updateRotation = false; // rotamos manual para control suave
            agent.stoppingDistance = stoppingDistance;
            agent.speed = patrolSpeed;
        }
#endif
    }

    void Start()
    {
        shockTimer = shockInterval * 0.5f; // primera onda llega más rápido
        GoToNextWaypoint();
    }

    void Update()
    {
        if (!player) return;

        // --- Transición de estados
        float d = Vector3.Distance(transform.position, player.position);
        bool sees = HasSightOnPlayer();

        switch (state)
        {
            case BossState.Patrol:
                MovePatrol();
                if (d <= detectRadius && (!requireLineOfSight || sees))
                    EnterChase();
                break;

            case BossState.Chase:
                MoveChase();
                if (d >= loseRadius || (requireLineOfSight && !sees))
                    EnterPatrol();
                break;

            case BossState.Cooldown:
                // reservado por si quieres animaciones/pausas
                EnterPatrol();
                break;
        }

        // --- Onda de choque temporizada
        shockTimer += Time.deltaTime;
        if (shockTimer >= shockInterval)
        {
            shockTimer = 0f;
            StartCoroutine(ShockwaveRoutine());
        }
    }

    // ---------- Movimiento ----------
    void MovePatrol()
    {
#if UNITY_AI_NAVIGATION || UNITY_2019_1_OR_NEWER
        if (agent)
        {
            agent.speed = patrolSpeed;
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                GoToNextWaypoint();

            FaceTowards(agent.desiredVelocity);
        }
        else
#endif
        {
            if (waypoints.Count == 0) return;
            Vector3 target = waypoints[currentWp].position;
            MoveTowards(target, patrolSpeed);
            if ((transform.position - target).sqrMagnitude <= stoppingDistance * stoppingDistance)
                GoToNextWaypoint();
        }
    }

    void MoveChase()
    {
#if UNITY_AI_NAVIGATION || UNITY_2019_1_OR_NEWER
        if (agent)
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
            FaceTowards(agent.desiredVelocity);
        }
        else
#endif
        {
            MoveTowards(player.position, chaseSpeed);
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Vector3 step = dir.normalized * speed * Time.deltaTime;
        transform.position += step;

        // Rotación suave
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Count == 0) return;
        currentWp = (currentWp + 1);
        if (currentWp >= waypoints.Count)
            currentWp = loopPatrol ? 0 : waypoints.Count - 1;

#if UNITY_AI_NAVIGATION || UNITY_2019_1_OR_NEWER
        if (agent) agent.SetDestination(waypoints[currentWp].position);
#endif
    }

    void FaceTowards(Vector3 velocity)
    {
        velocity.y = 0;
        if (velocity.sqrMagnitude < 0.001f) return;
        var rot = Quaternion.LookRotation(velocity);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
    }

    bool HasSightOnPlayer()
    {
        if (!requireLineOfSight) return true;

        Vector3 eye = transform.position + Vector3.up * sightHeight;
        Vector3 dir = (player.position + Vector3.up * sightHeight) - eye;
        if (Physics.Raycast(eye, dir.normalized, out RaycastHit hit, loseRadius, sightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    void EnterChase() { state = BossState.Chase; }
    void EnterPatrol() { state = BossState.Patrol; }

    // ---------- Shockwave ----------
    IEnumerator ShockwaveRoutine()
    {
        hitThisWave.Clear();

        // VFX
        GameObject ring = null;
        if (shockVfxPrefab)
        {
            ring = Instantiate(shockVfxPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity);
            ring.transform.localScale = Vector3.zero;
        }

        float t = 0f;
        while (t < shockGrowSeconds)
        {
            t += Time.deltaTime;
            float r = Mathf.Lerp(0f, shockMaxRadius, t / shockGrowSeconds);

            // chequeo de daño
            DealShockDamage(r);

            // escalar VFX
            if (ring) ring.transform.localScale = Vector3.one * (r * 2f);

            yield return null;
        }

        if (ring) Destroy(ring, 0.3f);
    }

    void DealShockDamage(float radius)
    {
        // daña al jugador una sola vez por onda
        if (!player || hitThisWave.Contains(player)) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= radius)
        {
            var hp = player.GetComponentInParent<PlayerHealth>();
            if (hp) hp.TakeDamage(shockDamage);
            hitThisWave.Add(player);
        }
    }

    // ---------- Gizmos ----------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = detectColor;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = loseColor;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        Gizmos.color = shockColor;
        Gizmos.DrawWireSphere(transform.position, shockMaxRadius);
    }
}