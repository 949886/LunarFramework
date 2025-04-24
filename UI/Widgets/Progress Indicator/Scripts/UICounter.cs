using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UICounter : UIProgressIndicator
{
	[Header("Bank Counter")]
	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private Color maxLimitTextColor = Color.red;

	public Graphic breakIndicator;

	private Color originalCountColor;

	protected void Awake()
	{
		originalCountColor = countText.color;
	}

	public void UpdateDisplay(float progressPct, bool isMax = false)
	{
		if (progressPct > 1f) progressPct = 1f;
		
		countText.color = (isMax ? maxLimitTextColor : originalCountColor);
		countText.text = Mathf.Floor(progressPct * 100).ToString();
		base.UpdateDisplay(progressPct, isMax);
	}
}
