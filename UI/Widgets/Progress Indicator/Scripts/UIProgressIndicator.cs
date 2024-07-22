using UnityEngine;
using UnityEngine.UI;

public class UIProgressIndicator : UIWorldSpace
{
	[SerializeField]
	private Image progressFillImage;

	[SerializeField]
	private RectTransform transformToPop;

	private PopScaler popScaler;

	private bool shouldAnimateFill;

	private float currentFill;

	private float targetFill;

	public void UpdateDisplay(float progressPct, bool isMax = false)
	{
		if (isMax)
		{
			progressFillImage.fillAmount = 1f;
		}
		else
		{
			progressFillImage.fillAmount = progressPct;
		}
	}

	public void ShowMargin(float oldProgressPct, float newProgressPct, bool isMax = false)
	{
		shouldAnimateFill = true;
		currentFill = oldProgressPct;
		progressFillImage.fillAmount = currentFill;
		if (isMax || Mathf.Approximately(newProgressPct, 0f))
		{
			targetFill = 1f;
		}
		else if (newProgressPct < oldProgressPct)
		{
			targetFill = 1f + newProgressPct;
		}
		else
		{
			targetFill = newProgressPct;
		}
	}

	public void FlashDisplay()
	{
		popScaler = new PopScaler(0.2f);
	}

	private void Update()
	{
		if (shouldAnimateFill)
		{
			currentFill = ExtraMath.Damp(currentFill, targetFill, 9f, 0.005f, Time.deltaTime);
			if (Mathf.Approximately(currentFill, 1f))
			{
				progressFillImage.fillAmount = currentFill;
			}
			else
			{
				progressFillImage.fillAmount = currentFill % 1f;
			}
			if (Mathf.Approximately(currentFill, targetFill))
			{
				shouldAnimateFill = false;
			}
		}
		if (popScaler != null)
		{
			float t = popScaler.Update();
			transformToPop.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, t);
			if (popScaler.isComplete)
			{
				popScaler = null;
			}
		}
	}
}
