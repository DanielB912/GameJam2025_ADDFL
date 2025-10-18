using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeIn : MonoBehaviour
{
    public float duration = 1f;   // segundos de fade
    CanvasGroup cg; float t;

    void OnEnable()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f; t = 0f;
    }

    void Update()
    {
        if (cg.alpha >= 1f) return;
        t += Time.deltaTime / Mathf.Max(0.0001f, duration);
        cg.alpha = Mathf.SmoothStep(0f, 1f, t);
    }
}
