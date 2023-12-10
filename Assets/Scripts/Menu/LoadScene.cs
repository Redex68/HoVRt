using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] StringVariable SceneName;
    public void LoadDaScene()
    {
        SceneManager.LoadSceneAsync(SceneName.value);
    }
}
