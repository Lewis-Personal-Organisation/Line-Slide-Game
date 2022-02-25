using System.Collections;
using UnityEngine;
using UnityEditor.Advertisements;
using UnityEngine.Advertisements;
using System.Runtime.CompilerServices;

public class UnityAds : MonoBehaviour
{
    public static UnityAds Static;

    private static bool awaitingInterstitial;
    public static bool WaitingForInterstitial
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
        Static = this;
    }

    private void Start()
    {
        Advertisement.Initialize(GetPlatformAdsID(RuntimePlatform.Android));
        ShowInterstitialAd();
    }


    /// <summary>
    /// Attempt to show an Interstitial Ad. If one is not available, abort.
    /// If waiting for an Ad is important, use ShowInterstitialAd() instead.
    /// </summary>
    public void TryShowInterstitialAd()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
        else
        {
            Debug.LogWarning($"{this.name}::{MethodName()}:: Advertisement is not ready");
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
        WaitingForInterstitial = true;

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