using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    public Scenes sceneToLoad;

    public CanvasGroup splashCanvasGroup;
    public string splashCanvasTag = "SplashCanvas";

    public float waitTimeBetweenFades = 0.4F;
    public float splashScreenFadeSpeed = 1;

    public bool splashIsComplete;


    private void Start()
    {
        PreCheck();

        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(LoadSceneAsync());
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

        DontDestroyOnLoad(this.gameObject);
    }

    public IEnumerator LoadSceneAsync()
    {
        // Load our next scene asyncronously in the background
        // But don't allow the scene to activate
        AsyncOperation _sceneLoading = SceneManager.LoadSceneAsync((int)sceneToLoad);
        _sceneLoading.allowSceneActivation = false;

        // While our screen is loading, process our splash screen
        StartCoroutine(DoSplashScreen(waitTimeBetweenFades));
        
        // Wait until our splashScreen has finished
        yield return new WaitUntil(() => splashIsComplete);

        // Then allow scene activation
        _sceneLoading.allowSceneActivation = true;
    }

    public IEnumerator DoSplashScreen(float _waitTimeBetweenFades)
    {
        while (splashCanvasGroup.alpha < 1)
        {
            splashCanvasGroup.alpha += (Time.deltaTime * splashScreenFadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(_waitTimeBetweenFades);

        while (splashCanvasGroup.alpha > 0)
        {
            splashCanvasGroup.alpha -= (Time.deltaTime * splashScreenFadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        splashIsComplete = true;
    }

    private void OnSceneLoaded(Scene _scene, LoadSceneMode _sceneMode)
    {
        Debug.Log($"Loading Complete: {_scene.name}, {_sceneMode}");

        Scenes _sceneName = (Scenes)System.Enum.Parse(typeof(Scenes), _scene.name);

        switch (_sceneName)
        {
            case Scenes.Loading:
                break;
            case Scenes.MovementExperiment:
                splashCanvasGroup = GameObject.FindGameObjectWithTag(splashCanvasTag).GetComponent<CanvasGroup>();
                StartCoroutine(DoSplashScreen(0.2F));
                break;
            case Scenes.UIExperiment:
                break;
        }
    }
}