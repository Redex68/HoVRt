using System.Collections;
using UnityEngine;

public class GameOver: MonoBehaviour
{
    public void DelayedLoad(float time)
    {
        StartCoroutine(_DelayedLoad(time));
    }

    private IEnumerator _DelayedLoad(float time)
    {
        yield return new WaitForSeconds(time);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Main Scene");
    }
}