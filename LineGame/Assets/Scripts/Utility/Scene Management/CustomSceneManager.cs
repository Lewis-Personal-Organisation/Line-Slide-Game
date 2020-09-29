using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    public Scenes sceneToLoad;

    public CanvasGroup splashCanvasGroup;
    public float splashScreenFadeSpeed = 1;
    public bool splashIsComplete;


    private void Start()
    {
        PreCheck();
        StartCoroutine(LoadSceneAsync());
    }

    public IEnumerator LoadSceneAsync()
    {
        // Load our next scene asyncronously in the background
        // But don't allow the scene to activate
        AsyncOperation _sceneLoading = SceneManager.LoadSceneAsync((int)sceneToLoad);
        _sceneLoading.allowSceneActivation = false;

        // While our screen is loading, process our splash screen
        StartCoroutine(DoSplashScreen());

        // Wait until our splashScreen has finished
        yield return new WaitUntil(() => splashIsComplete);

        // Then allow scene activation
        _sceneLoading.allowSceneActivation = true;
    }

    public IEnumerator DoSplashScreen()
    {
        while (splashCanvasGroup.alpha < 1)
        {
            splashCanvasGroup.alpha += (Time.deltaTime * splashScreenFadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.4F);

        while (splashCanvasGroup.alpha > 0)
        {
            splashCanvasGroup.alpha -= (Time.deltaTime * splashScreenFadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        splashIsComplete = true;
    }

    public void PreCheck()
    {
        if (splashScreenFadeSpeed == 0)
        {
            Debug.LogError("Fade Speed is set to 0, Loading will never complete!!");
        }

        if (splashCanvasGroup == null)
        {
            Debug.LogError("Splash Canvas Group is null, assign it in the inspector to fix!");
        }
    }
}