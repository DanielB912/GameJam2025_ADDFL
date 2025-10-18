using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale feedback")]
    public float normalScale = 1f;
    public float hoverScale = 1.06f;
    public float pressedScale = 0.98f;
    public float scaleLerp = 12f;

    [Header("Glow feedback (auto child)")]
    public bool addGlow = true;
    public Color glowColor = new Color(0f, 0.91f, 1f, 0.12f);  // cian, alpha bajo
    public float glowAlphaHover = 0.28f;
    public float glowAlphaPressed = 0.18f;

    RectTransform rt;
    Image glowImg;
    float targetScale = 1f;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        targetScale = normalScale;

        if (addGlow)
        {
            // crea (o reutiliza) un hijo "Glow"
            Transform existing = transform.Find("Glow");
            if (!existing)
            {
                GameObject g = new GameObject("Glow", typeof(RectTransform), typeof(Image));
                g.transform.SetSiblingIndex(0); // detrás del contenido
                g.transform.SetParent(transform, false);
                glowImg = g.GetComponent<Image>();
                var grt = g.GetComponent<RectTransform>();
                grt.anchorMin = Vector2.zero; grt.anchorMax = Vector2.one;
                grt.offsetMin = Vector2.zero; grt.offsetMax = Vector2.zero;
            }
            else
            {
                glowImg = existing.GetComponent<Image>();
                if (!glowImg) glowImg = existing.gameObject.AddComponent<Image>();
            }
            glowImg.raycastTarget = false;
            glowImg.color = glowColor;
        }
    }

    void Update()
    {
        Vector3 s = Vector3.one * targetScale;
        rt.localScale = Vector3.Lerp(rt.localScale, s, scaleLerp * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;
        if (glowImg) SetGlowAlpha(glowAlphaHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
        if (glowImg) SetGlowAlpha(glowColor.a);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = pressedScale;
        if (glowImg) SetGlowAlpha(glowAlphaPressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = hoverScale;
        if (glowImg) SetGlowAlpha(glowAlphaHover);
    }

    void SetGlowAlpha(float a)
    {
        var c = glowImg.color; c.a = a; glowImg.color = c;
    }
}
