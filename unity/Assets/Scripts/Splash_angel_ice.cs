using UnityEngine;

public class Splash_angel_ice : MonoBehaviour
{
	private Transform mytransform;

	private Vector3 targetPos;

	private float delay = 2.5f;

	private void Awake()
	{
		mytransform = base.transform;
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		delay = 2.5f;
		targetPos = mytransform.position;
		Vector3 position = targetPos;
		position.y = -0.4f;
		mytransform.position = position;
	}

	private void Update()
	{
		if (delay < 0f)
		{
			mytransform.position -= Vector3.up * Time.deltaTime * 2f;
			if (mytransform.position.y < -0.4f)
			{
				mytransform.position = Vector3.up * 12f;
				base.gameObject.active = false;
			}
		}
		else
		{
			mytransform.position = Vector3.MoveTowards(mytransform.position, targetPos, Time.deltaTime * 2f);
			delay -= Time.deltaTime;
		}
	}
}
