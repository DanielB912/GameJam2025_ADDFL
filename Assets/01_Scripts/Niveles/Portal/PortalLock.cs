using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)] // se inicializa antes que el UI
public class PortalLock : MonoBehaviour
{
    [Header("Portal a habilitar")]
    public GameObject portalRoot;              // raíz visual del portal (puede ser hijo)
    public Collider portalCollider;            // trigger/collider del portal (opcional)
    public Renderer[] portalRenderers;         // VFX/meshes a mostrar/ocultar (opcional)

    [Header("Requisitos")]
    public bool autoFindNodes = true;          // busca todos los EnergyNodeInteractable de la escena
    public List<EnergyNodeInteractable> nodes = new List<EnergyNodeInteractable>();
    public int requiredCount = -1;             // -1 = usar total de nodos encontrados
    public bool hidePortalAtStart = true;      // oculta el portal hasta cumplir

    [Header("SFX al desbloquear")]
    public AudioSource unlockSfx;              // si lo asignas, se usa este
    public AudioClip unlockSfxClip;            // o reproduce un clip 2D/3D puntual
    [Range(0f, 1f)] public float unlockSfxVolume = 0.9f;

    [Header("Estado (debug)")]
    public int currentOnCount = 0;
    public int totalNodes = 0;
    private bool unlockedPlayed = false;

    // UI/otros pueden engancharse aquí
    public System.Action<int, int> OnProgressChanged;
    public System.Action OnUnlocked;           // notifica cuando se desbloquea

    void Reset()
    {
        if (!portalRoot) portalRoot = gameObject;
        if (!portalCollider) portalCollider = GetComponentInChildren<Collider>(true);
        if (portalRenderers == null || portalRenderers.Length == 0)
            portalRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void Awake()
    {
        // Recolectar nodos
        if (autoFindNodes)
        {
            nodes.Clear();
            nodes.AddRange(FindObjectsOfType<EnergyNodeInteractable>(true));
        }

        totalNodes = nodes.Count;
        if (requiredCount < 0 || requiredCount > totalNodes)
            requiredCount = totalNodes;

        // Suscripción + conteo inicial
        currentOnCount = 0;
        foreach (var n in nodes)
        {
            if (!n) continue;
            n.OnNodeToggled += OnNodeToggled;
            if (n.IsOn) currentOnCount++;
        }

        bool unlocked = currentOnCount >= requiredCount;
        SetPortalUnlocked(unlocked || !hidePortalAtStart, playSfx: false);

        OnProgressChanged?.Invoke(currentOnCount, requiredCount);
    }

    void OnDestroy()
    {
        foreach (var n in nodes)
            if (n) n.OnNodeToggled -= OnNodeToggled;
    }

    private void OnNodeToggled(EnergyNodeInteractable node, bool isOn)
    {
        currentOnCount += isOn ? 1 : -1;
        currentOnCount = Mathf.Clamp(currentOnCount, 0, totalNodes);

        if (currentOnCount >= requiredCount)
            SetPortalUnlocked(true, playSfx: true);

        OnProgressChanged?.Invoke(currentOnCount, requiredCount);
    }

    private void SetPortalUnlocked(bool enabled, bool playSfx)
    {
        if (portalRoot) portalRoot.SetActive(enabled);
        if (portalCollider) portalCollider.enabled = enabled;

        if (portalRenderers != null)
            foreach (var r in portalRenderers)
                if (r) r.enabled = enabled;

        if (enabled && playSfx && !unlockedPlayed)
        {
            unlockedPlayed = true;
            // SFX
            if (unlockSfx) unlockSfx.Play();
            else if (unlockSfxClip)
                AudioSource.PlayClipAtPoint(unlockSfxClip, transform.position, unlockSfxVolume);

            // Evento para UI o lógica extra
            OnUnlocked?.Invoke();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!portalRoot) portalRoot = gameObject;
        if (!portalCollider) portalCollider = GetComponentInChildren<Collider>(true);
        if (portalRenderers == null || portalRenderers.Length == 0)
            portalRenderers = GetComponentsInChildren<Renderer>(true);
    }
#endif
}
