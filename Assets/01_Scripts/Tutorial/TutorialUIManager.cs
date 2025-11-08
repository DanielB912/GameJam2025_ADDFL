using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialUIManager : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI tutorialText;
    public CanvasGroup canvasGroup;

    [Header("Configuración")]
    public float fadeDuration = 0.6f;
    public float displayTime = 5f;

    private Coroutine currentRoutine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = tutorialText.GetComponentInParent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = tutorialText.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
    }

    public void ShowMessage(string message, float time = -1f)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message, time > 0 ? time : displayTime));
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        tutorialText.text = message;

        // Fade In
        yield return StartCoroutine(Fade(1f));
        yield return new WaitForSecondsRealtime(duration);
        // Fade Out
        yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
