using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMapNodes : MonoBehaviour
{
    [Header("Referencias")]
    public RectTransform minimapRect;
    public Camera minimapCamera;
    public GameObject nodeIconPrefab;
    public GameObject portalIconPrefab;   // 🌀 Nuevo: ícono del portal
    public Transform portalTransform;     // 🌀 Nuevo: referencia al portal real en el mundo

    private List<Transform> nodes = new List<Transform>();
    private List<Image> nodeIcons = new List<Image>();
    private Dictionary<Transform, Image> nodeToIcon = new Dictionary<Transform, Image>();

    private GameObject portalIconInstance;
    private bool portalVisible = false;

    void Start()
    {
        // 🔹 Buscar nodos
        GameObject[] nodeObjects = GameObject.FindGameObjectsWithTag("Node");
        foreach (var node in nodeObjects)
        {
            nodes.Add(node.transform);

            GameObject icon = Instantiate(nodeIconPrefab, minimapRect);
            icon.SetActive(true);

            Image iconImage = icon.GetComponent<Image>();
            iconImage.color = Color.red; // color inicial
            nodeIcons.Add(iconImage);
            nodeToIcon[node.transform] = iconImage;
        }

        // 🔹 Crear ícono del portal (inicialmente oculto)
        if (portalIconPrefab && portalTransform)
        {
            portalIconInstance = Instantiate(portalIconPrefab, minimapRect);
            portalIconInstance.SetActive(false);
        }
    }

    void LateUpdate()
    {
        // Actualizar posiciones de nodos
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == null) continue;

            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(nodes[i].position);
            float x = (viewportPos.x - 0.5f) * minimapRect.rect.width;
            float y = (viewportPos.y - 0.5f) * minimapRect.rect.height;
            nodeIcons[i].rectTransform.anchoredPosition = new Vector2(x, y);
        }

        // Actualizar posición del portal si ya está visible
        if (portalVisible && portalTransform && portalIconInstance)
        {
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(portalTransform.position);
            float x = (viewportPos.x - 0.5f) * minimapRect.rect.width;
            float y = (viewportPos.y - 0.5f) * minimapRect.rect.height;
            portalIconInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }
    }

    public void SetNodeColor(Transform nodeTransform, Color newColor)
    {
        if (nodeToIcon.ContainsKey(nodeTransform))
        {
            nodeToIcon[nodeTransform].color = newColor;
        }

        CheckAllNodesActivated();
    }

    // 🌀 Nuevo: revisa si todos los nodos están activados
    private void CheckAllNodesActivated()
    {
        bool allActivated = true;

        foreach (var icon in nodeIcons)
        {
            if (icon.color != Color.gray) // si alguno no está gris aún
            {
                allActivated = false;
                break;
            }
        }

        if (allActivated && !portalVisible)
        {
            ShowPortalOnMinimap();
        }
    }

    private void ShowPortalOnMinimap()
    {
        if (portalIconInstance)
        {
            portalIconInstance.SetActive(true);
            portalVisible = true;
            Debug.Log("[Minimap] Portal mostrado en el minimapa ✅");
        }
    }
}
