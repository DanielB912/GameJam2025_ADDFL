using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactorAlignPuzzle : MonoBehaviour, IPuzzle
{
    [Serializable]
    public class Channel
    {
        public Slider slider;     // Asignar el Slider del canal
        public Image fill;        // (opcional) cambia color al bloquear
        public Image lockIcon;    // (opcional) icono check
        [HideInInspector] public bool locked;
        [HideInInspector] public float holdTimer;
        [HideInInspector] public float noiseSeed;
    }
    private EnergyNodeInteractable targetNode;


    [Header("UI")]
    public Channel[] channels;
    public TextMeshProUGUI statusText;  // "Bloqueadas: X/Y"
    public Button btnClose;             // opcional (X)

    [Header("Reglas de bloqueo")]
    [Tooltip("Distancia permitida al centro (0.5)")]
    public float tolerance = 0.06f;
    [Tooltip("Tiempo dentro del rango para bloquear")]
    public float lockTime = 0.9f;
    [Tooltip("Exigir TODAS bloqueadas para ganar")]
    public bool requireAll = true;

    [Header("Movimiento automático")]
    [Tooltip("Velocidad del ruido")]
    public float driftSpeed = 0.8f;
    [Tooltip("Fuerza del ruido")]
    public float driftStrength = 0.35f;

    [Header("Inicio")]
    [Tooltip("¿Randomizar valores al abrir?")]
    public bool randomizeOnOpen = true;
    [Tooltip("Rango aleatorio inicial [min,max]")]
    public Vector2 startRange = new Vector2(0.15f, 0.85f);
    [Tooltip("Barras que empiezan ya alineadas (0 = ninguna)")]
    public int startAlignedCount = 2;

    [Header("Pausa/cursor mientras está abierto")]
    public bool pauseGameTime = true;
    public bool showCursor = true;

    public Action OnSolved { get; set; }
    public Action OnClosed { get; set; }


    bool _running;

    void Awake()
    {
        if (btnClose) btnClose.onClick.AddListener(() => Close(false));
        // Inicialización básica (colores apagados/ocultar iconos)
        foreach (var c in channels)
        {
            if (c == null || c.slider == null) continue;
            if (c.fill) c.fill.color = new Color(1f, 1f, 1f, 0.25f);
            if (c.lockIcon) c.lockIcon.enabled = false;
            c.locked = false;
            c.holdTimer = 0f;
            c.noiseSeed = UnityEngine.Random.value * 10f;
            // asegurar configuración de slider
            c.slider.direction = Slider.Direction.BottomToTop;
            c.slider.minValue = 0f; c.slider.maxValue = 1f;
        }
        UpdateStatus(0, channels.Length);
    }

    void OnEnable()
    {
        if (pauseGameTime) Time.timeScale = 0f;
        if (showCursor) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        if (randomizeOnOpen)
        {
            // 1) Random en rango
            foreach (var c in channels)
            {
                if (c?.slider == null) continue;
                c.slider.value = UnityEngine.Random.Range(startRange.x, startRange.y);
            }
            // 2) Forzar algunas alineadas al centro de arranque (si quieres)
            int n = Mathf.Clamp(startAlignedCount, 0, channels.Length);
            for (int i = 0; i < n; i++)
            {
                var c = channels[i];
                if (c?.slider == null) continue;
                c.slider.value = 0.5f + UnityEngine.Random.Range(-tolerance * 0.35f, tolerance * 0.35f);
                c.holdTimer = lockTime; // quedarán bloqueadas en el primer Update
            }
        }

        _running = true;
    }

    void OnDisable()
    {
        if (pauseGameTime) Time.timeScale = 1f;
        if (showCursor) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    void Update()
    {
        if (!_running) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(false); return; }

        float t = Time.unscaledTime;
        float dt = Time.unscaledDeltaTime;

        int lockedCount = 0;

        for (int i = 0; i < channels.Length; i++)
        {
            var c = channels[i];
            if (c == null || c.slider == null) continue;

            // Si ya está bloqueado, contamos y seguimos
            if (c.locked) { lockedCount++; continue; }

            // Aplicar DRIFT (ruido) para que se mueva solo
            float n = Mathf.PerlinNoise(t * driftSpeed, c.noiseSeed) - 0.5f; // [-0.5, 0.5]
            c.slider.value = Mathf.Clamp01(c.slider.value + n * driftStrength * dt);

            // ¿cerca del centro?
            float dist = Mathf.Abs(c.slider.value - 0.5f);
            if (dist <= tolerance)
            {
                c.holdTimer += dt;
                if (c.holdTimer >= lockTime)
                {
                    // Bloquea este canal
                    c.locked = true;
                    lockedCount++;
                    if (c.fill) c.fill.color = new Color(1f, 0.95f, 0.3f, 0.95f); // amarillo fuerte
                    if (c.lockIcon) c.lockIcon.enabled = true;
                }
            }
            else
            {
                c.holdTimer = 0f;
            }
        }

        UpdateStatus(lockedCount, channels.Length);

        // ¿Terminado?
        if ((requireAll && lockedCount == channels.Length) ||
            (!requireAll && lockedCount >= Mathf.CeilToInt(channels.Length * 0.5f)))
        {
            Succeed();
        }
    }

    void UpdateStatus(int locked, int total)
    {
        if (statusText) statusText.text = $"Bloqueadas: {locked}/{total}";
    }

    void Succeed()
    {
        _running = false;
        OnSolved?.Invoke();
        Destroy(gameObject);
    }
    public void SetTargetNode(EnergyNodeInteractable node)
    {
        targetNode = node;
    }
    void Close(bool solved)
    {
        _running = false;
        if (solved) OnSolved?.Invoke(); else OnClosed?.Invoke();
        Destroy(gameObject);
    }
    public void Open()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}