using System;

using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// LockPickGameUI
/// </summary>
public class LockPickGameUI : MonoBehaviour
{
	[SerializeField]
	private LockPickGame lockPickGame;

	[SerializeField]
	private Text attemptsText;

	[SerializeField]
	private Text difficultyText;

	private int attempts = 0;

	private void Start()
	{
		attemptsText.text = $"{attempts}";
		lockPickGame.PickBreak += OnPickBreak;

		difficultyText.text = $"{lockPickGame.Difficulty}";
	}

	private void OnPickBreak(object sender, EventArgs args)
	{
		attempts++;
		attemptsText.text = $"{attempts}";
	}
}