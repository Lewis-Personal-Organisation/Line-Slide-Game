using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    [HideInInspector] public int includedScenes = 0;
    [HideInInspector] public List<string> options = new List<string>();

    [HideInInspector] public int sceneIndex;

    public CanvasGroup transparencyCanvasGroup;

    private AsyncOperation sceneLoadingOperation;

    // The time it will take to fade in and out
    public float fadeTime = 1;
    // The length of time between fading (How long our canvas group is visible)
    public float visibleTime = 0.4F;


    private void Start()
    {
        PreCheck();
        StartCoroutine(LoadSceneAsync());
    }

    public void PreCheck()
    {
        if (fadeTime == 0)
        {
            Debug.LogError("Fade Speed can't be 0. Set to 1.");
            fadeTime = 1;
        }

        if (transparencyCanvasGroup == null)
        {
            Debug.LogError("Splash Canvas Group is null, assign in inspector.");
        }

        if (sceneIndex == -1)
		{
			Debug.LogError("No Scenes are included to build. Tick at least 1 Scene to allow transitioning in File > Build Settings");
		}

        DontDestroyOnLoad(this.gameObject);
    }

    public IEnumerator LoadSceneAsync()
    {
        Debug.Log($"Loading Scene {sceneIndex} with index {sceneIndex} at path {System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneIndex))}");
        sceneLoadingOperation = SceneManager.LoadSceneAsync(sceneIndex);
        sceneLoadingOperation.allowSceneActivation = false;

        yield return StartCoroutine(FadeStartupScreen(visibleTime));

        sceneLoadingOperation.allowSceneActivation = true;
    }

    public IEnumerator FadeStartupScreen(float waitTimeBetweenFades)
    {
        while (transparencyCanvasGroup.alpha < 1)
        {
            transparencyCanvasGroup.alpha += ((1 / fadeTime) * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(waitTimeBetweenFades);

        while (transparencyCanvasGroup.alpha > 0)
        {
            transparencyCanvasGroup.alpha -= (Time.deltaTime * fadeTime);
            yield return new WaitForEndOfFrame();
        }
    }
}


// This occurs when the scene has finished loading. Unused but could be usefull
//SceneManager.sceneLoaded += OnSceneLoaded;