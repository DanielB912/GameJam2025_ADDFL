using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class BossMini : MonoBehaviour
{
    [Header("Waypoints (en orden)")]
    public Transform[] waypoints;
    public float waypointRadius = 0.35f;

    [Header("Movimiento")]
    public float patrolSpeed = 2.2f;
    public float chaseSpeed = 3.5f;
    public float turnSpeed = 10f;

    [Header("Detección")]
    public float aggroRange = 12f;
    public float loseRange = 18f;

    [Header("Ataque: Shockwave")]
    public GameObject shockwavePrefab;
    public float shockwaveCooldown = 4.0f;
    public float windupTime = 0.8f;
    public float shockSpawnYOffset = 0.03f;   // ← nuevo
    public Transform shockOrigin;

    [Header("Daño por contacto (opcional)")]
    public int touchDamage = 10;
    public float touchInterval = 1.0f;

    [Header("Refs (opcional)")]
    public Transform model;              // si dejas vacío usa transform

    // internos
    Transform _player;
    PlayerHealth _playerHealth;
    int _wpIndex = 0;
    float _cooldown;
    float _lastTouchTime = -999f;


    Rigidbody _rb;

    enum State { Patrol, Chase, Windup }
    State _state = State.Patrol;

    void Awake()
    {
        // Autolocaliza al jugador si no lo asignaste
        var ph = FindObjectOfType<PlayerHealth>();
        if (ph)
        {
            _playerHealth = ph;
            _player = ph.transform;
        }

        if (!model) model = transform;

        // Asegura collider/rigidbody correctos
        var col = GetComponent<Collider>();
        col.isTrigger = false;                // ← importante: NO trigger

        _rb = GetComponent<Rigidbody>();
        if (!_rb) _rb = gameObject.AddComponent<Rigidbody>();
        _rb.isKinematic = false;              // ← dinámico
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _cooldown = Random.Range(0f, shockwaveCooldown * 0.5f);
    }

    void Update()
    {
        if (_player == null) return;

        float dt = Time.deltaTime;
        _cooldown -= dt;

        float dist = Vector3.Distance(transform.position, _player.position);

        // FSM simple
        switch (_state)
        {
            case State.Patrol:
                DoPatrol(dt);
                if (dist <= aggroRange) _state = State.Chase;
                break;

            case State.Chase:
                DoChase(dt);
                if (dist >= loseRange) _state = State.Patrol;

                if (_cooldown <= 0f && dist <= aggroRange)
                {
                    StartCoroutine(CoWindupAndShockwave());
                }
                break;

            case State.Windup:
                // se maneja en la corrutina
                break;
        }
    }

    void DoPatrol(float dt)
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[_wpIndex];
        Vector3 to = target.position - transform.position;
        to.y = 0f;

        if (to.sqrMagnitude <= waypointRadius * waypointRadius)
        {
            _wpIndex = (_wpIndex + 1) % waypoints.Length;
            return;
        }

        Vector3 dir = to.normalized;
        model.rotation = Quaternion.Slerp(
            model.rotation,
            Quaternion.LookRotation(dir, Vector3.up),
            dt * turnSpeed
        );

        Vector3 next = transform.position + dir * patrolSpeed * dt;
        _rb.MovePosition(next);               // ← en lugar de transform.position += …
    }

    void DoChase(float dt)
    {
        Vector3 to = _player.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;

        Vector3 dir = to.normalized;
        model.rotation = Quaternion.Slerp(
            model.rotation,
            Quaternion.LookRotation(dir, Vector3.up),
            dt * turnSpeed
        );

        Vector3 next = transform.position + dir * chaseSpeed * dt;
        _rb.MovePosition(next);               // ← idem
    }

    IEnumerator CoWindupAndShockwave()
    {
        _state = State.Windup;
        _cooldown = shockwaveCooldown;

        // mira al jugador mientras “carga”
        float t = 0f;
        while (t < windupTime)
        {
            t += Time.deltaTime;
            if (_player)
            {
                Vector3 to = _player.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.0001f)
                {
                    Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
                    model.rotation = Quaternion.Slerp(model.rotation, look, Time.deltaTime * (turnSpeed * 1.5f));
                }
            }
            yield return null;
        }

        // instancia la onda
        if (shockwavePrefab)
        {
            Vector3 pos = shockOrigin ? shockOrigin.position
                                      : transform.position + Vector3.up * shockSpawnYOffset;
            Instantiate(shockwavePrefab, pos, Quaternion.identity);
        }

        _state = State.Chase; // vuelve a perseguir
    }

    void OnTriggerStay(Collider other)
    {
        if (_playerHealth == null) return;
        if (other.transform != _player) return;

        if (Time.time - _lastTouchTime >= touchInterval)
        {
            _playerHealth.TakeDamage(touchDamage);
            _lastTouchTime = Time.time;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = new Color(1, 0.5f, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, loseRange);

        if (waypoints != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (!waypoints[i]) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.15f);
                if (i + 1 < waypoints.Length && waypoints[i + 1])
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }

    public void OnTouchTrigger(Collider other)
    {
        if (_playerHealth == null) return;

        // ¿es el player?
        if (other.transform == _player || other.GetComponentInParent<PlayerHealth>() == _playerHealth)
        {
            if (Time.time - _lastTouchTime >= touchInterval)
            {
                _playerHealth.TakeDamage(touchDamage);
                _lastTouchTime = Time.time;
            }
        }
    }

}
