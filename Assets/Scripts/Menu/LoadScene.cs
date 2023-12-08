using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] string SceneName = "MainScene";
    public void StartGame()
    {
        SceneManager.LoadSceneAsync(SceneName);
    }
}
