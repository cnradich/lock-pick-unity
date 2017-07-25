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

	public override string ToString()
	{
		return String.Format("Range: {{0}, {1}}", Min, Max);
	}
}
