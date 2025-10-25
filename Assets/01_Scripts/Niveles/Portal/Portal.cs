using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Destino (opcional)")]
    public string overrideSceneName = "";

    [Header("Requisitos (futuro)")]
    public bool requireAllObjectives = false;

    [Header("Final")]
    public string endSceneName = "EndScene";

    [Header("Feedback")]
    public AudioSource sfx;
    public float delayBeforeLoad = 0f; // tiempo para FX/SFX
    public UIFader screenFader;          // opcional (fade)

    private bool isLoading = false;
    private string pendingSceneName = ""; // ← aquí guardamos a dónde cargar

    void Reset()
    {
        sfx = GetComponent<AudioSource>();
        if (!screenFader) screenFader = FindObjectOfType<UIFader>(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;
        if (!other.CompareTag("Player")) return;

        if (requireAllObjectives && !AllObjectivesCompleted())
        {
            Debug.Log("[Portal] Aún faltan objetivos.");
            return;
        }

        pendingSceneName = GetNextSceneName();
        if (string.IsNullOrEmpty(pendingSceneName))
        {
            Debug.LogError("[Portal] No se encontró escena destino.");
            return;
        }

        isLoading = true;
        if (sfx) sfx.Play();
        if (screenFader) screenFader.FadeOut(1f); // opcional

        if (delayBeforeLoad > 0f)
            StartCoroutine(LoadAfterDelay());
        else
            LoadNext();
    }

    IEnumerator LoadAfterDelay()
    {
        // 1️⃣ Inicia fade hacia negro (si existe el fader)
        if (screenFader)
        {
            screenFader.FadeOut(1f); // ← más lento, da tiempo a oscurecer
            yield return new WaitForSeconds(1.2f); // ← espera extra antes de cargar
        }
        else
        {
            yield return new WaitForSeconds(delayBeforeLoad);
        }

        // 2️⃣ Carga la siguiente escena ya con pantalla negra
        LoadNext();
    }


    void LoadNext()
    {
        SceneManager.LoadScene(pendingSceneName);
    }

    string GetNextSceneName()
    {
        if (!string.IsNullOrEmpty(overrideSceneName))
            return overrideSceneName;

        int current = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = current + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(nextIndex);
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        // si no hay más escenas, ir al final
        return endSceneName;
    }

    // FUTURO: conectar con tu administrador de objetivos
    bool AllObjectivesCompleted()
    {
        // return LevelObjectiveManager.Instance?.AllCompleted == true;
        return true;
    }

    void OnDrawGizmos()
    {
        var sc = GetComponent<SphereCollider>();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sc ? sc.radius : 1.3f);
    }

    void Awake()
    {
        if (!screenFader) screenFader = FindObjectOfType<UIFader>(true);
    }
}
