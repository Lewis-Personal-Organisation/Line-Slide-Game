using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private enum Scenes
    {
        Loading,
        Movement,
        UI,
    }

    public string[] scenes;

    public CanvasGroup splashCanvasGroup;

    public bool SplashComplete;


    private void Start()
    {
        scenes = new string[SceneManager.sceneCountInBuildSettings];

        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
        }

        StartCoroutine(LoadSceneAsync());
    }

    public IEnumerator LoadSceneAsync()
    {
        // Load our next scene asyncronously in the background
        // But don't allow the scene to activate
        AsyncOperation _sceneLoading = SceneManager.LoadSceneAsync((int)Scenes.Movement);
        _sceneLoading.allowSceneActivation = false;

        StartCoroutine(DoSplashScreen());

        // Wait until our splashScreen has finished
        yield return new WaitUntil(() => SplashComplete == true);

        // Then allow scene activation
        _sceneLoading.allowSceneActivation = true;
    }

    public IEnumerator DoSplashScreen()
    {
        while (splashCanvasGroup.alpha < 1)
        {
            splashCanvasGroup.alpha += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5F);

        while (splashCanvasGroup.alpha > 0)
        {
            splashCanvasGroup.alpha -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        SplashComplete = true;
    }
}