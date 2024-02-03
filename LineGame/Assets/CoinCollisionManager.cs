using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollisionManager : MonoBehaviour
{
	// When a coin triggers the collider, schedule for deactivation
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Coin"))
		{
			StartCoroutine(WaitToDeactivate(other.gameObject));
		}
	}

	// Deactivate coin after 1 second, so we cant see it disappearing
	IEnumerator WaitToDeactivate(GameObject coin)
	{
		// Play water splash
		yield return new WaitForSeconds(1F);
		coin.SetActive(false);
	}
}
