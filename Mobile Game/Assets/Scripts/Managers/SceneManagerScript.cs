using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public static SceneManagerScript sceneManager;

    public void Awake()
    {
        if (sceneManager != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            sceneManager = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void ChangeScene(string newScene)
    {
        StartCoroutine(LoadingScene(newScene));
    }

    private IEnumerator LoadingScene(string scene)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Loading");
        yield return new WaitForSeconds(1f);
        AsyncOperation asycOp = SceneManager.LoadSceneAsync(scene);
        while (!asycOp.isDone)
        {
            yield return null;
        }
        yield return null;
    }

    private void OnEnable()
    {
        MyEventManager.SceneManagement.OnLoadNewScene += ChangeScene;
    }

    private void OnDisable()
    {
        MyEventManager.SceneManagement.OnLoadNewScene -= ChangeScene;
    }
}
