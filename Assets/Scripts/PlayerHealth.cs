using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý Mạng (Lives), va chạm với đạn Enemy (EnemyBullet), và trạng thái Bất tử (Invincibility / I-frames) cho Player.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Lives Configuration")]
    public int maxLives = 3;
    public int currentLives;

    [Header("Invincibility Settings")]
    [Tooltip("Thời gian bất tử sau khi bị trúng đạn (giây)")]
    public float invincibilityDuration = 2f;
    public float flashInterval = 0.1f;

    [Header("Visual Components")]
    public SpriteRenderer spriteRenderer;

    [Header("Events")]
    public UnityEvent<int> OnLivesChanged; // Gửi số mạng còn lại cho UI
    public UnityEvent OnPlayerHit;
    public UnityEvent OnGameOver;

    private bool isInvincible = false;
    private Color originalColor;

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

    void Start()
    {
        currentLives = maxLives;
        OnLivesChanged?.Invoke(currentLives);
    }

    /// <summary>
    /// Trừ 1 mạng của Player.
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        // Nếu đang trong thời gian bất tử thì không nhận sát thương
        if (isInvincible || currentLives <= 0) return;

        currentLives -= damage;
        currentLives = Mathf.Max(0, currentLives);

        OnLivesChanged?.Invoke(currentLives);
        OnPlayerHit?.Invoke();

        if (currentLives > 0)
        {
            StartCoroutine(InvincibilityRoutine());
        }
        else
        {
            Die();
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;

        // Hiệu ứng chớp tắt Player trong thời gian bất tử
        while (timer < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }
        isInvincible = false;
    }

    private void Die()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        OnGameOver?.Invoke();
        Debug.Log("[PlayerHealth] Player đã hết mạng - Game Over!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Xử lý va chạm với đạn Enemy (kiểm tra component EnemyBullet hoặc Tag "EnemyBullet")
        EnemyBullet bullet = collision.GetComponent<EnemyBullet>();
        if (bullet != null || collision.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);

            // Trả đạn về Pool khi trúng Player
            if (bullet != null)
            {
                bullet.ReturnToPool();
            }
            else
            {
                collision.gameObject.SetActive(false);
            }
        }
        // Hoặc khi đụng trực tiếp vào thân thể Enemy (Tag "Enemy")
        else if (collision.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    public bool IsInvincible => isInvincible;
}
