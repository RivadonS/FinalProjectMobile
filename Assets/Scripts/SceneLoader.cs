using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f; // กันเหนียวเผื่อเปลี่ยนฉากตอนเกมหยุดเวลาอยู่
        SceneManager.LoadScene(sceneName);
    }
}