using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SimpleCreditScroll : MonoBehaviour
{
    [Header("Scroll")]
    public float pixelsPerSecond = 40f;
    public float startDelay = 0.5f;
    public float stopAtY = 500f;         // hasta dónde sube el texto

    [Header("Final (mostrar botones con fade)")]
    public CanvasGroup buttonsGroup;     // arrastra un contenedor con los botones
    public float buttonsFadeDuration = 0.6f;

    RectTransform rt;
    float timer;
    bool finished;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (buttonsGroup != null)
        {
            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }
    }

    void OnEnable()
    {
        timer = 0f;
        finished = false;
        // Si quieres resetear la posición cada vez:
        // rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -40f);
        if (buttonsGroup != null)
        {
            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < startDelay || finished) return;

        var p = rt.anchoredPosition;
        if (p.y < stopAtY)
        {
            p.y += pixelsPerSecond * Time.deltaTime;
            rt.anchoredPosition = p;
        }
        else
        {
            finished = true;
            if (buttonsGroup != null)
                StartCoroutine(FadeButtonsIn());
        }
    }

    System.Collections.IEnumerator FadeButtonsIn()
    {
        float t = 0f;
        buttonsGroup.interactable = true;
        buttonsGroup.blocksRaycasts = true;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, buttonsFadeDuration);
            buttonsGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }
    }
}
