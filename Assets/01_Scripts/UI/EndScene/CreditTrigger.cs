using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public string endSceneName = "EndScene"; // Nombre exacto de la escena de créditos
    public bool requirePlayerTag = true;      // Solo reacciona al Player
    public float delayBeforeLoad = 1f;        // Pequeña pausa antes de cargar

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // Evita múltiples activaciones

        if (!requirePlayerTag || other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(LoadCreditsAfterDelay());
        }
    }

    System.Collections.IEnumerator LoadCreditsAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(endSceneName);
    }
}
