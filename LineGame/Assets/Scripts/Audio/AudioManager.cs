using System.Collections;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
	public AudioSource fireworksAudioSource;
	public AudioClip fireworksAudio;
	public AnimationCurve fireworksFadeCurve;

	public void PlayFireworksAudio()
	{
		StartCoroutine(ApplyFireworksAnimationCurve());
	}

	public IEnumerator ApplyFireworksAnimationCurve()
	{
		fireworksAudioSource.Play();

		while (fireworksAudioSource.isPlaying)
		{
			//Debug.Log(GetNormalisedTimeOnClip(fireworksAudioSource));
			fireworksAudioSource.volume = fireworksFadeCurve.Evaluate(GetNormalisedTimeOnClip(fireworksAudioSource));
			yield return null;
		}
	}

	/// <summary>
	/// Get's the current time of the AudioSource clip
	/// </summary>
	public static float GetTimeOnClip(AudioSource source)
	{
		return source.timeSamples / (float)source.clip.frequency;
	}

	/// <summary>
	/// Gets the current normalised time (0 to 1) of the AudioSource clip
	/// </summary>
	public static float GetNormalisedTimeOnClip(AudioSource source)
	{
		return source.timeSamples / (float)source.clip.frequency * (1F / source.clip.length);
	}

	//public void FadeInAudio(AudioSource soundSource, float time, float startVolume, float endVolume)
	//{
	//	StartCoroutine(FadeAudio(soundSource, time, startVolume, endVolume));
	//}

	//public IEnumerator FadeAudio(AudioSource soundSource, float time, float startVolume, float endVolume)
	//{
	//	float v = startVolume;
	//	float multiplier = 1F - startVolume;
	//	soundSource.volume = v;

	//	while (soundSource.volume < endVolume)
	//	{
	//		v += (Time.deltaTime / time) * multiplier;
	//		soundSource.volume = Mathf.Lerp(startVolume, endVolume, v);
	//		Debug.Log(v);
	//		yield return null;
	//	}
	//}
}
