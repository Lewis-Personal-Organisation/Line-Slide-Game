using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Advertisements;
using UnityEngine.Advertisements;
using System.Runtime.CompilerServices;

public class UnityAds : MonoBehaviour
{
    public static UnityAds inst;

    private static bool awaitingInterstitial;
    public static bool AwaitingInterstitial
    {
        get
        {
            return awaitingInterstitial;
        }
        private set
        {
            awaitingInterstitial = value;
        }
    }



    private void Awake()
    {
        inst = this;
        Advertisement.Initialize(GetPlatformAdsID(RuntimePlatform.Android));
    }

    private void Start()
    {
        ShowInterstitialAd();
    }


    /// <summary>
    /// Attempt to show an Interstitial Ad now. If one is not available, abort.
    /// </summary>
    public void ShowInterstitialAdNow()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
        else
        {
            Debug.LogWarning($"{this.name} :: {MethodName()} -- Advertisement is not ready");
        }
    }

    /// <summary>
    /// Attempt to show an Interstitial Ad now. If one is not available, wait until one is. Never Abort.
    /// </summary>
    public void ShowInterstitialAd()
    {
        StartCoroutine(ShowInterstitialAdProc());
    }

    private IEnumerator ShowInterstitialAdProc()
    {
        AwaitingInterstitial = true;

        yield return new WaitUntil(() => Advertisement.IsReady());

        Advertisement.Show();
    }


    string MethodName([CallerMemberName] string name = "")
    {
        return name;
    }


    /// <summary>
    /// Returns the ID of a specific advertisement platform as requested. Logs an error if an ID for the platform does not exist
    /// </summary>
    /// <param name="_platform"></param>
    /// <returns></returns>
    public string GetPlatformAdsID(RuntimePlatform _platform)
    {
        string _id = AdvertisementSettings.GetGameId(_platform);

        if (_id == string.Empty)
        {
            Debug.LogError($"UnityAds :: Error! _id is empty using Platform: {_platform}. This Platform does not yet have an ID!", this.gameObject);
            return null;
        }

        return _id;
    }
}
