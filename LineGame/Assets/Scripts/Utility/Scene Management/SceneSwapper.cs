using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class SceneSwapper : MonoBehaviour
{
    private enum Scenes
    {
        Loading,
        Movement,
        UI,
    }

    public Button sceneButton;

    public string[] sceneNames;


    private void Awake()
    {
        sceneNames = new string[SceneManager.sceneCountInBuildSettings];

        for (int i = 0; i < sceneNames.Length; i++)
        {
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
        }

        if (SceneManager.GetActiveScene().name.Contains("UI Exp"))
        {
            sceneButton.onClick.AddListener(delegate { SceneManager.LoadScene(2); });
            sceneButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"To Movement";
        }
        else if (SceneManager.GetActiveScene().name.Contains("Movement"))
        {
            sceneButton.onClick.AddListener(delegate { SceneManager.LoadScene(1); });
            sceneButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"To UI";
        }

        //for (int x = 0; x < sceneNames.Length; x++)
        //{
        //    Debug.Log($"Checking if {SceneManager.GetActiveScene().name} is equal to {sceneNames[x]}");
        //    if(SceneManager.GetActiveScene().name != sceneNames[x])
        //    {
        //        sceneButton.onClick.AddListener(delegate { SceneManager.LoadScene(sceneNames[x]);});
        //        sceneButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"To {sceneNames[x]}";
        //        Debug.Log($"Match fouind, adding listeneter for scene name {sceneNames[x]}");
        //        return;
        //    }
        //}
    }
}