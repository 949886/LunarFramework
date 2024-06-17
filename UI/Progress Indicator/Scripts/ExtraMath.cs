using System;
using UnityEngine;

public static class ExtraMath
{
	public static float Damp(float a, float b, float lambda, float epsilon, float dt)
	{
		if (Mathf.Abs(a - b) > epsilon)
		{
			return Mathf.Lerp(a, b, 1f - Mathf.Exp((0f - lambda) * dt));
		}
		return b;
	}

	public static Vector3 DampVector(Vector3 a, Vector3 b, float lambda, float epsilon, float dt)
	{
		return new Vector3(Damp(a.x, b.x, lambda, epsilon, dt), Damp(a.y, b.y, lambda, epsilon, dt), Damp(a.z, b.z, lambda, epsilon, dt));
	}

	public static Vector3 DampVector(Vector3 a, Vector3 b, Vector3 lambda, float epsilon, float dt)
	{
		return new Vector3(Damp(a.x, b.x, lambda.x, epsilon, dt), Damp(a.y, b.y, lambda.y, epsilon, dt), Damp(a.z, b.z, lambda.z, epsilon, dt));
	}

	public static bool GetLaunchVelocityFromAngle(Vector3 from, Vector3 to, float angle, float g, out Vector3 launchVelocity)
	{
		float f = angle * ((float)Math.PI / 180f);
		Vector3 vector = to - from;
		vector.y = 0f;
		float num = to.y - from.y;
		float num2 = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z) / Mathf.Cos(f);
		float num3 = 0.5f * Mathf.Abs(g) * (num2 * num2) / (num2 * Mathf.Sin(f) - num);
		if (num3 > 0f)
		{
			Vector3 axis = Vector3.Cross(vector, new Vector3(0f, 1f, 0f));
			launchVelocity = (Quaternion.AngleAxis(angle, axis) * vector).normalized * Mathf.Sqrt(num3);
			return true;
		}
		launchVelocity = vector;
		return false;
	}

	public static bool GetLaunchVelocityFromSpeed(Vector3 from, Vector3 to, float speed, bool useHighArc, float g, out Vector3 launchVelocity)
	{
		Vector3 vector = to - from;
		vector.y = 0f;
		g = Mathf.Abs(g);
		float num = to.y - from.y;
		float num2 = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
		float num3 = speed * speed * speed * speed - g * (g * (num2 * num2) + 2f * num * (speed * speed));
		bool result = true;
		if (num3 > 0f)
		{
			num3 = ((!useHighArc) ? (Mathf.Atan((speed * speed - Mathf.Sqrt(num3)) / (g * num2)) * 57.29578f) : (Mathf.Atan((speed * speed + Mathf.Sqrt(num3)) / (g * num2)) * 57.29578f));
		}
		else if (num2 == 0f)
		{
			num3 = ((!(num < 0f)) ? 90f : (-90f));
		}
		else
		{
			num3 = 45f;
			result = false;
		}
		Vector3 axis = Vector3.Cross(vector, new Vector3(0f, 1f, 0f));
		launchVelocity = (Quaternion.AngleAxis(num3, axis) * vector).normalized * speed;
		return result;
	}
}
