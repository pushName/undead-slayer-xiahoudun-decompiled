using System.Collections;
using UnityEngine;

public class Cha_Skill : MonoBehaviour
{
	private const int MAXSKILL = 20;

	private Ef_rotfog script_rotfog;

	private Cam_Move script_cam;

	private Cha_Control script_cha1;

	private Cha_Costume script_costume;

	private DB_Skill script_DBskill;

	public AudioClip snd_jin;

	private int[] cur_skill_grade = new int[20];

	private int skill_index = -1;

	private float castingdelay;

	private float skillatk;

	private float repeatatk;

	private short motionkind;

	private short g_skillatk;

	private short g_motionkind;

	private int repeattime;

	private int repeatkind;

	private float repeatdelay;

	private float currentdelay = 10f;

	private float pt_off_delay = 10f;

	private bool repeat;

	private float basedamage = 1f;

	private float tempradian;

	public Transform swordwind;

	public Transform rapidstab;

	public Transform wheelwind;

	public Transform meteo;

	public Transform supersword;

	public Transform energypillar;

	public Transform pt_attackup;

	public Transform pt_defenceup;

	public Transform pt_boom;

	public Transform sworddance;

	public Transform pt_poison;

	public Transform ef_gather;

	public Transform deathhand;

	public Transform chosun;

	public Transform junwui;

	public Transform jin;

	public Transform dragonhead;

	public Transform ef_rotfog;

	public Transform fincanon;

	public Transform bamboo;

	public Transform crow;

	public Transform bamboobase;

	public Transform swordrain_b;

	public Transform swordrain_s;

	public Transform swordrain;

	public Transform wing;

	public Transform ef_groundbreak;

	public Transform bombsphere;

	public Transform lightningblade;

	private Transform directionArrow;

	private Transform pt_ki;

	private Transform mytransform;

	private Animation myanimation;

	private Transform c_defenceup;

	private Transform c_sworddance;

	private Transform c_poison;

	private Transform c_rapidstab;

	private Transform c_wheelwind;

	private Transform c_hand;

	private Transform c_chosun;

	private Transform c_junwui;

	private Transform c_dragonhead;

	private Transform c_swordwind;

	private Transform c_fincanon;

	private Transform c_crow;

	private Transform c_bamboobase;

	private Transform c_swordrain_b;

	private Transform c_swordrain_s;

	private Transform c_swordrain;

	private Transform c_wing;

	private Transform c_groundbreak;

	private Transform c_bombsphere;

	private Transform c_icespear;

	private Transform c_lightningblade;

	private Transform[] c_bamboo = new Transform[3];

	private Transform c_energypillar;

	private Transform c_supersword;

	private Transform r_hand;

	private Transform[] c_generalskill = new Transform[7];

	private GameObject load_skobj;

	private Vector3 rndpos;

