using UnityEngine;

public class OrdenarPalabrasPuzzle : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    public GameObject puzzleUI; // El Canvas del minijuego

    private bool isActive = false;

    public string GetPrompt()
    {
        return "Presiona [E] para ordenar las palabras";
    }

    public void Interact()
    {
        if (puzzleUI == null)
        {
            Debug.LogWarning("⚠️ No se asignó el puzzleUI en OrdenarPalabrasPuzzle");
            return;
        }

        isActive = !isActive;
        puzzleUI.SetActive(isActive);

        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isActive;
        Time.timeScale = isActive ? 0f : 1f;
    }
}
