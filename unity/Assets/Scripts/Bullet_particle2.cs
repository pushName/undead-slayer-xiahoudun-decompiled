using UnityEngine;

public class Bullet_particle2 : MonoBehaviour
{
	public float emitfinishdelay;

	public float disable_delay;

	public float expand_factor;

	private Transform mytransform;

	private Vector3 originscale;

	private void Awake()
	{
		mytransform = base.transform;
		originscale = mytransform.localScale;
	}

	private void OnEnable()
	{
		Invoke("StopEmit", emitfinishdelay);
		Invoke("finishObj", disable_delay);
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		mytransform.localScale = originscale;
		base.GetComponent<Collider>().enabled = true;
	}

	private void StopEmit()
	{
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
		base.GetComponent<Collider>().enabled = false;
	}

	private void finishObj()
	{
		base.gameObject.active = false;
	}

	private void Update()
	{
		mytransform.localScale += Vector3.one * Time.deltaTime * expand_factor;
	}
}
