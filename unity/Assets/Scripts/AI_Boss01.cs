using System.Collections;
using UnityEngine;

public class AI_Boss01 : MonoBehaviour
{
	public int enemykind;

	public Transform[] ef_weapon = new Transform[3];

	public Transform ef_secondweapon;

	private Texture originTex;

	public short monmovestat;

	public AudioClip snd_move;

	public AudioClip snd_scream;

	public AudioClip snd_attack;

	public Transform direction_arrow;

	private AnimationState attack_impact;

	private Transform[] clone_weapon = new Transform[3];

	private Transform mytransform;

	private Animation myanimation;

	private Transform hpbar;

	public Transform weapon;

	private bool life = true;

	private bool impact;

	private bool attackstart;

	private bool bosscutin;

	private short[] attach_weaponEf = new short[3];

	private bool[] collideroff = new bool[3];

	private float turnspeed;

	private float behaviour_delay = 2f;

	private short level;

	private short maxhp;

	private short hp;

	private short[] power = new short[3];

	private float haveExp;

	private short block;

	private int behaviour;

	private short currentAtk;

	private float petrify_rate;

	private float runspeed;

	private float firerange1;

	private float firerange2;

	private float firerange3;

	private short[] dash = new short[3];

	private Vector2[] moving_atk = new Vector2[3];

	private Vector3 arrowTargetVector;

	private float m_atk_delay;

	private float attackrange;

	private float attackforce;

	private int blockrate;

	private bool poison;

	private float poison_delay;

	private short old_delay;

	private float poison_damage;

	private bool showme;

	private bool jump;

	private bool setattackkind;

	private short atkkind;

	private Collider mycollider;

	private float pluspower = 1f;

	private float plusmaxhp = 1f;

	private float restrictArea;

	private bool invince;

	private short att_status = 1;

	private int chamovestat;

	private Transform cha1;

	private Transform target;

	private Transform clone_arrow;

	private Vector3 attackdir;

	private Vector3 directionVector;

	private Quaternion lookrotation;

	private float damage;

	private int atk;

	private int accuracy;

	private AudioSource myaudio;

	private Renderer monrender;

	private Cha_Control script_cha;

	private SoundEf_slash script_sound;

	private Cam_Move script_cam;

	private Hp_bar script_hpbar;

	private Monster_efs script_monEf;

	private Spawn script_spawn;

