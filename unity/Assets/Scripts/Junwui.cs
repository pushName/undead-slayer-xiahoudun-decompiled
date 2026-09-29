using UnityEngine;

public class Junwui : MonoBehaviour
{
	private Transform mytransform;

	private Collider mycollider;

	private float power;

	private Vector3 rndpos;

	private Quaternion rotate;

	private Transform cha1;

	public Transform ef_splash;

	private Transform c_splash;

	private Animation myanimation;

	private bool attack_start = true;

	private Vector3 scalereduce;

	public Transform pt_body;

	private int attack_count;

	private AnimationState finish;

	public AudioClip snd_attack;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		myanimation = base.GetComponent<Animation>();
		myanimation["junwui_appear"].speed = 0.2f;
		myanimation["junwui_attack"].speed = 0.3f;
		myanimation["junwui_idle"].speed = 0.2f;
		myanimation["junwui_finish"].speed = 0.2f;
		myanimation["junwui_appear"].layer = 2;
		myanimation["junwui_attack"].layer = 1;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		InvokeRepeating("SetRndPosition", 0f, 2f);
		scalereduce = new Vector3(1f, -1f, 1f);
	}

	private void OnEnable()
	{
		myanimation.Stop();
		mycollider.enabled = true;
		myanimation.Play("junwui_appear");
		myanimation.Play("junwui_idle");
		{
			var emission = pt_body.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		mytransform.localScale = Vector3.one * 1.5f;
		attack_count = 0;
	}

	public void SetRndPosition()
	{
		rndpos = Random.onUnitSphere * 0.4f + cha1.position;
		rndpos[1] = 0f;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!attack_start && other.gameObject.layer == 8)
		{
			myanimation.Play("junwui_attack");
			attack_start = true;
			rotate = Quaternion.LookRotation(other.transform.position - mytransform.position);
		}
	}

	public void SetDamage(float _damage)
	{
		power = _damage;
	}

	private void Attack()
	{
		base.GetComponent<AudioSource>().clip = snd_attack;
		base.GetComponent<AudioSource>().Play();
		if (c_splash == null)
		{
			c_splash = (Transform)Object.Instantiate(ef_splash, mytransform.position, mytransform.rotation);
			c_splash.GetComponent<Rigidbody>().mass = power;
		}
		else
		{
			c_splash.position = mytransform.position;
			c_splash.rotation = mytransform.rotation;
			c_splash.gameObject.active = true;
		}
		mycollider.enabled = false;
		attack_count++;
		if (attack_count >= 12)
		{
			finish = myanimation.PlayQueued("junwui_finish");
			finish.speed = 0.2f;
			finish.layer = 1;
		}
	}

	private void Update()
	{
		if (myanimation.IsPlaying("junwui_finish"))
		{
			mytransform.localScale -= scalereduce * Time.deltaTime * 3f;
			if ((double)mytransform.localScale.x < 0.1)
			{
				mytransform.position = Vector3.one * 20f;
				{
					var emission = pt_body.GetComponent<ParticleSystem>().emission;
					emission.enabled = false;
				}
				base.gameObject.active = false;
			}
		}
		else if (myanimation.IsPlaying("junwui_appear"))
		{
			attack_start = true;
		}
		else if (myanimation.IsPlaying("junwui_attack"))
		{
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, rotate, Time.deltaTime * 8f);
			attack_start = true;
		}
		else if (myanimation.IsPlaying("junwui_idle"))
		{
			if (attack_start)
			{
				attack_start = false;
				mycollider.enabled = true;
			}
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(rndpos - mytransform.position), Time.deltaTime * 2f);
			mytransform.position = Vector3.Lerp(mytransform.position, rndpos, Time.deltaTime);
		}
	}
}
