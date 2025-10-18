using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;

    [Header("Scene")]
    public string gameSceneName = "Game"; // Cambia si tu escena se llama distinto

    void Start()
    {
        Time.timeScale = 1f; // por si vuelves desde el juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainPanel) mainPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    // Botón PLAY
    public void PlayGame()
    {
        // Carga simple (sin pantalla de carga)
        SceneManager.LoadScene(gameSceneName);
        // Para carga async:
        // StartCoroutine(LoadAsync(gameSceneName));
    }

    // Botón OPTIONS
    public void OpenOptions()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    // Botón BACK en Options
    public void BackToMain()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(true);
    }

    // Botón QUIT
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Ejemplo de carga asíncrona (opcional)
    /*
    private IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            // aquí podrías actualizar una barra de progreso
            yield return null;
        }
    }
    */
}
