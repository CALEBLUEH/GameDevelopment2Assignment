using UnityEngine;
using System;
using UnityEngine.UI;

public class EnemyStatus : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth;
    [SerializeField] protected float healthBarYOffset = 2f;

    [Header("References")]
    [SerializeField] protected GameObject healthBarPrefab;

    protected int currentHealth;
    protected GameObject healthBarInstance;
    protected Slider healthSlider;

    public event Action OnDestroyed;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        if (healthBarPrefab == null) return;

        healthBarInstance = Instantiate(healthBarPrefab, transform);
        healthBarInstance.transform.localPosition = new Vector3(0, healthBarYOffset, 0);

        healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
        healthSlider.gameObject.SetActive(false);
        UpdateHealthBar();
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHealthDisplay();

        if (currentHealth <= 0) Die();
    }

    private void UpdateHealthDisplay()
    {
        UpdateHealthBar();
        UpdateHealthBarVisibility();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void UpdateHealthBarVisibility()
    {
        if (healthSlider == null) return;

        bool shouldShow = currentHealth < maxHealth;
        healthSlider.gameObject.SetActive(shouldShow);
    }

    protected virtual void Die()
    {
        OnDestroyed?.Invoke();
        Destroy(healthBarInstance);
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (healthBarInstance == null) return;

        healthBarInstance.transform.rotation = Camera.main.transform.rotation;

        healthBarInstance.transform.position = transform.position + Vector3.up * healthBarYOffset;
    }
}
