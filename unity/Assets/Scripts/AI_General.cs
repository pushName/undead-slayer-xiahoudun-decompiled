using UnityEngine;

public class AI_General : MonoBehaviour
{
	private Transform mytransform;

	private Animation myanimation;

	private Collider mycollider;

	private int maxhp;

	public int hp;

	private float power;

	private int defence;

	private int dash;

	private float runspeed;

	private float atkspeed;

	private float distance_cha;

	private float hp_length;

	private float fb;

	private bool life = true;

	private bool call;

	private bool disable;

	private float delay_invicibility;

	private float delay_call;

	private Vector3 rndpos;

	private Vector3 directionVector;

	private Vector3 behitdir;

	private Transform cha1;

	private Transform g_hp;

	private Transform c_weapon;

	private Transform c_spweapon;

	private Transform sp_selweapon;

	private AnimationState attack_i;

	private bool attack_start;

	private bool superarmor;

	private bool showme;

	private int generalmovestat;

	private int grade;

	private int atkcount;

	private int damage;

	private short unique = -1;

	private bool efattach;

	public AudioClip snd_attack;

	public Transform ef_weapon;

	public Transform[] sp_weapon = new Transform[2];

	public Transform direction_arrow;

	private Vector3 efpos;

	private Vector3 arrowTargetVector;

	private Vector3 arrowRotVector;

	private Quaternion efrot;

	private Gauge_UV gauge_hp;

