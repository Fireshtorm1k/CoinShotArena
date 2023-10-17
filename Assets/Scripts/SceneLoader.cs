using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public Slider loadingSlider;
    public static int SceneToLoad;
    private void Start()
    {
        StartLoading();
    }

    private void StartLoading()
    {
        StartCoroutine(LoadSceneAsync(SceneToLoad));
    }


    private IEnumerator LoadSceneAsync(int scene)
    {
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);

        while (!operation.isDone)
        {
            // The progress value is between 0 and 0.9 when loading. We scale it to be between 0 and 1.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            yield return null;
        }
    }
}
