using UnityEngine;

public class Pet_horse : MonoBehaviour
{
	private float horsedistance;

	private Transform cha1;

	private bool rideon;

	private bool skillon;

	private bool disappear;

	private float disappeardelay;

	private Cha_Control script_cha1;

	public AudioClip horse_cry;

	private Transform mytransform;

	private AnimationState temp;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		base.GetComponent<Animation>()["horse_run"].layer = 1;
		base.GetComponent<Animation>()["horse_stand"].layer = 1;
		base.GetComponent<Animation>()["horse_cry"].layer = 1;
		base.GetComponent<Animation>()["horse_jump"].layer = 1;
		base.GetComponent<Animation>()["horse_run"].speed = 0.6f;
		base.GetComponent<Animation>()["horse_stand"].speed = 0.1f;
		base.GetComponent<Animation>()["horse_cry"].speed = 0.32f;
		base.GetComponent<Animation>()["horse_jump"].speed = 0.3f;
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha1 = cha1.GetComponent<Cha_Control>();
		base.GetComponent<Animation>().Play("horse_stand");
	}

	public void Rideing(bool _stop)
	{
		if (_stop)
		{
			rideon = false;
			mytransform.parent = null;
			mytransform.position = Vector3.one * 71f;
		}
		else
		{
			rideon = true;
			mytransform.parent = cha1;
			mytransform.position = mytransform.root.position;
			mytransform.rotation = mytransform.root.rotation;
		}
	}

	public void GetOffHorse()
	{
		rideon = false;
		mytransform.parent = null;
		base.GetComponent<Animation>().Stop();
		base.GetComponent<Animation>().Play("horse_cry");
		temp = base.GetComponent<Animation>().PlayQueued("horse_run", QueueMode.CompleteOthers);
		temp.speed = base.GetComponent<Animation>()["horse_run"].speed;
		disappear = true;
		base.GetComponent<AudioSource>().Stop();
	}

	public void RideandCry()
	{
		mytransform.parent = cha1;
		mytransform.position = mytransform.root.position;
		mytransform.rotation = mytransform.root.rotation;
		base.GetComponent<Animation>().Stop();
		base.GetComponent<AudioSource>().PlayOneShot(horse_cry);
		base.GetComponent<Animation>().Play("horse_cry");
		temp = base.GetComponent<Animation>().PlayQueued("horse_stand");
		temp.speed = base.GetComponent<Animation>()["horse_stand"].speed;
	}

	public void RunStart(bool a)
	{
		if (a)
		{
			base.GetComponent<Animation>().Play("horse_run");
			base.GetComponent<AudioSource>().Play();
		}
		else
		{
			base.GetComponent<Animation>().CrossFade("horse_stand");
			base.GetComponent<AudioSource>().Stop();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.IsChildOf(cha1) && !rideon)
		{
			script_cha1.RideHorse();
			rideon = true;
		}
	}

	public void SkillOn()
	{
		base.gameObject.active = true;
		float f = Random.Range(0, 360);
		Vector3 position = cha1.position + new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * 2f;
		mytransform.position = position;
		skillon = true;
		disappear = false;
		Vector3 vector = cha1.position - mytransform.position;
		vector = new Vector3(vector.x, 0f, vector.z);
		if (vector != Vector3.zero)
		{
			mytransform.rotation = Quaternion.LookRotation(vector);
		}
		base.GetComponent<Animation>().Play("horse_run");
	}

	private void Update()
	{
		if (disappear && !base.GetComponent<Animation>().IsPlaying("horse_cry"))
		{
			if (disappeardelay < 3f)
			{
				disappeardelay += Time.deltaTime;
				mytransform.position += mytransform.forward * Time.deltaTime * 0.8f;
				return;
			}
			disappear = false;
			disappeardelay = 0f;
			mytransform.position = Vector3.one * 4f;
			base.gameObject.active = false;
		}
		else if (skillon)
		{
			horsedistance = Vector3.Distance(cha1.position, mytransform.position);
			if (horsedistance <= 1.5f && !rideon)
			{
				base.GetComponent<Animation>().Stop();
				base.GetComponent<Animation>().Play("horse_jump");
				temp = base.GetComponent<Animation>().PlayQueued("horse_stand");
				temp.speed = base.GetComponent<Animation>()["horse_stand"].speed;
				rideon = true;
			}
			if (!base.GetComponent<Animation>().IsPlaying("horse_stand"))
			{
				mytransform.position = Vector3.Lerp(mytransform.position, cha1.position, Time.deltaTime * 3.5f);
			}
			else
			{
				skillon = false;
			}
		}
	}
}
