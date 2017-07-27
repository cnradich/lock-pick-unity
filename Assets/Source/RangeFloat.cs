using System;

using UnityEngine;


[Serializable]
public struct RangeFloat
{

	[SerializeField]
	private float r1;

	[SerializeField]
	private float r2;

	public RangeFloat(float value1, float value2)
	{
		r1 = value1;
		r2 = value2;
	}

	/// <summary>
	/// The smaller value of the range.
	/// </summary>
	public float Min
	{
		get
		{
			return r1 < r2 ? r1 : r2;
		}

		set
		{
			if(r1 < r2)
			{
				r1 = value;
			}
			else
			{
				r2 = value;
			}
		}
	}

	/// <summary>
	/// The larger value of the range.
	/// </summary>
	public float Max
	{
		get
		{
			return r1 > r2 ? r1 : r2;
		}

		set
		{
			if (r1 > r2)
			{
				r1 = value;
			}
			else
			{
				r2 = value;
			}
		}
	}

	/// <summary>
	/// The length of the range.
	/// </summary>
	public float Length
	{
		get
		{
			return Max - Min;
		}

		set
		{
			Max = Min + value;
		}
	}

	/// <summary>
	/// ToString
	/// </summary>
	/// <returns>String representation of the range.</returns>
	public override string ToString() => $"Range: {{{Min}, {Max}}}";
}
