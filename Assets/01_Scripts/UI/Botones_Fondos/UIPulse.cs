using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIPulse : MonoBehaviour
{
    public enum PulseMode { Brightness, Alpha, Both }

    [Header("Pulse")]
    [Tooltip("Velocidad del 'respirar' principal.")]
    public float speed = 1f;
    [Tooltip("Intensidad del brillo (0.03–0.12 recomendado).")]
    [Range(0f, 0.3f)] public float intensity = 0.08f;
    [Tooltip("Usar tiempo independiente del TimeScale (UI más estable).")]
    public bool useUnscaledTime = true;
    [Tooltip("Curva del pulso (Easing). X=0..1, Y=0..1.")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Qué componente del color pulsa.")]
    public PulseMode mode = PulseMode.Brightness;

    [Header("Micro-flicker (electricidad sutil)")]
    [Tooltip("Intensidad del flicker perlin (0 para desactivar).")]
    [Range(0f, 0.15f)] public float flickerIntensity = 0.03f;
    [Tooltip("Velocidad del flicker perlin.")]
    public float flickerSpeed = 30f;
    [Tooltip("Semilla del ruido (para variar el patrón).")]
    public float flickerSeed = 0.123f;

    [Header("Tint opcional")]
    [Tooltip("Multiplicador de color global (deja en blanco para no teñir).")]
    public Color multiplyTint = Color.white;

    Image img;
    Color baseColor;

    void Awake()
    {
        img = GetComponent<Image>();
        baseColor = img.color; // conserva color/alpha original
    }

    void OnEnable()
    {
        // por si lo reactivan en runtime, retomamos color base
        img.color = baseColor;
    }

    void Update()
    {
        float t = (useUnscaledTime ? Time.unscaledTime : Time.time);

        // Pulso principal con curva (0..1)
        float wave = Mathf.Sin(t * speed) * 0.5f + 0.5f;
        float shaped = curve != null ? curve.Evaluate(wave) : wave;

        // Micro-flicker tipo chispas (puedes desactivar poniendo 0)
        float flicker = 0f;
        if (flickerIntensity > 0f)
        {
            float n = Mathf.PerlinNoise(t * flickerSpeed, flickerSeed);
            // remap 0..1 -> -1..1 para que oscile +/- alrededor del valor base
            flicker = (n * 2f - 1f) * flickerIntensity;
        }

        // Cantidad total de modulación (clamp para evitar exageraciones)
        float mod = Mathf.Clamp01(intensity * shaped + Mathf.Abs(flicker));

        // Brillo resultado (1 = sin cambio, <1 oscurece ligeramente)
        float brightness = 1f - mod;

        // Calcula color final respetando alpha original
        Color c = baseColor;

        // Aplica tint opcional (blanco = no afecta)
        c.r *= multiplyTint.r;
        c.g *= multiplyTint.g;
        c.b *= multiplyTint.b;

        if (mode == PulseMode.Brightness || mode == PulseMode.Both)
        {
            c.r *= brightness;
            c.g *= brightness;
            c.b *= brightness;
        }

        if (mode == PulseMode.Alpha || mode == PulseMode.Both)
        {
            // Reducimos alfa levemente con el mismo mod, manteniendo base
            c.a = baseColor.a * (1f - (mod * 0.6f));
        }
        else
        {
            // mantiene el alfa original si no estamos modulando alfa
            c.a = baseColor.a;
        }

        img.color = c;
    }
}
