using UnityEngine;

public class Screen_effect : MonoBehaviour
{
	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.one * 6.5f;
	}

	private void Update()
	{
		if (mytransform.localScale.x < 10f)
		{
			mytransform.localScale += Vector3.one * Time.deltaTime * 6f;
		}
		else
		{
			base.gameObject.active = false;
		}
	}
}
