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
    [Tooltip("Text hiển thị số mạng (Hỗ trợ TextMeshPro)")]
    public TextMeshProUGUI livesText;
    [Tooltip("Text hiển thị số mạng (Hỗ trợ UI Text thường)")]
    public Text livesTextLegacy;

    [Header("Boss Health Bar UI")]
    [Tooltip("Thanh Slider máu của Boss")]
    public Slider bossHealthSlider;
    [Tooltip("Panel/GameObject chứa thanh máu Boss để ẩn/hiện")]
    public GameObject bossHealthContainer;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public Text scoreTextLegacy;
    private int currentScore = 0;

    [Header("Game Over UI")]
    [Tooltip("Panel màn hình Game Over hiển thị khi thua")]
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (bossHealthContainer != null) bossHealthContainer.SetActive(false);
        UpdateScoreUI();
    }

    /// <summary>
    /// Cập nhật hiển thị số mạng Player (Gán vào Event OnLivesChanged của PlayerHealth).
    /// </summary>
    public void UpdateLivesUI(int currentLives)
    {
        string textContent = "Lives: " + currentLives;
        if (livesText != null) livesText.text = textContent;
        if (livesTextLegacy != null) livesTextLegacy.text = textContent;
    }

    /// <summary>
    /// Cập nhật thanh máu Boss (Gán vào Event OnHealthChanged của EnemyHealth).
    /// </summary>
    public void UpdateBossHealthUI(float currentHP, float maxHP)
    {
        if (bossHealthSlider != null)
        {
            if (bossHealthContainer != null && !bossHealthContainer.activeSelf)
            {
                bossHealthContainer.SetActive(true);
            }

            bossHealthSlider.maxValue = maxHP;
            bossHealthSlider.value = currentHP;

            // Tự ẩn thanh máu khi Boss bị tiêu diệt
            if (currentHP <= 0f && bossHealthContainer != null)
            {
                bossHealthContainer.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Cộng điểm số khi tiêu diệt Enemy.
    /// </summary>
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        string scoreFormatted = "Score: " + currentScore.ToString("D6");
        if (scoreText != null) scoreText.text = scoreFormatted;
        if (scoreTextLegacy != null) scoreTextLegacy.text = scoreFormatted;
    }

    /// <summary>
    /// Hiển thị màn hình Game Over (Gán vào Event OnGameOver của PlayerHealth).
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hàm chơi lại màn chơi (Gán vào nút Restart trong Game Over Panel).
    /// </summary>
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
