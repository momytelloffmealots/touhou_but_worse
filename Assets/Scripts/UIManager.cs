using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý UI:
/// - Lives
/// - Boss Health
/// - Score
/// - YOU WIN / YOU LOSE
/// - Play Again
/// - Go Back Main Menu
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

    [Header("Scene References")]
    public PlayerHealth playerHealth;
    public PlayerController playerController;
    public PlayerShooting playerShooting;
    public EnemyHealth bossHealth;

    [Header("Game End UI")]
    public GameObject gameOverPanel;

    // Chữ YOU WIN / YOU LOSE
    public TextMeshProUGUI resultText;

    // Tên Scene Main Menu
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        IsGameOver = false;

        // Ẩn panel lúc bắt đầu game
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Score
        UpdateScoreUI();

        // =========================
        // PLAYER
        // =========================

        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged.RemoveListener(UpdateLivesUI);
            playerHealth.OnLivesChanged.AddListener(UpdateLivesUI);

            playerHealth.OnGameOver.RemoveListener(ShowLose);
            playerHealth.OnGameOver.AddListener(ShowLose);

            // Hiển thị mạng ban đầu
            UpdateLivesUI(playerHealth.currentLives);
        }

        // =========================
        // BOSS
        // =========================

        if (bossHealth == null)
            bossHealth = FindObjectOfType<EnemyHealth>();

        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.RemoveListener(UpdateBossHealthUI);
            bossHealth.OnHealthChanged.AddListener(UpdateBossHealthUI);

            UpdateBossHealthUI(
                bossHealth.currentHealth,
                bossHealth.maxHealth
            );
        }
    }

    // =====================================================
    // LIVES
    // =====================================================

    public void UpdateLivesUI(int currentLives)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }

    // =====================================================
    // BOSS HEALTH
    // =====================================================

    public void UpdateBossHealthUI(float currentHP, float maxHP)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHP;
            bossHealthSlider.value = currentHP;
        }
    }

    // =====================================================
    // SCORE
    // =====================================================

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString("D6");
        }
    }

    // =====================================================
    // YOU LOSE
    // =====================================================

    public void ShowLose()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

        if (resultText != null)
        {
            resultText.text = "YOU LOSE";
        }

        ShowGameEndPanel();
    }

    // =====================================================
    // YOU WIN
    // =====================================================

    public void ShowWin()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

        if (resultText != null)
        {
            resultText.text = "YOU WIN";
        }

        ShowGameEndPanel();
    }

    // =====================================================
    // HIỆN PANEL
    // =====================================================

    private void ShowGameEndPanel()
    {
        // Hiện panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Dừng Player
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            playerController.SetPlayerControl(false);
            playerController.enabled = false;
        }

        // Dừng bắn
        if (playerShooting == null)
            playerShooting = FindObjectOfType<PlayerShooting>();

        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }
    }

    // =====================================================
    // PLAY AGAIN
    // =====================================================

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // =====================================================
    // GO BACK MAIN MENU
    // =====================================================

    public void GoBackMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}