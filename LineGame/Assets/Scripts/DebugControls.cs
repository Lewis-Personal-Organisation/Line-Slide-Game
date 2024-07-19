using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugControls : Singleton<DebugControls>
{
    public GameObject levelTimeTextPrefab;
    public Transform levelTimeTextHolder;
    public List<TextMeshProUGUI> levelTimes = new List<TextMeshProUGUI>();


	private void Start()
	{
        SpawnLevelTextUIElements();
	}

	/// <summary>
	/// Listens for Key Presses for Toggling Editor Pause/Play
	/// </summary>
	/// 
	[ExecuteAlways]
    void Update()
	{
		if (Application.isPlaying && Application.isEditor)
        {
            if (Input.GetKey(KeyCode.Pause))
            {
                Debug.Break();
            }
            else if (Input.GetKey(KeyCode.N))
            {
				for (int i = 1; i < LevelManager.Instance.levels.Count+1; i++)
				{
                    levelTimes[i].text = $"Level {i}: {GameSave.GetLevelTime(i)}";
				}
			}
        }
    }

    public void SpawnLevelTextUIElements()
    {
        for (int i = 1; i < LevelManager.Instance.levels.Count + 1; i++)
        {
            SpawnLevelText(i, GameSave.GetLevelTime(i));
        }

    }

    private void SpawnLevelText(int level, float levelTime)
    {
        RectTransform levelText = GameObject.Instantiate(levelTimeTextPrefab).GetComponent<RectTransform>();
        levelText.SetParent(levelTimeTextHolder);
        levelText.position = Vector3.zero;
        TextMeshProUGUI levelTimeText = levelText.GetComponent<TextMeshProUGUI>();
        levelTimeText.text = $"Level {level}: {levelTime}";
		levelTimes.Add(levelTimeText);
    }
}