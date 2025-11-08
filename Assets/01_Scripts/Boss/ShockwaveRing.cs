using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ShockwaveRing : MonoBehaviour
{
    [Header("Visual")]
    public int segments = 72;           // puntos del círculo
    public float ringWidth = 0.6f;      // grosor visual (mundo)
    public Gradient colorOverLife;      // brillo/alpha en el tiempo
    public float initialY = 0.05f;      // levanta sobre el piso

    [Header("Tiempo / Radio")]
    public float duration = 0.8f;       // cuánto tarda en expandirse
    public float startRadius = 0.2f;
    public float endRadius = 8f;
    public AnimationCurve radiusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Daño (banda del anillo)")]
    public LayerMask hitMask = ~0;      // capa del player o Everything
    public int damage = 12;
    public bool hitOncePerTarget = true;
    [Tooltip("Qué tan gruesa es la banda que hace daño (mitad hacia adentro/afuera)")]
    public float damageBand = 0.6f;

    LineRenderer _lr;
    float _t;
    readonly HashSet<Transform> _hit = new();

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        if (colorOverLife == null || colorOverLife.colorKeys.Length == 0)
        {
            // gradiente por defecto (cian brillante que se apaga)
            var g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.4f, 0.9f, 1f), 0f),
                    new GradientColorKey(new Color(0.4f, 0.9f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.85f, 0f),
                    new GradientAlphaKey(0.0f, 1f)
                }
            );
            colorOverLife = g;
        }

        // Ajustes iniciales del LR
        _lr.loop = true;
        _lr.useWorldSpace = true;
        _lr.positionCount = segments;
        _lr.widthMultiplier = ringWidth;
        _lr.colorGradient = colorOverLife;

        // Levantar ligeramente
        var p = transform.position; p.y += initialY; transform.position = p;

        // Dibuja el primer frame
        UpdateRing(startRadius);
    }

    void Update()
    {
        _t += Time.deltaTime / Mathf.Max(0.0001f, duration);
        float k = Mathf.Clamp01(_t);
        float r01 = radiusCurve.Evaluate(k);
        float radius = Mathf.Lerp(startRadius, endRadius, r01);

        // Visual
        _lr.widthMultiplier = ringWidth;
        _lr.colorGradient = colorOverLife;
        UpdateRing(radius);

        // Daño solo en banda del anillo (no todo el disco)
        DoDamage(radius, damageBand);

        if (_t >= 1f) Destroy(gameObject);
    }

    void UpdateRing(float radius)
    {
        float step = Mathf.PI * 2f / segments;
        Vector3 center = transform.position;
        float y = center.y;

        for (int i = 0; i < segments; i++)
        {
            float a = i * step;
            float x = center.x + Mathf.Cos(a) * radius;
            float z = center.z + Mathf.Sin(a) * radius;
            _lr.SetPosition(i, new Vector3(x, y, z));
        }
    }

    void DoDamage(float radius, float band)
    {
        // Revisa solo objetos en banda [r - band, r + band]
        // Para eficiencia solo mira al Player (filtra con hitMask)
        var hits = Physics.OverlapSphere(transform.position, radius + band, hitMask, QueryTriggerInteraction.Collide);
        float rMin = Mathf.Max(0f, radius - band);
        float rMinSqr = rMin * rMin;
        float rMaxSqr = (radius + band) * (radius + band);

        foreach (var h in hits)
        {
            var root = h.attachedRigidbody ? h.attachedRigidbody.transform : h.transform;
            if (hitOncePerTarget && _hit.Contains(root)) continue;

            // Distancia en XZ al centro
            Vector3 v = root.position - transform.position;
            v.y = 0f;
            float d2 = v.sqrMagnitude;

            if (d2 >= rMinSqr && d2 <= rMaxSqr)
            {
                var ph = root.GetComponentInParent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                    _hit.Add(root);
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * initialY, endRadius);
    }
#endif
}
