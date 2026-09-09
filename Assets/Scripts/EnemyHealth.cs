using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using _Project._Scripts.Gameplay.Projectiles;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Configuration")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Visual Feedback")]
    [Tooltip("SpriteRenderer của Enemy để tạo hiệu ứng nháy khi trúng đạn")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float flashDuration = 0.08f;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged; // Gửi (currentHP, maxHP) cho UI Boss
    public UnityEvent OnEnemyDeath;

    private Color originalColor;
    private Coroutine flashCoroutine;
    [Header("Score Settings")]
    public int scoreValue = 100; // Số điểm nhận được khi diệt Enemy này

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    /// Trừ HP của Enemy khi trúng sát thương.
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Hiệu ứng nháy màu khi trúng đạn
        if (spriteRenderer != null && gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }


    private void Die()
    {
        OnEnemyDeath?.Invoke();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddScore(scoreValue);
        }

        // Ẩn Enemy khi tiêu diệt (hoặc trả về ObjectPool)
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Va chạm với đạn của Player (Tag "PlayerBullet" hoặc có component Bullet)
        if (collision.CompareTag("PlayerBullet") || collision.GetComponent<Bullet>() != null)
        {
            float damage = 1f;

            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null)
            {
                damage = bullet.Damage;
            }

            TakeDamage(damage);

            // Trả đạn về Pool / ẩn đạn
            collision.gameObject.SetActive(false);
        }
    }
}
