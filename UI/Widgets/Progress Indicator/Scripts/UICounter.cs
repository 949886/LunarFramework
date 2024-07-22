using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UICounter : UIProgressIndicator
{
	[Header("Bank Counter")]
	[SerializeField]
	private TMP_Text countText;

	[SerializeField]
	private Color maxLimitTextColor = Color.red;

	public Graphic breakIndicator;

	private Color originalCountColor;

	protected override void Awake()
	{
		base.Awake();
		originalCountColor = countText.color;
	}

	public void UpdateDisplay(int count, float progressPct, bool isMax = false)
	{
		countText.color = (isMax ? maxLimitTextColor : originalCountColor);
		countText.text = count.ToString();
		UpdateDisplay(progressPct, isMax);
	}
}