	private Cam_Move script_cam;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		mycollider = base.GetComponent<Collider>();
		myanimation["move"].speed = 0.5f;
		myanimation["down"].speed = 0.2f;
		myanimation["dead_general"].speed = 0.2f;
		myanimation["idle"].speed = 0.25f;
		myanimation["down"].layer = 4;
		myanimation["dead_general"].layer = 4;
		myanimation["m_attack1"].layer = 1;
		myanimation["m_attack1_i"].layer = 1;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		InvokeRepeating("SetRndPosition", 0.1f, 1f);
		myanimation.Play("idle");
		g_hp = GameObject.FindWithTag("p_plan").GetComponent<MakeUI>().CreatCustomPlane(new Vector2(0.2f, 0.06f), 0f, new Vector3(-1.4f, 2.36f, 1.8f), new Vector2(0f, 0.5625f), new Vector2(0.25f, 0.625f), "general_hp", "Gauge_UV", 0.1f, 0);
		gauge_hp = g_hp.GetComponent<Gauge_UV>();
		HPgaugeSet();
		script_cam = Camera.main.GetComponent<Cam_Move>();
	}

	public void Set_stat(int _power, int _defence, int _mhp, int _hp, int _dash, float _speed, float _atkspeed, int _grade, short _unique, bool _efattach)
	{
		grade = _grade;
		unique = _unique;
		efattach = _efattach;
		if (unique == -1)
		{
			atkcount = -1;
		}
		sp_selweapon = sp_weapon[(int)((float)unique * 0.2f)];
		atkspeed = _atkspeed;
		myanimation["m_attack1"].speed = atkspeed;
		myanimation["m_attack1_i"].speed = atkspeed;
		power = 1f + (float)_power * 0.5f;
		defence = 1 + _defence;
		maxhp = _mhp;
		hp = _hp;
		if (hp > maxhp)
		{
			hp = maxhp;
		}
		runspeed = _speed;
		dash = _dash;
		myanimation["move"].speed = runspeed;
	}

	public void CallGeneral()
	{
		script_cam.LookTarget(mytransform, 30, 2f);
	}

	public void SetRndPosition()
	{
		rndpos = Random.onUnitSphere * 0.2f + cha1.position + cha1.forward * 0.3f;
		rndpos[1] = 0f;
		distance_cha = Vector3.Distance(mytransform.position, cha1.position);
		if (distance_cha > 0.35f)
		{
			myanimation.CrossFade("move");
			showme = true;
		}
		else
		{
			mycollider.enabled = true;
			myanimation.CrossFade("idle");
			showme = false;
		}
		if ((double)Mathf.Abs(mytransform.position.x) > 2.6)
		{
			fb = Mathf.Abs(mytransform.position.x) / mytransform.position.x;
			mytransform.position = new Vector3(fb * 2.6f, mytransform.position.y, mytransform.position.z);
		}
		if ((double)Mathf.Abs(mytransform.position.z) > 2.5)
		{
			fb = Mathf.Abs(mytransform.position.z) / mytransform.position.z;
			mytransform.position = new Vector3(mytransform.position.x, mytransform.position.y, fb * 2.5f);
		}
	}

	public void Damaged(Vector3 _damageVector)
	{
		if (superarmor || !life)
		{
			return;
		}
		damage = (int)_damageVector.y;
		_damageVector[1] = 0f;
		myanimation.Stop();
		base.GetComponent<Rigidbody>().AddForce(_damageVector * 60f);
		damage = Mathf.Max(1, damage - defence);
		hp -= damage;
		mycollider.enabled = false;
		delay_invicibility = 3.5f;
		superarmor = true;
		HPgaugeSet();
		if (hp <= 0)
		{
			delay_invicibility = 0f;
			myanimation.Play("dead_general");
			life = false;
			CancelInvoke();
			showme = false;
			if (c_weapon != null)
			{
				c_weapon.gameObject.active = false;
				c_weapon.transform.position = Vector3.up * 24f;
			}
			g_hp.gameObject.active = false;
			hp = 1;
		}
		else if (!call)
		{
			myanimation.Play("down");
		}
	}

	public void HPgaugeSet()
	{
		hp_length = (1f - (float)hp / (float)maxhp) * 0.25f;
		gauge_hp.UvMove(Vector2.right * hp_length);
	}

	public void HPfull()
	{
		g_hp.gameObject.active = true;
		hp = maxhp;
		HPgaugeSet();
		InvokeRepeating("SetRndPosition", 0f, 1f);
		disable = false;
		life = true;
		superarmor = false;
		mytransform.position = new Vector3(cha1.position.x, 0f, cha1.position.z);
		myanimation.Play("idle");
		mycollider.enabled = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 8)
		{
			AttackOn(other.transform.position);
			mycollider.enabled = false;
		}
	}

	public void AttackOn(Vector3 _enemypos)
	{
		if (!life || showme || call)
		{
			mycollider.enabled = false;
		}
		else if (!attack_start)
		{
			if (unique >= 0)
			{
				atkcount = (atkcount + 1) % (5 - grade);
			}
			directionVector = _enemypos - mytransform.position;
			myanimation.Play("m_attack1");
			attack_i = myanimation.PlayQueued("m_attack1_i");
			attack_i.layer = 1;
			attack_i.speed = atkspeed;
			attack_start = true;
			mytransform.rotation = Quaternion.LookRotation(directionVector);
		}
	}

	private void Update()
	{
		if (disable)
		{
			return;
		}
		if (!life)
		{
			delay_invicibility += Time.deltaTime;
			if (delay_invicibility > 2f)
			{
				delay_invicibility = 0f;
				mytransform.position += Vector3.up * 14f;
				myanimation.Stop();
				disable = true;
			}
			return;
		}
		if (superarmor)
		{
			delay_invicibility -= Time.deltaTime;
			if (delay_invicibility < 0f)
			{
				delay_invicibility = 0f;
				superarmor = false;
				mycollider.enabled = true;
			}
		}
		if (call)
		{
			delay_call += Time.deltaTime;
			if (delay_call > 3f)
			{
				delay_call = 0f;
				call = false;
			}
		}
		if (myanimation.IsPlaying("down"))
		{
			attack_start = true;
		}
		else if (myanimation.IsPlaying("m_attack1_i"))
		{
			if (generalmovestat == 2)
			{
				return;
			}
			generalmovestat = 2;
			if (atkcount != 0)
			{
				efpos = mytransform.position + Vector3.up * ef_weapon.localPosition.y + mytransform.forward * ef_weapon.localPosition.z + mytransform.right * ef_weapon.localPosition.x;
				efrot = Quaternion.Euler(ef_weapon.eulerAngles.x, ef_weapon.eulerAngles.y + mytransform.eulerAngles.y, ef_weapon.eulerAngles.z);
				if (c_weapon == null)
				{
					c_weapon = (Transform)Object.Instantiate(ef_weapon, efpos, efrot);
					if (efattach)
					{
						c_weapon.parent = mytransform;
					}
					c_weapon.gameObject.active = true;
					c_weapon.GetComponent<Rigidbody>().mass = power;
				}
				else
				{
					c_weapon.position = efpos;
					c_weapon.rotation = efrot;
					c_weapon.gameObject.active = true;
				}
			}
			else
			{
				efpos = mytransform.position + Vector3.up * sp_selweapon.localPosition.y + mytransform.forward * sp_selweapon.localPosition.z + mytransform.right * sp_selweapon.localPosition.x;
				efrot = Quaternion.Euler(sp_selweapon.eulerAngles.x, sp_selweapon.eulerAngles.y + mytransform.eulerAngles.y, sp_selweapon.eulerAngles.z);
				if (c_spweapon == null)
				{
					c_spweapon = (Transform)Object.Instantiate(sp_selweapon, efpos, efrot);
					c_spweapon.gameObject.active = true;
					c_spweapon.GetComponent<Rigidbody>().mass = power;
				}
				else
				{
					c_spweapon.position = efpos;
					c_spweapon.rotation = efrot;
					c_spweapon.gameObject.active = true;
				}
			}
		}
		else if (myanimation.IsPlaying("m_attack1"))
		{
			if (generalmovestat != 1)
			{
				generalmovestat = 1;
				base.GetComponent<Rigidbody>().AddForce(directionVector * dash);
				if (atkcount == 0)
				{
					delay_invicibility = 1.5f;
					superarmor = true;
				}
			}
		}
		else if (myanimation.IsPlaying("move"))
		{
			if (generalmovestat != 0)
			{
				mycollider.enabled = true;
				generalmovestat = 0;
			}
			attack_start = false;
			directionVector = cha1.position - mytransform.position;
			if (directionVector != Vector3.zero)
			{
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(directionVector), Time.deltaTime * 5f);
			}
			mytransform.position += mytransform.forward * Time.deltaTime * (runspeed - 0.1f);
		}
		else if (myanimation.IsPlaying("idle"))
		{
			if (generalmovestat != 0)
			{
				mycollider.enabled = true;
				generalmovestat = 0;
			}
			attack_start = false;
			directionVector = rndpos - mytransform.position;
			if (directionVector != Vector3.zero)
			{
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(directionVector), Time.deltaTime * 2f);
			}
			mytransform.position += mytransform.forward * Time.deltaTime * 0.1f;
		}
	}
}
