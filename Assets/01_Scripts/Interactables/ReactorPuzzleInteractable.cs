using UnityEngine;

public class ReactorPuzzleInteractable : MonoBehaviour, IInteractable
{
    public GameObject reactorPrefab;           // UI_EnergyAlignPuzzle.prefab
    public EnergyNodeInteractable targetNode;  // opcional

    bool busy;
    public string GetPrompt() => busy ? "" : "Alinear energía";

    public void Interact()
    {
        if (busy || !reactorPrefab) return;
        busy = true;

        var ui = GameObject.Instantiate(reactorPrefab);
        var p = ui.GetComponentInChildren<ReactorAlignPuzzle>(true);
        p.OnSolved = () => { if (targetNode) targetNode.UpdateVisual(); busy = false; };
        p.OnClosed = () => { busy = false; };
    }
}
