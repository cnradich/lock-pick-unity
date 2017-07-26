using System;
using System.Collections;

using UnityEngine;


/// <summary>
/// LockPickGame
/// </summary>
[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class LockPickGame : MonoBehaviour
{
	[SerializeField]
	[Range(0, 1)]
	private float maxSolutionZeroBias = .5f;

	[SerializeField]
	private RangeFloat solutionDifficultyRange = new RangeFloat(.01f, .1f);

	[SerializeField]
	private RangeFloat solutionFalloffDifficultyRange = new RangeFloat(.1f, .25f);

	[SerializeField]
	private RangeFloat pickDegradationDifficultyRange = new RangeFloat(.1f, 1f);

	[SerializeField]
	private Cylinder cylinder;

	[SerializeField]
	private Pick pick;

	private Animator animator;

	private AudioSource cylinderAudio;

	private AudioSource pickAudio;

	private int lockDifficulty;

	private float solutionCenter;

	private float solutionRange;

	private float solutionFalloff;

	private int baseLayer;

	private int cylinderRotationLayer;

	private int pickRotationLayer;

	private int feedbackLayer;

	public bool MovementEnabled
	{
		get { return pick.MovementEnabled && cylinder.MovementEnabled; }
		set { pick.MovementEnabled = cylinder.MovementEnabled = value; }
	}

	public void SetMovementEnabled(int enabled) => MovementEnabled = enabled != 0;

	/// <summary>
	/// Initialize
	/// </summary>
	private void Start()
	{
		InitComponents();

		pick.OnStateChange += PickStateChange;

		baseLayer             = animator.GetLayerIndex("Base Layer");
		cylinderRotationLayer = animator.GetLayerIndex("Cylinder Rotation");
		pickRotationLayer     = animator.GetLayerIndex("Pick Rotation");
		feedbackLayer         = animator.GetLayerIndex("Feedback");

		InitLock(50);
	}

	private void InitComponents()
	{
		animator = GetComponent<Animator>();

		AudioSource[] audioSources = GetComponents<AudioSource>();
		cylinderAudio = audioSources[0];
		pickAudio     = audioSources.Length > 1 ? audioSources[1] : gameObject.AddComponent<AudioSource>();
	}

	/// <summary>
	/// Update
	/// </summary>
	private void Update()
	{
		cylinder.MaxTensionRotation = DetermineMaxCylinderRotation();
		//Debug.Log("Max: " + maxCylinderRotation);

		cylinder.Update();
		pick.Update();

		pick.Breaking = cylinder.State == Cylinder.StateType.Stuck;

		animator.Play("A_LockPick_Rotate", pickRotationLayer, pick.RotationNormalized);
		animator.Play("A_LockCylinder_Rotate", cylinderRotationLayer, cylinder.Rotation);
	}

	private void PickStateChange(object sender, EventArgs args)
	{
		Pick.StateChangeEventArgs stateArgs = args as Pick.StateChangeEventArgs;

		switch(pick.State)
		{
			case Pick.StateType.Idle:
			case Pick.StateType.Moving:
				animator.Play("Empty", feedbackLayer);
				break;
			case Pick.StateType.Breaking:
				animator.Play("A_LockPick_Shake", feedbackLayer);
				break;
			case Pick.StateType.Broken:
				animator.Play("A_LockPick_Break", feedbackLayer);
				MovementEnabled = false;
				StartCoroutine(RefreshPick());
				break;
		}
	}

	/// <summary>
	/// Determines the maximum amount the cylinder can rotate form 0 to 1 considering the current pick rotation.
	/// </summary>
	/// <returns>The maximum amount the cylinder can rotate under tension.</returns>
	private float DetermineMaxCylinderRotation()
	{
		// offset the pick rotation value to reflect a virtually centered solution
		float offset = 0f - solutionCenter;
		float zeroedPickRotation = Mathf.Abs(pick.Rotation + offset);

		// normalize the pick rotation relative to the falloff range
		float normalizedPickRotation = zeroedPickRotation - solutionRange;
		normalizedPickRotation = Mathf.Clamp(normalizedPickRotation * (1f / solutionFalloff), 0f, 1f);

		return 1f - normalizedPickRotation;
	}

	private void InitLock(int difficulty)
	{
		float normalizedDifficulty = Mathf.Clamp(difficulty, 0, 100) / 100f;

		// Pick the center point of the solution
		solutionCenter = UnityEngine.Random.Range(-1f, 1f);
		// Apply zero bias to center point
		float sign = solutionCenter >= 0 ? 1f : -1f;
		float solutionCenterHeavyBias = Mathf.Pow(solutionCenter, 10);
		float zeroBias = Mathf.Lerp(0f, maxSolutionZeroBias, normalizedDifficulty);
		solutionCenter = Mathf.Lerp(Mathf.Abs(solutionCenter), solutionCenterHeavyBias, zeroBias) * sign;

		// Get valid solution range
		solutionRange = Mathf.Lerp(solutionDifficultyRange.Max, solutionDifficultyRange.Min, normalizedDifficulty);

		// Get valid solution falloff
		solutionFalloff = Mathf.Lerp(solutionFalloffDifficultyRange.Max, solutionFalloffDifficultyRange.Min, 
		                             normalizedDifficulty);

		// Get pick degradation
		pick.Degradation = Mathf.Lerp(pickDegradationDifficultyRange.Min, pickDegradationDifficultyRange.Max, 
		                              normalizedDifficulty);
	}

	private IEnumerator RefreshPick()
	{
		yield return new WaitForSeconds(.5f);

		animator.Play("A_LockPickGame_NewPick", baseLayer);
		animator.speed = 0;
		while(cylinder.Rotation > 0f)
		{
			cylinder.Rotation -= 5f * Time.deltaTime;
			yield return null;
		}
		pick.Reset(pick.Degradation);
		animator.speed = 1;
	}


	/// <summary>
	/// Helper class to maintain cylinder data and state for use by the lock pick mini game.
	/// </summary>
	[Serializable]
	private class Cylinder
	{
		/// <summary>
		/// The speed at which applying tension will rotate the cylinder completely per second.
		/// </summary>
		[SerializeField]
		[Tooltip("The speed at which applying tension will rotate the cylinder completely per second.")]
		private float rotationTensionSpeed = 1f;

		/// <summary>
		/// The speed at which no tension will rotate the cylinder back to its resting position completely per second.
		/// </summary>
		[SerializeField]
		[Tooltip("The speed at which no tension will rotate the cylinder back to its resting position completely " +
		         "per second.")]
		private float rotationReturnSpeed = 1f;

		/// <summary>
		/// See Rotation property.
		/// </summary>
		private float rotation = 0f;

		/// <summary>
		/// The possible states the cylinder can exist in.
		/// </summary>
		public enum StateType
		{
			Idle,
			Moving,
			Stuck,
		}

		/// <summary>
		/// If false, all movement of the cylinder will cease and vice-versa
		/// </summary>
		public bool MovementEnabled { get; set; } = true;

		/// <summary>
		/// The current state of the cylinder.
		/// </summary>
		public StateType State { get; private set; } = StateType.Idle;

		/// <summary>
		/// The current rotation of the cylinder. This value is naturally normalized 0 to 1.
		/// </summary>
		public float Rotation
		{
			get { return rotation; }
			set { rotation = Mathf.Clamp(value, 0f, 1f); }
		}

		/// <summary>
		/// The maximum rotation that can be achieved by applying tension to the cylinder. This does not necessarily
		/// represent the maximum rotation the cylinder can currently maintain. For instance, when the cylinder is at
		/// .9 rotation, and this MaxTensionRotation is set to 0f before the cylinder can return to 0, it will not
		/// automatically snap to 0. It will slowly return to 0, existing in a state of rotation beyond this max rotation.
		/// </summary>
		public float MaxTensionRotation { get; set; } = 1f;

		/// <summary>
		/// Updates the cylinder data and state.
		/// </summary>
		public void Update()
		{
			float driveDelta = GetDriveDelta();

			switch(State)
			{
				case StateType.Idle: // Idle state
					if(driveDelta != 0f)
					{
						State = StateType.Moving;
						goto case StateType.Moving;
					}
					break;

				case StateType.Moving: // Moving state
					if(driveDelta == 0f)
					{
						State = StateType.Idle;
						break;
					}
					float prevRotation = Rotation;
					Rotation += driveDelta;

					if(Rotation >= MaxTensionRotation && driveDelta > 0f)
					{
						Rotation = prevRotation;
						State = StateType.Stuck;
						goto case StateType.Stuck;
					}
					break;

				case StateType.Stuck: // Stuck state
					if(driveDelta <= 0f)
					{
						State = StateType.Moving;
						goto case StateType.Moving;
					}
					break;
			}

		}

		/// <summary>
		/// Calculates and returns the drive delta value on the cylinder. The drive is the amount of "force" being applied
		/// to move the cylinder in a given direction.
		/// </summary>
		/// <returns>The drive delta value.</returns>
		private float GetDriveDelta()
		{
			if(! MovementEnabled)
			{
				return 0f;
			}

			float cylinderRotationInput = Input.GetAxis("Rotate Cylinder") * rotationTensionSpeed;
			if (cylinderRotationInput == 0f && Rotation != 0f)
			{
				cylinderRotationInput = -1f * rotationReturnSpeed;
			}
			return cylinderRotationInput* Time.deltaTime;
		}

	} // class Cylinder


	/// <summary>
	/// Helper class to maintain pick data and state for use by the lock pick mini game.
	/// </summary>
	[Serializable]
	private class Pick
	{
		public event EventHandler OnStateChange;

		/// <summary>
		/// The speed at which rotating the pick will complete half a rotation per second.
		/// </summary>
		[SerializeField]
		[Tooltip("The speed at which rotating the pick will complete half a rotation per second.")]
		private float rotationSpeed = 1f;

		/// <summary>
		/// See Rotation property.
		/// </summary>
		private float rotation = 0f;

		/// <summary>
		/// The possible states the pick can exist in.
		/// </summary>
		public enum StateType
		{
			Idle,
			Moving,
			Breaking,
			Broken,
		}

		/// <summary>
		/// If false, all movement of the pick will cease and vice-versa
		/// </summary>
		public bool MovementEnabled { get; set; } = true;

		/// <summary>
		/// The normalized amount of life this pick has left. 1 is max life, 0 is no life.
		/// </summary>
		public float Life { get; private set; } = 1f;

		/// <summary>
		/// The amount of degradation the pick incurs while breaking, at amount of life per second.
		/// </summary>
		public float Degradation { get; set; } = 0f;

		/// <summary>
		/// The current state of the pick.
		/// </summary>
		public StateType State { get; private set; } = StateType.Idle;

		/// <summary>
		/// The current rotation of the pick, from -1 to 1.
		/// </summary>
		public float Rotation
		{
			get { return rotation; }
			set { rotation = Mathf.Clamp(value, -1f, 1f); }
		}

		/// <summary>
		/// The current rotation of the pick, normalized from 0 to 1.
		/// </summary>
		public float RotationNormalized
		{
			get { return (rotation + 1f) / 2f; }
		}

		/// <summary>
		/// Provides external control to notify the pick that it is being damaged.
		/// </summary>
		public bool Breaking
		{
			get { return State == StateType.Breaking; }
			set
			{
				if(State == StateType.Broken) { return; } // ignore if the pick is already broken
				StateType oldState = State;
				State = value ? StateType.Breaking : StateType.Idle;

				if(oldState != State)
				{
					OnStateChange?.Invoke(this, new StateChangeEventArgs { OldState = oldState });
				}
			}
		}

		/// <summary>
		/// Updates the pick data and state.
		/// </summary>
		public void Update()
		{
			float driveDelta = GetDriveDelta();
			StateType oldState = State;

			switch(State)
			{
				case StateType.Idle: // Idle state
					if(driveDelta != 0f)
					{
						State = StateType.Moving;
						OnStateChange?.Invoke(this, new StateChangeEventArgs { OldState = oldState });
						goto case StateType.Moving;
					}
					break;

				case StateType.Moving: // Moving state
					if(driveDelta == 0f)
					{
						State = StateType.Idle;
						OnStateChange?.Invoke(this, new StateChangeEventArgs { OldState = oldState });
						break;
					}
					Rotation += driveDelta;
					break;

				case StateType.Breaking: // Breaking state
					Life -= Degradation * Time.deltaTime;
					if(Life <= 0f)
					{
						State = StateType.Broken;
						OnStateChange?.Invoke(this, new StateChangeEventArgs { OldState = oldState });
					}
					break;

				case StateType.Broken: // Broken state
					//Reset(Degradation);
					break;
			}
		}

		/// <summary>
		/// Resets all necessary data of this pick to default values.
		/// </summary>
		/// <param name="degradation">See Degradation property.</param>
		public void Reset(float degradation)
		{
			Life = 1f;
			Degradation = degradation;
			State = StateType.Idle;
			Rotation = 0f;
		}

		/// <summary>
		/// Calculates and returns the drive delta value on the pick. The drive is the amount of "force" being applied
		/// to move the pick in a given direction.
		/// </summary>
		/// <returns>The drive delta value.</returns>
		private float GetDriveDelta() => MovementEnabled ? Input.GetAxis("Rotate Pick") * rotationSpeed * Time.deltaTime 
		                                                   : 0f;

		/// <summary>
		/// EventArgs class to provide the state change event with appropriate data. Namely the old state.
		/// </summary>
		public class StateChangeEventArgs : EventArgs { public StateType OldState; }

	} // class Pick
}