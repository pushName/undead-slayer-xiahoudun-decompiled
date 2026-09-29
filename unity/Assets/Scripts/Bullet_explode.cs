using UnityEngine;

public class Bullet_explode : MonoBehaviour
{
	private Transform pt1;

	private Transform pt2;

	private Transform ptm;

	private float c_delay;

	private Transform mytransform;

	private bool c_emit;

	public float distance;

	private void Awake()
	{
		mytransform = base.transform;
		pt1 = mytransform.GetChild(1);
		pt2 = mytransform.GetChild(2);
		ptm = mytransform.GetChild(0);
		ptm.GetComponent<Animation>()["dt1"].speed = 0.35f;
	}

	private void OnEnable()
	{
		{
			var emission = pt1.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		{
			var emission = pt2.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		ptm.GetComponent<Animation>().Play("dt1");
		base.GetComponent<Collider>().enabled = true;
		mytransform.position += mytransform.forward * distance;
	}

	private void Update()
	{
		c_delay += Time.deltaTime;
		if (c_delay > 1.5f)
		{
			ptm.GetComponent<Animation>().Stop("dt1");
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 25f;
			c_delay = 0f;
			c_emit = false;
		}
		else if (!c_emit && c_delay > 0.5f)
		{
			{
				var emission = pt1.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			{
				var emission = pt2.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			base.GetComponent<Collider>().enabled = false;
			c_emit = true;
		}
	}
}
