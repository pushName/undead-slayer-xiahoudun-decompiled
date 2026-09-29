using UnityEngine;

public class Tank : MonoBehaviour
{
	private Hp_bar script_hpbar;

	private Tower_base script_castle;

	public AudioClip snd_break;

	public AudioClip snd_hit;

	public Transform pt_hit;

	public Transform tank_destroy;

	public Transform behit_collider;

	private Transform mytransform;

	private Transform hpbar;

	private Transform c_destroy;

	private Animation myanimation;

	private float delay;

	private float movespeed = 0.1f;

	private int damage;

	private short maxhp;

	private short hp;

	private bool life = true;

	private bool super;

	private bool stophit;

	private short cur_stage_index;

	private Cam_Move script_cam;

	private void Awake()
	{
		cur_stage_index = (short)Crypto.Load_int_key("cur_stage_index");
		maxhp = (short)(200 + 3 * cur_stage_index);
		hp = maxhp;
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		mytransform.position = -Vector3.forward * 0.7f;
		hpbar = GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().CreatHpbar(new Vector2(0.1f, 0.02f), _turretmode: false, _ally: true);
		hpbar.position = mytransform.position + Vector3.up * 0.3f;
		hpbar.parent = mytransform;
		script_hpbar = hpbar.GetComponent<Hp_bar>();
	}

	private void OnEnable()
	{
		life = true;
		behit_collider.GetComponent<Collider>().enabled = true;
	}

	private void Start()
	{
		myanimation["tank_run"].speed = 0.4f;
		myanimation["tank_damage"].speed = 0.3f;
		myanimation["tank_attack"].speed = 0.3f;
		base.GetComponent<AudioSource>().Play();
		script_castle = GameObject.FindWithTag("Finish").GetComponent<Tower_base>();
		script_cam = Camera.main.GetComponent<Cam_Move>();
	}

	public void Damaged(Vector3 _damageVector)
	{
		if (!life || super)
		{
			return;
		}
		damage = (int)_damageVector.y;
		_damageVector[1] = 0f;
		myanimation.Stop();
		myanimation.Play("tank_damage");
		delay = 2f;
		base.GetComponent<AudioSource>().Stop();
		base.GetComponent<AudioSource>().PlayOneShot(snd_break);
		{
			var emission = pt_hit.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		hp -= (short)damage;
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.2f, -1);
		if (hp <= 0)
		{
			if (c_destroy == null)
			{
				c_destroy = (Transform)Object.Instantiate(tank_destroy, mytransform.position, mytransform.rotation);
			}
			else
			{
				c_destroy.position = mytransform.position;
				c_destroy.gameObject.active = true;
			}
			behit_collider.GetComponent<Collider>().enabled = false;
			mytransform.position = Vector3.up * 20f;
			hp = maxhp;
			script_hpbar.Damaged(maxhp, hp, mytransform, 0.2f, -1);
			base.gameObject.active = false;
			life = false;
		}
	}

	public void HurryUp()
	{
		movespeed = 0.5f;
		myanimation["tank_run"].speed = 2f;
	}

	public void AttakCastle()
	{
		if (!script_castle.TankDamage())
		{
			stophit = true;
		}
		script_cam.Hitcam();
		base.GetComponent<AudioSource>().PlayOneShot(snd_hit);
	}

	private void Update()
	{
		if (!life)
		{
			return;
		}
		if (delay > 0f)
		{
			delay -= Time.deltaTime;
			if (delay <= 0f)
			{
				myanimation.CrossFade("tank_run");
				base.GetComponent<AudioSource>().Play();
			}
			else if (delay < 1.5f)
			{
				{
					var emission = pt_hit.GetComponent<ParticleSystem>().emission;
					emission.enabled = false;
				}
			}
		}
		else if (!stophit)
		{
			if (mytransform.position.z > 9.25f)
			{
				myanimation.Play("tank_attack");
			}
			else if (life)
			{
				mytransform.position += Vector3.forward * Time.deltaTime * movespeed;
			}
		}
	}
}
