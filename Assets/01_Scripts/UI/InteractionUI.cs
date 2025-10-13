using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("Referencias")]
    public InteractProximity proximity;      // Player con InteractProximity
    public TextMeshProUGUI promptText;       // TMP del texto
    public CanvasGroup canvasGroup;          // CanvasGroup del texto

    [Header("Aparición/Desaparición")]
    public float fadeInSpeed = 12f;
    public float fadeOutSpeed = 10f;
    public float minAlphaToShow = 0.02f;     // oculta si es casi transparente

    [Header("Animación visual (opcional)")]
    public bool enablePopEffect = true;
    public float popScale = 1.05f;           // escala al aparecer
    public float popSpeed = 8f;              // velocidad del pop

    private float targetAlpha = 0f;
    private Vector3 baseScale;

    void Start()
    {
        // Guardar escala base y asegurar referencias
        if (promptText == null) promptText = GetComponentInChildren<TextMeshProUGUI>();
        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();
        if (proximity == null) proximity = FindObjectOfType<InteractProximity>();

        if (promptText != null)
            baseScale = promptText.transform.localScale;
    }

    void Update()
    {
        if (!proximity || !canvasGroup || !promptText)
            return;

        var current = proximity.Current;

        // === Mostrar texto ===
        if (current != null)
        {
            promptText.text = "[E] " + current.GetPrompt();
            targetAlpha = 1f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeInSpeed * Time.deltaTime);

            // Pop visual al aparecer
            if (enablePopEffect)
            {
                Vector3 target = baseScale * popScale;
                promptText.transform.localScale = Vector3.Lerp(promptText.transform.localScale, target, popSpeed * Time.deltaTime);
            }
        }
        // === Ocultar texto ===
        else
        {
            targetAlpha = 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeOutSpeed * Time.deltaTime);

            if (enablePopEffect)
            {
                promptText.transform.localScale = Vector3.Lerp(promptText.transform.localScale, baseScale, popSpeed * Time.deltaTime);
            }
        }

        // Bloquear raycasts si está invisible (opcional)
        bool visible = canvasGroup.alpha >= minAlphaToShow;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        // Activar/desactivar el objeto según visibilidad (optimiza draw calls)
        if (!visible && promptText.gameObject.activeSelf)
            promptText.gameObject.SetActive(false);
        else if (visible && !promptText.gameObject.activeSelf)
            promptText.gameObject.SetActive(true);
    }
}
