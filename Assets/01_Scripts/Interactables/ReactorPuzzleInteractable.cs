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

        var ui = Instantiate(reactorPrefab);
        var puzzle = ui.GetComponentInChildren<ReactorAlignPuzzle>(true);

        // 🔌 Pasamos el nodo al puzzle
        if (targetNode != null)
            puzzle.SetTargetNode(targetNode);

        puzzle.OnSolved = () =>
        {
            if (targetNode)
            {
                var method = targetNode.GetType().GetMethod("SetState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(targetNode, new object[] { true });
            }

            busy = false;
        };

        puzzle.OnClosed = () =>
        {
            busy = false;
        };
    }
}