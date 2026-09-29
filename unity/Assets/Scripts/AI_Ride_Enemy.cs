using UnityEngine;

public class AI_Ride_Enemy : MonoBehaviour
{
	private Transform mytransform;

	private Transform cha1;

	private Vector3 rndposition = Vector3.zero;

	private bool life = true;

	private float attackrangeZ;

	private float attackrangeX;

	private float attackdelay;

	private Transform ef_hit;

	public Transform mon;

	private SoundEf_slash script_sound;

	private Cam_Move_ride script_cam;

	private Cha_Control_ride_cha script_cha1;

	private AI_Ride_Enemy2 script_mon;

	private void Awake()
	{
		mytransform = base.transform;
		script_mon = mon.GetComponent<AI_Ride_Enemy2>();
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha1 = cha1.GetComponent<Cha_Control_ride_cha>();
		base.GetComponent<Animation>()["down"].speed = 0.3f;
		base.GetComponent<Animation>().Play("move");
		InvokeRepeating("SetRndPosition", Random.Range(0.1f, 3f), Random.Range(1f, 2f));
		base.gameObject.active = false;
		script_mon.Defeat();
		script_sound = GameObject.FindWithTag("sound").GetComponent<SoundEf_slash>();
		script_cam = Camera.main.GetComponent<Cam_Move_ride>();
		ef_hit = GameObject.FindWithTag("icon_skill").transform;
	}

	private void OnEnable()
	{
		base.GetComponent<Animation>().Play("move");
		life = true;
		script_mon.Wake();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.IsChildOf(cha1))
		{
			life = false;
			base.GetComponent<Animation>().Play("down");
			script_sound.SoundOn(3);
			script_cam.Hitcam();
			ef_hit.gameObject.active = true;
			ef_hit.position = mytransform.position + Vector3.up * 0.05f;
			script_cha1.GetSoulStone(3);
			script_mon.Defeat();
		}
	}

	public void SetRndPosition()
	{
		rndposition = Vector3.right * Random.Range(-1f, 1f) + Vector3.forward * Random.Range(-0.4f, 0.6f);
	}

	private void Update()
	{
		if (life)
		{
			mytransform.position = Vector3.Lerp(mytransform.position, rndposition, Time.deltaTime * 0.5f);
		}
		else
		{
			mytransform.position -= Vector3.forward * Time.deltaTime * 1f;
			if (mytransform.position.z < -1f)
			{
				mytransform.position = Vector3.one * 4f;
				base.gameObject.active = false;
			}
		}
		attackrangeZ = mytransform.position.z - cha1.position.z;
		if (attackdelay > 0f)
		{
			attackdelay -= Time.deltaTime;
		}
		else
		{
			if (!(Mathf.Abs(attackrangeZ) < 0.15f))
			{
				return;
			}
			attackrangeX = mytransform.position.x - cha1.position.x;
			if (Mathf.Abs(attackrangeX) < 0.25f)
			{
				if (attackrangeX > 0f)
				{
					script_cha1.Attack(_isright: true);
				}
				else
				{
					script_cha1.Attack(_isright: false);
				}
				attackdelay = 0.2f;
			}
		}
	}
}
