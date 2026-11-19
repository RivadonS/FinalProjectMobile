using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Unity Cloud Settings")]
    [SerializeField] private string environmentName = "development";
    [SerializeField] private string leaderboardId = "High_Score";
    [SerializeField] private int maxEntries = 10;

    [Header("UI Panels & Prefabs")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform contentParent; // ลาก Content ใน Scroll View มาใส่
    [SerializeField] private GameObject rowPrefab;    // ลาก Prefab LeaderboardRow มาใส่

    [Header("Buttons")]
    [SerializeField] private Button openLeaderboardButton;
    [SerializeField] private Button closeLeaderboardButton;

    [Header("Player Name (Optional - ถ้ายังไม่ได้สร้างเว้นว่างไว้ได้)")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button saveNameButton;

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.RebindUI(leaderboardPanel, contentParent, rowPrefab, openLeaderboardButton, closeLeaderboardButton, nameInputField, saveNameButton);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BindButtonEvents();
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    private async void Start()
    {
        await InitializeUnityServicesAsync();
    }

    private void RebindUI(GameObject panel, Transform content, GameObject prefab, Button openBtn, Button closeBtn, TMP_InputField input, Button saveBtn)
    {
        leaderboardPanel = panel;
        contentParent = content;
        rowPrefab = prefab;
        openLeaderboardButton = openBtn;
        closeLeaderboardButton = closeBtn;
        nameInputField = input;
        saveNameButton = saveBtn;

        BindButtonEvents();
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    private void BindButtonEvents()
    {
        if (openLeaderboardButton != null)
        {
            openLeaderboardButton.onClick.RemoveAllListeners();
            openLeaderboardButton.onClick.AddListener(OpenLeaderboard);
        }

        if (closeLeaderboardButton != null)
        {
            closeLeaderboardButton.onClick.RemoveAllListeners();
            closeLeaderboardButton.onClick.AddListener(CloseLeaderboard);
        }

        if (saveNameButton != null)
        {
            saveNameButton.onClick.RemoveAllListeners();
            saveNameButton.onClick.AddListener(OnSaveNameClicked);
        }
    }

    private async Task InitializeUnityServicesAsync()
    {
        try
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName(environmentName);
            await UnityServices.InitializeAsync(options);

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"<color=green>[Leaderboard Auth]</color> ล็อกอินสำเร็จ ID: {AuthenticationService.Instance.PlayerId}");
            }

            // บังคับทดสอบเรียกตัวโมดูล Leaderboards ทันทีหลัง Init
            var testInstance = LeaderboardsService.Instance;
            isInitialized = (testInstance != null);

            if (isInitialized)
            {
                Debug.Log("<color=green>[Leaderboard]</color> โมดูล Leaderboards พร้อมใช้งานแล้ว!");
            }

            if (nameInputField != null && !string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
            {
                nameInputField.text = AuthenticationService.Instance.PlayerName;
            }
        }
        catch (Exception e)
        {
            isInitialized = false;
            Debug.LogError($"[Leaderboard Error] โมดูล Leaderboards ไม่พร้อมทำงาน ({e.GetType().Name}): {e.Message}");
        }
    }

    // --- 1. เปิดหน้าต่างและดึงข้อมูลจาก Cloud ---
    public async void OpenLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);

        if (!isInitialized)
        {
            Debug.LogWarning("[Leaderboard] ระบบกำลังเชื่อมต่อ Cloud กรุณารอสักครู่...");
            return;
        }

        await RefreshLeaderboardUI();
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public async Task RefreshLeaderboardUI()
    {
        try
        {
            // ล้างแถวเก่าออกก่อน
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            // ดึงคะแนน Top 10 จาก Unity Cloud
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = maxEntries }
            );

            foreach (LeaderboardEntry entry in scoresResponse.Results)
            {
                GameObject row = Instantiate(rowPrefab, contentParent);

                // ค้นหา TextMeshPro ทั้ง 3 ช่องจากชื่อวัตถุลูก
                TMP_Text rankText = row.transform.Find("RankText")?.GetComponent<TMP_Text>();
                TMP_Text nameText = row.transform.Find("NameText")?.GetComponent<TMP_Text>();
                TMP_Text scoreText = row.transform.Find("ScoreText")?.GetComponent<TMP_Text>();

                // Rank ของ Unity เริ่มนับที่ 0 จึงต้อง +1
                if (rankText != null) rankText.text = $"#{entry.Rank + 1}";
                if (nameText != null) nameText.text = entry.PlayerName;
                if (scoreText != null) scoreText.text = entry.Score.ToString("N0");
            }

            Debug.Log($"<color=cyan>[Leaderboard]</color> โหลดข้อมูลสำเร็จทั้งหมด {scoresResponse.Results.Count} อันดับ");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Leaderboard Error] ดึงข้อมูลล้มเหลว: {e.Message}");
        }
    }

    // --- 2. ส่งคะแนนขึ้น Cloud (เรียกใช้ตอนจบเกมหรือผู้เล่นตาย) ---
    public async void SubmitScore(int score)
    {
        if (!isInitialized)
        {
            await InitializeUnityServicesAsync();
        }

        // ถ้ายังเชื่อมต่อไม่สำเร็จ ให้หยุดส่งคะแนนทันทีเพื่อป้องกัน Error ซ้ำซ้อน
        if (!isInitialized) return;

        try
        {
            var entry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log($"<color=green>[Leaderboard]</color> ส่งคะแนน {score} สำเร็จ! อันดับปัจจุบัน: #{entry.Rank + 1}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Leaderboard Error] ส่งคะแนนล้มเหลว: {e.Message}");
        }
    }

    // --- 3. เปลี่ยนชื่อผู้เล่น ---
    private async void OnSaveNameClicked()
    {
        if (!isInitialized || nameInputField == null) return;

        // กฎของ Unity Auth: ห้ามมีเว้นวรรคในชื่อ
        string cleanName = nameInputField.text.Trim().Replace(" ", "_");
        if (string.IsNullOrEmpty(cleanName)) return;

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(cleanName);
            Debug.Log($"<color=green>[Leaderboard]</color> เปลี่ยนชื่อผู้เล่นเป็น: {AuthenticationService.Instance.PlayerName}");
            await RefreshLeaderboardUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Leaderboard Error] เปลี่ยนชื่อไม่สำเร็จ: {e.Message}");
        }
    }

    // --- เครื่องมือทดสอบใน Inspector ---
    [ContextMenu("Debug: ส่งคะแนนทดสอบ (1,500 คะแนน)")]
    private void TestSubmitScore()
    {
        SubmitScore(1500);
    }
}