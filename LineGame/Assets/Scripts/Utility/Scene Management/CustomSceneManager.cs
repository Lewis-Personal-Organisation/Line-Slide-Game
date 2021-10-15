using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    public Scenes sceneToLoad;

    public CanvasGroup transparencyCanvasGroup;

    private AsyncOperation sceneLoadingOperation;

    // The time it will take to fade in and out
    public float fadeTime = 1;
    // The length of time between fading (How long our canvas group is visible)
    public float visibleTime = 0.4F;

    private bool fadingIsComplete;


    private void Start()
    {
        PreCheck();

        StartCoroutine(LoadSceneAsync());
    }

    public void PreCheck()
    {
        if (fadeTime == 0)
        {
            Debug.LogError("Fade Speed is set to 0, Loading would have never completed!! Changed to 1.");
            fadeTime = 1;
        }

        if (transparencyCanvasGroup == null)
        {
            Debug.LogError("Splash Canvas Group is null, assign it in the inspector!");
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public IEnumerator LoadSceneAsync()
    {
        fadingIsComplete = false;
        sceneLoadingOperation = SceneManager.LoadSceneAsync((int)sceneToLoad);
        sceneLoadingOperation.allowSceneActivation = false;

        StartCoroutine(FadeStartupScreen(visibleTime));
        
        yield return new WaitUntil(() => fadingIsComplete);

        sceneLoadingOperation.allowSceneActivation = true;
    }

    public IEnumerator FadeStartupScreen(float _waitTimeBetweenFades)
    {
        while (transparencyCanvasGroup.alpha < 1)
        {
            transparencyCanvasGroup.alpha += ((1 / fadeTime) * Time.deltaTime);// Time.deltaTime * fadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(_waitTimeBetweenFades);

        while (transparencyCanvasGroup.alpha > 0)
        {
            transparencyCanvasGroup.alpha -= (Time.deltaTime * fadeTime);
            yield return new WaitForEndOfFrame();
        }

        fadingIsComplete = true;
    }
}


// This occurs when the scene has finished loading. Unused but could be usefull
//SceneManager.sceneLoaded += OnSceneLoaded;