	private AnimationState temp;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
	}

	private void Start()
	{
		GameObject gameObject = GameObject.FindWithTag("ui");
		script_DBskill = gameObject.GetComponent<DB_Skill>();
		cur_skill_grade = PlayerPrefsX.GetIntArray("n22");
		script_cha1 = GetComponent<Cha_Control>();
		script_costume = GetComponent<Cha_Costume>();
		script_rotfog = ef_rotfog.GetComponent<Ef_rotfog>();
		script_cam = Camera.main.GetComponent<Cam_Move>();
		pt_ki = mytransform.Find("pt_ki");
		directionArrow = mytransform.Find("directionArrow");
	}

	public void SetBaseDamage(float _dmg)
	{
		basedamage = _dmg;
	}

	public void Set_General_Skill(short _maxatk, short _skillatk, short _motionkind)
	{
		g_skillatk = (short)((float)(_maxatk * _skillatk) * 0.01f);
		g_skillatk = (short)Mathf.Max(g_skillatk, 1);
		g_motionkind = _motionkind;
	}

	public void Set_R_hand(Transform _hand)
	{
		r_hand = _hand;
	}

	public void PetSkillOn(int index)
	{
		ef_rotfog.position = mytransform.position;
		script_rotfog.RotfogOn(3f, 4f, 80, 5, 3f);
		script_cam.ZoomIn(5, 15, 1f);
		switch (index)
		{
		case 0:
			script_cha1.CallHorse();
			break;
		case 1:
			script_cha1.Fly();
			break;
		}
	}

	public void SkillOn(int _index, bool _general)
	{
		script_cha1.SkillStart();
		{
			var emission = pt_ki.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		base.GetComponent<Rigidbody>().drag = 20f;
		skill_index = _index;
		ef_rotfog.position = mytransform.position;
		script_rotfog.RotfogOn(1.5f, 40f, 800, 150, 0.5f);
		Time.timeScale = 0.1f;
		if (!_general)
		{
			motionkind = script_DBskill.ss[skill_index, 0]._kind;
			cur_skill_grade[skill_index] = Mathf.Max(0, cur_skill_grade[skill_index]);
			skillatk = script_DBskill.ss[skill_index, cur_skill_grade[skill_index]]._attackpoint;
		}
		else
		{
			motionkind = g_motionkind;
			skillatk = g_skillatk;
			skill_index += 21;
		}
		if (motionkind == 1)
		{
			directionArrow.GetComponent<Renderer>().enabled = true;
			myanimation.Play("cast1");
		}
		else if (motionkind == 2)
		{
			script_cam.ZoomIn(20, 16, 0.3f);
			myanimation.Play("cast2");
			if (skill_index == 13 || skill_index == 15)
			{
				base.GetComponent<AudioSource>().PlayOneShot(snd_jin);
				jin.gameObject.active = true;
				jin.position = mytransform.position + mytransform.forward * 0.2f + Vector3.up * 0.01f;
			}
			else if (skill_index == 19)
			{
				wing.gameObject.active = true;
			}
		}
		else if (motionkind == 3)
		{
			directionArrow.GetComponent<Renderer>().enabled = true;
			myanimation.Play("cast3");
			if (skill_index == 8)
			{
				ef_gather.position = mytransform.position + Vector3.up * 0.22f;
				ef_gather.gameObject.active = true;
				ef_gather.GetComponent<Ef_energy_gather>().SetTex(0, 1.2f);
			}
			else if (skill_index == 9)
			{
				ef_gather.position = mytransform.position + Vector3.up * 0.22f;
				ef_gather.gameObject.active = true;
				ef_gather.GetComponent<Ef_energy_gather>().SetTex(1, 0.8f);
			}
		}
		else
		{
			if (motionkind != 4)
			{
				return;
			}
			directionArrow.GetComponent<Renderer>().enabled = true;
			myanimation.Play("cast4");
			if (skill_index == 16)
			{
				jin.gameObject.active = true;
				jin.position = mytransform.position + Vector3.up * 0.01f;
				if (c_fincanon == null)
				{
					c_fincanon = (Transform)Object.Instantiate(fincanon, mytransform.position, mytransform.rotation);
					c_fincanon.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
					c_fincanon.parent = mytransform;
				}
				else
				{
					c_fincanon.gameObject.active = true;
					c_fincanon.position = mytransform.position;
					c_fincanon.rotation = mytransform.rotation;
					c_fincanon.parent = mytransform;
				}
			}
		}
	}

	public void LaunchSkill(int index)
	{
		base.GetComponent<AudioSource>().PlayOneShot(snd_jin);
		{
			var emission = pt_ki.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
		switch (index)
		{
		case 0:
			myanimation.Play("skill01");
			if (c_swordwind == null)
			{
				c_swordwind = (Transform)Object.Instantiate(swordwind, mytransform.position, mytransform.rotation);
				c_swordwind.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			}
			else
			{
				c_swordwind.gameObject.active = true;
				c_swordwind.position = mytransform.position;
				c_swordwind.rotation = mytransform.rotation;
			}
			break;
		case 1:
		{
			myanimation.Play("jumpattack_air");
			AnimationState animationState = myanimation.PlayQueued("jumpattack_impact");
			animationState.speed = 0.25f;
			break;
		}
		case 2:
			myanimation.Play("wheelwind");
			if (c_wheelwind == null)
			{
				c_wheelwind = (Transform)Object.Instantiate(wheelwind, mytransform.position + Vector3.up * 0.1f, Quaternion.Euler(180f, 0f, 0f));
				c_wheelwind.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
				c_wheelwind.parent = mytransform;
			}
			else
			{
				c_wheelwind.gameObject.active = true;
			}
			break;
		case 3:
			myanimation.Play("skill03");
			if (c_defenceup == null)
			{
				c_defenceup = (Transform)Object.Instantiate(pt_defenceup, mytransform.position, Quaternion.identity);
			}
			else
			{
				CancelInvoke("Sk_DefenceUp");
			}
			pt_boom.GetComponent<Ef_boom>().SetTex(1, mytransform.position, _collider: true);
			script_cha1.DefenceUp();
			Invoke("Sk_DefenceUp", skillatk);
			break;
		case 4:
			myanimation.Play("rapidstab");
			if (c_rapidstab == null)
			{
				c_rapidstab = (Transform)Object.Instantiate(rapidstab, mytransform.position + mytransform.forward * 0.25f + Vector3.up * 0.04f, mytransform.rotation);
				c_rapidstab.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			}
			else
			{
				c_rapidstab.gameObject.active = true;
				c_rapidstab.position = mytransform.position + mytransform.forward * 0.25f + Vector3.up * 0.04f;
				c_rapidstab.rotation = mytransform.rotation;
			}
			break;
		case 5:
			myanimation["attackex3"].speed = 0.35f;
			myanimation.Play("attackex3");
			temp = myanimation.PlayQueued("attackex3_impact");
			temp.speed = 0.25f;
			if (c_lightningblade == null)
			{
				c_lightningblade = (Transform)Object.Instantiate(lightningblade, mytransform.position + Vector3.up * 0.05f, mytransform.rotation);
				c_lightningblade.parent = mytransform;
				c_lightningblade.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			}
			else
			{
				c_lightningblade.gameObject.active = true;
			}
			break;
		case 6:
			myanimation.Play("skill03");
			if (c_icespear == null)
			{
				c_icespear = (Transform)Object.Instantiate(meteo, mytransform.position + Vector3.up * 0.2f, mytransform.rotation);
				c_icespear.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
				break;
			}
			c_icespear.GetComponent<SpiritSword>().Init();
			c_icespear.position = mytransform.position + Vector3.up * 0.2f;
			c_icespear.rotation = mytransform.rotation;
			c_icespear.gameObject.active = true;
			break;
		case 7:
		{
			myanimation.Play("skill03");
			ParticleSystem particleEmitter = pt_attackup.GetComponent<ParticleSystem>();
			if (!particleEmitter.emission.enabled)
			{
				{
					var emission = particleEmitter.emission;
					emission.enabled = true;
				}
			}
			else
			{
				CancelInvoke("Sk_AttackUp");
			}
			pt_boom.GetComponent<Ef_boom>().SetTex(0, mytransform.position, _collider: true);
			script_cha1.AttakUp(1.5f);
			Invoke("Sk_AttackUp", skillatk);
			break;
		}
		case 8:
			myanimation.Play("skill02");
			c_poison = (Transform)Object.Instantiate(pt_poison, mytransform.position + Vector3.up * 0.22f, Quaternion.identity);
			c_poison.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			if (c_poison.GetComponent<Rigidbody>().mass < 1f)
			{
				c_poison.GetComponent<Rigidbody>().mass = 1f;
			}
			c_poison.parent = mytransform;
			break;
		case 9:
			myanimation.Play("skill04");
			if (c_hand == null)
			{
				c_hand = (Transform)Object.Instantiate(deathhand, mytransform.position + Vector3.up * 0.05f, mytransform.rotation);
				c_hand.GetComponent<Rigidbody>().mass = skillatk;
				c_hand.parent = mytransform;
			}
			else
			{
				c_hand.gameObject.active = true;
			}
			break;
		case 10:
			myanimation.Play("skill_chargesmash");
			if (c_supersword == null)
			{
				c_supersword = (Transform)Object.Instantiate(supersword, r_hand.position, r_hand.rotation);
				c_supersword.up = r_hand.forward;
				c_supersword.parent = r_hand;
				c_supersword.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			}
			else
			{
				c_supersword.gameObject.active = true;
			}
			break;
		case 11:
			script_cha1.Invincibility(5f);
			myanimation.Play("skill03");
			repeatatk = skillatk * basedamage * 0.01f;
			Repeatskill(skill_index, 5, 0.8f);
			break;
		case 12:
			myanimation.Play("skill01");
			if (c_sworddance == null)
			{
				c_sworddance = (Transform)Object.Instantiate(sworddance, mytransform.position + Vector3.up * 0.05f, Quaternion.LookRotation(-mytransform.forward + mytransform.right));
				c_sworddance.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			}
			else
			{
				c_sworddance.position = mytransform.position + Vector3.up * 0.05f;
				c_sworddance.rotation = Quaternion.LookRotation(-mytransform.forward + mytransform.right);
				c_sworddance.gameObject.active = true;
			}
			StartCoroutine(Sk_SwordDance());
			script_costume.Disappear();
			script_cha1.StopControl();
			break;
		case 13:
			myanimation.Play("skill03");
			script_cha1.Invincibility(4f);
			if (c_junwui == null)
			{
				c_junwui = (Transform)Object.Instantiate(junwui, mytransform.position + mytransform.forward * 0.2f, mytransform.rotation);
				c_junwui.GetComponent<Junwui>().SetDamage(skillatk * basedamage * 0.01f);
			}
			else
			{
				c_junwui.gameObject.active = false;
				c_junwui.gameObject.active = true;
				c_junwui.position = mytransform.position + mytransform.forward * 0.2f;
				c_junwui.rotation = mytransform.rotation;
			}
			script_cam.LookTarget(c_junwui, 25, 0.7f);
			break;
		case 14:
			myanimation.Play("skill04");
			script_cha1.Invincibility(4f);
			if (c_dragonhead == null)
			{
				c_dragonhead = (Transform)Object.Instantiate(dragonhead, mytransform.position - mytransform.forward * 0.2f, mytransform.rotation);
				c_dragonhead.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
				break;
			}
			c_dragonhead.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			c_dragonhead.gameObject.active = true;
			c_dragonhead.position = mytransform.position - mytransform.forward * 0.2f;
			c_dragonhead.rotation = mytransform.rotation;
			break;
		case 15:
			myanimation.Play("skill03");
			script_cha1.Invincibility(4f);
			if (c_chosun == null)
			{
				c_chosun = (Transform)Object.Instantiate(chosun, mytransform.position + mytransform.forward * 0.2f, chosun.localRotation);
				c_chosun.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
			}
			else
			{
				c_chosun.gameObject.active = false;
				c_chosun.gameObject.active = true;
				c_chosun.position = mytransform.position + mytransform.forward * 0.2f;
				c_chosun.rotation = chosun.localRotation;
			}
			script_cam.LookTarget(c_chosun, 28, 0.5f);
			break;
		case 16:
			myanimation.Play("skill03");
			temp = myanimation.PlayQueued("skill03");
			temp.speed = 0.2f;
			break;
		case 17:
			if (c_crow == null)
			{
				c_crow = (Transform)Object.Instantiate(crow, mytransform.position + mytransform.forward * 0.35f + Vector3.up * 0.3f, mytransform.rotation);
				c_bamboobase = (Transform)Object.Instantiate(bamboobase, mytransform.position + mytransform.forward * 0.35f + Vector3.up * 0.01f, mytransform.rotation);
			}
			else
			{
				c_crow.position = mytransform.forward * 0.35f + Vector3.up * 0.3f;
				c_bamboobase.position = mytransform.forward * 0.35f + Vector3.up * 0.01f;
				c_crow.rotation = mytransform.rotation;
				c_bamboobase.rotation = mytransform.rotation;
			}
			myanimation.Play("skill03");
			repeatatk = skillatk * basedamage * 0.01f;
			Repeatskill(skill_index, 3, 0.2f);
			StartCoroutine(BambooFin());
			break;
		case 18:
			myanimation.Play("skill03");
			if (c_swordrain_s == null)
			{
				c_swordrain_s = (Transform)Object.Instantiate(swordrain_s, mytransform.position, Quaternion.identity);
				c_swordrain = (Transform)Object.Instantiate(swordrain, mytransform.position + Vector3.up * 1f, mytransform.rotation);
				c_swordrain_b = (Transform)Object.Instantiate(swordrain_b, mytransform.position + Vector3.up * -0.1f, mytransform.rotation);
				c_swordrain_b.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
				break;
			}
			c_swordrain_s.gameObject.active = true;
			c_swordrain_s.position = mytransform.position;
			c_swordrain.gameObject.active = true;
			c_swordrain.position = mytransform.position + Vector3.up * 1f;
			c_swordrain.rotation = mytransform.rotation;
			c_swordrain_b.gameObject.active = true;
			c_swordrain_b.position = mytransform.position + Vector3.up * -0.1f;
			c_swordrain_b.rotation = mytransform.rotation;
			break;
		case 19:
			myanimation.Play("wing");
			temp = myanimation.PlayQueued("wing_i");
			temp.speed = 0.2f;
			break;
		case 21:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_spiritsword") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 0.2f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position + Vector3.up * 0.2f;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 22:
			myanimation.Play("skill_chargesmash");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_spinaxe") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, r_hand.position + r_hand.forward * 0.2f, r_hand.rotation);
				c_generalskill[0].parent = r_hand;
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				{
					var emission = c_generalskill[0].GetChild(0).GetComponent<ParticleSystem>().emission;
					emission.enabled = true;
				}
			}
			pt_off_delay = 2.5f;
			StartCoroutine(Pt_Off(c_generalskill[0].GetChild(0)));
			break;
		case 23:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_fly") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 0.1f, Quaternion.identity);
			}
			else
			{
				c_generalskill[0].position = mytransform.position + Vector3.up * 0.1f;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 24:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_sister") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 25:
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_ice_spear") as GameObject;
				GameObject gameObject = Resources.Load("general_icebg") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(gameObject.transform, mytransform.position + mytransform.forward * 0.15f + Vector3.up * 0.01f, mytransform.rotation);
			}
			else
			{
				{
					var emission = c_generalskill[0].GetComponent<ParticleSystem>().emission;
					emission.enabled = true;
				}
				c_generalskill[0].position = mytransform.position + mytransform.forward * 0.15f + Vector3.up * 0.01f;
				c_generalskill[0].rotation = mytransform.rotation;
			}
			myanimation.Play("skill03");
			repeatatk = skillatk;
			pt_off_delay = 6f;
			StartCoroutine(Pt_Off(c_generalskill[0]));
			Repeatskill(skill_index, 5, 0.12f);
			break;
		case 26:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_sworddance") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position, Quaternion.identity);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				c_generalskill[0].position = mytransform.position;
			}
			break;
		case 27:
			myanimation.Play("wheelwind");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_axedance") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 28:
			myanimation.Play("skill04");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_arrowrain") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 1f - mytransform.forward * 0.7f, mytransform.rotation);
				c_generalskill[0].parent = mytransform;
				load_skobj = Resources.Load("general_arrowrain_base") as GameObject;
				c_generalskill[1] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * -0.03f + mytransform.forward * 0.35f, mytransform.rotation);
				c_generalskill[1].GetComponent<Rigidbody>().mass = skillatk;
				c_generalskill[1].parent = mytransform;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				c_generalskill[0].position = mytransform.position + Vector3.up * 1f - mytransform.forward * 0.7f;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[1].gameObject.active = true;
				c_generalskill[1].position = mytransform.position + Vector3.up * -0.03f + mytransform.forward * 0.35f;
				c_generalskill[1].rotation = mytransform.rotation;
			}
			break;
		case 29:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_risespear") as GameObject;
			}
			repeatatk = skillatk;
			Repeatskill(skill_index, 6, 0.1f);
			break;
		case 30:
			myanimation.Play("skill02");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_meteo") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 4.2f - mytransform.forward * 5.5f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position + Vector3.up * 4.2f - mytransform.forward * 5.5f;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 31:
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_mirage") as GameObject;
			}
			myanimation.Play("skill03");
			repeatatk = skillatk;
			Repeatskill(skill_index, 6, 0.05f);
			StartCoroutine(Sk_Mirage());
			script_costume.Disappear();
			script_cha1.StopControl();
			break;
		case 32:
			myanimation.Play("dodge");
			temp = myanimation.PlayQueued("attackex2");
			temp.speed = 0.28f;
			temp.layer = 2;
			temp = myanimation.PlayQueued("attackex2_impact");
			temp.speed = 0.25f;
			temp.layer = 2;
			base.GetComponent<Rigidbody>().AddForce(mytransform.forward * 180f);
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_edge_wheel") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 0.1f + mytransform.forward * 0.2f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position + Vector3.up * 0.1f + mytransform.forward * 0.2f;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 33:
			myanimation.Play("skill04");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_swamp") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 0.01f + mytransform.forward * 0.5f, Quaternion.identity);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position + Vector3.up * 0.01f + mytransform.forward * 0.5f;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 34:
			myanimation.Play("skill04");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_multispear") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 0.1f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position + Vector3.up * 0.1f;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 35:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_fincanon") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
				c_generalskill[0].parent = mytransform;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				c_generalskill[0].position = mytransform.position;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[0].parent = mytransform;
			}
			base.GetComponent<Collider>().enabled = false;
			script_cha1.Invincibility(3f);
			break;
		case 36:
			myanimation.Play("wheelwind");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_flowerfan") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + Vector3.up * 0.1f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].position = mytransform.position + Vector3.up * 0.1f;
				c_generalskill[0].rotation = mytransform.rotation;
				c_generalskill[0].gameObject.active = true;
			}
			break;
		case 37:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_chain") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position - Vector3.up * 0.1f, Quaternion.identity);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				c_generalskill[0].position = mytransform.position - Vector3.up * 0.1f;
			}
			break;
		case 38:
			myanimation.Play("skill03");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_arrow_poison") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + mytransform.forward * 0.05f + Vector3.up * 0.07f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				c_generalskill[0].position = mytransform.position + mytransform.forward * 0.1f + Vector3.up * 0.07f;
				c_generalskill[0].rotation = mytransform.rotation;
			}
			break;
		case 39:
			myanimation.Play("rapidstab");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_rapidthrust") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + mytransform.forward * 0.15f + Vector3.up * 0.05f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				c_generalskill[0].position = mytransform.position + mytransform.forward * 0.15f + Vector3.up * 0.05f;
				c_generalskill[0].rotation = mytransform.rotation;
			}
			break;
		case 40:
			myanimation.Play("skill04");
			if (load_skobj == null)
			{
				load_skobj = Resources.Load("general_elec_shock") as GameObject;
				c_generalskill[0] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position + mytransform.forward * 0.22f + Vector3.up * 0.06f, mytransform.rotation);
				c_generalskill[0].GetComponent<Rigidbody>().mass = skillatk;
			}
			else
			{
				c_generalskill[0].gameObject.active = true;
				c_generalskill[0].position = mytransform.position + mytransform.forward * 0.22f + Vector3.up * 0.06f;
				c_generalskill[0].rotation = mytransform.rotation;
			}
			break;
		}
		skill_index = -1;
	}

	public void DelaySkill(int _index)
	{
		if (c_groundbreak == null)
		{
			c_groundbreak = (Transform)Object.Instantiate(ef_groundbreak, mytransform.position, Quaternion.identity);
		}
		else
		{
			c_groundbreak.position = mytransform.position;
			c_groundbreak.gameObject.active = true;
		}
		c_groundbreak.GetComponent<Rigidbody>().mass = skillatk * basedamage * 0.01f;
		switch (_index)
		{
		case 2:
			c_groundbreak.localScale = Vector3.one * 0.5f;
			script_cam.Hitcam2(2f);
			break;
		case 19:
			c_groundbreak.localScale = Vector3.one * 1f;
			script_cam.Hitcam2(4f);
			Time.timeScale = 0.6f;
			StartCoroutine(ResetTimeScale());
			c_bombsphere = (Transform)Object.Instantiate(bombsphere, mytransform.position, Quaternion.identity);
			Object.Destroy(c_bombsphere.gameObject, 1.2f);
			break;
		}
	}

	public void Repeatskill(int _kind, int _count, float _delay)
	{
		repeat = true;
		repeatkind = _kind;
		repeattime = _count;
		repeatdelay = _delay;
		currentdelay = repeatdelay;
		switch (repeatkind)
		{
		case 11:
			rndpos = mytransform.position + mytransform.forward * 0.3f + mytransform.right * Random.Range(-0.2f, 0.2f);
			rndpos[1] = 0.01f;
			c_energypillar = (Transform)Object.Instantiate(energypillar, rndpos, Quaternion.identity);
			c_energypillar.GetComponent<Rigidbody>().mass = repeatatk;
			Object.Destroy(c_energypillar.gameObject, 1.5f);
			script_cam.Hitcam2(1.5f);
			break;
		case 17:
			rndpos = mytransform.position + mytransform.forward * Random.Range(0.3f, 0.45f) + mytransform.right * (2 - repeattime) * 0.5f;
			rndpos[1] = -1f;
			if (c_bamboo[3 - repeattime] == null)
			{
				c_bamboo[3 - repeattime] = (Transform)Object.Instantiate(bamboo, rndpos, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
				c_bamboo[3 - repeattime].GetComponent<Rigidbody>().mass = repeatatk;
			}
			else
			{
				c_bamboo[3 - repeattime].position = rndpos;
				c_bamboo[3 - repeattime].gameObject.active = true;
			}
			script_cam.Hitcam2(1.5f);
			break;
		case 25:
			tempradian = ((float)((3 - repeattime) * 45) - mytransform.eulerAngles.y + 90f) * 0.01745f;
			rndpos = mytransform.position + 0.3f * Vector3.right * Mathf.Cos(tempradian) + 0.3f * Vector3.forward * Mathf.Sin(tempradian);
			rndpos[1] = -1f;
			if (c_generalskill[repeattime] == null)
			{
				c_generalskill[repeattime] = (Transform)Object.Instantiate(load_skobj.transform, rndpos, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
				c_generalskill[repeattime].localScale = Vector3.one + Vector3.up * Random.Range(-0.1f, 0.5f);
				c_generalskill[repeattime].localScale *= Random.Range(0.7f, 1.2f);
				c_generalskill[repeattime].GetComponent<Rigidbody>().mass = repeatatk;
			}
			else
			{
				c_generalskill[repeattime].localScale = Vector3.one + Vector3.up * Random.Range(-0.1f, 0.5f);
				c_generalskill[repeattime].localScale *= Random.Range(0.7f, 1.2f);
				c_generalskill[repeattime].position = rndpos;
				c_generalskill[repeattime].gameObject.active = true;
			}
			break;
		case 29:
			rndpos = mytransform.position + mytransform.forward * (0.1f + 0.14f * (float)(6 - repeattime)) + mytransform.right * Random.Range(-0.1f, 0.1f);
			rndpos[1] = -1f;
			if (c_generalskill[repeattime] == null)
			{
				c_generalskill[repeattime] = (Transform)Object.Instantiate(load_skobj.transform, rndpos, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
				c_generalskill[repeattime].GetComponent<Rigidbody>().mass = repeatatk;
			}
			else
			{
				c_generalskill[repeattime].position = rndpos;
				c_generalskill[repeattime].gameObject.active = true;
			}
			break;
		case 31:
			if (c_generalskill[repeattime] == null)
			{
				c_generalskill[repeattime] = (Transform)Object.Instantiate(load_skobj.transform, mytransform.position, Quaternion.Euler(0f, repeattime * 60, 0f));
				c_generalskill[repeattime].GetComponent<Rigidbody>().mass = repeatatk;
			}
			else
			{
				c_generalskill[repeattime].position = mytransform.position;
				c_generalskill[repeattime].rotation = Quaternion.Euler(0f, repeattime * 60, 0f);
				c_generalskill[repeattime].gameObject.active = true;
			}
			break;
		}
	}

	public IEnumerator Pt_Off(Transform _pt)
	{
		yield return new WaitForSeconds(pt_off_delay);
		{
			var emission = _pt.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
	}

	public void BoomOn(int _index, bool _collider)
	{
		pt_boom.GetComponent<Ef_boom>().SetTex(_index, mytransform.position, _collider);
	}

	public IEnumerator ResetTimeScale()
	{
		yield return new WaitForSeconds(0.5f);
		Time.timeScale = 1f;
		pt_boom.GetComponent<Ef_boom>().SetTex(0, mytransform.position, _collider: false);
	}

	private void Sk_AttackUp()
	{
		{
			var emission = pt_attackup.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
		script_cha1.ResetAtk();
	}

	public IEnumerator BambooFin()
	{
		yield return new WaitForSeconds(3.5f);
		{
			var emission = c_crow.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
		{
			var emission = c_bamboobase.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
	}

	private void Sk_DefenceUp()
	{
		Object.Destroy(c_defenceup.gameObject);
		script_cha1.ResetDef();
	}

	public IEnumerator Sk_SwordDance()
	{
		yield return new WaitForSeconds(0.5f);
		myanimation.Stop();
		AnimationState comein = myanimation.PlayQueued("change_in_atk");
		comein.speed = 0.5f;
		script_costume.Appear();
		script_cha1.StartControl();
	}

	public IEnumerator Sk_Mirage()
	{
		yield return new WaitForSeconds(1f);
		script_costume.Appear();
		script_cha1.StartControl();
		base.GetComponent<Animation>().Play("attackex2_impact");
	}

	private void Update()
	{
		if (skill_index != -1)
		{
			castingdelay += Time.deltaTime;
			if (castingdelay > 0.18f)
			{
				base.GetComponent<Rigidbody>().drag = 5f;
				Time.timeScale = 1f;
				myanimation.Stop();
				LaunchSkill(skill_index);
				castingdelay = 0f;
				directionArrow.GetComponent<Renderer>().enabled = false;
			}
		}
		if (!repeat || !(currentdelay > 0f))
		{
			return;
		}
		currentdelay -= Time.deltaTime;
		if (currentdelay <= 0f)
		{
			repeattime--;
			if (repeattime > 0)
			{
				Repeatskill(repeatkind, repeattime, repeatdelay);
			}
			else
			{
				repeat = false;
			}
		}
	}
}
