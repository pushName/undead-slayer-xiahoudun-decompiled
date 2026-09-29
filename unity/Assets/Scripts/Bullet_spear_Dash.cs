using UnityEngine;

public class Bullet_spear_Dash : MonoBehaviour
{
	private Transform mytransform;

	private Vector3 originscale;

	private float delay;

	private bool shoot;

	private void Awake()
	{
		mytransform = base.transform;
		originscale = mytransform.localScale;
	}

	private void OnEnable()
	{
		mytransform.localScale = originscale;
		delay = 0f;
		shoot = false;
	}

	private void Update()
	{
		if (delay < 0.4f)
		{
			delay += Time.deltaTime;
		}
		else if (!shoot)
		{
			mytransform.position += mytransform.forward * Time.deltaTime * 1.5f;
			mytransform.localScale += Vector3.forward * 30f * Time.deltaTime;
			if (mytransform.localScale.z > 3f)
			{
				mytransform.localScale = Vector3.one * 2f + Vector3.forward * 2f;
				shoot = true;
			}
		}
		else if (shoot)
		{
			mytransform.localScale -= Vector3.forward * 3f * Time.deltaTime;
			mytransform.position += mytransform.forward * Time.deltaTime * 0.5f;
			if (mytransform.localScale.z < 0.1f)
			{
				base.gameObject.active = false;
			}
		}
	}
}
