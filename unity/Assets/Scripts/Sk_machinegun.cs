using UnityEngine;

public class Sk_machinegun : MonoBehaviour
{
	private Transform mytransform;

	private float delay;

	private Collider mycollider;

	private bool shooton;

	public Transform beam1;

	public Transform beam2;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		base.GetComponent<Animation>()["shoot"].speed = 0.2f;
		base.GetComponent<Animation>()["creat"].speed = 0.5f;
		base.GetComponent<Animation>()["destroy"].speed = 0.4f;
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		shooton = false;
		delay = 0f;
		InvokeRepeating("Shoot", 0.8f, 0.2f);
		base.GetComponent<Animation>().Play("creat");
		{
			var emission = beam1.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
		{
			var emission = beam2.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
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
		if (delay > 4.5f)
		{
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 40f;
			mytransform.parent = null;
		}
		else if (delay > 4f)
		{
			{
				var emission = beam1.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			{
				var emission = beam2.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			base.GetComponent<Animation>().Play("destroy");
			CancelInvoke("Shoot");
			mycollider.enabled = false;
		}
		else if (delay > 0.6f && !shooton)
		{
			shooton = true;
			{
				var emission = beam1.GetComponent<ParticleSystem>().emission;
				emission.enabled = true;
			}
			{
				var emission = beam2.GetComponent<ParticleSystem>().emission;
				emission.enabled = true;
			}
			base.GetComponent<Animation>().Play("shoot");
		}
	}
}
