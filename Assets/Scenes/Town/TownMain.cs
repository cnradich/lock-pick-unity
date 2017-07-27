using System;
using System.Collections;

using UnityEngine;

using Random = UnityEngine.Random;


/// <summary>
/// TownMain
/// </summary>
public class TownMain : MonoBehaviour
{

	/// <summary>
	/// Initialize
	/// </summary>
	private void Start()
	{
		StartCoroutine(StartNewGame());
	}
	
	private IEnumerator StartNewGame()
	{
		// orient the camera to a random location for funsies
		Quaternion oldRotation = Camera.main.transform.rotation;
		Quaternion targetRotation = Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(0f, 360f), 0f);// Random.rotation;

		float alpha = 0f;
		while(alpha < 1f)
		{
			Camera.main.transform.rotation = Quaternion.Lerp(oldRotation, targetRotation, alpha);
			alpha += Time.deltaTime;
			yield return null;
		}
		Camera.main.transform.rotation = targetRotation;

		yield return new WaitForSeconds(.25f);

		// Start the game at a random difficulty
		int difficulty = Random.Range(0, 101);
		LockPickGameManager.Instance.StartGame(difficulty);
		LockPickGameManager.Instance.GameFinished += OnGameFinished;

	}

	private void OnGameFinished(object sender, EventArgs args)
	{
		LockPickGameManager.Instance.GameFinished -= OnGameFinished;
		StartCoroutine(StartNewGame());
	}

}