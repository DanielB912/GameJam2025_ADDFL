using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NodesProgressUI : MonoBehaviour
{
    [Header("Refs")]
    public PortalLock portalLock;          // puede quedar vacío; se autolocaliza
    public TextMeshProUGUI labelTMP;       // asigna tu Text (TMP)
    public Text labelLegacy;               // fallback si no usas TMP

    [Header("Texto")]
    public string format = "Energía restaurada: {0}/{1}";
    public string completedText = "¡Portal activado!";

    [Header("Comportamiento al completar")]
    public bool hideOnComplete = true;     // ocultar cuando se active el portal
    public float completeShowSeconds = 2f; // cuánto se muestra el mensaje final
    public bool fadeOnHide = true;
    public float fadeSeconds = 0.5f;

    void OnEnable()
    {
        TryBindOrSchedule();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void TryBindOrSchedule()
    {
        if (!portalLock)
        {
            // Encuentra incluso si está desactivado
            var all = Resources.FindObjectsOfTypeAll<PortalLock>();
            if (all != null && all.Length > 0) portalLock = all[0];
        }

        if (!portalLock)
        {
            StartCoroutine(CoRetryBind());
            UpdateText(0, 0);
            return;
        }

        portalLock.OnProgressChanged += UpdateText;
        portalLock.OnUnlocked += OnUnlockedHandler;
        UpdateText(portalLock.currentOnCount, portalLock.requiredCount);
    }

    IEnumerator CoRetryBind()
    {
        yield return null; // espera un frame
        var all = Resources.FindObjectsOfTypeAll<PortalLock>();
        if (all != null && all.Length > 0) portalLock = all[0];

        if (portalLock)
        {
            portalLock.OnProgressChanged += UpdateText;
            portalLock.OnUnlocked += OnUnlockedHandler;
            UpdateText(portalLock.currentOnCount, portalLock.requiredCount);
        }
        else
        {
            UpdateText(0, 0);
        }
    }

    void Unsubscribe()
    {
        if (portalLock)
        {
            portalLock.OnProgressChanged -= UpdateText;
            portalLock.OnUnlocked -= OnUnlockedHandler;
        }
    }

    void UpdateText(int current, int required)
    {
        string msg = string.Format(format, current, required);
        if (labelTMP) labelTMP.text = msg;
        else if (labelLegacy) labelLegacy.text = msg;

        // También detecta “completado” por si no llega OnUnlocked (por seguridad)
        if (required > 0 && current >= required)
            OnUnlockedHandler();
    }

    void OnUnlockedHandler()
    {
        StopAllCoroutines();
        StartCoroutine(CoShowCompletedAndMaybeHide());
    }

    IEnumerator CoShowCompletedAndMaybeHide()
    {
        // Muestra el texto de completado
        if (labelTMP) labelTMP.text = completedText;
        else if (labelLegacy) labelLegacy.text = completedText;

        if (!hideOnComplete)
            yield break;

        // Espera visible
        yield return new WaitForSeconds(Mathf.Max(0f, completeShowSeconds));

        if (fadeOnHide)
        {
            // Fade manual sobre el color
            float t = 0f;
            float dur = Mathf.Max(0.001f, fadeSeconds);

            if (labelTMP)
            {
                Color c0 = labelTMP.color;
                while (t < dur)
                {
                    t += Time.deltaTime;
                    float a = Mathf.Lerp(1f, 0f, t / dur);
                    labelTMP.color = new Color(c0.r, c0.g, c0.b, a);
                    yield return null;
                }
            }
            else if (labelLegacy)
            {
                Color c0 = labelLegacy.color;
                while (t < dur)
                {
                    t += Time.deltaTime;
                    float a = Mathf.Lerp(1f, 0f, t / dur);
                    labelLegacy.color = new Color(c0.r, c0.g, c0.b, a);
                    yield return null;
                }
            }
        }

        // Oculta el objeto UI
        gameObject.SetActive(false);
    }
}
