using UnityEngine;

public class SpinPulse : MonoBehaviour
{
    public Vector3 spinEulerPerSecond = new Vector3(0f, 60f, 0f);
    public float pulseAmplitude = 0.08f;
    public float pulseSpeed = 2f;

    Vector3 baseScale;

    void Awake() { baseScale = transform.localScale; }

    void Update()
    {
        transform.Rotate(spinEulerPerSecond * Time.deltaTime, Space.Self);
        float k = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.localScale = baseScale * k;
    }
}
