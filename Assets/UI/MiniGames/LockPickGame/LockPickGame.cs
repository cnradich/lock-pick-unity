using UnityEngine;

/// <summary>
/// LockPickGame
/// </summary>
public class LockPickGame : MonoBehaviour
{
	[SerializeField]
	private Animator cylinderAnimator;

	[SerializeField]
	private Animator pickAnimator;

	[SerializeField]
	[Range(0, 1)]
	private float maxSolutionZeroBias = .5f;

	[SerializeField]
	private float minSolutionRange = .1f;

	[SerializeField]
	private float maxSolutionRange = .5f;

	[SerializeField]
	private float minSolutionFalloff = .2f;

	[SerializeField]
	private float maxSolutionFalloff = 1f;

	[SerializeField]
	private float minPickDegradation = .1f;

	[SerializeField]
	private float maxPickDegradation = 1f;

	[SerializeField]
	private float pickRotationSpeed = 2.5f;

	[SerializeField]
	private float cylinderRotationSpeed = 1.5f;

	private int lockDifficulty;

	private float solutionCenter;

	private float solutionRange;

	private float solutionFalloff;

	private float pickDegradation;

	private float pickLife = 1f;

	private float pickRotation = 0f;

	private float cylinderRotation = 0f;

	private int cylinderRotationLayer;

	private int pickRotationLayer;

	private int pickFeedbackLayer;


	/// <summary>
	/// Initialize
	/// </summary>
	private void Start()
	{
		pickRotationLayer = pickAnimator.GetLayerIndex("Rotation");
		pickFeedbackLayer = pickAnimator.GetLayerIndex("Feedback");
		//pickAnimator.speed = 0f;
		//cylinderAnimator.speed = 0f;

		InitLock(50);
	}

	/// <summary>
	/// Update
	/// </summary>
	private void Update()
	{
		float maxCylinderRotation = DetermineMaxCylinderRotation();
		//Debug.Log("Max: " + maxCylinderRotation);

		float cylinderRotationDelta;
		float pickRotationDelta;
		GetRotations(out cylinderRotationDelta, out pickRotationDelta);

		// true if the player is actively giving tension
		bool pushing = cylinderRotationDelta > 0f;

		bool breakingPick = cylinderRotation >= maxCylinderRotation && maxCylinderRotation < 1f && pushing;

		if(breakingPick)
		{
			pickRotationDelta = 0f;
			pickAnimator.Play("A_LockPick_Shake", pickFeedbackLayer);
		}
		else
		{
			pickAnimator.Play("Empty", pickFeedbackLayer);
		}

		pickRotation += pickRotationDelta;
		pickRotation = Mathf.Clamp(pickRotation, -1f, 1f);
		float lockPickRotationNormalized = (pickRotation + 1f) / 2f;

		pickAnimator.Play("A_LockPick_Rotate", pickRotationLayer, lockPickRotationNormalized);

		if(pushing)
		{
			maxCylinderRotation = maxCylinderRotation > cylinderRotation ? maxCylinderRotation : cylinderRotation;
		}
		else
		{
			maxCylinderRotation = 1f;
		}

		cylinderRotation += cylinderRotationDelta;
		cylinderRotation = Mathf.Clamp(cylinderRotation, 0f, maxCylinderRotation);

		cylinderAnimator.Play("A_LockCylinder_Rotate", cylinderRotationLayer, cylinderRotation);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="cylinderRotationDelta"></param>
	/// <param name="pickRotationDelta"></param>
	private void GetRotations(out float cylinderRotationDelta, out float pickRotationDelta)
	{
		pickRotationDelta = Input.GetAxis("Rotate Pick") * pickRotationSpeed * Time.deltaTime;

		float cylinderRotationInput = Input.GetAxis("Rotate Cylinder");
		if(cylinderRotationInput == 0f)
		{
			cylinderRotationInput = -1f;
		}
		cylinderRotationDelta = cylinderRotationInput * cylinderRotationSpeed * Time.deltaTime;
	}

	private float DetermineMaxCylinderRotation()
	{
		// offset the pick rotation value to reflect a virtually centered solution
		float offset = 0f - solutionCenter;
		float zeroedPickRotation = Mathf.Abs(pickRotation + offset);

		// normalize the pick rotation relative to the falloff range
		float normalizedPickRotation = zeroedPickRotation - solutionRange;
		normalizedPickRotation = Mathf.Clamp(normalizedPickRotation * (1f / solutionFalloff), 0f, 1f);

		return 1f - normalizedPickRotation;
	}

	private void InitLock(int difficulty)
	{
		float normalizedDifficulty = Mathf.Clamp(difficulty, 0, 100) / 100f;

		// Pick the center point of the solution
		solutionCenter = Random.Range(-1f, 1f);
		Debug.Log("Center: " + solutionCenter);
		// Apply zero bias to center point
		float sign = solutionCenter >= 0 ? 1f : -1f;
		float solutionCenterHeavyBias = Mathf.Pow(solutionCenter, 10);
		float zeroBias = Mathf.Lerp(0f, maxSolutionZeroBias, normalizedDifficulty);
		solutionCenter = Mathf.Lerp(Mathf.Abs(solutionCenter), solutionCenterHeavyBias, zeroBias) * sign;
		Debug.Log("Bias Center: " + solutionCenter);

		// Get valid solution range
		solutionRange = Mathf.Lerp(minSolutionRange, maxSolutionRange, normalizedDifficulty);

		// Get valid solution falloff
		solutionFalloff = Mathf.Lerp(minSolutionFalloff, maxSolutionFalloff, normalizedDifficulty);

		// Get pick degradation
		pickDegradation = Mathf.Lerp(minPickDegradation, maxPickDegradation, normalizedDifficulty);

		//Debug.Log("Center: " + solutionCenter);
		//Debug.Log("Range: " + solutionRange);
		//Debug.Log("Falloff: " + solutionFalloff);
	}


}