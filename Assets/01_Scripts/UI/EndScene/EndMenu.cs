using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour
{
    [Header("Scenes")]
    public string mainMenuScene = "MainMenu";

    [Header("UI")]
    public Button firstButton;     // arrastra el botón "Volver al Menú"

    void Start()
    {
        // Mostrar cursor en PC
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Foco inicial
        if (firstButton)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    void Update()
    {
        // Tecla ESC -> volver al menú
        if (Input.GetKeyDown(KeyCode.Escape))
            ReturnToMenu();

        // Enter -> activa el botón seleccionado
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            var go = EventSystem.current.currentSelectedGameObject;
            if (go)
            {
                var btn = go.GetComponent<Button>();
                if (btn) btn.onClick.Invoke();
            }
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
