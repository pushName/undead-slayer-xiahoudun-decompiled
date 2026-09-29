using System.Collections;
using UnityEngine;

public class AI_Enemy01 : MonoBehaviour
{
	public int enemykind;

	public Transform ef_weapon;

	public short monmovestat;

	private AudioClip snd_move;

	public AudioClip snd_attack;

	public Transform direction_arrow;

	private Transform clone_weapon;

	private Transform clone_arrow;

	private Transform cha1;

	private Transform target;

	private Transform target_onlyone;

	private Transform hpbar;

	private Texture originTex;

	private bool life = true;

	private bool spawn_ing;

	private bool pierce;

	private bool impact;

	private bool attackstart;

	private bool attach_weaponEf;

	private bool showme;

	private bool lastmon;

	private bool targetreset;

	private short kind;

	private int shadow_index;

	private bool target_fix;

	private bool poison;

	private float poison_delay;

	private float magnitude_behitdir;

	private short old_delay;

	private float poison_damage;

	private int petrify_rate;

	private float movespeed;

	private float behaviour_delay = 2f;

	private float hpbar_height;

	private int behaviour;

	private short hp;

	private short level;

	private short maxhp;

	private short power;

	private float haveExp;

	private short block;

	private short sizekind;

	private float runspeed;

	private float backspeed;

	private float firerange;

	private float moving_atk;

	private int grabstyle;

	private bool grabed;

	private bool downhigh;

	private bool risedrop;

	private float f_risefactor;

	private int chamovestat;

	private Vector3 directionVector;

	private Vector3 attackstartVector;

	private Vector3 attackdir;

	private float damage;

	private int accuracy;

	private float speed_idle;

	private float attackrange = 5f;

	private short dash;

	private Cha_Control script_cha;

	private SoundEf_slash script_sound;

	private Cam_Move script_cam;

	private Hp_bar script_hpbar;

	private Monster_efs script_monEf;

	private AnimationState bethrust;

	private AnimationState bekicked;

	private AnimationState attack_impact;

	private AnimationState getup;

	private Quaternion lookrotation;

	private float attackforce;

	private int playkind;

	private Transform mytransform;

	private Animation myanimation;

	private AudioSource myaudio;

	private Renderer monrender;

	private short att_status;

	private float restrictArea = 100f;

