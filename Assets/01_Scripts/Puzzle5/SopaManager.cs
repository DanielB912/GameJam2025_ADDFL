using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SopaManager : MonoBehaviour
{
    [Header("Configuración de la Sopa")]
    public int gridSize = 10;
    public GameObject baseLetter;
    public Transform gridParent;
    public List<string> palabras = new List<string> { "ENERGIA", "FLOW", "POWER" };

    [Header("Referencias externas")]
    public GameObject cuboRelacionado;

    [Header("UI")]
    public Transform panelPalabras;     // Panel lateral
    public TextMeshProUGUI textoActual; // Muestra las letras seleccionadas
    public TextMeshProUGUI textoError;  // Mensaje de error

    private List<Button> selectedButtons = new List<Button>();
    private HashSet<string> palabrasEncontradas = new HashSet<string>();
    private Dictionary<string, TextMeshProUGUI> textoPalabrasUI = new Dictionary<string, TextMeshProUGUI>();

    void Start()
    {
        GenerarSopa();
        CrearListaPalabras();
        textoActual.text = "";
        textoError.text = "";
    }
    void Update()
    {
        // Permitir borrar con tecla Backspace
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            BorrarUltimaLetra();
        }
    }

    void GenerarSopa()
    {
        baseLetter.SetActive(false);
        System.Random random = new System.Random();

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                GameObject cell = Instantiate(baseLetter, gridParent);
                cell.SetActive(true);

                char randomLetter = (char)random.Next('A', 'Z' + 1);
                cell.GetComponentInChildren<TextMeshProUGUI>().text = randomLetter.ToString();

                Button button = cell.GetComponent<Button>();
                button.onClick.AddListener(() => OnLetterClick(button));
            }
        }
    }

    void CrearListaPalabras()
    {
        foreach (string palabra in palabras)
        {
            GameObject nuevoTexto = new GameObject(palabra);
            nuevoTexto.transform.SetParent(panelPalabras);
            var tmp = nuevoTexto.AddComponent<TextMeshProUGUI>();
            tmp.text = palabra;
            tmp.fontSize = 22;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            textoPalabrasUI[palabra] = tmp;
        }
    }

    void OnLetterClick(Button btn)
    {
        if (!selectedButtons.Contains(btn))
        {
            selectedButtons.Add(btn);
            btn.image.color = Color.yellow;
        }

        ActualizarTextoActual();
    }

    void ActualizarTextoActual()
    {
        string seleccion = "";
        foreach (Button b in selectedButtons)
        {
            seleccion += b.GetComponentInChildren<TextMeshProUGUI>().text;
        }
        textoActual.text = seleccion;
    }

    public void ConfirmarPalabra()
    {
        string seleccion = textoActual.text;

        bool encontrada = false;
        foreach (string palabra in palabras)
        {
            if (seleccion == palabra && !palabrasEncontradas.Contains(palabra))
            {
                palabrasEncontradas.Add(palabra);
                encontrada = true;

                // Marcar palabra en la UI
                textoPalabrasUI[palabra].color = Color.green;

                foreach (Button b in selectedButtons)
                    b.image.color = Color.green;

                textoError.text = "";
                selectedButtons.Clear();
                textoActual.text = "";

                if (palabrasEncontradas.Count == palabras.Count)
                {
                    PuzzleCompletado();
                }

                break;
            }
        }

        if (!encontrada)
        {
            textoError.text = "❌ Palabra incorrecta!";
            foreach (Button b in selectedButtons)
                b.image.color = Color.red;

            Invoke(nameof(LimpiarSeleccion), 0.8f);
        }
    }

    void LimpiarSeleccion()
    {
        foreach (Button b in selectedButtons)
            b.image.color = Color.white;

        selectedButtons.Clear();
        textoActual.text = "";
        textoError.text = "";
    }

    void PuzzleCompletado()
    {
        Debug.Log("🎉 Puzzle completado. Todas las palabras encontradas!");

        if (cuboRelacionado != null)
        {
            Renderer r = cuboRelacionado.GetComponent<Renderer>();
            if (r != null)
                r.material.color = Color.green;
        }

        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }
    void BorrarUltimaLetra()
    {
        if (selectedButtons.Count > 0)
        {
            // Última letra seleccionada
            Button ultima = selectedButtons[selectedButtons.Count - 1];

            // Restaurar su color
            ultima.image.color = Color.white;

            // Quitarla de la lista
            selectedButtons.RemoveAt(selectedButtons.Count - 1);

            // Actualizar el texto mostrado
            ActualizarTextoActual();
        }
    }

}
