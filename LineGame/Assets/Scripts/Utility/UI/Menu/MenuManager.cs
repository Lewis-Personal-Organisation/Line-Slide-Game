
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

    public enum Menu
    {
        Test
    }

    public enum Curves
    {
        CenterToRight,
    }

public class MenuManager : Singleton<MenuManager>
{
    #region Mover
    // The Animation Curves for our transitions
    public AnimationCurve[] transitionCurves;

    // The Menu RectTransforms
    public RectTransform[] menus;

    // A Dcitionary for linking
    public Dictionary<Menu, RectTransform> menuTransforms = new Dictionary<Menu, RectTransform>();


    public void MoverSetup()
    {
        menuTransforms.Add(Menu.Test, menus[0]);
        AdjustCurvesForScreenSize();
    }

    // Adjust the curves for our screen size
    public void AdjustCurvesForScreenSize()
    {
        // An example of how to adjust an existing animation curve at runtime
        // First we assign a new set of keys (we can't just modify 1 key). Next, we must reset the tangents
        for (int i = 0; i < transitionCurves.Length; i++)
        {
            transitionCurves[i].keys = new Keyframe[]
            {
                transitionCurves[i].keys[0],
                new Keyframe( transitionCurves[i].keys[1].time, Screen.width)
            };
            //AnimationUtility.SetKeyLeftTangentMode(transitionCurves[i], 0, AnimationUtility.TangentMode.Auto);
            //AnimationUtility.SetKeyLeftTangentMode(transitionCurves[i], 1, AnimationUtility.TangentMode.Auto);
        }
    }

    public void MoveMenu(Menu _menu, Curves _curve)
    {
        StartCoroutine(MoveMenuEx(_menu, _curve));
    }

    // Responsible for moving a menus position on X axis
    public IEnumerator MoveMenuEx(Menu _menu, Curves _curve)
    {
        int _x = (int)_curve;
        float _timer = 0;

        while (_timer < transitionCurves[_x].keys[1].time)
        {
            menuTransforms[_menu].localPosition = new Vector3(transitionCurves[_x].Evaluate(_timer), menuTransforms[_menu].localPosition.y, menuTransforms[_menu].localPosition.z);
            _timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    // Resets our menu position to center screen
    private void ResetMenuTransition(Menu _menu)
    {
        menuTransforms[_menu].localPosition = Vector3.zero;
    }
    #endregion

    #region Fader
    // Our Canvas Groups in the scene
    [SerializeField] private CanvasGroup[] canvasGroups;

    // Our lookup dictionary
    private Dictionary<Menu, CanvasGroup> cgLookup = new Dictionary<Menu, CanvasGroup>();

    // The default time for fading
    private const float defaultFadeTime = 1;


    public void FaderSetup()
    {
        cgLookup.Add(Menu.Test, canvasGroups[0]);
    }

    // The helper function to start our fading. Saves us writing "StartCoroutine()" every time we want to fade
    public void Fade(Menu _menu, float _targetAlpha, float _duration = defaultFadeTime)
    {
        //StartCoroutine(FadeEx(_menu, _targetAlpha, _duration));
        StartCoroutine(MenuManager.instance.ChangeOverTime(0, _menu, _targetAlpha, _duration));
    }

    public CanvasGroup FadableMenu(Menu _menu)
    {
        return cgLookup[_menu];
    }
    #endregion

    #region Blurer
    [SerializeField] private Material[] blurMaterials;

    public Camera blurCamera;
    public Material blurMaterial;

    public void BlurSetup()
    {
        if (blurCamera.targetTexture != null)
        {
            blurCamera.targetTexture.Release();
        }
        blurCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, 1);
        blurMaterial.SetTexture("_RenTex", blurCamera.targetTexture);
    }

    public void Blur(Menu _tag, float _blurTarget, float _duration, float _clamp = float.MaxValue)
    {
        //StartCoroutine(BlurEx(_tag, _blurTarget, _duration, _clamp));
        StartCoroutine(MenuManager.instance.ChangeOverTime(1, _tag, _blurTarget, _duration, _clamp));
    }

    //public IEnumerator BlurEx(Menu _tag, float _blurTarget, float _duration, float _clamp = float.MaxValue)
    //{
    //    float _timer = 0;
    //    float _amount = blurMaterials[(int)_tag].GetFloat("_Size") - _blurTarget;           //e.g, 15 - 10 = 5 (pos), 1 - 15 = -15 (neg)

    //    while (_timer < _duration)
    //    {
    //        blurMaterials[(int)_tag].SetFloat("_Size", Mathf.Clamp(blurMaterials[(int)_tag].GetFloat("_Size") - (Time.deltaTime * (_amount / _duration)), 0, _clamp));   //e.g -= time.deltatime * (0.5f / 1)
    //        _timer += Time.deltaTime;
    //        yield return new WaitForEndOfFrame();
    //    }
    //}

    // Blur our material over time
    //public IEnumerator BlurOverTime(BlurTags _tag, float _blurTarget, float _duration, float _clamp = float.MaxValue)
    //{
    //    float _timer = 0;
    //    float _step = _blurTarget / _duration;

    //    while (_timer < _duration)
    //    {
    //        if (_timer + Time.deltaTime > _duration)
    //        {
    //            blurMaterials[(int)_tag].SetFloat("_Size", Mathf.Clamp(_blurTarget, 0, _clamp));
    //            _timer = _duration;
    //        }
    //        else
    //        {
    //            _timer += Time.deltaTime;
    //            blurMaterials[(int)_tag].SetFloat("_Size", Mathf.Clamp(_timer * _step, 0, _clamp));
    //            //Debug.Log($"Timer: {_timer}, calc: {_timer} * ({_blurTarget} / {_timer}). New value: {blurMaterial.GetFloat("_Size")}");
    //            yield return new WaitForEndOfFrame();
    //        }
    //    }
    //}

    // Blur our material Instantly
    public void Blur(Menu _tag, float _blurTarget)
    {
        blurMaterials[(int)_tag].SetFloat("_Size", _blurTarget);
    }

    public Material BlurarbleMaterial(Menu _tag)
    {
        return blurMaterials[(int)_tag];
    }
    #endregion


    #region Path Progress
    public TextMeshProUGUI levelProgress;
    private static float percent;

    public void UpdateLevelProgress(float _distanceTravelled, float pathLength)
    {
        percent = (_distanceTravelled / pathLength) * 100;
        levelProgress.text = $"{(int)Mathf.Clamp(percent, 0, 100)}%";
    }
    #endregion



    // Sets up this class
    private void Awake()
    {
        //Debug.Log($"{this.GetType().Name} is Awakening!");

        MoverSetup();
        FaderSetup();
        //BlurSetup();

        base.Awake();
    }

    public IEnumerator ChangeOverTime(int _type, Menu _tag, float _target, float _duration, float _clamp = float.MaxValue)
    {
        float _timer = 0;
        float _amount = (_type == 0) ? BlurarbleMaterial(_tag).GetFloat("_Size") - _target : FadableMenu(_tag).alpha - _target;

        while (_timer < _duration)
        {
            if (_type == 0)
            {
                BlurarbleMaterial(_tag).SetFloat("_Size", Mathf.Clamp(BlurarbleMaterial(_tag).GetFloat("_Size") - (Time.deltaTime * (_amount / _duration)), 0, _clamp));
            }
            else
            {
                FadableMenu(_tag).alpha -= Time.deltaTime * (_amount / _duration);
            }

            _timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}