using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    public CanvasGroup group;
    public bool startHidden = true;
    public bool deactivateAfterFade = false;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (startHidden)
        {
            group.alpha = 0f;
            // OJO: no desactiva el GameObject, solo lo deja invisible
        }
    }

    public void FadeOut(float duration)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(group.alpha, 1f, duration));
    }

    public void FadeIn(float duration)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(group.alpha, 0f, duration));
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        group.alpha = to;

        if (deactivateAfterFade && to <= 0f)
            gameObject.SetActive(false);
    }
}
