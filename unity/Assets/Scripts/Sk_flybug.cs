using UnityEngine;

public class Sk_flybug : MonoBehaviour
{
	private Transform mytransform;

	private float delay;

	private Collider mycollider;

	private bool shootfinish;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		shootfinish = false;
		delay = 0f;
		InvokeRepeating("Shoot", 0.5f, 0.3f);
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		{
			var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		mycollider.enabled = false;
	}

	public void Shoot()
	{
		mycollider.enabled = false;
		mycollider.enabled = true;
	}

	private void Update()
	{
		delay += Time.deltaTime;
		if (delay > 6f)
		{
			base.gameObject.active = false;
			mytransform.GetChild(0).gameObject.active = false;
		}
		else if (delay > 3f && !shootfinish)
		{
			{
				var emission = base.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			{
				var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			CancelInvoke("Shoot");
			mycollider.enabled = false;
			shootfinish = true;
		}
	}
}
