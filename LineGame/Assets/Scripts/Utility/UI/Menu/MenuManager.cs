
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public enum Menu
    {
        Test
    }

public enum Curves
{
    ToDefaultPosition,
    ToDefaultPositionX,
    ToDefaultPositionY,
    LeftToCenter,
    CenterToRight,
}

public enum ScalePoint
{
    Top,
    Bottom,
    Left,
    Right,
}

public class MenuManager : Singleton<MenuManager>
{
    [System.Serializable]
    public class MenuMover
    {
        public IMenu[] menuList;       // The Menu RectTransforms

        public Dictionary<Menu, IMenu> menus = new Dictionary<Menu, IMenu>();      // A Dictionary for linking

        public ScalePoint scalepoint;

        public static float ScreenXMidPoint
        {
            get
            {
                return Screen.width / 2;
            }
        }
        public static float ScreenYMidPoint
        {
            get
            {
                return Screen.height / 2;
            }
        }


        public void MoveMenu(Menu _menu, Curves _curve, float _time)
        {
            if (_curve == Curves.ToDefaultPosition)
            {
                instance.StartCoroutine(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionX, _time));
                instance.StartCoroutine(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionY, _time));

            }
            else
            {
                instance.StartCoroutine(MoveMenuExpermintal(_menu, _curve, _time));
            }
        }

        /// <summary>
        /// Increase the scale of our menu by X percent. Note this is a base of 100% + _sizePercent.
        /// </summary>
        /// <param name="_menu"></param>
        /// <param name="_point"></param>
        /// <param name="_sizePercent"></param>
        /// <param name="_time"></param>
        public void ScaleMenu(Menu _menu, ScalePoint _point, float _sizePercent, float _time)
        {
            instance.StartCoroutine(MenuScaleExperimental(_menu, _point, _sizePercent, _time));
        }

        /// <summary>
        /// Moves a Menu Item in a specific direction, over a specified period of time using an Animation Curve.
        /// Transition is position perfect and scales with any Screen Size.
        /// </summary>
        /// <param name="_menu"></param>
        /// <param name="_direction"></param>
        /// <param name="_time"></param>
        /// <returns></returns>
        public IEnumerator MoveMenuExpermintal(Menu _menu, Curves _direction, float _time)
        {
            AnimationCurve _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0, 0));
            Vector3 perfectValue = new Vector3();

            float _countingTimer = 0;

            switch (_direction)
            {
                case Curves.ToDefaultPositionX:
                    _curve = new AnimationCurve(new Keyframe(0, menus[_menu].item.localPosition.x), new Keyframe(_time, menus[_menu].defaultPosition.x));
                    perfectValue = new Vector3(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                    break;

                case Curves.ToDefaultPositionY:
                    _curve = new AnimationCurve(new Keyframe(0, menus[_menu].item.localPosition.y), new Keyframe(_time, menus[_menu].defaultPosition.y));
                    perfectValue = new Vector3(menus[_menu].item.localPosition.x, _curve.Evaluate(_time), menus[_menu].item.localPosition.z);
                    break;

                case Curves.LeftToCenter:
                    _curve = new AnimationCurve(new Keyframe(0, -ScreenXMidPoint + (-menus[_menu].item.rect.size.x / 2)), new Keyframe(_time, 0));
                    perfectValue = new Vector3(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                    break;

                case Curves.CenterToRight:
                    _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(_time, ScreenXMidPoint + (menus[_menu].item.sizeDelta.x) / 2));
                    perfectValue = new Vector3(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                    break;
            }

            while (_countingTimer < _time)
            {
                if (_direction == Curves.ToDefaultPositionY)
                {
                    menus[_menu].item.localPosition = new Vector3(menus[_menu].item.localPosition.x, _curve.Evaluate(_countingTimer), menus[_menu].item.localPosition.z);
                }
                else
                {
                    menus[_menu].item.localPosition = new Vector3(_curve.Evaluate(_countingTimer), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                    Debug.Log(_curve.Evaluate(_countingTimer));
                }

                if (!menus[_menu].item.gameObject.activeSelf)
                    menus[_menu].item.gameObject.SetActive(true);

                 _countingTimer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            menus[_menu].item.localPosition = perfectValue;
        }

        public IEnumerator MenuScaleExperimental(Menu _menu, ScalePoint _point, float _scalePercent, float _time)
        {

            Vector2 _tempScale = menus[_menu].item.rect.size;
            Vector3 _increment = new Vector3(_point == ScalePoint.Left || _point == ScalePoint.Right ? _tempScale.x / 100 * _scalePercent : 0F,
                                                                        _point == ScalePoint.Top || _point == ScalePoint.Bottom ? _tempScale.y / 100 * _scalePercent : 0F,
                                                                        0F);

            Debug.Log($"\nLocal Scale: {menus[_menu].item.localScale}\nSize Delta: {menus[_menu].item.sizeDelta}\nRect Size: {menus[_menu].item.rect.size} \nIncrement:{_increment}");

            float elapsedTime = 0F;

            while (elapsedTime < _time)
            {
                _tempScale += (Vector2)_increment /_time* Time.deltaTime;

                menus[_menu].item.rect.Set(menus[_menu].item.rect.x, menus[_menu].item.rect.y,_tempScale.x, _tempScale.y);

                //menus[_menu].item.localPosition += (_point == ScalePoint.Left || _point == ScalePoint.Bottom ? -1 : 1) * (_increment * 50 / _time * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        // Resets our menu position to center screen
        private void ResetMenuTransition(Menu _menu)
        {
            menus[_menu].item.localPosition = menus[_menu].defaultPosition;
        }
    }

    [System.Serializable]
    public class MenuFader
    {
        // Our Canvas Groups in the scene
        public CanvasGroup[] canvasGroups = null;

        // Our lookup dictionary
        public Dictionary<Menu, CanvasGroup> cgLookup = new Dictionary<Menu, CanvasGroup>();

        // The default time for fading
        private const float defaultFadeTime = 1;


        // The helper function to start our fading. Saves us writing "StartCoroutine()" every time we want to fade
        public void Fade(Menu _menu, float _targetAlpha, float _duration = defaultFadeTime)
        {
            //StartCoroutine(FadeEx(_menu, _targetAlpha, _duration));
            instance.StartCoroutine(MenuManager.instance.ChangeOverTime(0, _menu, _targetAlpha, _duration));
        }

        public CanvasGroup FadableMenu(Menu _menu)
        {
            return cgLookup[_menu];
        }
    }

    [System.Serializable]
    public class MenuBlurer
    {
        [SerializeField] private Material[] blurMaterials = null;

        public Camera blurCamera;
        public Material blurMaterial;

        //public void BlurSetup()
        //{
        //    if (blurCamera.targetTexture != null)
        //    {
        //        blurCamera.targetTexture.Release();
        //    }
        //    blurCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, 1);
        //    blurMaterial.SetTexture("_RenTex", blurCamera.targetTexture);
        //}

        public void Blur(Menu _tag, float _blurTarget, float _duration, float _clamp = float.MaxValue)
        {
            //StartCoroutine(BlurEx(_tag, _blurTarget, _duration, _clamp));
            instance.StartCoroutine(MenuManager.instance.ChangeOverTime(1, _tag, _blurTarget, _duration, _clamp));
        }

        public void Blur(Menu _tag, float _blurTarget)
        {
            blurMaterials[(int)_tag].SetFloat("_Size", _blurTarget);
        }

        public Material BlurarbleMaterial(Menu _tag)
        {
            return blurMaterials[(int)_tag];
        }
    }


    public MenuMover menuMover = new MenuMover();
    public MenuFader menuFader = new MenuFader();
    public MenuBlurer menuBlurer = new MenuBlurer();

    public SlicedFilledImage levelProgressImage;
    public static float percent;


    // Sets up this class
    new private void Awake()
    {
        base.Awake();
        Setup();
    }

    private void Start()
    {
        //menuMover.MoveMenu(Menu.Test, Curves.ToDefaultPosition, 1F);
        //menuMover.MoveMenu(Menu.Test, Curves.ToDefaultPositionX, 1F);
        //menuMover.MoveMenu(Menu.Test, Curves.LeftToCenter, 2F);

        //menuMover.ScaleMenu(Menu.Test, ScalePoint.Bottom, 10F, 1F);
        menuMover.ScaleMenu(Menu.Test, menuMover.scalepoint, 10F, 2F);
        //menuMover.ScaleMenu(Menu.Test, ScalePoint.Left, 10F, 1F);
    }

    public void Setup()
    {
        menuMover.menus.Add(Menu.Test, menuMover.menuList[0]);
        menuFader.cgLookup.Add(Menu.Test, menuFader.canvasGroups[0]);
    }

    // Sets the Level Progress Image to the correct percentage of the path in terms of fill amount
    public void UpdateLevelProgress(float _distanceTravelled, float pathLength)
    {
        levelProgressImage.fillAmount = _distanceTravelled / pathLength;
        //percent = (_distanceTravelled / pathLength) * 100;
        //levelProgress.text = $"{(int)Mathf.Clamp(percent, 0, 100)}%";
    }

    public IEnumerator ChangeOverTime(int _type, Menu _tag, float _target, float _duration, float _clamp = float.MaxValue)
    {
        float _timer = 0;
        float _amount = (_type == 0) ? menuBlurer.BlurarbleMaterial(_tag).GetFloat("_Size") - _target : menuFader.FadableMenu(_tag).alpha - _target;

        while (_timer < _duration)
        {
            if (_type == 0)
            {
                menuBlurer.BlurarbleMaterial(_tag).SetFloat("_Size", Mathf.Clamp(menuBlurer.BlurarbleMaterial(_tag).GetFloat("_Size") - (Time.deltaTime * (_amount / _duration)), 0, _clamp));
            }
            else
            {
                menuFader.FadableMenu(_tag).alpha -= Time.deltaTime * (_amount / _duration);
            }

            _timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}