using UnityEngine;

public class Touch_zoom : MonoBehaviour
{
	private bool iszoom;

	private UI_map script_ui;

	private void Start()
	{
		script_ui = GetComponent<UI_map>();
	}

	private void Update()
	{
		float num = 0f;
		float num2 = 0f;
		if (iszoom || Input.touchCount != 2)
		{
			return;
		}
		if (Input.GetTouch(1).phase == TouchPhase.Began)
		{
			num = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
		}
		else if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
		{
			iszoom = false;
		}
		else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
		{
			num2 = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
			if (Mathf.Abs(num2 - num) > 1f)
			{
				iszoom = true;
				script_ui.Zoom_finger(_zoomout: true, Vector3.one);
			}
		}
	}
}
