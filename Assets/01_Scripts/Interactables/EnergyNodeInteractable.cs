using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer))]
public class EnergyNodeInteractable : MonoBehaviour, IInteractable
{
    [Header("Texto")]
    [TextArea] public string prompt = "Activar nodo de energía";

    [Header("Puzzle (opcional)")]
    public CablePuzzleController puzzle;
    public bool oneShot = true;

    [Header("Feedback visual y sonoro")]
    public Light linkedLight;                     // Luz principal
    public List<Light> linkedLightsExtra = new(); // ✅ Varias luces adicionales
    public List<Renderer> linkedRenderers = new(); // ✅ Opcional, para materiales con emisión
    public Color offColor = Color.gray;
    public Color onColor = new Color(0.2f, 0.8f, 1f);
    public AudioClip solvedClip;                 // ✅ Clip directo
    public UnityEvent onSolved;                  // Eventos extra (abrir puertas, etc.)

    [Header("Estado")]
    [SerializeField] private bool isOn = false;

    // Cache
    private Renderer rend;
    private MaterialPropertyBlock mpb;
    private PlayerMotor3D player;
    private AudioSource audioSrc;

    public bool IsOn => isOn;
    public event System.Action<EnergyNodeInteractable, bool> OnNodeToggled;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        player = FindObjectOfType<PlayerMotor3D>();
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.spatialBlend = 0.5f;

        if (!linkedLight)
            linkedLight = GetComponentInChildren<Light>(true);

        UpdateVisual();
    }

    public string GetPrompt()
    {
        if (oneShot && isOn) return "";
        if (puzzle && !isOn) return "Iniciar puzzle";
        return isOn ? "E — Apagar nodo" : prompt;
    }

    public void Interact()
    {
        if (oneShot && isOn) return;

        if (puzzle)
        {
            if (!player) player = FindObjectOfType<PlayerMotor3D>();
            puzzle.Open(player, OnPuzzleSolved);
        }
        else
        {
            Debug.Log("[EnergyNode] Activado directamente (sin puzzle)");
            OnPuzzleSolved();
        }
    }

    private void OnPuzzleSolved()
    {
        Debug.Log("[EnergyNode] Puzzle solved — ejecutando efectos.");
        SetState(true);
        onSolved?.Invoke();
    }

    private void SetState(bool on)
    {
        // ✅ Evita apagado: solo se puede activar, nunca desactivar
        if (isOn && !on) return;

        isOn = on;
        ApplySolvedEffectsIfNeeded(on);
        UpdateVisual();
        OnNodeToggled?.Invoke(this, isOn);
    }

    private void ApplySolvedEffectsIfNeeded(bool on)
    {
        // 🔹 Luz principal
        if (linkedLight)
        {
            if (on) linkedLight.enabled = true; // nunca la apaga
            var fade = linkedLight.GetComponentInParent<LightFadeIn>();
            if (on && fade) fade.TurnOn();
        }

        // 🔹 Luces adicionales
        if (linkedLightsExtra != null && linkedLightsExtra.Count > 0)
        {
            foreach (var l in linkedLightsExtra)
            {
                if (!l) continue;
                if (on) l.enabled = true; // nunca las apaga

                var fade = l.GetComponentInParent<LightFadeIn>();
                if (on && fade) fade.TurnOn();
            }
        }

        // 🔹 Materiales emisivos (opcional)
        /*
        if (linkedRenderers != null && linkedRenderers.Count > 0)
        {
            foreach (var r in linkedRenderers)
            {
                if (!r) continue;
                var mats = r.materials;
                foreach (var m in mats)
                {
                    if (m.HasProperty("_EmissionColor"))
                    {
                        m.EnableKeyword("_EMISSION");
                        m.SetColor("_EmissionColor", on ? onColor * 2f : Color.black);
                    }
                }
            }
        }
        */

        // 🔹 Sonido
        if (on && solvedClip)
            audioSrc.PlayOneShot(solvedClip);
    }

    public void UpdateVisual()
    {
        if (!rend) return;
        rend.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", isOn ? onColor : offColor);
        rend.SetPropertyBlock(mpb);
    }
}
