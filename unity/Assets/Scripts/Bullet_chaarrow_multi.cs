using UnityEngine;

public class Bullet_chaarrow_multi : MonoBehaviour
{
	private Transform mytransform;

	private float finish_delay;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (finish_delay > 0.2f)
		{
			base.gameObject.active = false;
			finish_delay = 0f;
			mytransform.localScale = Vector3.one * 0.1f;
		}
		else
		{
			finish_delay += Time.deltaTime;
			mytransform.localScale += 4f * Vector3.one * Time.deltaTime;
			mytransform.position += 4f * base.transform.forward * Time.deltaTime;
		}
	}
}
