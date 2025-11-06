using UnityEngine;
using UnityEngine.UI;

public class MiniMapIcon : MonoBehaviour
{
    [Header("Referencias")]
    public RectTransform minimapRect;     // El RawImage o contenedor del minimapa
    public RectTransform playerIcon;      // La flecha
    public Transform player;              // El jugador en el mundo
    public Camera minimapCamera;          // La cámara del minimapa

    void LateUpdate()
    {
        if (!player || !minimapCamera || !playerIcon || !minimapRect) return;

        // Convierte la posición mundial del jugador a coordenadas de la cámara minimapa
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(player.position);

        // Posición del icono dentro del minimapa
        float x = (viewportPos.x - 0.5f) * minimapRect.rect.width;
        float y = (viewportPos.y - 0.5f) * minimapRect.rect.height;

        playerIcon.anchoredPosition = new Vector2(x, y);

        // Rotación del ícono según la orientación del jugador
        minimapRect.localEulerAngles = new Vector3(0, 0, player.eulerAngles.y);

    }
}
