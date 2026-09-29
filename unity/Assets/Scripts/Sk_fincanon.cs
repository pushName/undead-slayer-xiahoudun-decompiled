using UnityEngine;

public class Sk_fincanon : MonoBehaviour
{
	public Transform beam;

	private Transform mytransform;

	private float delay;

	private CapsuleCollider beamcollider;

	private Vector3 originscale = new Vector3(1f, 1f, 0.1f);

	private bool stat;

	private void Awake()
	{
		mytransform = base.transform;
		base.GetComponent<Animation>()["wait"].speed = 1.2f;
		base.GetComponent<Animation>()["creat"].speed = 0.25f;
		base.GetComponent<Animation>()["destroy"].speed = 0.25f;
		base.GetComponent<Animation>()["beam"].speed = 0.8f;
		beam.gameObject.active = false;
		beamcollider = (CapsuleCollider)beam.GetComponent<Collider>();
	}

	private void Start()
	{
		beam.GetComponent<Rigidbody>().mass = base.GetComponent<Rigidbody>().mass;
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.one * 1.5f;
		stat = false;
		beam.localScale = Vector3.zero;
		delay = 0f;
		beamcollider.enabled = false;
		beamcollider.height = 0f;
		InvokeRepeating("Shoot", 0.1f, 0.2f);
		beam.gameObject.active = true;
		base.GetComponent<Animation>().Play("wait");
	}

	public void Shoot()
	{
		if (delay > 0.5f)
		{
			beamcollider.enabled = false;
			beamcollider.enabled = true;
		}
	}

	private void Update()
	{
		delay += Time.deltaTime;
		if (delay > 3.4f)
		{
			base.gameObject.active = false;
			mytransform.localScale = Vector3.zero;
			beam.gameObject.active = false;
			CancelInvoke("Shoot");
		}
		else if (delay > 2.2f)
		{
			base.GetComponent<Animation>().Play("destroy");
			if (!stat)
			{
				beamcollider.enabled = false;
				stat = true;
			}
			beam.localScale = Vector3.Lerp(beam.localScale, Vector3.forward, Time.deltaTime * 15f);
		}
		else if (delay > 1f)
		{
			beam.localScale = Vector3.Lerp(beam.localScale, Vector3.one, Time.deltaTime * 15f);
			base.GetComponent<Animation>().Play("beam");
		}
		else if (delay > 0.8f)
		{
			beamcollider.height = Mathf.Lerp(beamcollider.height, 0.7f, Time.deltaTime * 6f);
			beam.localScale = Vector3.Lerp(beam.localScale, originscale, Time.deltaTime * 6f);
			stat = false;
		}
		else if (delay > 0.2f)
		{
			if (!stat)
			{
				mytransform.parent = null;
				stat = true;
			}
			base.GetComponent<Animation>().Play("creat");
		}
	}
}
