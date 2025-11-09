using UnityEngine;
using System.Collections;
using System.Reflection;

public class PlayerDeathHandler : MonoBehaviour
{
    private Transform playerRoot;

    [Header("Referencias")]
    public PlayerHealth playerHealth;
    public PlayerMotor3D playerMotor;
    public Transform respawnPoint;
    public GameObject deathScreen; // Panel negro “HAS MUERTO”

    [Header("Configuración")]
    public float respawnDelay = 3f;

    private bool isDead = false;
    private bool wasPausedByPuzzle = false;

    void Start()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null) playerRoot = playerHealth.transform.root;

        if (playerHealth != null)
        {
            playerHealth.onDeath.RemoveListener(OnPlayerDeath);
            playerHealth.onDeath.AddListener(OnPlayerDeath);
        }

        if (deathScreen != null) deathScreen.SetActive(false);
    }

    public void OnPlayerDeath()
    {
        if (isDead) return;
        isDead = true;

        // ¿El puzzle pausó el juego?
        wasPausedByPuzzle = (Time.timeScale <= 0f);
        if (wasPausedByPuzzle) Time.timeScale = 1f;

        // Cerrar puzzles abiertos (no bloquea futuras aperturas)
        ForceCloseActivePuzzles();

        if (deathScreen != null) deathScreen.SetActive(true);

        if (playerMotor == null) playerMotor = FindObjectOfType<PlayerMotor3D>();
        if (playerMotor != null) playerMotor.enabled = false;

        // Desactivar entrada/control explícitamente
        SetBoolIfExists(playerMotor, "inputEnabled", false);
        SetBoolIfExists(playerMotor, "control", false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // Tiempo real: funciona aunque el puzzle haya puesto timeScale=0
        yield return new WaitForSecondsRealtime(respawnDelay);

        // Restaurar vida
        if (playerHealth != null)
            playerHealth.currentHealth = playerHealth.maxHealth;

        // Teletransporte robusto
        WarpToRespawn();

        // Reactivar motor y **ambos** flags
        if (playerMotor == null) playerMotor = FindObjectOfType<PlayerMotor3D>();
        if (playerMotor != null) playerMotor.enabled = true;
        SetBoolIfExists(playerMotor, "inputEnabled", true);
        SetBoolIfExists(playerMotor, "control", true);

        // UI / Cursor
        if (deathScreen != null) deathScreen.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 🔒 Siempre reanudar el juego
        Time.timeScale = 1f;

        isDead = false;
    }

    void WarpToRespawn()
    {
        if (respawnPoint == null)
        {
            Debug.LogError("⚠️ RespawnPoint no asignado.");
            return;
        }

        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        Transform root = playerHealth ? playerHealth.transform.root : null;
        if (root == null)
        {
            Debug.LogError("⚠️ No se encontró la raíz del Player.");
            return;
        }

        var cc = root.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        var rb = root.GetComponent<Rigidbody>();
        bool hadRb = rb != null;
        if (hadRb)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        root.position = respawnPoint.position;
        root.rotation = respawnPoint.rotation;

        if (hadRb) rb.isKinematic = false;
        if (cc != null) cc.enabled = true;
    }

    // ——— Utilidades ———
    void ForceCloseActivePuzzles()
    {
        var behaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            var t = b.GetType();
            if (!t.Name.Contains("Puzzle")) continue;

            bool isOpen = false;

            // Propiedad IsOpen (si existe)
            var prop = t.GetProperty("IsOpen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                try { isOpen = (bool)prop.GetValue(b); } catch { isOpen = false; }
            }

            // Heurística: ¿canvas activo en hijos?
            if (!isOpen)
            {
                foreach (var c in (b as Component).GetComponentsInChildren<Canvas>(true))
                {
                    if (c != null && c.gameObject.activeInHierarchy) { isOpen = true; break; }
                }
            }

            if (!isOpen) continue;

            // Cerrar con Close(false) o Close()
            try
            {
                var mBool = t.GetMethod("Close", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
                if (mBool != null) { mBool.Invoke(b, new object[] { false }); continue; }

                var mNoArgs = t.GetMethod("Close", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
                if (mNoArgs != null) { mNoArgs.Invoke(b, null); }
            }
            catch { /* ignorar */ }
        }
    }

    void SetBoolIfExists(object target, string name, bool value)
    {
        if (target == null) return;

        // Campo público
        var fPub = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (fPub != null && fPub.FieldType == typeof(bool)) { fPub.SetValue(target, value); return; }

        // Campo privado/protegido
        var fPriv = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (fPriv != null && fPriv.FieldType == typeof(bool)) { fPriv.SetValue(target, value); return; }

        // Propiedad
        var p = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(bool) && p.CanWrite) { p.SetValue(target, value); }
    }
}
