using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeypadPuzzle : MonoBehaviour, IPuzzle
{

    [Header("Referencias UI")]
    public Canvas canvas; // 🔹 nuevo campo
    public TextMeshProUGUI targetCodeText; // muestra el código correcto
    public TextMeshProUGUI inputText;      // lo que el jugador ingresa
    public TextMeshProUGUI statusText;     // muestra “Correcto / Incorrecto”

    [Header("Botones")]
    public Button[] digitButtons; // 0–9
    public Button btnClear;       // X
    public Button btnSubmit;      // ✓

    [Header("Configuración")]
    [Range(3, 8)] public int codeLength = 5;
    public bool useFixedCode = true;
    public string fixedCode = "32568";

    [Header("Opciones")]
    public bool pauseGameTime = true;
    public bool showCursor = true;

    // Eventos
    public Action OnSolved { get; set; }
    public Action OnClosed { get; set; }

    // Internos
    private string _targetCode;
    private StringBuilder _input = new StringBuilder();
    private bool _open;
    private EnergyNodeInteractable targetNode;

    public void SetTargetNode(EnergyNodeInteractable node)
    {
        targetNode = node;
    }
    void Awake()
    {
        AutoBindIfEmpty();
        WireButtons();
    }

    void OnEnable() => Open();

    void Update()
    {
        if (!_open) return;
        if (Input.GetKeyDown(KeyCode.Escape))
            Close(false);
    }

    public void Open()
    {
        _open = true;
        GenerateNewCode();
        _input.Clear();
        RefreshUI();
        SetStatus("");

        if (canvas) canvas.gameObject.SetActive(true); // 🔹 ACTIVAR UI
        if (pauseGameTime) Time.timeScale = 0f;
        if (showCursor) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        Debug.Log("[KeypadPuzzle] Puzzle abierto."); // 🧠 depuración
    }

    void GenerateNewCode()
    {
        if (useFixedCode) _targetCode = fixedCode;
        else
        {
            System.Random rng = new System.Random();
            var sb = new StringBuilder(codeLength);
            for (int i = 0; i < codeLength; i++) sb.Append(rng.Next(0, 10));
            _targetCode = sb.ToString();
        }
        if (targetCodeText) targetCodeText.text = _targetCode;
    }

    void WireButtons()
    {
        if (digitButtons != null && digitButtons.Length > 0)
        {
            for (int i = 0; i < digitButtons.Length; i++)
            {
                int d = i;
                digitButtons[i].onClick.AddListener(() => PressDigit(d));
            }
        }
        if (btnClear) btnClear.onClick.AddListener(ClearInput);
        if (btnSubmit) btnSubmit.onClick.AddListener(Submit);
    }

    void PressDigit(int d)
    {
        if (_input.Length >= codeLength) return;
        _input.Append(d);
        RefreshUI();
    }

    void ClearInput()
    {
        _input.Clear();
        RefreshUI();
        SetStatus("");
    }

    void Submit()
    {
        if (_input.Length != codeLength)
        {
            Fail("Código incompleto");
            return;
        }

        if (_input.ToString() == _targetCode)
        {
            SetStatus("<color=#6CFF8B>Correcto</color>");

            // Si hay un nodo, lo activamos
            if (targetNode)
            {
                // Usamos reflexión para llamar SetState(true)
                var method = targetNode.GetType().GetMethod("SetState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(targetNode, new object[] { true });
            }

            Close(true);
        }

        else
        {
            Fail("Código incorrecto");
        }
    }

    void Fail(string msg)
    {
        SetStatus($"<color=#FF6C6C>{msg}</color>");
        _input.Clear();
        RefreshUI();

        // 💥 Daño al jugador por fallo
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1); // ajusta el daño si quieres
            Debug.Log("❌ Fallo en Keypad: jugador pierde 10 de vida.");
        }
    }

    void RefreshUI()
    {
        if (inputText) inputText.text = _input.ToString();
    }

    void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }

    void Close(bool solved)
    {
        _open = false;
        if (canvas) canvas.gameObject.SetActive(false);
        if (pauseGameTime) Time.timeScale = 1f;
        if (showCursor) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        // 🔹 Elimina esta línea:
        // if (solved) OnSolved?.Invoke();

        if (!solved) OnClosed?.Invoke();
        Destroy(gameObject);
    }

    // === AUTO-BIND ===
    void AutoBindIfEmpty()
    {
        if (digitButtons == null || digitButtons.Length < 10)
        {
            digitButtons = new Button[10];
            for (int i = 0; i <= 9; i++)
            {
                var t = transform.Find($"Btn{i}");
                if (t) digitButtons[i] = t.GetComponent<Button>();
            }
        }
        if (!btnClear) { var t = transform.Find("BtnClear"); if (t) btnClear = t.GetComponent<Button>(); }
        if (!btnSubmit) { var t = transform.Find("BtnSubmit"); if (t) btnSubmit = t.GetComponent<Button>(); }

        if (!targetCodeText) { var t = transform.Find("TargetCodeText"); if (t) targetCodeText = t.GetComponent<TextMeshProUGUI>(); }
        if (!inputText) { var t = transform.Find("InputText"); if (t) inputText = t.GetComponent<TextMeshProUGUI>(); }
        if (!statusText) { var t = transform.Find("StatusText"); if (t) statusText = t.GetComponent<TextMeshProUGUI>(); }
    }
}
