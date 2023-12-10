using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver: MonoBehaviour
{
    public void DelayedLoad(float time)
    {
        StartCoroutine(_DelayedLoad(time));
    }

    private IEnumerator _DelayedLoad(float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}