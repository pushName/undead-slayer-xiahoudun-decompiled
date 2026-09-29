using UnityEngine;

public class Sk_chainbreak : MonoBehaviour
{
	private ParticleSystem myparticleEmitter;

	private ParticleSystem.EmissionModule myemission;

	private ParticleSystem.EmissionModule pt_exp_emission;

	private Transform mytransform;

	private Transform pt_exp;

	private float delay_exp;

	private short p_step;

	private SphereCollider childcollider;

	private Collider mycollider;

	private void Awake()
	{
		myparticleEmitter = base.GetComponent<ParticleSystem>();
		myemission = myparticleEmitter.emission;
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		pt_exp = mytransform.GetChild(0);
		childcollider = pt_exp.GetComponent<Collider>() as SphereCollider;
		pt_exp_emission = pt_exp.GetComponent<ParticleSystem>().emission;
	}

	private void Start()
	{
		pt_exp.GetComponent<Rigidbody>().mass = base.GetComponent<Rigidbody>().mass;
		pt_exp.gameObject.active = false;
	}

	private void OnEnable()
	{
		myparticleEmitter.Clear();
		mycollider.enabled = false;
		myemission.enabled = true;
		delay_exp = 0f;
		InvokeRepeating("RepeatDamage", 0.1f, 0.4f);
		p_step = 0;
		base.GetComponent<AudioSource>().Play();
	}

	private void RepeatDamage()
	{
		mycollider.enabled = false;
		mycollider.enabled = true;
	}

	private void Update()
	{
		delay_exp += Time.deltaTime;
		if (mytransform.position.y < 0.1f)
		{
			mytransform.position += Vector3.up * 0.6f * Time.deltaTime;
		}
		if (p_step == 2)
		{
			if (delay_exp > 4f)
			{
				base.gameObject.active = false;
				pt_exp.gameObject.active = false;
				mytransform.position = Vector3.one * 30f;
				p_step = 0;
			}
		}
		else if (p_step == 1)
		{
			if (delay_exp > 2.5f)
			{
				pt_exp_emission.enabled = false;
				childcollider.enabled = false;
				childcollider.radius = 0.01f;
				p_step = 2;
			}
		}
		else if (p_step == 0 && delay_exp > 2f)
		{
			mycollider.enabled = false;
			CancelInvoke();
			p_step = 1;
			myparticleEmitter.Clear();
			myemission.enabled = false;
			pt_exp.gameObject.active = true;
			pt_exp_emission.enabled = true;
			childcollider.enabled = true;
			childcollider.radius = 0.7f;
		}
	}
}
