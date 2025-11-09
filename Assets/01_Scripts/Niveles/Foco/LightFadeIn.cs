using UnityEngine;
using System.Collections;

public class LightFadeIn : MonoBehaviour
{
    [Header("Light Target")]
    public Light targetLight;
    public float fadeTime = 1.5f;
    public float targetIntensity = 6f;

    private float offIntensity = 0f;
    private bool isOn = false;
    private Coroutine fadeRoutine;

    void Start()
    {
        // 🔹 Asegura que inicie apagada
        if (targetLight)
        {
            targetLight.intensity = 0f;
            targetLight.gameObject.SetActive(false);
        }
    }

    public void TurnOn()
    {
        if (!targetLight) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        targetLight.gameObject.SetActive(true); // 🔹 Activa la luz al encender
        fadeRoutine = StartCoroutine(FadeLight(offIntensity, targetIntensity));
        isOn = true;
    }

    public void TurnOff()
    {
        if (!targetLight) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeLight(targetIntensity, offIntensity));
        isOn = false;
    }

    IEnumerator FadeLight(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            if (targetLight)
                targetLight.intensity = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }

        // 🔹 Si terminó apagándose, desactiva el objeto de luz
        if (to <= 0.01f && targetLight)
            targetLight.gameObject.SetActive(false);
    }
}
