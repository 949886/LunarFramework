using System;
using UnityEngine;

public sealed class PopScaler
{
	private float t;

	private float duration;

	private const float SIN_END = 150f;

	private float sinEndValue;

	public bool isComplete
	{
		get
		{
			return t >= duration;
		}
	}

	public PopScaler(float duration)
	{
		t = 0f;
		this.duration = duration;
		sinEndValue = Mathf.Sin(2.6179938f);
	}

	public void Reset()
	{
		t = 0f;
	}

	public float Update()
	{
		t += Time.deltaTime;
		return Mathf.Sin(Mathf.Clamp01(t / duration) * 150f * ((float)Math.PI / 180f)) / sinEndValue;
	}
}
