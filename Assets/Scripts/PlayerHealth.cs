using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText; // โชว์คะแนนตอนตาย
    [SerializeField] private TMP_Text finalKillsText; // (ถ้ามี) โชว์จำนวนที่ฆ่าได้ตอนตาย
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;   // ปุ่มกลับไป StartScene

    [Header("Scene Settings")]
    [SerializeField] private string startSceneName = "StartScene";

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(ReturnToStartScene);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage! Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Died!");

        // 1. หยุดเวลาในเกม
        Time.timeScale = 0f;

        // 2. ดึงคะแนนและจำนวน Kill จาก ScoreManager
        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        int finalKills = ScoreManager.Instance != null ? ScoreManager.Instance.KillCount : 0;

        // 3. ส่งคะแนนขึ้น Unity Cloud (Leaderboard)
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SubmitScore(finalScore);
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] ไม่พบ LeaderboardManager (หากกด Play จากหน้านี้โดยตรงจะส่งคะแนนไม่ได้ ต้องเริ่มจาก StartScene)");
        }

        // 4. เปิดหน้าต่าง Game Over และแสดงผลสรุป
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Score: {finalScore:N0}";
        }

        if (finalKillsText != null)
        {
            finalKillsText.text = $"Kills: {finalKills}";
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToStartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }
}