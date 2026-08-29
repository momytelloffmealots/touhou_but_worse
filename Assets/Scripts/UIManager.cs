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

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateScoreUI();

        // 1. Tự tìm Player và lắng nghe sự kiện Mạng & Game Over
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Đăng ký nhận sự kiện cập nhật Mạng
            playerHealth.OnLivesChanged.AddListener(UpdateLivesUI);
            // Đăng ký nhận sự kiện Game Over
            playerHealth.OnGameOver.AddListener(ShowGameOver);

            // Khởi tạo hiển thị mạng ban đầu
            UpdateLivesUI(playerHealth.currentLives);
        }

        // 2. Tự tìm Boss/Enemy và lắng nghe thanh máu Boss (nếu có Boss trong Scene lúc đầu)
        EnemyHealth bossHealth = FindObjectOfType<EnemyHealth>();
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.AddListener(UpdateBossHealthUI);
        }
    }
    /// Cập nhật hiển thị số mạng Player (Gán vào Event OnLivesChanged của PlayerHealth).
    public void UpdateLivesUI(int currentLives)
    {
        string textContent = "Lives: " + currentLives;
        if (livesText != null) livesText.text = textContent;
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
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
