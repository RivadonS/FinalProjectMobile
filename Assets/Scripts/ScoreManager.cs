using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Current Run Stats")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int killCount = 0;

    [Header("In-Game HUD UI")]
    [SerializeField] private TMP_Text scoreText;     // โชว์คะแนนมุมจอบน
    [SerializeField] private TMP_Text killCountText; // (ถ้ามี) โชว์จำนวนมอนสเตอร์ที่ฆ่าได้

    // เปิดให้สคริปต์อื่น (เช่น PlayerHealth) ดึงค่าไปอ่านได้ แต่แก้ตรงๆ ไม่ได้
    public int CurrentScore => currentScore;
    public int KillCount => killCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateHUD();
    }

    // เรียกใช้เมื่อฆ่ามอนสเตอร์ได้ (บวกทั้งจำนวน Kill และคะแนน)
    public void AddKill(int scoreReward)
    {
        killCount++;
        currentScore += scoreReward;
        UpdateHUD();
    }

    // เรียกใช้กรณีได้คะแนนจากอย่างอื่นที่ไม่ใช่การฆ่า (เช่น เก็บไอเทม, ผ่านด่าน)
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }

        if (killCountText != null)
        {
            killCountText.text = $"Kills: {killCount}";
        }
    }
}