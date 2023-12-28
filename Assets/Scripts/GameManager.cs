using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const int target_FPS = 60;

    void Start()
    {
        QualitySettings.vSyncCount = 0;  
        Application.targetFrameRate = target_FPS;
    }
    public void Restart()
    {
        SceneManager.LoadScene(1); 
    }
}
