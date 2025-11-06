using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CablePuzzleController : MonoBehaviour, IPuzzle
{
    [Header("Refs UI")]
    public Canvas canvas;
    public Button[] leftSockets;
    public Button[] rightSockets;
    public Image bulbImage;
    public Sprite bulbOff;
    public Sprite bulbOn;
    public Button closeButton;
    public TextMeshProUGUI title;

    [Header("Config")]
    [Tooltip("rightIndexCorrect[i] = índice de la derecha que empareja con L[i]")]
    public int[] rightIndexCorrect;

    public bool IsOpen { get; private set; }

    [Header("Líneas (cables)")]
    public RectTransform linesLayer;
    public Image linePrefab;
    public float lineThickness = 6f;
    public bool useLeftSocketColor = true;
    [Range(0.5f, 2f)] public float lockedBrighten = 1.0f;
    [Range(0f, 1f)] public float alphaSelected = 0.9f;
    [Range(0f, 1f)] public float alphaLocked = 0.95f;

    private readonly Dictionary<int, Image> lineByLeft = new();

    private int? pendingLeft = null;
    private Dictionary<int, int> currentPairs = new();
    private PlayerMotor3D player;

    // === NUEVO ===
    private EnergyNodeInteractable targetNode;
    public Action OnSolved { get; set; }
    public Action OnClosed { get; set; }

    public void SetTargetNode(EnergyNodeInteractable node) => targetNode = node;

    public void Open()
    {
        if (!player)
            player = FindObjectOfType<PlayerMotor3D>();

        Open(player, () =>
        {
            OnSolved?.Invoke();
            if (targetNode != null)
            {
                var method = targetNode.GetType().GetMethod("SetState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(targetNode, new object[] { true });
            }
        });
    }
    // === FIN NUEVO ===

    private Action onSolved;

    void Awake()
    {
        canvas.gameObject.SetActive(false);
        for (int i = 0; i < leftSockets.Length; i++)
        {
            int li = i;
            leftSockets[i].onClick.AddListener(() => OnLeftClick(li));
        }
        for (int j = 0; j < rightSockets.Length; j++)
        {
            int rj = j;
            rightSockets[j].onClick.AddListener(() => OnRightClick(rj));
        }
        closeButton.onClick.AddListener(Close);
        SetBulb(false);
    }

    void LateUpdate()
    {
        if (IsOpen) UpdateAllLines();
    }

    public void Open(PlayerMotor3D playerMotor, Action onSolvedCallback = null)
    {
        player = playerMotor;
        onSolved = onSolvedCallback;
        pendingLeft = null;
        currentPairs.Clear();
        SetBulb(false);
        ResetButtonVisuals();

        foreach (var kv in lineByLeft.Values.ToList())
            if (kv) Destroy(kv.gameObject);
        lineByLeft.Clear();

        if (player) player.inputEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        canvas.gameObject.SetActive(true);
        IsOpen = true;
    }

    public void Close()
    {
        if (!IsOpen) return;

        canvas.gameObject.SetActive(false);
        IsOpen = false;

        if (player) player.inputEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnClosed?.Invoke(); // ⚙️ disparar evento de cierre
    }

    void OnLeftClick(int leftIndex)
    {
        if (currentPairs.ContainsKey(leftIndex))
        {
            int right = currentPairs[leftIndex];
            currentPairs.Remove(leftIndex);
            SetButtonState(leftSockets[leftIndex], normal: true);
            SetButtonState(rightSockets[right], normal: true);
            SetBulb(false);
            RemoveLineForLeft(leftIndex);
            return;
        }

        pendingLeft = leftIndex;
        HighlightOnly(leftSockets[leftIndex]);
    }

    void OnRightClick(int rightIndex)
    {
        if (pendingLeft == null) return;
        int li = pendingLeft.Value;
        pendingLeft = null;

        // si R ya estaba usado, liberarlo
        int occupiedBy = -1;
        foreach (var kv in currentPairs)
            if (kv.Value == rightIndex) { occupiedBy = kv.Key; break; }
        if (occupiedBy != -1)
        {
            currentPairs.Remove(occupiedBy);
            SetButtonState(leftSockets[occupiedBy], normal: true);
            RemoveLineForLeft(occupiedBy);
        }

        // --- 🔸 Verificamos si la conexión es correcta ---
        bool correctConnection = rightIndexCorrect != null &&
                                 li < rightIndexCorrect.Length &&
                                 rightIndex == rightIndexCorrect[li];

        if (correctConnection)
        {
            currentPairs[li] = rightIndex;
            SetButtonState(leftSockets[li], selected: true);
            SetButtonState(rightSockets[rightIndex], selected: true);
            ClearLeftHighlights();

            // Dibuja el cable con color del socket
            var color = useLeftSocketColor
                ? GetSocketColor(leftSockets[li])
                : GetSocketColor(rightSockets[rightIndex]);
            color.a = alphaSelected;
            EnsureLineForPair(li, rightIndex, locked: false, colorOverride: color);

            if (IsSolved())
            {
                SetBulb(true);
                UpdateAllLines();
                Close();
                onSolved?.Invoke();
            }
        }
        else
        {
            // ❌ conexión incorrecta: feedback y NO se dibuja línea
            StartCoroutine(FlashWrong(rightSockets[rightIndex]));
            ClearLeftHighlights();
        }
    }

    System.Collections.IEnumerator FlashWrong(Button btn)
    {
        var img = btn.targetGraphic as Image;
        if (!img) yield break;

        Color original = img.color;
        img.color = Color.white;
        yield return new WaitForSeconds(0.15f);
        img.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        img.color = original;
    }

    bool IsSolved()
    {
        if (rightIndexCorrect == null || rightIndexCorrect.Length != leftSockets.Length) return false;
        if (currentPairs.Count != leftSockets.Length) return false;

        for (int i = 0; i < leftSockets.Length; i++)
        {
            if (!currentPairs.TryGetValue(i, out int r)) return false;
            if (rightIndexCorrect[i] != r) return false;
        }
        return true;
    }

    void SetBulb(bool on)
    {
        if (bulbImage) bulbImage.sprite = on ? bulbOn : bulbOff;
    }

    void ResetButtonVisuals()
    {
        foreach (var b in leftSockets) SetButtonState(b, normal: true);
        foreach (var b in rightSockets) SetButtonState(b, normal: true);
    }

    void HighlightOnly(Button target)
    {
        foreach (var b in leftSockets) SetButtonState(b, normal: true);
        SetButtonState(target, highlight: true);
    }

    void ClearLeftHighlights()
    {
        foreach (var b in leftSockets) SetButtonState(b, normal: true);
    }

    void SetButtonState(Button b, bool normal = false, bool highlight = false, bool selected = false)
    {
        if (normal) b.targetGraphic.canvasRenderer.SetAlpha(1f);
        if (highlight) b.targetGraphic.canvasRenderer.SetAlpha(0.8f);
        if (selected) b.targetGraphic.canvasRenderer.SetAlpha(0.6f);
    }

    // === Líneas ===
    RectTransform LRect(int i) => leftSockets[i].GetComponent<RectTransform>();
    RectTransform RRect(int j) => rightSockets[j].GetComponent<RectTransform>();

    void EnsureLineForPair(int leftIndex, int rightIndex, bool locked, Color? colorOverride = null)
    {
        if (!linesLayer || !linePrefab) return;

        if (!lineByLeft.TryGetValue(leftIndex, out var img) || img == null)
        {
            img = Instantiate(linePrefab, linesLayer);
            img.gameObject.SetActive(true);
            lineByLeft[leftIndex] = img;
        }

        Color baseColor = colorOverride ?? Color.white;
        if (locked && !Mathf.Approximately(lockedBrighten, 1f))
            baseColor = Brighten(baseColor, lockedBrighten);

        img.color = baseColor;

        var rt = img.rectTransform;
        rt.SetAsFirstSibling();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, lineThickness);

        UpdateLineBetween(rt, LRect(leftIndex), RRect(rightIndex));
    }

    void RemoveLineForLeft(int leftIndex)
    {
        if (lineByLeft.TryGetValue(leftIndex, out var img) && img)
            Destroy(img.gameObject);
        lineByLeft.Remove(leftIndex);
    }

    void UpdateAllLines()
    {
        foreach (var kv in currentPairs.ToList())
        {
            int li = kv.Key;
            int rj = kv.Value;
            if (li < 0 || li >= leftSockets.Length) continue;
            if (rj < 0 || rj >= rightSockets.Length) continue;

            bool locked = IsSolved();
            var c = useLeftSocketColor ? GetSocketColor(leftSockets[li])
                                       : GetSocketColor(rightSockets[rj]);
            c.a = locked ? alphaLocked : alphaSelected;
            if (locked && !Mathf.Approximately(lockedBrighten, 1f))
                c = Brighten(c, lockedBrighten);

            EnsureLineForPair(li, rj, locked, c);
        }
    }

    void UpdateLineBetween(RectTransform line, RectTransform from, RectTransform to)
    {
        if (!linesLayer) return;

        Vector2 a = WorldToLocal(from);
        Vector2 b = WorldToLocal(to);
        Vector2 dir = b - a;

        float len = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        line.anchoredPosition = a;
        line.localRotation = Quaternion.Euler(0, 0, angle);
        line.sizeDelta = new Vector2(len, line.sizeDelta.y);
    }

    Vector2 WorldToLocal(RectTransform rt)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(linesLayer, screen, null, out var local);
        return local;
    }

    // === Utilidades de color ===
    Color GetSocketColor(Button b)
    {
        var img = b.targetGraphic as Image;
        if (img) return img.color;
        var any = b.GetComponentInChildren<Image>();
        return any ? any.color : Color.white;
    }

    Color Brighten(Color c, float mult)
    {
        float a = c.a;
        c.r = Mathf.Clamp01(c.r * mult);
        c.g = Mathf.Clamp01(c.g * mult);
        c.b = Mathf.Clamp01(c.b * mult);
        c.a = a;
        return c;
    }
}
