
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                instance.menuStack.Enqueue(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionX, _time, true));
                instance.menuStack.Enqueue(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionY, _time, true));
                //instance.StartCoroutine(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionX, _time));
                //instance.StartCoroutine(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionY, _time));

            }
            else
            {
                instance.menuStack.Enqueue(MoveMenuExpermintal(_menu, _curve, _time));
                //instance.StartCoroutine(MoveMenuExpermintal(_menu, _curve, _time));
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
        public IEnumerator MoveMenuExpermintal(Menu _menu, Curves _direction, float _time, bool _simultaniousExec = false)
        {
            instance.simultaniousExec = _simultaniousExec;

            AnimationCurve _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0, 0));
            Vector3 perfectValue = new Vector3();

            float _countingTimer = 0;

            switch (_direction)
            {
                case Curves.ToDefaultPositionX:
                    _curve = new AnimationCurve(new Keyframe(0, menus[_menu].item.localPosition.x), new Keyframe(_time, menus[_menu].defaultPosition.x));
                    perfectValue = new Vector3(menus[_menu].defaultPosition.x, menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                    Debug.Log($"Default Pos : {menus[_menu].defaultPosition.x}, {menus[_menu].defaultPosition.y}");
                    break;

                case Curves.ToDefaultPositionY:
                    _curve = new AnimationCurve(new Keyframe(0, menus[_menu].item.localPosition.y), new Keyframe(_time, menus[_menu].defaultPosition.y));
                    perfectValue = new Vector3(menus[_menu].defaultPosition.y, _curve.Evaluate(_time), menus[_menu].item.localPosition.z);
                    break;

                case Curves.LeftToCenter:
                    _curve = new AnimationCurve(new Keyframe(0, -ScreenXMidPoint + (-menus[_menu].item.rect.size.x / 2)), new Keyframe(_time, 0));
                    perfectValue = new Vector3(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                    break;

                case Curves.CenterToRight:
                    _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(_time, ScreenXMidPoint + (menus[_menu].item.rect.size.x) / 2));
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
                }

                if (!menus[_menu].item.gameObject.activeSelf)
                    menus[_menu].item.gameObject.SetActive(true);

                 _countingTimer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            menus[_menu].item.localPosition = perfectValue;

            if (!_simultaniousExec)
                instance.routineRunning = false;

            if (instance.simultaniousExec)
                instance.simExecCount++;
        }

        // Finish in white direction on the axis this should move
        public IEnumerator MenuScaleExperimental(Menu menu, ScalePoint point, float scalePercent, float time)
        {
            // Which axis are we resizing?
            RectTransform.Axis chosenAxis = point == ScalePoint.Left || point == ScalePoint.Right ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;

            // The base value before we manipulate the transform. We use this to append the other values
            float baseValue = chosenAxis == RectTransform.Axis.Horizontal ? menus[menu].item.rect.size.x : menus[menu].item.rect.size.y;

            // The ultimate size value we want to append on completion, either X or Y axis
            float appendingSizeValue = (chosenAxis == RectTransform.Axis.Horizontal ? menus[menu].item.rect.size.x : menus[menu].item.rect.size.y) / 100 * scalePercent;

            // The ultimate pos value we want to append on completion, either X or Y axis
            Vector2 appendingPosValue = (chosenAxis == RectTransform.Axis.Horizontal ? Vector2.right : Vector2.up) * (appendingSizeValue / 2) * (point == ScalePoint.Left || point == ScalePoint.Bottom ? -1 : 1);


            float _elapsedTime = 0F;

            while (_elapsedTime < time)
            {
                menus[menu].item.SetSizeWithCurrentAnchors(chosenAxis, baseValue + (appendingSizeValue * (_elapsedTime / time)));
                menus[menu].item.anchoredPosition = (appendingPosValue * (_elapsedTime / time));

                _elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        // Resets our menu position to default position
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

    Queue<IEnumerator> menuStack = new Queue<IEnumerator>();
    public bool simultaniousExec = false;
    public byte simExecCount = 0;
    public bool queueIsPaused = false;
    public bool routineRunning = false;


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
        //menuMover.ScaleMenu(Menu.Test, menuMover.scalepoint, 10F, 1F);
        //menuMover.ScaleMenu(Menu.Test, ScalePoint.Left, 10F, 1F);

        menuMover.MoveMenu(Menu.Test, Curves.LeftToCenter, 1);
        menuMover.MoveMenu(Menu.Test, Curves.CenterToRight, 1);
        menuMover.MoveMenu(Menu.Test, Curves.ToDefaultPosition, 1);
    }

    public IEnumerator RealtimeMenuExecution()
    {
        while(true)
        {
            if (menuStack.Count > 0)
            {
                if (!routineRunning)
                {
                    routineRunning = true;
                    StartCoroutine(menuStack.Dequeue());

                    if (simultaniousExec)
                        StartCoroutine(menuStack.Dequeue());
                }
            }

            if (simExecCount == 2)
            {
                simExecCount = 0;
                routineRunning = false;
                simultaniousExec = false;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void Setup()
    {
        menuMover.menus.Add(Menu.Test, menuMover.menuList[0]);
        menuFader.cgLookup.Add(Menu.Test, menuFader.canvasGroups[0]);
        StartCoroutine(RealtimeMenuExecution());
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