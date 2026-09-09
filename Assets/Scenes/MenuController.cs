using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuController : MonoBehaviour
{
    // Chuyển sang Scene SampleScene
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Dừng chế độ Play trong Editor hoặc Thoát game khi đã Build
    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Thoát chế độ Play trong Unity Editor
#else
            Application.Quit(); // Thoát ứng dụng thật khi đã build
#endif
    }
}