using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public void LoadLevelScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
