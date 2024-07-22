using UnityEngine;

public class UIWorldSpace : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Zero duration = doesn't timeout")]
	private float displayDuration = 1f;

	[SerializeField]
	private LayerMask blockingMask = 0;

	[SerializeField]
	[Tooltip("Optional: used to hide when off-screen temporarily")]
	private CanvasGroup visibleGroup;

	private float displayTimer;

	private bool isRelativePosition;

	private Transform relativeTf;

	private Vector3 localPosition;

	private Vector3 camOffset;

	protected Camera mainCamera { get; private set; }

	public static RectTransform worldUIRect { get; set; }

	private bool canBeHidden
	{
		get
		{
			return blockingMask.value != 0;
		}
	}

	protected virtual void Awake()
	{
		mainCamera = Camera.main;
	}

	public void SetPosition(Transform relativeTf, Vector3 localPosition, Vector3 camOffset)
	{
		isRelativePosition = relativeTf != null;
		this.relativeTf = relativeTf;
		this.localPosition = localPosition;
		this.camOffset = camOffset;
		base.transform.SetParent(worldUIRect, false);
		if (!canBeHidden || IsVisible())
		{
			UpdateScreenPosition();
		}
		else
		{
			visibleGroup.alpha = 0f;
			visibleGroup.blocksRaycasts = false;
		}
		displayTimer = 0f;
	}

	public void SetPosition(Transform relativeTf, Vector3 localPosition)
	{
		SetPosition(relativeTf, localPosition, Vector3.zero);
	}

	public void SetPosition(Vector3 worldPosition)
	{
		SetPosition(null, worldPosition, Vector3.zero);
	}

	public void RefreshDuration()
	{
		displayTimer = 0f;
	}

	private bool IsVisible()
	{
		if (canBeHidden)
		{
			Vector3 position = mainCamera.transform.position;
			Vector3 direction = GetWorldPosition() - position;
			RaycastHit hitInfo;
			if (Physics.Raycast(position, direction, out hitInfo, direction.magnitude, blockingMask.value))
			{
				if (relativeTf != null && hitInfo.transform.IsChildOf(relativeTf))
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}

	protected Vector3 GetWorldPosition()
	{
		Vector3 vector = localPosition;
		if (relativeTf != null)
		{
			vector = relativeTf.position + relativeTf.rotation * localPosition;
		}
		return vector + mainCamera.transform.rotation * camOffset;
	}

	private void UpdateScreenPosition()
	{
		(base.transform as RectTransform).anchoredPosition = GetWorldUIPosition(GetWorldPosition());
	}

	private Vector2 GetWorldUIPosition(Vector3 worldPos)
	{
		Vector3 vector = mainCamera.WorldToScreenPoint(worldPos);
		Vector2 localPoint;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(worldUIRect, vector, null, out localPoint))
		{
			return localPoint;
		}
		return Vector2.zero;
	}

	private void LateUpdate()
	{
		if (isRelativePosition && relativeTf == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (displayDuration > 0f)
		{
			displayTimer += Time.deltaTime;
			if (displayTimer >= displayDuration)
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}
		if (canBeHidden)
		{
			bool flag = IsVisible();
			visibleGroup.alpha = (flag ? 1f : 0f);
			visibleGroup.blocksRaycasts = flag;
			if (flag)
			{
				UpdateScreenPosition();
			}
		}
		else
		{
			UpdateScreenPosition();
		}
	}
}
