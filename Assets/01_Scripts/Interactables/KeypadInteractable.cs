using UnityEngine;

public class KeypadInteractable : MonoBehaviour, IInteractable
{
    [Header("UI Prefab")]
    public GameObject keypadPrefab; // arrastra el prefab del Canvas

    [Header("Opcional")]
    public EnergyNodeInteractable targetNode; // para encender al acertar

    bool _busy;

    public string GetPrompt()
    {
        return _busy ? "" : "Activar nodo de energía";
    }

    public void Interact()
    {
        if (_busy || keypadPrefab == null) return;
        _busy = true;

        var ui = Instantiate(keypadPrefab); // Canvas Overlay
        var keypad = ui.GetComponentInChildren<KeypadPuzzle>(true);

        keypad.OnSolved = () =>
        {
            if (targetNode) targetNode.UpdateVisual();
            _busy = false;
        };
        keypad.OnClosed = () =>
        {
            _busy = false;
        };
    }
}
