using System;

using UnityEngine;


/// <summary>
/// LockPickGameManager
/// </summary>
public class LockPickGameManager : Singleton<LockPickGameManager>
{

	/// <summary>
	/// 
	/// </summary>
	[SerializeField]
	private LockPickGame lockPickGamePrefab;

	private LockPickGame lockPickGame;

	private Camera previousCamera;

	/// <summary>
	/// Event that is raised when the game is finished.
	/// </summary>
	public event EventHandler GameFinished;

	public bool GameActive { get; private set; } = false;

	/// <summary>
	/// 
	/// </summary>
	public void StartGame(int difficulty)
	{
		GameActive = true;

		// Render the current camera view and set it as the background of the mini game
		RenderTexture background = new RenderTexture(Screen.width, Screen.height, 16);
		Camera currentCamera = Camera.main;

		int mask = currentCamera.cullingMask;
		int UILayerBitMask = 1 << LayerMask.NameToLayer("UI");
		currentCamera.cullingMask = ~0 & mask ^ UILayerBitMask;
		currentCamera.targetTexture = background;
		currentCamera.Render();
		currentCamera.cullingMask = mask;
		currentCamera.targetTexture = null;

		// Create the mini game far from the scene. (1000 units down)
		float yRot = UnityEngine.Random.Range(-30f, 30f);
		lockPickGame = Instantiate(lockPickGamePrefab, new Vector3(0f, -1000f, 0f), Quaternion.Euler(0f, yRot, 0f));
		lockPickGame.Difficulty = difficulty;
		lockPickGame.name = lockPickGamePrefab.name;
		lockPickGame.Background = background;

		lockPickGame.Unlock += OnUnlock;

		previousCamera = currentCamera;
		previousCamera.gameObject.SetActive(false);
	}


	/// <summary>
	/// Handles the event when the game is finished.
	/// </summary>
	private void OnUnlock(object sender, EventArgs args)
	{
		lockPickGame.gameObject.SetActive(false);
		Destroy(lockPickGame.gameObject);
		previousCamera.gameObject.SetActive(true);
		GameActive = false;
		GameFinished?.Invoke(this, EventArgs.Empty);
	}
}