using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EnemyRangeCircle : MonoBehaviour
{
    [Header("Radius Source")]
    public EnemyChase chase;                 // drag the EnemyChase (or auto-locates)
    public bool useChaseRanges = true;       // use chaseRange/stopRange from EnemyChase
    public float manualRadius = 6f;          // if no EnemyChase is assigned

    [Header("Appearance")]
    public int segments = 96;
    public float thickness = 0.1f;
    public float yOffset = 0.05f;            // raise if hidden by ground
    public Color idleColor = Color.yellow;
    public Color chaseColor = Color.red;

    private LineRenderer lr;
    private float currentRadius = -1f;
    private Vector3 lastCenter;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (!chase) chase = GetComponent<EnemyChase>();

        // Use Unlit material so it always stays visible
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = segments;
        lr.alignment = LineAlignment.View;
        lr.numCornerVertices = 4;
        lr.numCapVertices = 4;
        lr.startWidth = lr.endWidth = thickness;

        lastCenter = transform.position;
        UpdateGeometry(true); // first draw
    }

    void LateUpdate()
    {
        // 1) Radius based on state (idle vs chasing)
        float r = useChaseRanges && chase
            ? (chase.IsChasing ? chase.stopRange : chase.chaseRange)
            : manualRadius;

        // 2) Did the center move?
        bool centerMoved = (transform.position - lastCenter).sqrMagnitude > 0.0001f;

        // 3) Redraw if radius or position changed
        if (centerMoved || Mathf.Abs(r - currentRadius) > 0.001f)
        {
            currentRadius = r;
            lastCenter = transform.position;
            UpdateGeometry(false);
        }

        // 4) Update color based on chasing state
        var c = (chase && chase.IsChasing) ? chaseColor : idleColor;
        lr.startColor = lr.endColor = c;
    }

    void UpdateGeometry(bool forceCenterUpdate)
    {
        Vector3 center = transform.position;

        for (int i = 0; i < segments; i++)
        {
            float t = (i / (float)segments) * Mathf.PI * 2f;
            float x = Mathf.Cos(t) * currentRadius;
            float z = Mathf.Sin(t) * currentRadius;
            lr.SetPosition(i, new Vector3(center.x + x, center.y + yOffset, center.z + z));
        }
    }
}
