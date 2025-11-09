using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBounds : MonoBehaviour
{
    [Header("Map y Respawn automáticos (por tag)")]
    public string mapTag = "Map";         // tag del mapa
    public string respawnTag = "Respawn"; // tag del punto de respawn

    [Header("Fade visual (opcional)")]
    public UIFader screenFader;           // arrastra el ScreenFade (o se autolocaliza)
    public float fadeOutTime = 0.5f;
    public float fadeInTime = 0.5f;
    public float waitBlackSeconds = 0.3f; // tiempo que se mantiene oscuro antes del respawn

    [Header("Chequeo")]
    public float checkInterval = 1f;
    public float fallThresholdExtra = 5f; // margen debajo del mapa
    public bool autoDetectBounds = true;

    private Transform player;
    private GameObject mapObject;
    private Transform respawnPoint;
    private Vector3 minBounds;
    private Vector3 maxBounds;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        player = transform;
        DetectMapAndRespawn();
        InvokeRepeating(nameof(CheckBounds), checkInterval, checkInterval);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PlayerBounds] Escena cargada: {scene.name}");
        DetectMapAndRespawn();
    }

    // Detectar mapa y punto de respawn automáticamente
    void DetectMapAndRespawn()
    {
        // Buscar mapa
        mapObject = GameObject.FindGameObjectWithTag(mapTag);
        if (mapObject && autoDetectBounds)
            CalculateMapBounds();

        // Buscar respawn
        var respawnGO = GameObject.FindGameObjectWithTag(respawnTag);
        if (respawnGO)
            respawnPoint = respawnGO.transform;

        // Buscar screen fade (si no está asignado)
        if (!screenFader)
            screenFader = FindObjectOfType<UIFader>(true);

        if (!mapObject)
            Debug.LogWarning("[PlayerBounds] ⚠️ No se detectó objeto con tag 'Map'.");
        if (!respawnPoint)
            Debug.LogWarning("[PlayerBounds] ⚠️ No se detectó objeto con tag 'Respawn'.");
    }

    // Calcular los límites del mapa
    void CalculateMapBounds()
    {
        Bounds bounds = new Bounds(mapObject.transform.position, Vector3.zero);

        Renderer[] renderers = mapObject.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        Collider[] colliders = mapObject.GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            bounds.Encapsulate(c.bounds);

        minBounds = bounds.min;
        maxBounds = bounds.max;

        Debug.Log($"[PlayerBounds] Límites detectados: {minBounds} → {maxBounds}");
    }

    // Chequear si el jugador sale de los límites o cae
    void CheckBounds()
    {
        if (player == null) return;
        if (mapObject == null) return;

        Vector3 pos = player.position;
        float fallThreshold = minBounds.y - fallThresholdExtra;

        if (pos.y < fallThreshold ||
            pos.x < minBounds.x || pos.x > maxBounds.x ||
            pos.z < minBounds.z || pos.z > maxBounds.z)
        {
            Debug.LogWarning("[PlayerBounds] Player fuera de límites → Respawn");
            StartCoroutine(FadeAndRespawn());
        }
    }

    System.Collections.IEnumerator FadeAndRespawn()
    {
        if (screenFader)
        {
            screenFader.FadeOut(fadeOutTime);
            yield return new WaitForSeconds(fadeOutTime + waitBlackSeconds);
        }

        RespawnPlayer();

        if (screenFader)
        {
            screenFader.FadeIn(fadeInTime);
        }
    }

    void RespawnPlayer()
    {
        if (!respawnPoint)
        {
            Debug.LogError("[PlayerBounds] No se encontró punto de respawn.");
            return;
        }

        player.position = respawnPoint.position;
        player.rotation = respawnPoint.rotation;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("[PlayerBounds] Jugador respawneado con éxito.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 size = maxBounds - minBounds;
        if (size.magnitude > 0)
            Gizmos.DrawWireCube((minBounds + maxBounds) / 2, size);
    }
}
