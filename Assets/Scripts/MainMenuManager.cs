using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("พิมพ์ชื่อ Scene เกมหลักให้ตรงเป๊ะๆ")]
    [SerializeField] private string gameplaySceneName = "SampleScene";
    [Tooltip("พิมพ์ชื่อ Scene ร้านค้าให้ตรงเป๊ะๆ")]
    [SerializeField] private string shopSceneName = "IAP";

    [Header("UI Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;

    [Header("Option Panel & Sub-Buttons")]
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Button shopButton;        // ปุ่มเปิดหน้า Shop (IAP)
    [SerializeField] private Button closeOptionButton;

    // ตัวแปร Static เอาไว้ส่งสัญญาณข้าม Scene ว่าผู้เล่นกด "Load" หรือ "Start ใหม่"
    public static bool IsLoadingSavedRun { get; private set; } = false;

    private string saveFilePath;

    private void Awake()
    {
        // กำหนดที่อยู่ไฟล์เซฟสำหรับ Roguelike Run (เก็บในเครื่องผู้เล่น)
        saveFilePath = Path.Combine(Application.persistentDataPath, "run_save.json");
    }

    private void Start()
    {
        // 1. ผูกคำสั่งเข้ากับปุ่มต่างๆ
        startButton.onClick.AddListener(OnStartClicked);
        loadButton.onClick.AddListener(OnLoadClicked);
        optionButton.onClick.AddListener(OnOptionClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OnShopClicked);
        }

        if (closeOptionButton != null)
        {
            closeOptionButton.onClick.AddListener(OnCloseOptionClicked);
        }

        // 2. ปิดหน้าต่าง Option เผื่อเผลอเปิดค้างไว้
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }

        // 3. เช็กสถานะไฟล์เซฟเพื่อเปิด/ปิดปุ่ม Load
        CheckSaveData();
    }

    private void CheckSaveData()
    {
        bool hasSave = File.Exists(saveFilePath);
        
        // ถ้าไม่มีไฟล์เซฟ ปุ่ม Load จะกดไม่ได้ (และสีจะดรอปลงตามค่า Disabled Color ของ Button)
        loadButton.interactable = hasSave;
    }

    private void OnStartClicked()
    {
        IsLoadingSavedRun = false;

        // ลบเซฟเก่าที่เล่นค้างไว้ทิ้ง (ตามสไตล์ Roguelike เมื่อเริ่มรอบใหม่)
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnLoadClicked()
    {
        if (!File.Exists(saveFilePath)) return;

        IsLoadingSavedRun = true;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnOptionClicked()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(true);
        }
    }

    private void OnShopClicked()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    private void OnCloseOptionClicked()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("กำลังออกจากเกม...");
        Application.Quit();

        // คำสั่งเสริมเพื่อให้กดปุ่ม Quit แล้วหยุดเล่นในหน้า Unity Editor ได้จริง
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- เครื่องมือสำหรับเทสใน Inspector (คลิกขวาที่ชื่อสคริปต์แล้วกดเรียกใช้ได้เลย) ---
    [ContextMenu("Debug: สร้างไฟล์ Save จำลอง")]
    private void CreateDummySave()
    {
        File.WriteAllText(saveFilePath, "{\"playerHP\": 80, \"wave\": 3}");
        CheckSaveData();
        Debug.Log($"สร้างไฟล์เซฟจำลองที่: {saveFilePath}");
    }

    [ContextMenu("Debug: ลบไฟล์ Save จำลอง")]
    private void DeleteDummySave()
    {
        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        CheckSaveData();
        Debug.Log("ลบไฟล์เซฟจำลองเรียบร้อย");
    }
}