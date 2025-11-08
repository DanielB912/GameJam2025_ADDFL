using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrdenarPalabrasManager : MonoBehaviour
{
    [Header("Configuración del puzzle")]
    public List<string> palabrasCorrectas = new List<string> { "ENERGIA", "FLOW", "POWER" };
    public Transform letrasContainer;
    public GameObject letraBase; // botón base (debe estar desactivado en escena)
    public TextMeshProUGUI textoActual;
    public TextMeshProUGUI textoMensaje;
    public GameObject cuboRelacionado; // cubo que cambiará de color al finalizar

    private int palabraActualIndex = 0;
    private string palabraCorrecta;
    private string palabraFormada = "";
    private List<Button> botonesLetras = new List<Button>();
    private HashSet<string> palabrasCompletadas = new HashSet<string>();
    private bool puzzleCompletado = false;

    void Awake()
    {
        if (letrasContainer == null)
        {
            Debug.LogError("⚠️ No se asignó letrasContainer en el inspector.");
            return;
        }

        if (letraBase == null)
        {
            letraBase = letrasContainer.GetComponentInChildren<Button>(true)?.gameObject;
            if (letraBase == null)
            {
                Debug.LogError("⚠️ No se encontró LetraBase dentro del contenedor en Awake.");
                return;
            }
        }

        letraBase.SetActive(false);
    }

    void OnEnable()
    {
        if (!gameObject.activeInHierarchy) return;

        Debug.Log("🟢 Puzzle activado. Reiniciando...");
        ReiniciarPuzzle();
    }

    void ReiniciarPuzzle()
    {
        palabraActualIndex = 0;
        palabraFormada = "";
        textoActual.text = "";
        textoMensaje.text = "";
        palabrasCompletadas.Clear();
        puzzleCompletado = false;

        if (letraBase == null)
        {
            letraBase = letrasContainer.GetComponentInChildren<Button>(true)?.gameObject;
            if (letraBase == null)
            {
                Debug.LogError("❌ No se encontró letraBase al reiniciar.");
                return;
            }
        }

        letraBase.SetActive(false);
        CargarNuevaPalabra();
    }

    void CargarNuevaPalabra()
    {
        Debug.Log("🧩 Cargando nueva palabra...");

        if (palabraActualIndex >= palabrasCorrectas.Count)
        {
            PuzzleCompletado();
            StartCoroutine(CerrarDespuesDeRetraso(1.5f));
            return;
        }

        foreach (Transform child in letrasContainer)
        {
            if (child.gameObject != letraBase)
                Destroy(child.gameObject);
        }

        botonesLetras.Clear();
        palabraFormada = "";
        textoActual.text = "";
        textoMensaje.text = "";

        palabraCorrecta = palabrasCorrectas[palabraActualIndex];
        string palabraDesordenada = Desordenar(palabraCorrecta);

        Debug.Log($"✅ Palabra cargada: {palabraCorrecta} (desordenada: {palabraDesordenada})");

        foreach (char c in palabraDesordenada)
        {
            if (letraBase == null)
            {
                Debug.LogError("❌ letraBase no está asignada.");
                return;
            }

            GameObject letraObj = Instantiate(letraBase, letrasContainer);
            letraObj.SetActive(true);
            letraObj.name = "Letra_" + c;

            TextMeshProUGUI letraTMP = letraObj.GetComponentInChildren<TextMeshProUGUI>();
            if (letraTMP != null)
                letraTMP.text = c.ToString();

            Button btn = letraObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnLetraClick(btn));
                botonesLetras.Add(btn);
            }

            Debug.Log($"🟩 Letra generada: {c}");
        }

        textoMensaje.text = $"Palabra {palabraActualIndex + 1} de {palabrasCorrectas.Count}";
        textoMensaje.color = Color.white;
    }

    string Desordenar(string palabra)
    {
        System.Random rnd = new System.Random();
        char[] letras = palabra.ToCharArray();
        for (int i = letras.Length - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (letras[i], letras[j]) = (letras[j], letras[i]);
        }
        return new string(letras);
    }

    void OnLetraClick(Button btn)
    {
        if (btn == null) return;

        TextMeshProUGUI letraTMP = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (letraTMP == null) return;

        string letra = letraTMP.text;
        palabraFormada += letra;
        textoActual.text = palabraFormada;

        btn.interactable = false;
    }

    public void ConfirmarPalabra()
    {
        if (puzzleCompletado) return;

        if (palabraFormada == palabraCorrecta)
        {
            palabrasCompletadas.Add(palabraCorrecta);
            textoMensaje.color = Color.green;
            textoMensaje.text = $"Correcto! ({palabraFormada})";
            palabraActualIndex++;

            StartCoroutine(SiguientePalabraConRetraso(1.2f));
        }
        else
        {
            textoMensaje.color = Color.red;
            textoMensaje.text = "Incorrecto. Intenta de nuevo!";

            // 💥 Daño al jugador por error
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // ajusta el valor si quieres
                Debug.Log("❌ OrdenarPalabras: jugador pierde 10 de vida por error.");
            }

            StartCoroutine(ReintentarPalabraConRetraso(1f));
        }
    }

    private System.Collections.IEnumerator SiguientePalabraConRetraso(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        CargarNuevaPalabra();
    }

    private System.Collections.IEnumerator ReintentarPalabraConRetraso(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        ReintentarPalabra();
    }

    void ReintentarPalabra()
    {
        CargarNuevaPalabra();
    }

    void PuzzleCompletado()
    {
        if (puzzleCompletado) return;
        puzzleCompletado = true;

        textoMensaje.color = Color.yellow;
        textoMensaje.text = "¡Todas las palabras completadas!";

        if (cuboRelacionado != null)
        {
            Renderer r = cuboRelacionado.GetComponent<Renderer>();
            if (r != null)
                r.material.color = Color.green;
        }
    }

    private System.Collections.IEnumerator CerrarDespuesDeRetraso(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        CerrarPuzzle();
    }

    void CerrarPuzzle()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        Debug.Log("🟢 Puzzle completado y cerrado.");
    }

    public void SalirPuzzle()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }
}
