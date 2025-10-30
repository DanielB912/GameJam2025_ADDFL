using UnityEngine;

public class SopaInteractable : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    public GameObject sopaUI;     // Canvas o panel del minijuego
    public Transform player;      // Jugador (para distancia opcional)
    public float activationDistance = 3f;

    private bool isActive = false;

    public string GetPrompt()
    {
        return "Presiona [E] para abrir la Sopa de Letras";
    }

    public void Interact()
    {
        if (sopaUI == null)
        {
            Debug.LogWarning("⚠️ No se asignó el objeto del minijuego (sopaUI)");
            return;
        }

        ToggleSopa();
    }

    private void ToggleSopa()
    {
        isActive = !isActive;
        sopaUI.SetActive(isActive);

        // Bloquea o libera el cursor
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isActive;

        // Pausa o reanuda el tiempo del juego
        Time.timeScale = isActive ? 0f : 1f;
    }
}