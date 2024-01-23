//using System.Collections;
//using UnityEngine;
//using UnityEditor.Advertisements;
//using UnityEngine.Advertisements;
//using System.Runtime.CompilerServices;

//public class UnityAds : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
//{
//	public static UnityAds Static;

//	private string _androidGameID;
//	private string _androidAdUnitID = "Interstitial_Android";

//	private static bool awaitingInterstitial;
//	public static bool WaitingForInterstitial
//	{
//		get
//		{
//			return awaitingInterstitial;
//		}
//		private set
//		{
//			awaitingInterstitial = value;
//		}
//	}


//	private void Awake()
//	{
//		Static = this;
//	}

//	private void Start()
//	{
//		_androidGameID = GetPlatformAdsID(RuntimePlatform.Android);

//		Advertisement.Initialize(_androidGameID, true, this);
//	}

//	/// <summary>
//	/// Attempt to show an Interstitial Ad. If one is not available, abort.
//	/// If waiting for an Ad is important, use ShowInterstitialAd() instead.
//	/// </summary>
//	public void LoadAdvertisement()
//	{
//		Debug.Log($"Loading Advertisement with ID: {_androidGameID}");
//		Advertisement.Load(_androidAdUnitID, this);
//	}


//	/// <summary>
//	/// Returns the ID of a specific advertisement platform as requested. Logs an error if an ID for the platform does not exist
//	/// </summary>
//	/// <param name="_platform"></param>
//	/// <returns></returns>
//	public string GetPlatformAdsID(RuntimePlatform _platform)
//	{
//		string _id = AdvertisementSettings.GetGameId(_platform);

//		if (_id == string.Empty)
//		{
//			Debug.LogError($"UnityAds :: Error! _id is empty using Platform: {_platform}. This Platform does not yet have an ID!", this.gameObject);
//			return null;
//		}

//		return _id;
//	}

//	public void OnInitializationComplete()
//	{
//		Debug.Log($"Unity Ads Initialisation Complete!");
//		//LoadAdvertisement();
//	}

//	public void OnInitializationFailed(UnityAdsInitializationError error, string message)
//	{
//		Debug.Log($"Unity Ads Initialisation Failed!");
//	}

//	public void OnUnityAdsAdLoaded(string placementId)
//	{
//		Debug.Log($"Unity Ads Loaded with placement ID {placementId}");
//		Advertisement.Show(_androidAdUnitID, this);
//	}

//	public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
//	{
//		Debug.Log($"Unity Ads Failed to Load with placement ID {placementId}. Error: {error}. Message: {message}");
//	}

//	public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
//	{
//		Debug.Log($"Unity Ads Failed to Show with placement ID {placementId}. Error: {error}. Message: {message}");
//	}

//	public void OnUnityAdsShowStart(string placementId)
//	{
//		throw new System.NotImplementedException();
//	}

//	public void OnUnityAdsShowClick(string placementId)
//	{
//		throw new System.NotImplementedException();
//	}

//	public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
//	{
//		throw new System.NotImplementedException();
//	}
//}