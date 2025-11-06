using UnityEngine;
using UnityEngine.UI;
using TMPro; // 👈 para mostrar el valor en texto

public class PauseMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject canvas;
    public Button resumeButton;
    public Button exitButton;
    public Slider volumeSlider;          // 🔹 Slider de volumen
    public TextMeshProUGUI volumeText;   // 🔹 Texto que muestra el valor del volumen

    [Header("Opciones")]
    public KeyCode pauseKey = KeyCode.F1;
    private bool isPaused = false;

    void Start()
    {
        // 🔹 Ocultamos el panel al inicio
        if (canvas)
            canvas.gameObject.SetActive(false);

        // 🔹 Botones principales
        if (resumeButton)
            resumeButton.onClick.AddListener(ResumeGame);

        if (exitButton)
            exitButton.onClick.AddListener(ExitGame);

        // 🔹 Configurar slider
        if (volumeSlider)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 100f;
            float savedVolume = PlayerPrefs.GetFloat("GameVolume", 100f); // valor por defecto = 100%
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        if (canvas) canvas.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (canvas) canvas.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void SetVolume(float value)
    {
        // 🔹 Convertir 0–100 a 0–1 (Unity usa 0–1 internamente)
        float normalizedVolume = value / 100f;
        AudioListener.volume = normalizedVolume;

        // 🔹 Actualizar texto
        if (volumeText)
            volumeText.text = $"Volumen: {Mathf.RoundToInt(value)}%";

        // 🔹 Guardar preferencia
        PlayerPrefs.SetFloat("GameVolume", value);
    }
}
