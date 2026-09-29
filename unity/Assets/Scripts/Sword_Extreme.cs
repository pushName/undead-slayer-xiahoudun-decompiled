using UnityEngine;

public class Sword_Extreme : MonoBehaviour
{
	private float delay;

	private bool efOn;

	private Transform mytransform;

	private Transform cha1;

	private Collider mycollider;

	private Vector3 startPos;

	private void Awake()
	{
		mytransform = base.transform;
		cha1 = GameObject.FindWithTag("Player").transform;
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		delay = 1.5f;
		efOn = false;
		base.gameObject.layer = 16;
		mycollider.enabled = true;
		startPos = mytransform.position;
	}

	public void SetPower(int _amount)
	{
		base.GetComponent<Rigidbody>().mass = (float)_amount * 0.4f;
	}

	private void ColliderOn()
	{
		mycollider.enabled = false;
		mycollider.enabled = true;
	}

	private void Update()
	{
		delay -= Time.deltaTime;
		if ((double)delay < -1.5)
		{
			base.GetComponent<ParticleSystem>().Clear();
			base.gameObject.active = false;
		}
		else if (delay < -1f)
		{
			CancelInvoke();
		}
		else if (delay < 0f)
		{
			mytransform.position = Vector3.Lerp(mytransform.position, cha1.position + Vector3.up * 0.2f, Time.deltaTime * 3.5f);
			if (!efOn)
			{
				mytransform.position = startPos;
				base.gameObject.layer = 17;
				InvokeRepeating("ColliderOn", 0.01f, 0.1f);
				efOn = true;
				base.GetComponent<ParticleSystem>().Play();
			}
		}
		else
		{
			mytransform.position = cha1.position;
		}
	}
}
