using UnityEngine;
using System.Collections.Generic;

public class Shockwave : MonoBehaviour
{
    [Header("Crecimiento")]
    public float duration = 1.2f;         // tiempo que tarda en expandirse
    public float maxRadius = 10f;         // radio final en unidades de mundo
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Daño")]
    public int damage = 15;
    public LayerMask hitMask;             // pon aquí la capa del Player (o Everything)
    public bool damageOncePerTarget = true;

    [Header("Visual")]
    public Transform visual;              // el Quad del prefab (si lo dejas vacío usa el transform)
    public float initialY = 0.05f;        // levantar un poco para que no z-Flickee con el piso

    float _t;
    HashSet<Transform> _alreadyHit = new HashSet<Transform>();

    void Awake()
    {
        if (!visual) visual = transform;
        var p = transform.position; p.y += initialY; transform.position = p;
        SetScale(0f);
    }

    void Update()
    {
        _t += Time.deltaTime / Mathf.Max(0.0001f, duration);
        float k = Mathf.Clamp01(_t);
        float r01 = scaleCurve.Evaluate(k);  // 0..1
        float currentRadius = r01 * maxRadius;

        // Escalar el quad (X/Z) para que coincida con el radio
        SetScale(currentRadius * 2f); // diámetro

        // Hacer daño mientras crece
        DoDamage(currentRadius);

        if (_t >= 1f)
            Destroy(gameObject);
    }

    void SetScale(float diameter)
    {
        if (!visual) return;

        // En un Quad rotado 90° (para que quede “plano” en el piso),
        // el tamaño visible está en X e Y, no en Z.
        visual.localScale = new Vector3(diameter, diameter, 1f);
    }


    void DoDamage(float radius)
    {
        var hits = Physics.OverlapSphere(transform.position, radius, hitMask, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
        {
            var root = h.attachedRigidbody ? h.attachedRigidbody.transform : h.transform;
            if (damageOncePerTarget && _alreadyHit.Contains(root)) continue;

            var ph = root.GetComponentInParent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                _alreadyHit.Add(root);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
#endif
}