	private monster enemy;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		myaudio = base.GetComponent<AudioSource>();
		script_monEf = GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>();
		enemy = GameObject.FindWithTag("Respawn").GetComponent<DB_Monster>().enemy[enemykind];
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha = cha1.GetComponent<Cha_Control>();
		script_sound = GameObject.FindWithTag("sound").GetComponent<SoundEf_slash>();
		script_cam = Camera.main.GetComponent<Cam_Move>();
	}

	private void Start()
	{
		maxhp += enemy._maxhp;
		power += enemy._power;
		haveExp += enemy._haveExp;
		block += enemy._block;
		sizekind = enemy._sizekind;
		runspeed = enemy._runspeed;
		backspeed = enemy._backspeed;
		firerange = enemy._firerange;
		moving_atk = enemy._moving_atk;
		kind = enemy._kind;
		dash = enemy._dash;
		attach_weaponEf = enemy._attach_ef;
		myanimation["move"].speed = enemy._speed_move;
		myanimation["down"].speed = 0.24f;
		myanimation["down_high"].speed = 0.28f;
		myanimation["m_attack1"].speed = enemy._speed_m_attack1;
		myanimation["m_attack1_i"].speed = enemy._speed_m_attack1_i;
		myanimation["idle"].speed = enemy._speed_idle - 0.05f;
		speed_idle = myanimation["idle"].speed;
		myanimation["down"].layer = 2;
		myanimation["down_high"].layer = 2;
		myanimation["move"].layer = 0;
		myanimation["m_attack1"].layer = 1;
		myanimation["m_attack1_i"].layer = 1;
		myanimation["idle"].layer = 0;
		monmovestat = 0;
		monrender = mytransform.GetChild(1).GetComponent<Renderer>();
		originTex = monrender.material.mainTexture;
		InvokeRepeating("SetDir", 0.1f, 0.5f);
		hp = maxhp;
		accuracy = script_cha.hitrate;
		snd_move = script_monEf.snd_move[Random.Range(0, 3)];
		hpbar = script_monEf.SetHpbar();
		hpbar.position = mytransform.position;
		hpbar_height = 0.1f + (float)sizekind * 0.007f;
		script_hpbar = hpbar.GetComponent<Hp_bar>();
		script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, 0);
		shadow_index = script_monEf.CreatShadow(mytransform.GetChild(0), 1f);
	}

	private void OnTriggerEnter(Collider other)
	{
		int layer = other.gameObject.layer;
		if (grabed || !life || layer < 16)
		{
			return;
		}
		accuracy = script_cha.hitrate;
		downhigh = script_cha.attack_rising;
		if (layer == 28)
		{
			attackdir = mytransform.position - cha1.position;
			attackdir.y = 0f;
			attackdir = Vector3.Normalize(attackdir);
		}
		else
		{
			attackdir = mytransform.position - other.transform.position;
			attackdir.y = 0f;
			magnitude_behitdir = attackdir.magnitude;
			if (magnitude_behitdir != 0f)
			{
				if (magnitude_behitdir < 0.08f)
				{
					attackdir = attackdir / magnitude_behitdir * 1.6f;
				}
				else
				{
					attackdir /= magnitude_behitdir;
				}
			}
		}
		switch (layer)
		{
		case 20:
		{
			attackforce = 40f;
			int num = Random.Range(0, 100);
			if (num < block - accuracy && monmovestat >= 0)
			{
				script_cha.Blocked(mytransform.position);
				base.GetComponent<Rigidbody>().AddForce(attackdir * 10f);
				return;
			}
			damage = script_cha.atk;
			script_cam.Hitcam();
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		}
		case 21:
			attackforce = 30f;
			damage = script_cha.atk;
			script_cam.Hitcam2(1f);
			downhigh = true;
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			break;
		case 22:
			attackforce = 10f;
			damage = other.GetComponent<Rigidbody>().mass;
			script_cam.Hitcam();
			downhigh = true;
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			break;
		case 23:
		{
			attackforce = 10f;
			float mass = other.GetComponent<Rigidbody>().mass;
			if (mass == 0.1f)
			{
				damage = 0f;
				poison = true;
				poison_damage = (float)script_cha.atk * 0.6f;
				poison_delay = 4f;
				downhigh = false;
				target = cha1;
				return;
			}
			poison = true;
			poison_damage = mass;
			damage = poison_damage * 50f;
			script_cam.Hitcam();
			downhigh = true;
			poison_delay = 5f;
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			break;
		}
		case 24:
			attackforce = 0f;
			script_cam.Hitcam();
			life = false;
			petrify_rate = (int)other.GetComponent<Rigidbody>().mass;
			base.gameObject.layer = 10;
			StartCoroutine(Petrify());
			monrender.material.mainTexture = script_monEf.attribute_tex[1];
			target = cha1;
			script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, 2);
			break;
		case 25:
			attackforce = 60f;
			damage = script_cha.atk;
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			downhigh = false;
			break;
		case 26:
			myanimation.AddClip(script_monEf.pierce, "pierced");
			myanimation["pierced"].speed = 0.3f;
			StartCoroutine(Pierced());
			attackforce = 0f;
			pierce = true;
			life = false;
			base.GetComponent<Collider>().enabled = false;
			damage = other.GetComponent<Rigidbody>().mass;
			hp -= (short)damage;
			target = cha1;
			script_hpbar.Damaged(maxhp, 0, mytransform, 9f, 0);
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			break;
		case 27:
			attackforce = 10f;
			damage = other.GetComponent<Rigidbody>().mass;
			target = other.transform;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			targetreset = true;
			downhigh = false;
			break;
		case 28:
			attackforce = 40f;
			damage = script_cha.atk;
			script_cam.Hitcam();
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 29:
			attackforce = 0f;
			damage = (float)script_cha.atk * 0.4f;
			downhigh = false;
			if (risedrop)
			{
				f_risefactor = 0.6f;
			}
			else
			{
				mytransform.rotation = Random.rotation;
				f_risefactor = 3.4f;
			}
			risedrop = true;
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			break;
		case 30:
			attackforce = 40f;
			damage = script_cha.atk;
			if (other.transform.root == cha1)
			{
				script_cam.Hitcam();
			}
			target = cha1;
			if (att_status != 2)
			{
				monrender.material.mainTexture = script_monEf.attribute_tex[0];
				script_monEf.CreatBlood(mytransform.position, attackdir);
				InvokeRepeating("BurnDamage", 0.5f, 1f);
				StartCoroutine(Burn());
				att_status = 1;
			}
			break;
		case 31:
			attackforce = 40f;
			downhigh = false;
			damage = script_cha.atk;
			if (other.transform.root == cha1)
			{
				script_cam.Hitcam();
			}
			target = cha1;
			if (att_status != 1 && !myanimation.IsPlaying("down_high"))
			{
				myanimation.enabled = false;
				monrender.material.mainTexture = script_monEf.attribute_tex[1];
				script_monEf.CreatBlood(mytransform.position, attackdir);
				StartCoroutine(Freeze((short)damage));
				att_status = 2;
			}
			break;
		case 16:
			attackforce = -10f;
			downhigh = false;
			damage = 0f;
			target = cha1;
			if (att_status != 1)
			{
				myanimation.enabled = false;
				StartCoroutine(Freeze((short)damage));
				att_status = 2;
			}
			break;
		case 17:
			attackforce = 0f;
			damage = (float)script_cha.atk * 0.2f;
			script_cam.Hitcam2(0.08f);
			downhigh = false;
			if (risedrop)
			{
				f_risefactor = 0.6f;
			}
			else
			{
				mytransform.rotation = Random.rotation;
				f_risefactor = 1.2f;
			}
			risedrop = true;
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 18:
			attackforce = 40f;
			damage = script_cha.atk;
			if (other.transform.root == cha1)
			{
				script_cam.Hitcam();
			}
			target = cha1;
			if (att_status != 3)
			{
				monrender.material.mainTexture = script_monEf.attribute_tex[2];
				script_monEf.CreatBlood(mytransform.position, attackdir);
				InvokeRepeating("ShockDamage", 0.5f, 0.1f);
				StartCoroutine(Shock((short)damage));
				att_status = 3;
			}
			break;
		case 19:
			attackforce = 40f;
			damage = script_cha.atk;
			if (other.transform.root == cha1)
			{
				script_cam.Hitcam();
			}
			target = cha1;
			if (att_status != 4)
			{
				monrender.material.mainTexture = script_monEf.attribute_tex[3];
				script_monEf.CreatBlood(mytransform.position, attackdir);
				StartCoroutine(Darken());
				att_status = 4;
			}
			break;
		}
		movespeed = 0f;
		script_sound.SoundOn(1);
		if (!life)
		{
			if (pierce)
			{
				myanimation.Play("pierced");
				pierce = false;
			}
			else
			{
				myanimation.Stop();
			}
		}
		else
		{
			myanimation.Stop();
			if (downhigh)
			{
				mytransform.Rotate(0f, Random.Range(0, 360), 0f);
				myanimation.Play("down_high");
			}
			else
			{
				myanimation.Play("down");
			}
			base.GetComponent<Rigidbody>().AddForce(attackdir * attackforce);
			if (directionVector == Vector3.zero)
			{
				directionVector = -Vector3.forward;
			}
			if (damage > 0f)
			{
				hp -= (short)damage;
				script_monEf.SetDamageNum(mytransform.position, (short)damage, attackdir);
				script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, -1);
			}
			if (Random.Range(0, 5) == 0)
			{
				myaudio.clip = script_monEf.ScreamSFX();
				myaudio.Play();
			}
		}
		if (hp <= 0 && life)
		{
			if (!risedrop)
			{
				Dead(2);
			}
		}
		else if (target_fix)
		{
			target = target_onlyone;
		}
	}

	public void SummonStart()
	{
		float time = 1.5f;
		if (enemy._kind == 2)
		{
			myanimation.AddClip(script_monEf.summon[0], "summon");
			myanimation["summon"].speed = 0.2f;
		}
		else
		{
			myanimation.AddClip(script_monEf.summon[1], "summon");
			myanimation["summon"].speed = 0.6f;
			time = 0.4f;
		}
		spawn_ing = true;
		mytransform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		myanimation.Stop();
		myanimation.Play("summon");
		Invoke("SummonFinish", time);
	}

	public void SummonFinish()
	{
		spawn_ing = false;
	}

	public void SetLevel(short _level, int _playkind, bool _issummon, float _restrictArea)
	{
		level = (short)(_level + 1);
		restrictArea = _restrictArea;
		maxhp += (short)(0.1445f * (float)level * (float)level + 6.3873f * (float)level - 5f);
		power += (short)(0.0058f * (float)level * (float)level + 1.008f * (float)level - 5f);
		block += (short)((float)level * 0.4f);
		hp = maxhp;
		haveExp += (float)level * 0.1f;
		playkind = _playkind;
		if (playkind == 6)
		{
			target_fix = true;
			target_onlyone = GameObject.FindWithTag("general").transform;
			target = target_onlyone;
		}
		else
		{
			target = cha1;
		}
		if (_issummon)
		{
			SummonStart();
		}
		else
		{
			myanimation.Stop();
		}
	}

	private void BurnDamage()
	{
		short num = (short)(script_cha.atk / 10);
		if (num < 1)
		{
			num = 1;
		}
		hp -= num;
		script_monEf.SetDamageNum(mytransform.position, num, attackdir);
		script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, -1);
		if (hp <= 0)
		{
			Dead(0);
		}
	}

	private void ShockDamage()
	{
		myanimation.Stop();
		myanimation.Play("down");
	}

	public IEnumerator Freeze(short _damage)
	{
		yield return new WaitForSeconds(2f);
		att_status = 0;
		myanimation.enabled = true;
		monrender.material.mainTexture = originTex;
	}

	public IEnumerator Burn()
	{
		yield return new WaitForSeconds(3f);
		att_status = 0;
		monrender.material.mainTexture = originTex;
		CancelInvoke("BurnDamage");
	}

	public IEnumerator Shock(short _damage)
	{
		yield return new WaitForSeconds(1.2f);
		att_status = 0;
		monrender.material.mainTexture = originTex;
		CancelInvoke("ShockDamage");
	}

	public IEnumerator Darken()
	{
		yield return new WaitForSeconds(2f);
		att_status = 0;
		monrender.material.mainTexture = originTex;
	}

	public IEnumerator Petrify()
	{
		int _rate = Random.Range(0, 100);
		yield return new WaitForSeconds(2.5f);
		if (petrify_rate > _rate)
		{
			Dead(0);
			yield break;
		}
		base.gameObject.layer = 8;
		monrender.material.mainTexture = originTex;
		life = true;
		script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, 0);
	}

	public IEnumerator Pierced()
	{
		yield return new WaitForSeconds(4f);
		if (hp <= 0)
		{
			Dead(0);
		}
		else
		{
			script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, 0);
		}
		myanimation.Stop();
		myanimation.Play("down");
		script_cam.Hitcam();
		script_monEf.CreatBlood_Only(mytransform.position, attackdir);
		movespeed = 0f;
		script_sound.SoundOn(1);
		base.GetComponent<Collider>().enabled = true;
		life = true;
	}

	public void Dead(int _kind)
	{
		monmovestat = -4;
		life = false;
		script_hpbar.FreeSelect();
		lastmon = false;
		hp = 0;
		Vector3 position = mytransform.position;
		attackdir[1] = 0f;
		switch (_kind)
		{
		case 2:
			script_sound.SoundOn(4);
			Object.Destroy(base.gameObject);
			script_monEf.EnemyDead(kind, position, monrender.material.mainTexture, mytransform.localScale, attackdir);
			break;
		case 1:
			Object.Destroy(base.gameObject, 2f);
			bekicked.wrapMode = WrapMode.ClampForever;
			script_monEf.EnemyDead(0, position, null, Vector3.zero, Vector3.zero);
			script_monEf.DestroyShadow(shadow_index);
			break;
		case 0:
			script_sound.SoundOn(4);
			Object.Destroy(base.gameObject);
			script_monEf.EnemyDead(1, position, null, Vector3.zero, Vector3.zero);
			break;
		case 3:
			script_sound.SoundOn(4);
			Object.Destroy(base.gameObject);
			script_monEf.EnemyDead(3, position, monrender.material.mainTexture, mytransform.localScale, Vector3.zero);
			break;
		}
		script_cha.GainExp(haveExp);
		script_monEf.SetItemBox(level, mytransform.position);
		if (clone_arrow != null)
		{
			Object.Destroy(clone_arrow.gameObject);
			clone_arrow = null;
		}
		if (clone_weapon != null)
		{
			Object.Destroy(clone_weapon.gameObject);
			clone_weapon = null;
		}
	}

	public void Grabed()
	{
		if (att_status <= 0 && !grabed)
		{
			attackdir = Vector3.Normalize(mytransform.position - Vector3.right * cha1.position.x - Vector3.forward * cha1.position.z);
			mytransform.forward = -attackdir;
			myanimation.Stop();
			grabstyle = Random.Range(0, 3);
			if (sizekind == 20)
			{
				grabstyle += 3;
			}
			else if (kind != 2)
			{
				grabstyle = 6;
			}
			Object.Destroy(base.GetComponent<Collider>());
			if (grabstyle != 6)
			{
				myanimation.AddClip(script_monEf.begrab[grabstyle], "downforever");
				myanimation.AddClip(script_monEf.bethrust[grabstyle], "bethrust");
				myanimation.AddClip(script_monEf.bekicked[grabstyle], "bekicked");
				myanimation.AddClip(script_monEf.getup[grabstyle], "getup");
			}
			myanimation.Play("downforever");
			myanimation["downforever"].speed = script_monEf.speed_begrab[grabstyle];
			myanimation["downforever"].layer = 3;
			bethrust = myanimation.PlayQueued("bethrust");
			bethrust.speed = script_monEf.speed_bethrust[grabstyle];
			bethrust.layer = 3;
			bekicked = myanimation.PlayQueued("bekicked");
			bekicked.speed = script_monEf.speed_bekicked[grabstyle];
			bekicked.layer = 3;
			base.GetComponent<Rigidbody>().AddForce(attackdir * script_monEf.force_grab[grabstyle]);
			script_cha.Grab(grabstyle, mytransform.position);
			grabed = true;
			poison_delay = -1f;
		}
	}

	public void CountDown()
	{
		if (life)
		{
			clone_arrow = (Transform)Object.Instantiate(direction_arrow, cha1.position, Quaternion.identity);
			showme = true;
			lastmon = true;
		}
	}

	public void SetDir()
	{
		if (!life | spawn_ing)
		{
			return;
		}
		chamovestat = script_cha.chamovestat;
		if (target == null)
		{
			target = cha1;
		}
		switch (att_status)
		{
		case 4:
			attackrange = 2f;
			directionVector = mytransform.position - target.position;
			directionVector[1] = 0f;
			directionVector = Vector3.Normalize(directionVector);
			break;
		case 3:
			directionVector = -mytransform.forward;
			attackrange = 2f;
			break;
		case 2:
			directionVector = Vector3.zero;
			attackrange = 2f;
			break;
		default:
			attackrange = Vector3.Distance(mytransform.position, target.position);
			directionVector = target.position - mytransform.position;
			directionVector[1] = 0f;
			directionVector = Vector3.Normalize(directionVector);
			break;
		}
		if (directionVector != Vector3.zero)
		{
			lookrotation = Quaternion.LookRotation(directionVector);
		}
		if (behaviour_delay < 0f)
		{
			behaviour = Random.Range(0, 6);
			if (behaviour == 0)
			{
				myaudio.clip = snd_move;
				myaudio.Play();
			}
			behaviour_delay = 2f;
		}
		if (attackrange < firerange)
		{
			if (attackstart)
			{
				return;
			}
			if (chamovestat < 110 && monmovestat >= 0)
			{
				myanimation.Play("m_attack1");
				attack_impact = myanimation.PlayQueued("m_attack1_i", QueueMode.CompleteOthers);
				attack_impact.speed = myanimation["m_attack1_i"].speed;
				attackstart = true;
				attackstartVector = directionVector;
				if (targetreset)
				{
					target = cha1;
					targetreset = false;
				}
			}
			else
			{
				movespeed = backspeed;
				myanimation["idle"].speed = speed_idle * -0.5f;
				myanimation.CrossFade("idle");
				behaviour = -1;
				behaviour_delay = 1f;
			}
		}
		else if (monrender.isVisible)
		{
			if (behaviour != -1)
			{
				if (behaviour >= 3)
				{
					myanimation["idle"].speed = speed_idle;
					movespeed = runspeed * 0.4f;
					myanimation.CrossFade("idle");
				}
				else
				{
					movespeed = runspeed;
					myanimation.CrossFade("move");
				}
			}
			if (lastmon)
			{
				showme = false;
				clone_arrow.GetComponent<Renderer>().enabled = false;
			}
		}
		else
		{
			movespeed = runspeed * Random.Range(0.7f, 1f);
			myanimation.Stop();
			if (directionVector != Vector3.zero)
			{
				mytransform.rotation = Quaternion.LookRotation(directionVector);
			}
			if (lastmon)
			{
				showme = true;
				clone_arrow.GetComponent<Renderer>().enabled = true;
			}
		}
	}

	private void Update()
	{
		if (!life)
		{
			return;
		}
		if (risedrop)
		{
			mytransform.position += Vector3.up * f_risefactor * Time.deltaTime;
			f_risefactor -= Time.deltaTime * 5f;
			if (!(mytransform.position.y < 0f))
			{
				return;
			}
			myanimation.Play("down");
			mytransform.position = new Vector3(mytransform.position.x, 0f, mytransform.position.z);
			risedrop = false;
			f_risefactor = 0f;
			damage = script_cha.atk;
			hp -= (short)damage;
			if (hp <= 0)
			{
				Dead(3);
			}
			else if (attackdir != Vector3.zero)
			{
				mytransform.rotation = Quaternion.LookRotation(attackdir);
			}
			else
			{
				mytransform.rotation = Quaternion.identity;
			}
		}
		behaviour_delay -= Time.deltaTime;
		if (grabed)
		{
			if (myanimation.IsPlaying("downforever"))
			{
				script_cha.Grabfinish(mytransform.position, mytransform.forward);
				monmovestat = -2;
				mytransform.position = cha1.position;
			}
			else if (myanimation.IsPlaying("bethrust"))
			{
				script_cha.Grabfinish(mytransform.position, mytransform.forward);
				if (monmovestat != -3)
				{
					hp = 0;
					script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, 0);
					Transform transform = GameObject.Find("weapon_dummy").transform;
					Quaternion dir = Quaternion.LookRotation(-transform.forward, new Vector3(0f, -1.3f, 1f));
					script_monEf.CreatGrabBlood(transform.position + transform.forward * 0.01f, dir);
					script_cam.Hitcam2(1f);
					script_sound.SoundOn(3);
				}
				monmovestat = -3;
			}
			else if (myanimation.IsPlaying("bekicked"))
			{
				if (monmovestat != -4)
				{
					base.GetComponent<Rigidbody>().AddForce(mytransform.forward * script_monEf.force_kick[grabstyle]);
					script_cam.Hitcam2(1f);
					script_sound.SoundOn(2);
					if (hp <= 0)
					{
						Dead(1);
					}
					else
					{
						if (grabstyle != 6)
						{
							myanimation.AddClip(script_monEf.getup[grabstyle], "getup");
						}
						getup = myanimation.PlayQueued("getup");
						getup.speed = script_monEf.speed_getup[grabstyle];
					}
				}
				monmovestat = -4;
				grabed = false;
			}
		}
		else if (myanimation.IsPlaying("getup"))
		{
			monmovestat = 0;
		}
		else if (myanimation.IsPlaying("summon"))
		{
			monmovestat = 0;
		}
		else if (myanimation.IsPlaying("down_high"))
		{
			monmovestat = -1;
		}
		else if (myanimation.IsPlaying("down"))
		{
			monmovestat = -1;
		}
		else if (myanimation.IsPlaying("m_attack1"))
		{
			impact = false;
			monmovestat = 11;
			lookrotation = Quaternion.LookRotation(attackstartVector);
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, lookrotation, Time.deltaTime * 6f);
			mytransform.position += attackstartVector * Time.deltaTime * moving_atk;
		}
		else if (myanimation.IsPlaying("m_attack1_i"))
		{
			monmovestat = 12;
			if (!impact)
			{
				impact = true;
				Vector3 position = mytransform.position + Vector3.up * ef_weapon.localPosition.y;
				base.GetComponent<Rigidbody>().AddForce(mytransform.forward * dash);
				if (clone_weapon == null)
				{
					clone_weapon = (Transform)Object.Instantiate(ef_weapon, position, mytransform.rotation);
					clone_weapon.GetComponent<WeaponDamage>().PressDamage(power);
					if (attach_weaponEf)
					{
						clone_weapon.parent = mytransform;
					}
				}
				else
				{
					clone_weapon.position = position;
					clone_weapon.rotation = mytransform.rotation;
					clone_weapon.gameObject.active = true;
				}
				base.GetComponent<AudioSource>().clip = snd_attack;
				myaudio.Play();
				mytransform.position = Vector3.right * mytransform.position.x + Vector3.forward * mytransform.position.z;
			}
		}
		else if (myanimation.IsPlaying("move") || myanimation.IsPlaying("idle") || !myanimation.isPlaying)
		{
			attackstart = false;
			grabed = false;
			monmovestat = 1;
		}
		else
		{
			monmovestat = 1;
		}
		if (monmovestat == 1)
		{
			mytransform.position += directionVector * Time.deltaTime * movespeed;
			if (myanimation.isPlaying)
			{
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, lookrotation, Time.deltaTime * 3f);
			}
		}
		if (lastmon && showme)
		{
			Vector3 vector = Vector3.Normalize(mytransform.position - cha1.position);
			if (vector != Vector3.zero)
			{
				clone_arrow.rotation = Quaternion.LookRotation(vector);
			}
			vector = cha1.position + Vector3.up * 0.02f + vector * 0.3f;
			clone_arrow.position = Vector3.Lerp(clone_arrow.position, vector, Time.deltaTime * 4f);
		}
		if (poison)
		{
			poison_delay -= Time.deltaTime;
			if (poison_delay < 0f)
			{
				poison = false;
				script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, 0);
			}
			else if ((short)poison_delay != old_delay)
			{
				hp -= (short)poison_damage;
				script_monEf.SetDamageNum(mytransform.position, (short)poison_damage, attackdir);
				script_hpbar.Damaged(maxhp, hp, mytransform, hpbar_height, 1);
				old_delay = (short)poison_delay;
				if (hp <= 0)
				{
					Dead(0);
				}
			}
		}
		Vector2 a = new Vector2(mytransform.position.x, mytransform.position.z);
		float num = Vector2.SqrMagnitude(a);
		if (num > restrictArea)
		{
			base.GetComponent<Rigidbody>().AddForce(-new Vector3(a.x, 0f, a.y) * 5f);
		}
	}
}
