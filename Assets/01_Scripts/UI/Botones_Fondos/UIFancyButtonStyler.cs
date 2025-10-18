using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Aplica automáticamente un estilo "futurista" a todos los botones
/// dentro del Canvas (colores, texto, sombra, escala y glow en hover),
/// sin depender de ningún asset externo.
/// </summary>
[ExecuteAlways]
public class UIFancyButtonStyler : MonoBehaviour
{
    [Header("Scope")]
    public Transform root;             // Si está vacío, usa este transform (Canvas o Panel)
    public bool includeInactive = true;

    [Header("Color Tint")]
    public Color normal = new Color32(0x1E, 0x1E, 0x1E, 0xFF);       // gris oscuro
    public Color highlighted = new Color32(0x00, 0xE8, 0xFF, 0xFF);  // cian brillante
    public Color pressed = new Color32(0x00, 0x94, 0xA0, 0xFF);      // cian oscuro
    public Color selected = new Color32(0x00, 0xE8, 0xFF, 0xFF);
    public Color disabled = new Color32(0x2E, 0x2E, 0x2E, 0xFF);
    public float fadeDuration = 0.18f;

    [Header("Shape")]
    [Range(0f, 1f)] public float cornerRoundness = 0.25f;
    public Color outlineColor = new Color(0f, 0.91f, 1f, 0.4f); // cian semitransparente

    [Header("Text Style")]
    public int fontSize = 30;
    public bool bold = true;
    public Color textColor = Color.white;
    public bool addShadow = true;

    [Header("Hover Feedback")]
    public float hoverScale = 1.07f;
    public float pressedScale = 0.96f;
    public float scaleLerp = 10f;

    void Start()
    {
        ApplyAll();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
                ApplyAll();
        };
    }
#endif


    void ApplyAll()
    {
        var scope = root ? root : transform;
        var buttons = scope.GetComponentsInChildren<Button>(includeInactive);

        foreach (var btn in buttons)
        {
            // ---------- COLOR TINT ----------
            btn.transition = Selectable.Transition.ColorTint;
            var cb = btn.colors;
            cb.normalColor = normal;
            cb.highlightedColor = highlighted;
            cb.pressedColor = pressed;
            cb.selectedColor = selected;
            cb.disabledColor = disabled;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = fadeDuration;
            btn.colors = cb;

            // ---------- SHAPE ----------
            var img = btn.GetComponent<Image>();
            if (img)
            {
                // Sprite blanco genérico (reemplaza al antiguo UISprite.psd)
                img.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                img.material = null;
                img.color = normal;
            }

            // Añadir borde (Outline)
            var outline = btn.GetComponent<Outline>();
            if (!outline) outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2, -2);
            outline.useGraphicAlpha = true;

            // ---------- TEXTO ----------
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp)
            {
                tmp.fontSize = fontSize;
                tmp.color = textColor;
                tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;

                if (addShadow && !tmp.GetComponent<Shadow>())
                {
                    var sh = tmp.gameObject.AddComponent<Shadow>();
                    sh.effectDistance = new Vector2(1, -1);
                    sh.effectColor = new Color(0, 0, 0, 0.5f);
                }
            }

            // ---------- HOVER FEEDBACK ----------
            var eff = btn.GetComponent<UIHoverFeedback>();
            if (!eff) eff = btn.gameObject.AddComponent<UIHoverFeedback>();
            eff.hoverScale = hoverScale;
            eff.pressedScale = pressedScale;
            eff.scaleLerp = scaleLerp;
            eff.addGlow = true;
        }
    }
}
