using UnityEngine;

public class Splash_angel_poison : MonoBehaviour
{
	private Transform mytransform;

	private float delay = 2.5f;

	private void Awake()
	{
		mytransform = base.transform;
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		delay = 2.5f;
	}

	private void Update()
	{
		if (delay < 0f)
		{
			mytransform.position -= Vector3.up * Time.deltaTime * 0.05f;
			if (mytransform.position.y < -0.05f)
			{
				base.gameObject.active = false;
			}
		}
		else
		{
			delay -= Time.deltaTime;
		}
	}
}
