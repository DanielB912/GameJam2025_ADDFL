using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Regeneración")]
    public float regenInterval = 30f; // cada 30 segundos
    public int regenAmount = 1; // 1 punto por tick
    private float regenTimer = 0f;

    [Header("Eventos")]
    public UnityEvent onDeath;
    public UnityEvent onDamage;
    public UnityEvent onHeal;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // regeneración automática
        regenTimer += Time.deltaTime;
        if (regenTimer >= regenInterval && currentHealth < maxHealth)
        {
            Heal(regenAmount);
            regenTimer = 0f;
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        onDamage?.Invoke();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            onDeath?.Invoke(); // ⚡ dispara el evento inmediatamente
        }

        Debug.Log($"Jugador recibió daño: -{amount} | Vida actual: {currentHealth}");
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        onHeal?.Invoke();
        Debug.Log($"Jugador recuperó vida: +{amount} | Vida actual: {currentHealth}");
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}