	private bossmonster enemy;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		myaudio = base.GetComponent<AudioSource>();
	}

	private void Start()
	{
		monrender = mytransform.Find("mon").GetComponent<Renderer>();
		GameObject gameObject = GameObject.FindWithTag("Respawn");
		script_spawn = gameObject.GetComponent<Spawn>();
		enemy = gameObject.GetComponent<DB_Boss>().boss[enemykind];
		mycollider = base.GetComponent<Collider>();
		maxhp = (short)((float)enemy._maxhp * plusmaxhp);
		power[0] = (short)((float)enemy._power1 * pluspower);
		power[1] = (short)((float)enemy._power2 * pluspower);
		power[2] = (short)((float)enemy._power3 * pluspower);
		haveExp += enemy._haveExp;
		block += enemy._block;
		runspeed = enemy._runspeed;
		turnspeed = enemy._turnspeed;
		firerange1 = enemy._firerange1;
		firerange2 = enemy._firerange2;
		firerange3 = enemy._firerange3;
		dash[0] = enemy._dash1;
		dash[1] = enemy._dash2;
		dash[2] = enemy._dash3;
		ref Vector2 reference = ref moving_atk[0];
		reference = enemy._moving_atk1;
		ref Vector2 reference2 = ref moving_atk[1];
		reference2 = enemy._moving_atk2;
		ref Vector2 reference3 = ref moving_atk[2];
		reference3 = enemy._moving_atk3;
		attach_weaponEf[0] = enemy._attach_ef1;
		attach_weaponEf[1] = enemy._attach_ef2;
		attach_weaponEf[2] = enemy._attach_ef3;
		collideroff[0] = enemy._collideroff1;
		collideroff[1] = enemy._collideroff2;
		collideroff[2] = enemy._collideroff3;
		myanimation["b_move"].speed = enemy._speed_move;
		myanimation["b_down"].speed = enemy._speed_down;
		myanimation["b_attack1"].speed = enemy._speed_b_attack1;
		myanimation["b_attack1_i"].speed = enemy._speed_b_attack1_i;
		myanimation["b_attack2"].speed = enemy._speed_b_attack2;
		myanimation["b_attack2_i"].speed = enemy._speed_b_attack2_i;
		myanimation["b_attack3"].speed = enemy._speed_b_attack3;
		myanimation["b_attack3_i"].speed = enemy._speed_b_attack3_i;
		myanimation["b_idle"].speed = enemy._speed_idle;
		myanimation["b_down"].layer = 2;
		myanimation["b_move"].layer = 1;
		myanimation["b_attack1"].layer = 1;
		myanimation["b_attack1_i"].layer = 1;
		myanimation["b_attack2"].layer = 1;
		myanimation["b_attack2_i"].layer = 1;
		myanimation["b_attack3"].layer = 1;
		myanimation["b_attack3_i"].layer = 1;
		myanimation["b_idle"].layer = 1;
		monmovestat = 0;
		originTex = monrender.material.mainTexture;
		cha1 = GameObject.FindWithTag("Player").transform;
		hp = maxhp;
		script_cha = cha1.gameObject.GetComponent<Cha_Control>();
		script_sound = GameObject.FindWithTag("sound").GetComponent<SoundEf_slash>();
		script_cam = Camera.main.GetComponent<Cam_Move>();
		script_monEf = GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>();
		hpbar = script_monEf.CreatHpbar(new Vector2(0.1f, 0.02f), _turretmode: false, _ally: false);
		hpbar.position = mytransform.position;
		script_hpbar = hpbar.GetComponent<Hp_bar>();
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.4f, 0);
		accuracy = script_cha.hitrate;
		atk = script_cha.atk;
		script_monEf.CreatShadow(mytransform.GetChild(0), 2.2f);
		clone_arrow = (Transform)Object.Instantiate(direction_arrow, cha1.position, Quaternion.identity);
		clone_arrow.GetComponent<Renderer>().enabled = false;
		if (enemykind == 0)
		{
			myanimation["trans"].speed = 0.25f;
			myanimation["trans"].layer = 3;
			myanimation.Play("trans");
			bosscutin = true;
			script_cam.LookTarget(mytransform, 30, 4f);
			cha1.GetComponent<Rigidbody>().AddForce(cha1.forward * -240f);
			script_cha.StopControl();
			invince = true;
			Invoke("TurnNormal", 3f);
		}
		else
		{
			InvokeRepeating("SetDir", 0.1f, 0.5f);
		}
		target = cha1;
	}

	private void TurnNormal()
	{
		invince = false;
		InvokeRepeating("SetDir", 0.1f, 0.5f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (invince)
		{
			return;
		}
		int layer = other.gameObject.layer;
		if (layer < 16)
		{
			return;
		}
		setattackkind = true;
		accuracy = script_cha.hitrate;
		atk = script_cha.atk;
		if (layer == 28)
		{
			attackdir = mytransform.position - cha1.position;
			attackdir[1] = 0f;
			attackdir = Vector3.Normalize(attackdir);
		}
		else
		{
			attackdir = mytransform.position - other.transform.position;
			attackdir[1] = 0f;
			attackdir = Vector3.Normalize(attackdir);
		}
		if (attackdir == Vector3.zero)
		{
			attackdir = -Vector3.forward;
		}
		switch (layer)
		{
		case 20:
			other.transform.root.GetComponent<Rigidbody>().AddForce(-attackdir * 80f);
			script_cam.Hitcam();
			attackforce = 60f;
			blockrate = Random.Range(0, 100);
			if (blockrate < block - accuracy && monmovestat >= 0)
			{
				base.GetComponent<Rigidbody>().AddForce(attackdir * 10f);
				script_cha.Blocked(mytransform.position);
				return;
			}
			damage = atk;
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 21:
			script_cam.Hitcam2(1f);
			attackforce = 100f;
			damage = atk;
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			break;
		case 22:
			script_cam.Hitcam();
			attackforce = 50f;
			damage = other.GetComponent<Rigidbody>().mass;
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 23:
		{
			float mass = other.GetComponent<Rigidbody>().mass;
			if (mass == 0.1f)
			{
				damage = 0f;
				poison = true;
				poison_damage = (float)script_cha.atk * 0.6f;
				poison_delay = 4f;
				target = cha1;
				return;
			}
			attackforce = 100f;
			poison_damage = mass;
			damage = poison_damage * 2f;
			script_cam.Hitcam();
			poison = true;
			poison_delay = 12f;
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		}
		case 24:
			attackforce = 100f;
			script_cam.Hitcam();
			life = false;
			petrify_rate = (float)(int)other.GetComponent<Rigidbody>().mass * 0.01f;
			base.gameObject.layer = 10;
			StartCoroutine(Petrify());
			monrender.material.mainTexture = script_monEf.attribute_tex[1];
			script_hpbar.Damaged(maxhp, hp, mytransform, 0.4f, 2);
			target = cha1;
			break;
		case 25:
			attackforce = 60f;
			damage = atk;
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			break;
		case 26:
			myanimation.Stop();
			attackforce = 10f;
			damage = other.GetComponent<Rigidbody>().mass;
			hp -= (short)damage;
			script_hpbar.Damaged(maxhp, 0, mytransform, 9f, 0);
			target = cha1;
			script_monEf.CreatBlood_Only(mytransform.position, attackdir);
			myanimation.Play("b_down");
			break;
		case 28:
			attackforce = 100f;
			damage = script_cha.atk;
			script_cam.Hitcam();
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 29:
			attackforce = 50f;
			damage = (float)script_cha.atk * 0.4f;
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 30:
			attackforce = 100f;
			damage = script_cha.atk;
			script_cam.Hitcam();
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 31:
			attackforce = 100f;
			damage = script_cha.atk;
			script_cam.Hitcam();
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 16:
			attackforce = -50f;
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
			attackforce = 40f;
			damage = other.GetComponent<Rigidbody>().mass;
			script_cam.Hitcam2(0.2f);
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 18:
			attackforce = 100f;
			damage = script_cha.atk;
			script_cam.Hitcam();
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		case 19:
			attackforce = 100f;
			damage = script_cha.atk;
			script_cam.Hitcam();
			target = cha1;
			script_monEf.CreatBlood(mytransform.position, attackdir);
			break;
		}
		if (!life)
		{
			script_sound.SoundOn(1);
			myanimation.Stop();
			return;
		}
		script_sound.SoundOn(1);
		int num = (int)((float)maxhp * 0.3f);
		int num2 = hp / num;
		if (hp == maxhp)
		{
			num2--;
		}
		hp -= (short)damage;
		script_monEf.SetDamageNum(mytransform.position, (short)damage, attackdir);
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.4f, -1);
		if (hp <= 0 && life)
		{
			Dead(2);
		}
		else if (hp < num * num2)
		{
			attackforce = 300f;
			myanimation.Stop();
			myanimation.Play("b_down");
			script_spawn.Summon(4, mytransform.position);
			if ((bool)clone_weapon[currentAtk])
			{
				clone_weapon[currentAtk].gameObject.active = false;
				clone_weapon[currentAtk].position = Vector3.one * 8f;
			}
			if (ef_secondweapon != null)
			{
				ef_secondweapon.gameObject.active = false;
			}
			myaudio.clip = snd_scream;
			myaudio.Play();
		}
		base.GetComponent<Rigidbody>().AddForce(attackdir * attackforce);
	}

	public void Grabed()
	{
	}

	public void SetLevel(short _level, float _restrictArea)
	{
		level = (short)(_level + 1);
		restrictArea = _restrictArea;
		plusmaxhp = 0.55f * ((float)level / 3f) + 0.5f;
		if (level >= 61)
		{
			pluspower = 0.05f * ((float)level / 3f) + 3.5f;
		}
		else if (level >= 31)
		{
			pluspower = 2.5f;
		}
		else
		{
			pluspower = 0.05f * ((float)level / 3f) + 0.8f;
		}
		block += (short)((float)level * 0.5f);
		haveExp += level;
		hp = maxhp;
	}

	public IEnumerator Freeze(short _damage)
	{
		yield return new WaitForSeconds(2f);
		att_status = 0;
		myanimation.enabled = true;
	}

	public IEnumerator Petrify()
	{
		int _rate = Random.Range(0, 100);
		yield return new WaitForSeconds(2.5f);
		if (petrify_rate > (float)_rate)
		{
			Dead(0);
			yield break;
		}
		base.gameObject.layer = 8;
		mytransform.Find("mon").GetComponent<Renderer>().sharedMaterial.mainTexture = originTex;
		life = true;
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.4f, 0);
	}

	public void Dead(int _kind)
	{
		monmovestat = -4;
		life = false;
		script_hpbar.FreeSelect();
		if (weapon != null)
		{
			weapon.parent = null;
			weapon.GetComponent<WeaponDrop>().Drop(_destroy: true);
		}
		script_sound.SoundOn(4);
		Object.Destroy(base.gameObject);
		Object.Destroy(hpbar.gameObject);
		Object.Destroy(clone_arrow.gameObject);
		for (int i = 0; i < 3; i++)
		{
			if (clone_weapon[i] != null)
			{
				Object.Destroy(clone_weapon[i].gameObject);
			}
		}
		if (ef_secondweapon != null)
		{
			Object.Destroy(ef_secondweapon.gameObject);
		}
		if (enemykind != 9)
		{
			script_cha.GainExp(haveExp);
			script_monEf.EnemyDead(_kind, mytransform.position, originTex, Vector3.one * 2.5f, attackdir);
			script_monEf.SetItemBox(level, mytransform.position);
			script_spawn.BossKill(enemykind);
		}
		else
		{
			script_spawn.ChangeBoss(mytransform.position, mytransform.rotation);
		}
	}

	public void Impact2()
	{
		ef_secondweapon.GetComponent<WeaponDamage>().PressDamage(power[currentAtk]);
		ef_secondweapon.gameObject.active = true;
		script_cam.Hitcam();
	}

	public void Impact()
	{
		ef_secondweapon.GetComponent<WeaponDamage>().PressDamage(power[currentAtk]);
		ef_secondweapon.gameObject.active = true;
	}

	public void SetDir()
	{
		if (!life)
		{
			return;
		}
		if (behaviour_delay < 0f && bosscutin)
		{
			behaviour = Random.Range(0, 6);
			if (behaviour == 0)
			{
				myaudio.clip = snd_move;
				myaudio.Play();
			}
			behaviour_delay = 1f;
		}
		if (target.position.y > 2f)
		{
			target = cha1;
		}
		attackrange = Vector3.Distance(mytransform.position, target.position);
		if (!bosscutin && attackrange < 0.7f)
		{
			script_spawn.BossCutin(enemykind);
			bosscutin = true;
			script_cam.LookTarget(mytransform, 30, 0f);
		}
		short num = att_status;
		if (num == 2)
		{
			directionVector = Vector3.zero;
			attackrange = 2f;
		}
		else
		{
			directionVector = target.position - mytransform.position;
			directionVector[1] = 0f;
			directionVector = Vector3.Normalize(directionVector);
		}
		directionVector = target.position - mytransform.position;
		directionVector[1] = 0f;
		directionVector = Vector3.Normalize(directionVector);
		chamovestat = script_cha.chamovestat;
		if (attackrange < firerange3)
		{
			showme = false;
			clone_arrow.GetComponent<Renderer>().enabled = false;
			if (attackstart)
			{
				return;
			}
			if (chamovestat < 50 && monmovestat >= 0)
			{
				if (setattackkind)
				{
					atkkind = (short)Random.Range(0, 3);
					setattackkind = false;
				}
				if (atkkind == 2)
				{
					if (attackrange < firerange3)
					{
						myanimation.Play("b_attack3");
						attack_impact = myanimation.PlayQueued("b_attack3_i", QueueMode.CompleteOthers);
						attack_impact.speed = myanimation["b_attack3_i"].speed;
						attack_impact.layer = 1;
						attack_impact = myanimation.PlayQueued("b_idle", QueueMode.CompleteOthers);
						attack_impact.speed = myanimation["b_idle"].speed;
						attack_impact.layer = 1;
						currentAtk = 2;
						setattackkind = true;
					}
					else
					{
						myanimation.CrossFade("b_move");
					}
				}
				else if (atkkind == 1)
				{
					if (attackrange < firerange2)
					{
						myanimation.Play("b_attack2");
						attack_impact = myanimation.PlayQueued("b_attack2_i", QueueMode.CompleteOthers);
						attack_impact.speed = myanimation["b_attack2_i"].speed;
						attack_impact.layer = 1;
						attack_impact = myanimation.PlayQueued("b_idle", QueueMode.CompleteOthers);
						attack_impact.speed = myanimation["b_idle"].speed;
						attack_impact.layer = 1;
						currentAtk = 1;
						setattackkind = true;
					}
					else
					{
						myanimation.CrossFade("b_move");
					}
				}
				else if (attackrange < firerange1)
				{
					myanimation.Play("b_attack1");
					attack_impact = myanimation.PlayQueued("b_attack1_i", QueueMode.CompleteOthers);
					attack_impact.speed = myanimation["b_attack1_i"].speed;
					attack_impact.layer = 1;
					attack_impact = myanimation.PlayQueued("b_idle", QueueMode.CompleteOthers);
					attack_impact.speed = myanimation["b_idle"].speed;
					attack_impact.layer = 1;
					currentAtk = 0;
					setattackkind = true;
				}
				else
				{
					myanimation.CrossFade("b_move");
				}
				attackstart = true;
			}
			else if (!attackstart)
			{
				myanimation.CrossFade("b_idle");
				behaviour = 4;
				behaviour_delay = 1f;
			}
		}
		else if (!attackstart)
		{
			if (behaviour >= 5)
			{
				myanimation.CrossFade("b_idle");
			}
			else
			{
				myanimation.CrossFade("b_move");
			}
			showme = true;
			clone_arrow.GetComponent<Renderer>().enabled = true;
		}
	}

	private void Update()
	{
		if (!life)
		{
			return;
		}
		behaviour_delay -= Time.deltaTime;
		if (myanimation.IsPlaying("trans"))
		{
			return;
		}
		if (myanimation.IsPlaying("b_down"))
		{
			monmovestat = -1;
		}
		else if (myanimation.IsPlaying("b_attack1") || myanimation.IsPlaying("b_attack2") || myanimation.IsPlaying("b_attack3"))
		{
			impact = false;
			if (directionVector != Vector3.zero)
			{
				lookrotation = Quaternion.LookRotation(directionVector);
			}
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, lookrotation, Time.deltaTime * turnspeed);
			if (monmovestat != 11)
			{
				if (collideroff[currentAtk] && !jump)
				{
					mycollider.enabled = false;
					jump = true;
				}
				if (attach_weaponEf[currentAtk] == 2)
				{
					ef_secondweapon.gameObject.active = true;
				}
			}
			if (moving_atk[currentAtk] != Vector2.zero)
			{
				if (m_atk_delay > moving_atk[currentAtk].x)
				{
					mytransform.position += directionVector * Time.deltaTime * moving_atk[currentAtk].y;
				}
				else
				{
					m_atk_delay += Time.deltaTime;
				}
			}
			monmovestat = 11;
		}
		else if (myanimation.IsPlaying("b_attack1_i") || myanimation.IsPlaying("b_attack2_i") || myanimation.IsPlaying("b_attack3_i"))
		{
			monmovestat = 12;
			if (!impact)
			{
				if (jump)
				{
					mycollider.enabled = true;
					jump = false;
				}
				base.GetComponent<Rigidbody>().AddForce(mytransform.forward * dash[currentAtk]);
				impact = true;
				Vector3 position = mytransform.position + Vector3.up * ef_weapon[currentAtk].localPosition.y + mytransform.forward * ef_weapon[currentAtk].localPosition.z + mytransform.right * ef_weapon[currentAtk].localPosition.x;
				Quaternion rotation = Quaternion.Euler(ef_weapon[currentAtk].eulerAngles.x, ef_weapon[currentAtk].eulerAngles.y + mytransform.eulerAngles.y, ef_weapon[currentAtk].eulerAngles.z);
				if (clone_weapon[currentAtk] == null)
				{
					clone_weapon[currentAtk] = (Transform)Object.Instantiate(ef_weapon[currentAtk], position, rotation);
					clone_weapon[currentAtk].gameObject.active = true;
					clone_weapon[currentAtk].GetComponent<WeaponDamage>().PressDamage(power[currentAtk]);
					if (attach_weaponEf[currentAtk] >= 1)
					{
						clone_weapon[currentAtk].parent = mytransform;
						if (attach_weaponEf[currentAtk] == 2)
						{
							ef_secondweapon.GetComponent<WeaponDamage>().PressDamage(power[currentAtk]);
						}
					}
				}
				else
				{
					clone_weapon[currentAtk].position = position;
					clone_weapon[currentAtk].rotation = rotation;
					clone_weapon[currentAtk].gameObject.active = true;
				}
				m_atk_delay = 0f;
				mytransform.position = Vector3.right * mytransform.position.x + Vector3.forward * mytransform.position.z;
			}
		}
		else if (myanimation.IsPlaying("b_move"))
		{
			attackstart = false;
			monmovestat = 1;
			mytransform.position += directionVector * Time.deltaTime * runspeed;
			if (directionVector != Vector3.zero)
			{
				lookrotation = Quaternion.LookRotation(directionVector);
			}
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, lookrotation, Time.deltaTime * 3f);
		}
		else
		{
			monmovestat = 1;
			attackstart = false;
		}
		if (showme)
		{
			arrowTargetVector = Vector3.Normalize(mytransform.position - cha1.position);
			if (arrowTargetVector != Vector3.zero)
			{
				clone_arrow.rotation = Quaternion.LookRotation(arrowTargetVector);
			}
			arrowTargetVector = cha1.position + Vector3.up * 0.02f + arrowTargetVector * 0.3f;
			clone_arrow.position = Vector3.Lerp(clone_arrow.position, arrowTargetVector, Time.deltaTime * 4f);
			clone_arrow.GetComponent<Renderer>().enabled = true;
			chamovestat = 0;
		}
		if (poison)
		{
			poison_delay -= Time.deltaTime;
			if (poison_delay < 0f)
			{
				poison = false;
				script_hpbar.Damaged(maxhp, hp, mytransform, 0.4f, 0);
			}
			else if ((short)poison_delay != old_delay)
			{
				hp -= (short)poison_damage;
				script_hpbar.Damaged(maxhp, hp, mytransform, 0.4f, 1);
				old_delay = (short)poison_delay;
				if (hp <= 0)
				{
					Dead(1);
				}
			}
		}
		if (Vector3.SqrMagnitude(mytransform.position) > restrictArea)
		{
			base.GetComponent<Rigidbody>().AddForce(-mytransform.position * 20f);
		}
	}
}
