using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public class EnergyNodeInteractable : MonoBehaviour, IInteractable
{
    [Header("Texto")]
    [TextArea] public string prompt = "Activar nodo de energía";

    [Header("Puzzle (opcional)")]
    public CablePuzzleController puzzle;      // Si lo asignas, se abrirá y al resolver se encenderá
    public bool oneShot = true;               // Si true, una vez encendido no se puede apagar

    [Header("Feedback")]
    public Light linkedLight;
    public Color offColor = Color.gray;
    public Color onColor = new Color(0.2f, 0.8f, 1f);
    public AudioSource solvedSfx;
    public UnityEvent onSolved;               // eventos extra al resolver (abrir puerta, etc.)

    [Header("Estado")]
    [SerializeField] private bool isOn = false;   // queda encendido al resolver

    // Cache
    private Renderer rend;
    private MaterialPropertyBlock mpb;
    private PlayerMotor3D player;

    // --- NEW: acceso de solo lectura al estado + evento de toggle ---
    public bool IsOn => isOn; // NEW
    public event System.Action<EnergyNodeInteractable, bool> OnNodeToggled; // NEW

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        player = FindObjectOfType<PlayerMotor3D>(); // opcional
        UpdateVisual();
    }

    public string GetPrompt()
    {
        // Si ya quedó encendido y es oneShot, no mostramos prompt (no hay nada que hacer)
        if (oneShot && isOn) return "";
        // Si hay puzzle, guiamos al jugador
        if (puzzle && !isOn) return "Iniciar puzzle";
        // Modo toggle simple (sin puzzle) o permitir apagar si oneShot == false
        return isOn ? "E — Apagar nodo" : prompt;
    }

    public void Interact()
    {
        // Si ya está encendido y es de un solo uso, no hacemos nada
        if (oneShot && isOn) return;

        // Si hay puzzle: abrirlo y esperar callback
        if (puzzle)
        {
            if (!player) player = FindObjectOfType<PlayerMotor3D>();
            puzzle.Open(player, OnPuzzleSolved);
            return;
        }

        // Si NO hay puzzle: modo toggle simple (como tu versión temporal)
        // --- NEW: centralizamos el cambio para notificar ---
        SetState(!isOn); // NEW
    }

    private void OnPuzzleSolved()
    {
        // --- NEW: usar SetState para notificar ---
        SetState(true);   // NEW
        onSolved?.Invoke();
    }

    // --- NEW: punto único para cambiar estado + notificar ---
    private void SetState(bool on) // NEW
    {
        if (isOn == on) return;
        isOn = on;
        ApplySolvedEffectsIfNeeded(on);
        UpdateVisual();
        OnNodeToggled?.Invoke(this, isOn); // notifica a quien escuche (PortalLock, UI, etc.)
    }

    private void ApplySolvedEffectsIfNeeded(bool on)
    {
        if (linkedLight) linkedLight.enabled = on;
        if (on && solvedSfx) solvedSfx.Play();
    }

    public void UpdateVisual()
    {
        if (!rend) return;
        rend.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", isOn ? onColor : offColor);
        rend.SetPropertyBlock(mpb);
    }

}
