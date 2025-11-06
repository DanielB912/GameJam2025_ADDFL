using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer))]
public class EnergyNodeInteractable : MonoBehaviour, IInteractable
{
    [Header("Texto")]
    [TextArea] public string prompt = "Activar nodo de energía";

    [Header("Puzzle (opcional)")]
    public MonoBehaviour puzzleBehaviour; // 👈 Puede ser cualquier prefab que implemente IPuzzle
    private IPuzzle puzzle;
    public bool oneShot = true;

    [Header("Feedback visual y sonoro")]
    public Light linkedLight;
    public List<Light> linkedLightsExtra = new();
    public List<Renderer> linkedRenderers = new();
    public Color offColor = Color.gray;
    public Color onColor = new Color(0.2f, 0.8f, 1f);
    public AudioClip solvedClip;
    public UnityEvent onSolved;

    [Header("Estado")]
    [SerializeField] private bool isOn = false;

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
        if (puzzleBehaviour != null && !isOn) return "Iniciar puzzle";
        return isOn ? "E — Apagar nodo" : prompt;
    }

    public void Interact()
    {
        if (oneShot && isOn) return;

        // 👇 Intentamos obtener la referencia de IPuzzle solo si no está cacheada
        if (puzzle == null && puzzleBehaviour != null)
            puzzle = puzzleBehaviour as IPuzzle;

        if (puzzle != null)
        {
            if (!player) player = FindObjectOfType<PlayerMotor3D>();
            puzzle.SetTargetNode(this);
            puzzle.Open();
            puzzle.OnSolved += OnPuzzleSolved;
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
        if (isOn && !on) return;

        isOn = on;
        ApplySolvedEffectsIfNeeded(on);
        UpdateVisual();
        OnNodeToggled?.Invoke(this, isOn);

        if (on)
        {
            MiniMapNodes minimap = FindObjectOfType<MiniMapNodes>();
            if (minimap != null)
            {
                minimap.SetNodeColor(transform, Color.gray);
            }
        }
    }

    private void ApplySolvedEffectsIfNeeded(bool on)
    {
        if (linkedLight)
        {
            if (on) linkedLight.enabled = true;
            var fade = linkedLight.GetComponentInParent<LightFadeIn>();
            if (on && fade) fade.TurnOn();
        }

        if (linkedLightsExtra != null && linkedLightsExtra.Count > 0)
        {
            foreach (var l in linkedLightsExtra)
            {
                if (!l) continue;
                if (on) l.enabled = true;

                var fade = l.GetComponentInParent<LightFadeIn>();
                if (on && fade) fade.TurnOn();
            }
        }

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
