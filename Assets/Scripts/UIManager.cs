using UnityEngine;
using UnityEngine.UI;
using TMPro; // Hỗ trợ cả TextMeshPro và UI Text mặc định của Unity

/// <summary>
/// Quản lý giao diện UI trong game: Mạng Player, Thanh máu Boss, Score và Màn hình Game Over.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Player Lives UI")]
    public TextMeshProUGUI livesText;

    [Header("Boss Health Bar UI")]
    public Slider bossHealthSlider;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;

    [Header("Scene References (Kéo trực tiếp vào Inspector)")]
    [Tooltip("Kéo PlayerHealth của Player vào đây")]
    public PlayerHealth playerHealth;

    [Tooltip("Kéo PlayerController của Player vào đây")]
    public PlayerController playerController;

    [Tooltip("Kéo PlayerShooting của Player vào đây")]
    public PlayerShooting playerShooting;

    [Tooltip("Kéo EnemyHealth của Boss vào đây")]
    public EnemyHealth bossHealth;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        IsGameOver = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateScoreUI();

        // 1. Lắng nghe sự kiện Mạng & Game Over của Player
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged.RemoveListener(UpdateLivesUI);
            playerHealth.OnLivesChanged.AddListener(UpdateLivesUI);

            playerHealth.OnGameOver.RemoveListener(ShowGameOver);
            playerHealth.OnGameOver.AddListener(ShowGameOver);

            // Khởi tạo hiển thị mạng ban đầu
            UpdateLivesUI(playerHealth.currentLives);
        }

        // 2. Lắng nghe thanh máu Boss
        if (bossHealth == null) bossHealth = FindObjectOfType<EnemyHealth>();
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.RemoveListener(UpdateBossHealthUI);
            bossHealth.OnHealthChanged.AddListener(UpdateBossHealthUI);
            UpdateBossHealthUI(bossHealth.currentHealth, bossHealth.maxHealth);
        }
    }

    /// <summary>
    /// Cập nhật hiển thị số mạng Player (Gán vào Event OnLivesChanged của PlayerHealth).
    /// </summary>
    public void UpdateLivesUI(int currentLives)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }

    /// <summary>
    /// Cập nhật thanh máu Boss (Gán vào Event OnHealthChanged của EnemyHealth).
    /// </summary>
    public void UpdateBossHealthUI(float currentHP, float maxHP)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHP;
            bossHealthSlider.value = currentHP;
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        string scoreFormatted = "Score: " + currentScore.ToString("D6");
        if (scoreText != null) scoreText.text = scoreFormatted;
    }

    public void ShowGameOver()
    {
        IsGameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Đảm bảo dừng cả Player khi Game Over xuất hiện
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetPlayerControl(false);
            playerController.enabled = false;
        }

        if (playerShooting == null) playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }
    }

    public void RestartGame()
    {
        IsGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
