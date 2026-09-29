using System.Collections;
using UnityEngine;

public class Cha_Control : MonoBehaviour
{
	private Quaternion rotate;

	private Vector3 directionVector;

	private Vector3 tempdir;

	private Vector3 attackVector;

	private Vector3 chapos;

	private Vector3 arrowpos;

	private float magnitude_attackdir;

	private float combotime;

	private float speedfactor = 7f;

	private float dubbleclick = 1f;

	private int curruntattack;

	private int currentPet;

	private int autorate;

	private int attack_horse;

	private int attack_eagle;

	private int damage;

	private int special_kind = -2;

	private int special_kind_origin = -2;

	private int special_amount;

	private int guard_break;

	private int battlestyle;

	private bool isplaycha;

	private bool storyrun;

	private bool exstart;

	private bool grabstart;

	private bool dunenter;

	private float pressdelay;

	private float fogdelay = 0.15f;

	private float expboost = 1f;

	private float itemboolst = 1f;

	public Transform screen_effect;

	public bool rideon;

	public int chamovestat;

	public int maxatk;

	public int defence;

	public int hitrate;

	public int vitality;

	public int evasion;

	public int minatk;

	public int endurance;

	public int critical;

	public int resist;

	public int atk;

	private int hp;

	private int cha_hp;

	private int cha_maxhp;

	private int maxhp;

	public int g_hp;

	private short g_maxhp;

	private short general_kind;

	private short general_weapon;

	public short g_atk;

	private short g_def;

	private float g_atkspd;

	public float sp;

	private float atkspd;

	private float atkspd_origin;

	private float grabheal_factor = 0.1f;

	private int origin_maxattack;

	private int origin_minattack;

	private int origin_hitrate;

	private int origin_defence;

	private int origin_endurance;

	private int hpplus;

	private int cur_weapon;

	public float maxsp;

	public Transform shadow;

	public Transform ef_arrow_multy;

	public Transform ef_arrow_single;

	public Transform pt_heal;

	public Transform pt_rising;

	public Transform ef_riseattack;

	public Transform ef_swordExtreme;

	public Transform ef_linehit;

	private float weaponweight = 2f;

	private float movespeed;

	private float movespeed_origin = 0.48f;

	private float longdash;

	private float restrictArea = 100f;

	public float limit_x = 2.65f;

	public float limit_y_b = -2.6f;

	public float limit_y_f = 2.5f;

	public int level;

	public float exp;

	public AudioClip[] yell = new AudioClip[3];

	public AudioClip[] skillyell = new AudioClip[2];

	private AudioClip[] g_yell = new AudioClip[2];

	private AudioClip[] g_skillyell = new AudioClip[2];

	public AudioClip footstep;

	public AudioClip snd_getitem;

	public AudioClip boom;

	public AudioClip snd_dodge;

	public AudioClip snd_swing;

	public AudioClip snd_skillstart;

	public AudioClip snd_riseattack;

	public Camera guicam;

	private Transform ef_swing1;

	private Transform ef_swing2;

	private Transform ef_swingex1;

	private Transform ef_swingex2;

	private AudioSource myaudio;

	private Collider mapcollider;

	private bool general;

	public Transform dummy_weapon;

	public Transform ef_blur;

	public Transform txtef;

	public Transform ef_block;

	public Transform ef_blockbreak;

	public Transform dummy_general;

	public Transform ef_punch;

	public Transform ef_firesplash;

	public Transform guide_circle;

	public Transform ef_super;

	public Transform magicspear;

	private Cha_Weapon script_weapon;

	private Cam_Move script_cam;

	private Spawn script_spawn;

	private Bullet_punch script_punch;

	private Txt_effect script_txtef;

	private Ef_swing1 script_swing;

	private Ef_swing1 script_swing_sub;

	private Ef_swing1 script_swingex1;

	private Ef_swing1 script_swingex2;

	private Ef_twirl script_getitem;

	private Cha_Skill script_skill;

	private Ef_swing1 script_thrust;

	private Ef_splash script_splash;

	private Ef_rotfog script_rotfog;

	private Pet_eagle script_eagle;

	private Pet_horse script_horse;

	private SoundEf_slash script_sound;

	private UI_Ingame script_ui;

	private UI_Ingame_story script_ui_story;

	private bool isinvincibility;

	private float delay_invincibility;

	private float target_invincibility;

	private bool grab_ing;

	private bool pet_ing;

	private bool skill_ing;

	private float commendDelay;

	private bool helper;

	private bool story_scene;

	private bool change_cha;

	private bool extremeStageHeal;

	public Transform pet_eagle;

	public Transform pet_horse;

	private int nowstep;

	private int nowthrust;

	public Transform ef_splash;

	public Transform ef_stepfog;

	public Transform ef_thrustfog;

	public Transform ef_rotfog;

	private Transform[] c_stepfog = new Transform[4];

	private Transform[] c_thrustfog = new Transform[4];

	private Transform bip01;

	private Transform targetenemy;

	private Transform mytransform;

	private Vector3 currentTouchPos;

	private Vector3 prevPoint;

	private Vector3 targetpos = Vector3.zero;

	private Vector3 collider_center;

	private Vector3 collider_dodge;

	private Vector3 collider_fly;

	private Vector3 collider_horse;

	private float collider_radius;

	private Rigidbody myrigidbody;

	private SphereCollider mycollider;

	private AnimationState clone_ride;

	private AnimationState grabfinish;

	private AnimationState impact;

	private AnimationState tempani;

	private AnimationState temp_fly;

	private AnimationState temp_attack;

	private Animation myanimation;

	private RaycastHit casthit;

	private Ray ray;

	private float attackDot;

	private float magnitude_temp;

	private float spplus_amount;

	private bool keydown;

	private bool savetouchposition;

	private bool skillicondown;

	public bool life = true;

	private bool noeagle;

	public int exattack;

	public int weapon_kind;

	private int attackkind_factor;

	public bool attack_rising;

	private bool infinitymode;

	public AnimationClip[] attack_blade = new AnimationClip[13];

	public AnimationClip[] attack_dual = new AnimationClip[13];

	public AnimationClip[] attack_spear = new AnimationClip[13];

	public AnimationClip[] attack_bow = new AnimationClip[13];

	public AnimationClip[] attack_staff = new AnimationClip[13];

	public AnimationClip[] attack_magic = new AnimationClip[13];

	private float skillboost = 1f;

	private int cur_difficulty;

	private int current_costume;

	private int[] accessory = new int[6];

	private int selequip_grade;

	private int selequip_special;

	private int selequip_hp;

	private short superMode;

	private void Awake()
	{
		myanimation = base.GetComponent<Animation>();
		mytransform = base.transform;
		myrigidbody = base.GetComponent<Rigidbody>();
		mycollider = base.GetComponent<Collider>() as SphereCollider;
		myaudio = base.GetComponent<AudioSource>();
		cur_weapon = Crypto.Load_int_key("n44");
		int num = Crypto.Load_int_key("play_kind");
		if (Application.loadedLevelName == "Story")
		{
			story_scene = true;
		}
		if (num == 7)
		{
			limit_x = 1.1f;
			limit_y_b = -0.34f;
			limit_y_f = 9.26f;
		}
		else if (num > 5)
		{
			limit_x = 1.1f;
			limit_y_b = -0.34f;
			limit_y_f = 17f;
		}
		else
		{
			limit_x = 2.65f;
			limit_y_b = -2.6f;
			limit_y_f = 2.5f;
		}
		GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().RestrictArea(limit_x, limit_y_b, limit_y_f);
		accessory = PlayerPrefsX.GetIntArray("n36");
		current_costume = Crypto.Load_int_key("n05");
		level = Crypto.Load_int_key("n47");
		exp = Crypto.Load_int_key("n11");
		maxatk = Crypto.Load_int_key("n31");
		skillboost = maxatk;
		minatk = Crypto.Load_int_key("n04");
		atkspd = (float)Crypto.Load_int_key("n35") * 0.01f;
		hpplus = Crypto.Load_int_key("n21");
		hitrate = Crypto.Load_int_key("n08");
		vitality = Crypto.Load_int_key("n26");
		evasion = Crypto.Load_int_key("n30");
		endurance = Crypto.Load_int_key("n28");
		critical = Crypto.Load_int_key("n32");
		maxsp = vitality;
		script_cam = Camera.main.GetComponent<Cam_Move>();
		script_spawn = GameObject.FindWithTag("Respawn").GetComponent<Spawn>();
		if (!story_scene)
		{
			script_ui = GameObject.FindWithTag("ui").GetComponent<UI_Ingame>();
			if (accessory[0] > 0)
			{
				maxatk = (int)((float)maxatk * 1.1f);
				minatk = (int)((float)minatk * 1.1f);
			}
			if (accessory[1] > 0)
			{
				defence = (int)((float)defence * 1.2f);
			}
			if (accessory[2] > 0)
			{
				script_ui.SoulStartplus();
			}
			if (accessory[4] > 0)
			{
				spplus_amount = 1.2f;
			}
			else
			{
				spplus_amount = 1f;
			}
			if (accessory[5] > 0)
			{
				atkspd += 0.02f;
			}
		}
		else
		{
			script_ui_story = GameObject.FindWithTag("ui").GetComponent<UI_Ingame_story>();
			script_cam.IconHideStop(_hidestop: true);
			script_cam.ThisIsStory();
		}
		origin_maxattack = maxatk;
		origin_minattack = minatk;
		origin_hitrate = hitrate;
		origin_defence = defence;
		origin_endurance = endurance;
		atkspd_origin = atkspd;
		ef_swing1 = mytransform.Find("ef_swing1");
		ef_swing2 = mytransform.Find("ef_swing2");
		ef_swingex1 = mytransform.Find("ef_swing_ex1");
		ef_swingex2 = mytransform.Find("ef_swing_ex2");
		mapcollider = GameObject.Find("ground").GetComponent<Collider>();
		script_weapon = dummy_weapon.GetComponent<Cha_Weapon>();
		script_swing = ef_swing1.GetComponent<Ef_swing1>();
		script_swing_sub = ef_swing2.GetComponent<Ef_swing1>();
		script_thrust = mytransform.Find("ef_thrust").GetComponent<Ef_swing1>();
		script_swingex1 = ef_swingex1.GetComponent<Ef_swing1>();
		script_swingex2 = ef_swingex2.GetComponent<Ef_swing1>();
		script_skill = GetComponent<Cha_Skill>();
		script_punch = ef_punch.GetComponent<Bullet_punch>();
		script_getitem = mytransform.Find("ef_getitem").GetComponent<Ef_twirl>();
		script_splash = ef_splash.GetComponent<Ef_splash>();
		script_rotfog = ef_rotfog.GetComponent<Ef_rotfog>();
		script_eagle = pet_eagle.GetComponent<Pet_eagle>();
		script_horse = pet_horse.GetComponent<Pet_horse>();
		script_sound = GameObject.FindWithTag("sound").GetComponent<SoundEf_slash>();
		cha_maxhp = 95 + level * 5;
		cha_maxhp += hpplus;
		maxhp = cha_maxhp;
		hp = maxhp;
		cha_hp = hp;
		sp = maxsp;
	}

	private void Start()
	{
		script_skill.Set_R_hand(dummy_weapon);
		exattack = Crypto.Load_int_key("exattack");
		for (int i = 0; i < 3; i++)
		{
			c_stepfog[i] = (Transform)Object.Instantiate(ef_stepfog, Vector3.one * 4f, base.transform.root.rotation);
			c_thrustfog[i] = (Transform)Object.Instantiate(ef_thrustfog, Vector3.one * 4f, ef_thrustfog.localRotation);
		}
		directionVector = Vector3.zero;
		myanimation["dead"].wrapMode = WrapMode.ClampForever;
		myanimation["cast1"].wrapMode = WrapMode.ClampForever;
		myanimation["cast2"].wrapMode = WrapMode.ClampForever;
		myanimation["cast3"].wrapMode = WrapMode.ClampForever;
		myanimation["cast4"].wrapMode = WrapMode.ClampForever;
		myanimation["change_out"].speed = 0.2f;
		myanimation["change_in"].speed = 0.3f;
		myanimation["change_in_atk"].speed = 0.3f;
		myanimation["dodge"].speed = 0.4f;
		myanimation["grabstart"].speed = 0.3f;
		myanimation["grab10"].speed = 0.25f;
		myanimation["grab10_finish"].speed = 0.5f;
		myanimation["grab10_finish2"].speed = 0.25f;
		myanimation["grab11"].speed = 0.28f;
		myanimation["grab11_finish"].speed = 0.28f;
		myanimation["grab11_finish2"].speed = 0.25f;
		myanimation["grab12"].speed = 0.24f;
		myanimation["grab12_finish"].speed = 0.28f;
		myanimation["grab12_finish2"].speed = 0.25f;
		myanimation["grab20"].speed = 0.35f;
		myanimation["grab20_finish"].speed = 0.25f;
		myanimation["grab20_finish2"].speed = 0.25f;
		myanimation["grab21"].speed = 0.4f;
		myanimation["grab21_finish"].speed = 0.25f;
		myanimation["grab21_finish2"].speed = 0.2f;
		myanimation["grab22"].speed = 0.22f;
		myanimation["grab22_finish"].speed = 0.25f;
		myanimation["grab22_finish2"].speed = 0.28f;
		myanimation["grab30"].speed = 0.35f;
		myanimation["grab30_finish"].speed = 0.35f;
		myanimation["grab30_finish2"].speed = 0.25f;
		myanimation["behitdown"].speed = 0.7f;
		myanimation["dead"].speed = 0.5f;
		myanimation["attackex1"].speed = 0.25f;
		myanimation["attackex1_impact"].speed = 0.25f;
		myanimation["attackex2"].speed = 0.24f;
		myanimation["attackex2_impact"].speed = 0.25f;
		myanimation["attackex3"].speed = 2.35f;
		myanimation["attackex3_impact"].speed = 0.25f;
		myanimation["dash_attack"].speed = 0.35f;
		myanimation["wing"].speed = 0.2f;
		myanimation["wing_i"].speed = 0.2f;
		myanimation["skill01"].speed = 0.2f;
		myanimation["skill02"].speed = 0.2f;
		myanimation["skill03"].speed = 0.25f;
		myanimation["skill04"].speed = 0.2f;
		myanimation["jumpattack_air"].speed = 0.3f;
		myanimation["evade"].speed = 0.35f;
		myanimation["wheelwind"].speed = 0.34f;
		myanimation["ride1"].speed = 0.6f;
		myanimation["ride_s"].speed = 0.2f;
		myanimation["attack_ride1"].speed = 0.3f;
		myanimation["attack_ride2"].speed = 0.3f;
		myanimation["blocked"].speed = 0.4f;
		myanimation["cast1"].speed = 2.5f;
		myanimation["cast2"].speed = 2.5f;
		myanimation["cast3"].speed = 2.5f;
		myanimation["cast4"].speed = 2.5f;
		myanimation["jump_changeweapon"].speed = 0.2f;
		myanimation["jump_changeweapon_i"].speed = 0.4f;
		myanimation["riseattack"].speed = 0.28f;
		myanimation["eagle"].speed = 0.1f;
		myanimation["eagle_start"].speed = 0.25f;
		myanimation["eagle_finish"].speed = 0.3f;
		myanimation["eagle_shoot"].speed = 0.5f;
		myanimation["horse_start"].speed = 0.3f;
		myanimation["horse_finish"].speed = 0.3f;
		myanimation["horse_chacry"].speed = 0.32f;
		myanimation["rapidstab"].speed = 0.3f;
		myanimation["skill_chargesmash"].speed = 0.16f;
		myanimation["change_out"].layer = 3;
		myanimation["change_in"].layer = 3;
		myanimation["change_in_atk"].layer = 1;
		myanimation["behitdown"].layer = 3;
		myanimation["dead"].layer = 4;
		myanimation["dodge"].layer = 2;
		myanimation["grabstart"].layer = 2;
		myanimation["grab10"].layer = 4;
		myanimation["grab10_finish"].layer = 4;
		myanimation["grab10_finish2"].layer = 4;
		myanimation["grab11"].layer = 4;
		myanimation["grab11_finish"].layer = 4;
		myanimation["grab11_finish2"].layer = 4;
		myanimation["grab12"].layer = 4;
		myanimation["grab12_finish"].layer = 4;
		myanimation["grab12_finish2"].layer = 4;
		myanimation["grab20"].layer = 4;
		myanimation["grab20_finish"].layer = 4;
		myanimation["grab20_finish2"].layer = 4;
		myanimation["grab21"].layer = 4;
		myanimation["grab21_finish"].layer = 4;
		myanimation["grab21_finish2"].layer = 4;
		myanimation["grab22"].layer = 4;
		myanimation["grab22_finish"].layer = 4;
		myanimation["grab22_finish2"].layer = 4;
		myanimation["grab30"].layer = 4;
		myanimation["grab30_finish"].layer = 4;
		myanimation["grab30_finish2"].layer = 4;
		myanimation["attackex1"].layer = 1;
		myanimation["attackex2"].layer = 1;
		myanimation["attackex3"].layer = 1;
		myanimation["attackex1_impact"].layer = 1;
		myanimation["attackex2_impact"].layer = 1;
		myanimation["attackex3_impact"].layer = 1;
		myanimation["dash_attack"].layer = 1;
		myanimation["wing"].layer = 2;
		myanimation["wing_i"].layer = 2;
		myanimation["skill01"].layer = 2;
		myanimation["skill02"].layer = 2;
		myanimation["skill03"].layer = 2;
		myanimation["skill04"].layer = 2;
		myanimation["jumpattack_air"].layer = 2;
		myanimation["jumpattack_impact"].layer = 2;
		myanimation["evade"].layer = 1;
		myanimation["wheelwind"].layer = 2;
		myanimation["ride1"].layer = 4;
		myanimation["ride_s"].layer = 4;
		myanimation["attack_ride1"].layer = 5;
		myanimation["attack_ride2"].layer = 5;
		myanimation["blocked"].layer = 3;
		myanimation["cast1"].layer = 3;
		myanimation["cast2"].layer = 3;
		myanimation["cast3"].layer = 3;
		myanimation["cast4"].layer = 3;
		myanimation["jump_changeweapon"].layer = 4;
		myanimation["jump_changeweapon_i"].layer = 4;
		myanimation["riseattack"].layer = 1;
		myanimation["eagle"].layer = 4;
		myanimation["eagle_start"].layer = 4;
		myanimation["eagle_finish"].layer = 4;
		myanimation["eagle_shoot"].layer = 5;
		myanimation["horse_start"].layer = 4;
		myanimation["horse_finish"].layer = 4;
		myanimation["horse_chacry"].layer = 4;
		myanimation["rapidstab"].layer = 3;
		myanimation["skill_chargesmash"].layer = 3;
		rotate = Quaternion.identity;
		combotime = 1f;
		script_txtef = txtef.GetComponent<Txt_effect>();
		Transform transform = (Transform)Object.Instantiate(shadow, mytransform.position, mytransform.rotation);
		bip01 = mytransform.Find("Bip01");
		transform.GetComponent<Shadow>().Pickparent(bip01, 1f);
		collider_dodge = new Vector3(0f, 0.05f, 0.05f);
		collider_fly = new Vector3(0f, 0.05f, 0.32f);
		collider_horse = new Vector3(0f, 0.05f, 0.22f);
		movespeed = movespeed_origin;
		RndAtk(_critical: false);
		if (!story_scene)
		{
			ef_swordExtreme.GetComponent<Sword_Extreme>().SetPower(origin_maxattack + origin_minattack);
			infinitymode = script_spawn.infinitymode;
			script_horse.Rideing(infinitymode);
			Costume_Special();
			InvokeRepeating("SpRecover", 3f, 3f);
			weapon_kind = Crypto.Load_int_key("n34");
			weapon_kind = (int)((float)weapon_kind * 0.01f);
			int num = Crypto.Load_int_key("n38");
			if (num >= 20)
			{
				special_kind = num % 10;
				special_kind_origin = special_kind;
				special_amount = num / 10;
				Weapon_Special();
			}
			script_skill.SetBaseDamage(skillboost);
			ChangeCharacter(weapon_kind);
		}
		else
		{
			collider_center = new Vector3(0f, 0.05f, 0.17f);
			collider_radius = 0.13f;
			atkspd = 0.02f;
			myanimation["attack1"].speed = 0.24f + atkspd;
			myanimation["attack2"].speed = 0.24f + atkspd;
			myanimation["attack3"].speed = 0.24f + atkspd;
			myanimation["attack4"].speed = 0.24f + atkspd;
			myanimation["attack5"].speed = 0.24f + atkspd;
			myanimation["idle"].speed = 0.1f;
			myanimation["walk"].speed = 0.9f;
			myanimation["run"].speed = 0.6f;
			myanimation["land_changeweapon"].speed = 0.2f;
			myanimation["idle"].layer = 0;
			myanimation["walk"].layer = 0;
			myanimation["run"].layer = 0;
			myanimation["1t"].layer = 1;
			myanimation["2t"].layer = 1;
			myanimation["3t"].layer = 1;
			myanimation["4t"].layer = 1;
			myanimation["attack1"].layer = 1;
			myanimation["attack2"].layer = 1;
			myanimation["attack3"].layer = 1;
			myanimation["attack4"].layer = 1;
			myanimation["attack5"].layer = 1;
			myanimation["land_changeweapon"].layer = 4;
			maxhp = 100;
			hp = maxhp;
			isplaycha = false;
		}
	}

	public void LoadingFinish()
	{
		int num = Crypto.Load_int_key("n06");
		if (!infinitymode)
		{
			mytransform.rotation = Quaternion.Euler(0f, 40f, 0f);
			pet_ing = true;
			PetSkillFinish(0);
			isplaycha = true;
		}
		else
		{
			Object.Destroy(pet_eagle.gameObject);
			noeagle = true;
			EnterDun();
		}
		if (num < 5)
		{
			Object.Destroy(pet_eagle.gameObject);
			noeagle = true;
		}
		cur_difficulty = script_ui.cur_difficulty;
	}

	public void EnterDun()
	{
		chapos = Vector3.zero;
		mytransform.forward = Vector3.forward;
		myanimation.Play("change_in");
		targetpos = Vector3.forward * 0.01f;
		script_ui.ResetExtreme();
	}

	public void SpRecover()
	{
		Spcharge(spplus_amount * 3f);
		if (current_costume >= 6)
		{
			if (!general)
			{
				hp = Mathf.Min(hp + 1, maxhp);
			}
			script_ui.StatUpdate_hp(hp, maxhp);
		}
	}

	public void AttackSpeedUp(float _amount)
	{
		_amount *= 0.02f;
		myanimation["attack1"].speed += _amount;
		myanimation["attack2"].speed += _amount;
		myanimation["attack3"].speed += _amount;
		myanimation["attack4"].speed += _amount;
		myanimation["attack5"].speed += _amount;
	}

	public void MoveSpeedUp(float _amount)
	{
		myanimation["run"].speed = _amount * 1.25f;
		movespeed_origin = _amount;
		movespeed = movespeed_origin;
	}

	public void SetUpArmor(int _index)
	{
		_index /= 100;
		int[] array = new int[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = _index % 10;
			_index /= 10;
		}
		selequip_grade = array[0];
		selequip_special = array[1];
		selequip_hp = _index;
	}

	public void Costume_Special()
	{
		int[] intArray = PlayerPrefsX.GetIntArray("n45");
		int upArmor = intArray[current_costume];
		SetUpArmor(upArmor);
		defence += selequip_hp;
		origin_defence = defence;
		switch (selequip_special)
		{
		case 1:
			itemboolst = (short)((float)(selequip_grade - 1) * 0.1f);
			break;
		case 2:
			expboost = 1.2f + (float)(selequip_grade - 1) * 0.1f;
			break;
		case 3:
			atkspd_origin += 0.005f + (float)selequip_grade * 0.005f;
			break;
		}
	}

	public void Weapon_Special()
	{
		switch (special_kind)
		{
		case -1:
			ef_swing1.GetComponent<Attribute_swing>().ChangeAttribute(0, 0);
			ef_swing2.GetComponent<Attribute_swing>().ChangeAttribute(0, 0);
			GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().BloodAttribute(0);
			break;
		case 0:
			ef_swing1.GetComponent<Attribute_swing>().ChangeAttribute(1, special_amount);
			ef_swing2.GetComponent<Attribute_swing>().ChangeAttribute(1, special_amount);
			GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().BloodAttribute(1);
			break;
		case 1:
			ef_swing1.GetComponent<Attribute_swing>().ChangeAttribute(2, special_amount);
			ef_swing2.GetComponent<Attribute_swing>().ChangeAttribute(2, special_amount);
			GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().BloodAttribute(2);
			break;
		case 2:
			ef_swing1.GetComponent<Attribute_swing>().ChangeAttribute(3, special_amount);
			ef_swing2.GetComponent<Attribute_swing>().ChangeAttribute(3, special_amount);
			GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().BloodAttribute(3);
			break;
		case 3:
			ef_swing1.GetComponent<Attribute_swing>().ChangeAttribute(4, special_amount);
			ef_swing2.GetComponent<Attribute_swing>().ChangeAttribute(4, special_amount);
			GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().BloodAttribute(0);
			break;
		case 4:
			guard_break = special_amount * 20;
			break;
		case 6:
			if (special_amount == 4)
			{
				skillboost *= 1.5f;
			}
			else if (special_amount == 3)
			{
				skillboost *= 1.1f;
			}
			else
			{
				skillboost *= 1.07f;
			}
			break;
		case 5:
			break;
		}
	}

	public void ItemBuff(int _index)
	{
		switch (_index)
		{
		case 1:
			origin_maxattack = (int)((float)origin_maxattack * 1.1f);
			origin_minattack = (int)((float)origin_minattack * 1.1f);
			maxatk = origin_maxattack;
			minatk = origin_minattack;
			break;
		case 2:
			origin_defence = (int)((float)origin_defence * 1.2f);
			defence = origin_defence;
			break;
		case 3:
			cha_maxhp = (int)((float)cha_maxhp * 1.15f);
			cha_hp = cha_maxhp;
			hp = cha_hp;
			break;
		case 4:
			skillboost *= 1.1f;
			script_skill.SetBaseDamage(skillboost);
			break;
		case 5:
			extremeStageHeal = true;
			break;
		case 6:
			grabheal_factor = 0.2f;
			break;
		}
	}

	public void SwingStart()
	{
		myaudio.PlayOneShot(snd_swing);
	}

	public void Set_General(short _weapon, short _kind, short _maxhp, short _maxatk, short _def, float _atkspd, short _unique, int _hp, short _voice)
	{
		general_weapon = _weapon;
		general_kind = _kind;
		g_maxhp = _maxhp;
		g_atk = _maxatk;
		g_def = _def;
		g_atkspd = _atkspd;
		g_hp = _hp;
		dummy_general.GetComponent<General_dummy>().SetCostume(general_kind, general_weapon);
		if (general_kind == 1)
		{
			dummy_general.localScale = Vector3.one * 1.4f;
		}
		else if (general_kind == 3)
		{
			dummy_general.localScale = Vector3.one * 1.1f;
		}
		if (accessory[3] > 0)
		{
			g_atk = (short)((float)g_atk * 1.1f);
		}
		g_yell[0] = Resources.Load("general_yell" + _voice + "a") as AudioClip;
		g_yell[1] = Resources.Load("general_yell" + _voice + "b") as AudioClip;
		g_skillyell[0] = Resources.Load("general_skillyell" + _voice + "a") as AudioClip;
		g_skillyell[1] = Resources.Load("general_skillyell" + _voice + "b") as AudioClip;
	}

	public void ChangeFinish(bool _generalin, bool _jumpatk)
	{
		change_cha = false;
		chamovestat = 5;
		if (_generalin)
		{
			myanimation.Stop();
			if (_jumpatk)
			{
				myanimation["1t"].speed = 0.24f;
				myanimation.Play("1t");
			}
			else
			{
				myanimation.Play("idle");
			}
			script_weapon.GeneralWeaponOn(general_kind + 1, general_weapon);
			GetComponent<Cha_Costume>().Costume(general_kind + 17);
			if (general_kind == 1)
			{
				mytransform.localScale = Vector3.one * 1.4f;
			}
			else if (general_kind == 3)
			{
				mytransform.localScale = Vector3.one * 1.1f;
			}
		}
	}

	public void MissionFail()
	{
		myanimation.Play("dead");
		Invincibility(10f);
		life = false;
	}

	public void GeneralOnOff(bool _general, short _dead)
	{
		if (_dead == 0)
		{
			Spcharge(-10f);
		}
		general = _general;
		change_cha = true;
		myanimation.Stop();
		script_cam.ZoomIn(3, 16, 0.5f);
		mytransform.localScale = Vector3.one;
		if (!general)
		{
			myaudio.clip = yell[0];
			base.GetComponent<AudioSource>().Play();
			special_kind = special_kind_origin;
			script_cam.IconHideStop(_hidestop: false);
			script_weapon.GeneralWeaponOff();
			maxhp = cha_maxhp;
			maxatk = origin_maxattack;
			minatk = origin_minattack;
			hitrate = origin_hitrate;
			defence = origin_defence;
			atkspd = atkspd_origin;
			hp = cha_hp;
			if (chamovestat < 10 && chamovestat >= 0)
			{
				myanimation.Play("change_in");
			}
			else
			{
				myanimation.Play("change_in_atk");
				temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
				temp_attack.speed = 0.24f;
				temp_attack.layer = 1;
				ef_swingex1.localScale = Vector3.one * 2f;
				ef_swingex1.localRotation = Quaternion.Euler(0f, 0f, 180f);
				ef_swingex1.position = mytransform.position + Vector3.up * 0.055f;
				script_swingex1.SwingOn(0.6f, 2, 2, 20, 1, 120);
				curruntattack = attackkind_factor;
			}
			dummy_general.position = mytransform.position;
			dummy_general.rotation = mytransform.rotation;
			ChangeCharacter(weapon_kind);
			if (_dead == 0)
			{
				dummy_general.GetComponent<General_dummy>().ShowOut();
			}
			else
			{
				dummy_general.GetComponent<General_dummy>().Dead();
				script_ui.GeneralDead();
				change_cha = false;
				chamovestat = 0;
				Invincibility(1f);
			}
			GetComponent<Cha_Costume>().ResetCostume();
		}
		else
		{
			myaudio.clip = g_yell[0];
			base.GetComponent<AudioSource>().Play();
			special_kind = -1;
			script_cam.IconHideStop(_hidestop: true);
			general = true;
			maxhp = g_maxhp;
			maxatk = g_atk;
			minatk = (int)((float)g_atk * 0.8f);
			hitrate = 100;
			defence = g_def;
			atkspd = g_atkspd;
			hp = g_hp;
			if (_dead == 0)
			{
				myanimation.Play("change_out");
			}
			else
			{
				myanimation.Play("dead");
			}
			if (chamovestat > 10 || chamovestat < 0)
			{
				ef_swingex1.localScale = Vector3.one * 2f;
				ef_swingex1.localRotation = Quaternion.Euler(0f, 0f, 180f);
				ef_swingex1.position = mytransform.position + Vector3.up * 0.055f;
				script_swingex1.SwingOn(0.6f, 2, 2, 20, 1, 120);
				curruntattack = attackkind_factor;
				dummy_general.GetComponent<General_dummy>().ShowIn_atk();
			}
			else
			{
				dummy_general.GetComponent<General_dummy>().ShowIn();
			}
			dummy_general.position = mytransform.position;
			dummy_general.rotation = mytransform.rotation;
			ChangeCharacter(general_kind + 1);
		}
		Weapon_Special();
		script_ui.StatUpdate_hp(hp, maxhp);
		chamovestat = 100;
	}

	public void ChangeCharacter(int _weapon_kind)
	{
		switch (_weapon_kind)
		{
		case 0:
		{
			collider_center = new Vector3(0f, 0.05f, 0.17f);
			collider_radius = 0.13f;
			for (int m = 0; m < 5; m++)
			{
				myanimation.AddClip(attack_blade[m], "attack" + (m + 1));
			}
			myanimation.AddClip(attack_blade[5], "1t");
			myanimation.AddClip(attack_blade[6], "2t");
			myanimation.AddClip(attack_blade[7], "3t");
			myanimation.AddClip(attack_blade[8], "4t");
			myanimation.AddClip(attack_blade[9], "run");
			myanimation.AddClip(attack_blade[10], "walk");
			myanimation.AddClip(attack_blade[11], "idle");
			myanimation.AddClip(attack_blade[12], "land_changeweapon");
			ef_swing1.localScale = new Vector3(1.6f, 2f, 2f);
			attackkind_factor = 0;
			myanimation["run"].speed = 0.6f;
			break;
		}
		case 1:
		{
			collider_center = new Vector3(0f, 0.05f, 0.16f);
			collider_radius = 0.13f;
			for (int k = 0; k < 5; k++)
			{
				myanimation.AddClip(attack_dual[k], "attack" + (k + 1));
			}
			myanimation.AddClip(attack_dual[5], "1t");
			myanimation.AddClip(attack_dual[6], "2t");
			myanimation.AddClip(attack_dual[7], "3t");
			myanimation.AddClip(attack_dual[8], "4t");
			myanimation.AddClip(attack_dual[9], "run");
			myanimation.AddClip(attack_dual[10], "walk");
			myanimation.AddClip(attack_dual[11], "idle");
			myanimation.AddClip(attack_dual[12], "land_changeweapon");
			attackkind_factor = 10;
			ef_swing1.localScale = new Vector3(1.3f, 1.7f, 1.7f);
			atkspd -= 0.02f;
			myanimation["run"].speed = 0.6f;
			break;
		}
		case 2:
		{
			collider_center = new Vector3(0f, 0.05f, 0.2f);
			collider_radius = 0.15f;
			for (int n = 0; n < 5; n++)
			{
				myanimation.AddClip(attack_spear[n], "attack" + (n + 1));
			}
			myanimation.AddClip(attack_spear[5], "1t");
			myanimation.AddClip(attack_spear[6], "2t");
			myanimation.AddClip(attack_spear[7], "3t");
			myanimation.AddClip(attack_spear[8], "4t");
			myanimation.AddClip(attack_spear[9], "run");
			myanimation.AddClip(attack_spear[10], "walk");
			myanimation.AddClip(attack_spear[11], "idle");
			myanimation.AddClip(attack_spear[12], "land_changeweapon");
			attackkind_factor = 20;
			if (!general)
			{
				ef_swing1.localScale = new Vector3(2f, 2.6f, 2.6f);
				myanimation["run"].speed = 0.6f;
			}
			else
			{
				ef_swing1.localScale = new Vector3(1.6f, 2f, 2f);
				myanimation["run"].speed = 0.45f;
			}
			break;
		}
		case 3:
		{
			collider_center = new Vector3(0f, 0.05f, 0.25f);
			collider_radius = 0.15f;
			for (int j = 0; j < 5; j++)
			{
				myanimation.AddClip(attack_bow[j], "attack" + (j + 1));
			}
			myanimation.AddClip(attack_bow[5], "1t");
			myanimation.AddClip(attack_bow[6], "2t");
			myanimation.AddClip(attack_bow[7], "3t");
			myanimation.AddClip(attack_bow[8], "4t");
			myanimation.AddClip(attack_bow[9], "run");
			myanimation.AddClip(attack_bow[10], "walk");
			myanimation.AddClip(attack_bow[11], "idle");
			myanimation.AddClip(attack_bow[12], "land_changeweapon");
			attackkind_factor = 30;
			atkspd += 0.02f;
			myanimation["run"].speed = 0.6f;
			break;
		}
		case 4:
		{
			collider_center = new Vector3(0f, 0.05f, 0.2f);
			collider_radius = 0.15f;
			for (int l = 0; l < 5; l++)
			{
				myanimation.AddClip(attack_staff[l], "attack" + (l + 1));
			}
			myanimation.AddClip(attack_staff[5], "1t");
			myanimation.AddClip(attack_staff[6], "2t");
			myanimation.AddClip(attack_staff[7], "3t");
			myanimation.AddClip(attack_staff[8], "4t");
			myanimation.AddClip(attack_staff[9], "run");
			myanimation.AddClip(attack_staff[10], "walk");
			myanimation.AddClip(attack_staff[11], "idle");
			myanimation.AddClip(attack_staff[12], "land_changeweapon");
			attackkind_factor = 40;
			atkspd -= 0.02f;
			myanimation["run"].speed = 0.6f;
			break;
		}
		case 5:
		{
			collider_center = new Vector3(0f, 0.05f, 0.25f);
			collider_radius = 0.15f;
			for (int i = 0; i < 5; i++)
			{
				myanimation.AddClip(attack_magic[i], "attack" + (i + 1));
			}
			myanimation.AddClip(attack_magic[5], "1t");
			myanimation.AddClip(attack_magic[6], "2t");
			myanimation.AddClip(attack_magic[7], "3t");
			myanimation.AddClip(attack_magic[8], "4t");
			myanimation.AddClip(attack_magic[9], "run");
			myanimation.AddClip(attack_magic[10], "walk");
			myanimation.AddClip(attack_magic[11], "idle");
			myanimation.AddClip(attack_magic[12], "land_changeweapon");
			ef_swing1.localScale = new Vector3(1.6f, 2f, 2f);
			attackkind_factor = 50;
			atkspd += 0.04f;
			myanimation["run"].speed = 0.6f;
			break;
		}
		}
		myanimation["attack1"].speed = 0.24f + atkspd;
		myanimation["attack2"].speed = 0.24f + atkspd;
		myanimation["attack3"].speed = 0.24f + atkspd;
		myanimation["attack4"].speed = 0.24f + atkspd;
		myanimation["attack5"].speed = 0.24f + atkspd;
		myanimation["idle"].speed = 0.1f;
		myanimation["walk"].speed = 0.9f;
		myanimation["land_changeweapon"].speed = 0.2f;
		myanimation["idle"].layer = 0;
		myanimation["walk"].layer = 0;
		myanimation["run"].layer = 0;
		myanimation["1t"].layer = 1;
		myanimation["2t"].layer = 1;
		myanimation["3t"].layer = 1;
		myanimation["4t"].layer = 1;
		myanimation["attack1"].layer = 1;
		myanimation["attack2"].layer = 1;
		myanimation["attack3"].layer = 1;
		myanimation["attack4"].layer = 1;
		myanimation["attack5"].layer = 1;
		curruntattack = attackkind_factor;
		mycollider.radius = collider_radius;
		mycollider.center = collider_center;
		battlestyle = _weapon_kind;
	}

	public void StopControl()
	{
		isplaycha = false;
		directionVector = Vector3.zero;
		if (rideon)
		{
			script_horse.RunStart(a: false);
			clone_ride.speed = 0.1f;
		}
		chamovestat = 300;
		script_weapon.HideBlade();
		isinvincibility = true;
		target_invincibility = 10f;
	}

	public void StartControl()
	{
		isplaycha = true;
		script_weapon.ShowBlade();
		if (!noeagle)
		{
			pet_eagle.gameObject.active = true;
		}
		isinvincibility = false;
		target_invincibility = 0f;
	}

	public void Resurection()
	{
		life = true;
		Time.timeScale = 1f;
		isplaycha = true;
		maxhp = cha_maxhp;
		cha_hp = maxhp;
		hp = maxhp;
		sp = maxsp;
		script_ui.StatUpdate_hp(hp, maxhp);
		script_ui.StatUpdate_sp(sp);
		if (general)
		{
			Spcharge(100f);
			script_ui.CallGeneral(1);
		}
		else
		{
			base.GetComponent<Animation>().Stop();
			base.GetComponent<Animation>().CrossFade("idle");
		}
		target_invincibility = 0f;
		isinvincibility = false;
		Invincibility(3f);
		script_skill.BoomOn(0, _collider: true);
		int num = Crypto.Load_int_key("resurrection");
		num++;
		Crypto.Save_int_key("resurrection", num);
	}

	public void Fly()
	{
		myaudio.PlayOneShot(snd_skillstart);
		pet_ing = true;
		currentPet = 1;
		myanimation.Stop();
		myanimation.Play("jump_changeweapon");
		AnimationState animationState = myanimation.PlayQueued("jump_changeweapon_i");
		animationState.speed = myanimation["jump_changeweapon_i"].speed;
		animationState = myanimation.PlayQueued("eagle_start");
		animationState.speed = myanimation["eagle_start"].speed;
		animationState = myanimation.PlayQueued("eagle");
		animationState.speed = myanimation["eagle"].speed;
		animationState.wrapMode = WrapMode.Loop;
		movespeed = 0.3f;
		if (!noeagle)
		{
			pet_eagle.gameObject.active = true;
			script_eagle.SkillOn();
		}
		AttakUp((float)attack_eagle * 0.01f);
		mycollider.radius = collider_radius;
		mycollider.center = collider_fly;
	}

	public void RndAtk(bool _critical)
	{
		atk = Random.Range(minatk, maxatk + 1);
		if (_critical && critical >= Random.Range(0, 100))
		{
			script_txtef.TxtEfOn(2);
			atk = maxatk;
		}
	}

	public void SetPetSkillLV(int _autorate, int _attack_horse, int _attack_eagle)
	{
		autorate = _autorate;
		attack_horse = _attack_horse;
		attack_eagle = _attack_eagle;
	}

	public void FindItem(Transform _finditem)
	{
		int num = Random.Range(0, 100);
		if (num < autorate && !noeagle)
		{
			pet_eagle.gameObject.active = true;
			script_eagle.GetItem(_finditem);
		}
	}

	public void FogOn(int _kind)
	{
		switch (_kind)
		{
		case 0:
			c_stepfog[nowstep].gameObject.active = true;
			c_stepfog[nowstep].position = mytransform.position;
			c_stepfog[nowstep].rotation = mytransform.rotation;
			nowstep = (nowstep + 1) % 3;
			break;
		case 1:
			c_thrustfog[nowthrust].gameObject.active = true;
			c_thrustfog[nowthrust].position = mytransform.position + mytransform.forward * 0.2f + Vector3.up * 0.15f;
			c_thrustfog[nowthrust].up = mytransform.forward;
			c_thrustfog[nowthrust].RotateAround(mytransform.forward, Random.Range(0, 360));
			nowthrust = (nowthrust + 1) % 3;
			break;
		case 2:
			c_thrustfog[nowthrust].gameObject.active = true;
			c_thrustfog[nowthrust].position = bip01.position + bip01.forward * 0.2f + Vector3.up * 0.15f;
			c_thrustfog[nowthrust].up = bip01.forward;
			c_thrustfog[nowthrust].RotateAround(bip01.forward, Random.Range(0, 360));
			nowthrust = (nowthrust + 1) % 3;
			break;
		}
	}

	public void CallHorse()
	{
		myaudio.PlayOneShot(snd_skillstart);
		pet_ing = true;
		currentPet = 0;
		myanimation.Stop();
		myanimation.Play("jump_changeweapon");
		clone_ride = myanimation.PlayQueued("jump_changeweapon_i");
		clone_ride.speed = myanimation["jump_changeweapon_i"].speed;
		clone_ride = myanimation.PlayQueued("horse_start");
		clone_ride.speed = myanimation["horse_start"].speed;
		clone_ride = myanimation.PlayQueued("horse_chacry");
		clone_ride.speed = myanimation["horse_chacry"].speed;
		clone_ride = myanimation.PlayQueued("ride1");
		clone_ride.wrapMode = WrapMode.Loop;
		clone_ride.speed = 0.05f;
		script_horse.SkillOn();
		movespeed = 0.8f;
		AttakUp((float)attack_horse * 0.01f);
		mycollider.radius = collider_radius;
		mycollider.center = collider_horse;
	}

	public void PetSkillFinish(int i)
	{
		curruntattack = attackkind_factor;
		myanimation.Stop();
		chapos[1] = 0f;
		chamovestat = 180;
		switch (i)
		{
		case 1:
			myanimation.Play("eagle_finish");
			tempani = myanimation.PlayQueued("land_changeweapon");
			tempani.layer = 4;
			tempani.speed = myanimation["land_changeweapon"].speed;
			script_eagle.SkillOff();
			break;
		case 0:
			myrigidbody.AddForce(-mytransform.forward * 120f);
			myanimation.Play("horse_finish");
			script_horse.GetOffHorse();
			tempani = myanimation.PlayQueued("land_changeweapon");
			tempani.layer = 4;
			tempani.speed = myanimation["land_changeweapon"].speed;
			rideon = false;
			break;
		}
		currentPet = -1;
		ResetAtk();
	}

	public void WakToward(Vector3 _pos, bool _run, bool _ride)
	{
		myanimation.Stop();
		isplaycha = false;
		if (!_run)
		{
			script_weapon.SwitchWepon(3, 3, 0f, 1f);
			myanimation["walk_free"].speed = 0.45f;
			myanimation.Play("walk_free");
		}
		else
		{
			script_weapon.SetStoryBlade();
			script_weapon.ReturnBlade();
			chamovestat = 2;
			myanimation.Play("run");
		}
		targetpos = _pos;
		storyrun = _run;
		directionVector = targetpos - mytransform.position;
	}

	public void GateEnter(Vector3 _pos)
	{
		mytransform.forward = targetpos - mytransform.position;
		isplaycha = false;
		dunenter = true;
		targetpos = _pos;
	}

	public void RideHorse()
	{
		pet_ing = true;
		myanimation.Stop();
		myanimation.Play("ride_s");
		AnimationState animationState = myanimation.PlayQueued("ride1");
		animationState.speed = 0f;
		movespeed = 0.8f;
	}

	public void MaxCombo()
	{
		superMode = 1;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 8)
		{
			if (chamovestat <= 10 && chamovestat >= 0)
			{
				targetenemy = other.transform;
				AttackOn(targetenemy.position);
				mycollider.enabled = false;
			}
			else if (chamovestat == 212)
			{
				Attack_spear(other.transform.position);
			}
			else if (chamovestat == 203)
			{
				Attack_arrow(other.transform.position);
			}
			else if (grabstart && !general)
			{
				other.gameObject.SendMessage("Grabed");
				grabstart = false;
			}
		}
	}

	public void AttackOn(Vector3 monpos)
	{
		attack_rising = false;
		attackVector = monpos - mytransform.position;
		attackVector[1] = 0f;
		magnitude_attackdir = attackVector.magnitude;
		attackVector /= magnitude_attackdir;
		attackDot = Vector3.Dot(attackVector, directionVector);
		if (magnitude_attackdir > 0.2f)
		{
			if (attackDot < 0.5f)
			{
				chamovestat = 17;
				mycollider.enabled = true;
				return;
			}
		}
		else if (attackDot < -0.2f)
		{
			chamovestat = 17;
			mycollider.enabled = true;
			return;
		}
		RndAtk(_critical: true);
		speedfactor = 5.2f;
		if (chamovestat == 3)
		{
			curruntattack = -1;
			Invincibility(1f);
		}
		else if (combotime > 0f)
		{
			curruntattack = (curruntattack + 1 - attackkind_factor) % 5 + attackkind_factor;
		}
		else if (combotime <= 0f)
		{
			curruntattack = attackkind_factor;
		}
		int num = Random.Range(0, 5);
		if (num < 2)
		{
			if (!general)
			{
				myaudio.clip = yell[num];
			}
			else
			{
				myaudio.clip = g_yell[num];
			}
			myaudio.Play();
		}
		mytransform.rotation = Quaternion.LookRotation(attackVector);
		FogOn(0);
		switch (curruntattack)
		{
		case -1:
			chamovestat = 19;
			pressdelay = 0f;
			myanimation.Play("dash_attack");
			temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swingex1.localScale = Vector3.one * 2.4f;
			ef_swingex1.localRotation = Quaternion.Euler(0f, 0f, 180f);
			ef_swingex1.position = mytransform.position + Vector3.up * 0.055f;
			script_swingex1.SwingOn(0.48f, 2, 2, 20, 1, 120);
			myrigidbody.AddForce(attackVector * 50f);
			curruntattack = attackkind_factor;
			script_cam.ZoomIn(3, 22, 0.2f);
			break;
		case 0:
			chamovestat = 11;
			myanimation.Play("attack1");
			temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 180f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.055f;
			script_swing.SwingOn(0.15f - atkspd, 2, 2, 20, 1, 0);
			myrigidbody.AddForce(attackVector * 110f);
			break;
		case 1:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack2");
			temp_attack = myanimation.PlayQueued("2t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 40f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.12f;
			script_swing.SwingOn(0.15f - atkspd, 2, 2, 20, 1, 0);
			myrigidbody.AddForce(attackVector * 110f);
			break;
		case 2:
			chamovestat = 11;
			myanimation.Play("attack3");
			temp_attack = myanimation.PlayQueued("3t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 330f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.1f;
			script_swing.SwingOn(0.2f - atkspd, 2, 2, 20, 1, 0);
			myrigidbody.AddForce(attackVector * 110f);
			break;
		case 3:
			chamovestat = 12;
			myanimation.Play("attack4");
			temp_attack = myanimation.PlayQueued("4t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			script_thrust.SwingOn(0.28f - atkspd, 4, 1, 20, 1, 100);
			myrigidbody.AddForce(attackVector * 30f);
			break;
		case 4:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack5");
			ef_swing1.localRotation = Quaternion.identity;
			ef_swing1.position = mytransform.position + Vector3.up * 0.055f;
			script_swing.SwingOn(0.21f - atkspd, 2, 2, 20, 1, 150);
			break;
		case 10:
			chamovestat = 11;
			myanimation.Play("attack1");
			temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 180f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.06f;
			script_swing.SwingOn(0.15f - atkspd, 2, 2, 20, 1, 0);
			ef_swing2.localRotation = Quaternion.Euler(0f, 0f, -40f);
			ef_swing2.position = mytransform.position + Vector3.up * 0.08f;
			script_swing_sub.SwingOn(0.4f - atkspd, 2, 2, 20, 1, 100);
			myrigidbody.AddForce(attackVector * 120f);
			break;
		case 11:
			chamovestat = 11;
			myanimation.Play("attack2");
			temp_attack = myanimation.PlayQueued("2t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 340f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.06f;
			script_swing.SwingOn(0.2f - atkspd, 2, 2, 20, 1, 0);
			ef_swing2.localRotation = Quaternion.Euler(0f, 0f, 10f);
			ef_swing2.position = mytransform.position + Vector3.up * 0.06f;
			script_swing_sub.SwingOn(0.4f - atkspd, 2, 2, 20, 1, 100);
			myrigidbody.AddForce(attackVector * 120f);
			break;
		case 12:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack3");
			temp_attack = myanimation.PlayQueued("3t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 90f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.14f;
			script_swing.SwingOn(0.15f - atkspd, 2, 2, 20, 1, 140);
			ef_swing2.localRotation = Quaternion.Euler(0f, 0f, 90f);
			ef_swing2.position = mytransform.position + Vector3.up * 0.16f;
			script_swing_sub.SwingOn(0.31f - atkspd, 2, 2, 20, 1, 80);
			break;
		case 13:
			chamovestat = 11;
			myanimation.Play("attack4");
			temp_attack = myanimation.PlayQueued("4t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(-20f, 0f, -90f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.06f;
			script_swing.SwingOn(0.11f - atkspd, 2, 2, 20, 1, 0);
			myrigidbody.AddForce(attackVector * 150f);
			break;
		case 14:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack5");
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 170f);
			ef_swing1.position = mytransform.position - mytransform.right * 0.07f + Vector3.up * 0.06f;
			script_swing.SwingOn(0.4f - atkspd, 2, 2, 20, 1, 180);
			ef_swing2.localRotation = Quaternion.Euler(0f, 0f, 10f);
			ef_swing2.position = mytransform.position + mytransform.right * 0.07f + Vector3.up * 0.055f;
			script_swing_sub.SwingOn(0.4f - atkspd, 2, 2, 20, 1, 0);
			break;
		case 20:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack1");
			temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.identity;
			ef_swing1.position = mytransform.position + Vector3.up * 0.06f;
			script_swing.SwingOn(0.15f - atkspd, 2, 2, 20, 1, 0);
			myrigidbody.AddForce(attackVector * 160f);
			break;
		case 21:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack2");
			temp_attack = myanimation.PlayQueued("2t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, -90f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.1f;
			script_swing.SwingOn(0.42f - atkspd, 2, 2, 20, 1, 0);
			script_splash.SplashOn(col: true, 0.1f, 0.5f - atkspd);
			myrigidbody.AddForce(attackVector * 130f);
			break;
		case 22:
			chamovestat = 12;
			myanimation.Play("attack3");
			temp_attack = myanimation.PlayQueued("3t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, 180f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.1f;
			script_swing.SwingOn(0.5f - atkspd, 2, 2, 20, 1, 90);
			myrigidbody.AddForce(attackVector * 90f);
			break;
		case 23:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack4");
			temp_attack = myanimation.PlayQueued("4t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			ef_swingex1.localRotation = Quaternion.Euler(0f, 0f, 160f);
			ef_swingex1.localScale = Vector3.one * 2.4f;
			ef_swingex1.position = mytransform.position + Vector3.up * 0.1f;
			script_swingex1.SwingOn(0.36f - atkspd, 2, 2, 20, 1, 120);
			break;
		case 24:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack5");
			ef_swing1.localRotation = Quaternion.Euler(0f, 0f, -20f);
			ef_swing1.position = mytransform.position + Vector3.up * 0.1f;
			script_swing.SwingOn(0.4f - atkspd, 2, 2, 20, 1, 200);
			break;
		case 30:
			chamovestat = 11;
			myanimation.Play("attack1");
			temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			Invoke("Arrow_shoot", 0.3f - atkspd);
			myrigidbody.AddForce(attackVector * -30f);
			break;
		case 31:
			chamovestat = 11;
			myanimation.Play("attack2");
			temp_attack = myanimation.PlayQueued("2t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			Invoke("Arrow_shoot", 0.7f - atkspd);
			myrigidbody.AddForce(attackVector * -30f);
			break;
		case 32:
			attack_rising = true;
			chamovestat = 12;
			myanimation.Play("attack3");
			temp_attack = myanimation.PlayQueued("3t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			Invoke("Arrow_shoot", 0.8f - atkspd);
			myrigidbody.AddForce(attackVector * -30f);
			break;
		case 33:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack4");
			temp_attack = myanimation.PlayQueued("4t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			Invoke("Arrow_shoot", 0.8f - atkspd);
			myrigidbody.AddForce(attackVector * -30f);
			break;
		case 34:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack5");
			Invoke("Arrow_shoot", 0.8f - atkspd);
			myrigidbody.AddForce(attackVector * -30f);
			break;
		case 40:
			chamovestat = 11;
			myanimation.Play("attack1");
			temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			script_thrust.SwingOn(0.1f - atkspd, 4, 1, 20, 1, 30);
			script_punch.PunchShoot(0.3f - atkspd, 0, targetenemy, 0f, 0f, 0.06f, _boom: false);
			break;
		case 41:
			attack_rising = true;
			chamovestat = 11;
			myanimation.Play("attack2");
			temp_attack = myanimation.PlayQueued("2t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			script_punch.PunchShoot(0.3f - atkspd, 40, targetenemy, -3f * atkspd * 15f, 0.8f, 0.1f, _boom: false);
			break;
		case 42:
			chamovestat = 11;
			myanimation.Play("attack3");
			temp_attack = myanimation.PlayQueued("3t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			script_thrust.SwingOn(0.24f - atkspd, 4, 1, 20, 1, 30);
			script_punch.PunchShoot(0.1f - atkspd, 0, targetenemy, 0f, 0f, 0.06f, _boom: false);
			break;
		case 43:
			attack_rising = true;
			chamovestat = 12;
			myanimation.Play("attack4");
			temp_attack = myanimation.PlayQueued("4t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			script_punch.PunchShoot(0.4f - atkspd, 0, targetenemy, 2f * atkspd * 15f, 0f, 0.15f, _boom: true);
			Invincibility(1.5f - atkspd);
			break;
		case 44:
			chamovestat = 11;
			myanimation.Play("attack5");
			script_thrust.SwingOn(0.2f - atkspd, 4, 1, 20, 1, 70);
			script_splash.SplashOn(col: true, 0.05f, 0.6f - atkspd);
			myrigidbody.AddForce(attackVector * 120f);
			break;
		case 50:
			chamovestat = 11;
			myanimation.Play("attack1");
			temp_attack = myanimation.PlayQueued("1t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			Invoke("Magic_shoot", 0.2f - atkspd);
			myrigidbody.AddForce(attackVector * 10f);
			break;
		case 51:
			chamovestat = 11;
			myanimation.Play("attack2");
			temp_attack = myanimation.PlayQueued("2t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.24f;
			temp_attack.layer = 1;
			Invoke("Magic_shoot", 0.4f - atkspd);
			myrigidbody.AddForce(attackVector * 10f);
			break;
		case 52:
			chamovestat = 11;
			myanimation.Play("attack3");
			temp_attack = myanimation.PlayQueued("3t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.4f;
			temp_attack.layer = 1;
			Invoke("Magic_shoot", 0.3f - atkspd);
			myrigidbody.AddForce(attackVector * 10f);
			break;
		case 53:
			chamovestat = 12;
			myanimation.Play("attack4");
			temp_attack = myanimation.PlayQueued("4t", QueueMode.CompleteOthers);
			temp_attack.speed = 0.4f;
			temp_attack.layer = 1;
			Invoke("FireSplash", 0.5f - atkspd);
			myrigidbody.AddForce(attackVector * -60f);
			break;
		case 54:
			chamovestat = 11;
			myanimation.Play("attack5");
			Invoke("Magic_shoot", 0.3f - atkspd);
			myrigidbody.AddForce(attackVector * 80f);
			break;
		}
		directionVector = attackVector;
	}

	public void FireSplash()
	{
		ef_firesplash.gameObject.active = true;
		ef_firesplash.position = mytransform.position + mytransform.forward * 0.4f;
	}

	public void Arrow_shoot()
	{
		ef_arrow_multy.position = mytransform.position + Vector3.up * 0.05f;
		ef_arrow_multy.rotation = mytransform.rotation;
		ef_arrow_multy.gameObject.active = true;
	}

	public void Magic_shoot()
	{
		magicspear.position = mytransform.position + Vector3.up * 0.05f;
		magicspear.rotation = mytransform.rotation;
		magicspear.gameObject.active = true;
	}

	public void Attack_spear(Vector3 monpos)
	{
		if (chamovestat == 212)
		{
			mycollider.enabled = false;
			RndAtk(_critical: true);
			attackVector = monpos - mytransform.position;
			attackVector[1] = 0f;
			attackVector = Vector3.Normalize(attackVector);
			myrigidbody.AddForce(attackVector * 40f);
			if (combotime > 0f)
			{
				curruntattack = (curruntattack + 1) % 2;
			}
			else if (combotime <= 0f)
			{
				curruntattack = 0;
			}
			int num = Random.Range(0, 5);
			if (num < 2)
			{
				myaudio.clip = yell[num];
				myaudio.Play();
			}
			switch (curruntattack)
			{
			case 0:
				chamovestat = 214;
				myanimation.Play("attack_ride1");
				ef_swingex1.localRotation = Quaternion.Euler(0f, 0f, 160f);
				ef_swingex1.position = mytransform.position + Vector3.up * 0.17f;
				ef_swingex1.localScale = Vector3.one * 2.8f;
				script_swingex1.SwingOn(0.18f, 2, 2, 20, 1, 0);
				break;
			case 1:
				chamovestat = 214;
				myanimation.Play("attack_ride2");
				ef_swingex1.localRotation = Quaternion.Euler(0f, 0f, 20f);
				ef_swingex1.position = mytransform.position + Vector3.up * 0.17f;
				ef_swingex1.localScale = Vector3.one * 3f;
				script_swingex1.SwingOn(0.18f, 2, 2, 20, 1, 0);
				break;
			}
		}
	}

	public void Attack_arrow(Vector3 monpos)
	{
		if (chamovestat == 203)
		{
			mycollider.enabled = false;
			RndAtk(_critical: false);
			chamovestat = 192;
			directionVector = monpos - mytransform.position;
			directionVector = Vector3.right * directionVector.x + Vector3.forward * directionVector.z;
			myanimation.Play("eagle_shoot");
			mytransform.rotation = Quaternion.LookRotation(directionVector);
			arrowpos = mytransform.position + Vector3.up * 0.24f + mytransform.forward * 0.1f;
			ef_arrow_single.position = arrowpos;
			ef_arrow_single.rotation = Quaternion.LookRotation(monpos - arrowpos + Vector3.up * 0.08f);
			ef_arrow_single.gameObject.active = true;
		}
	}

	public void Grab(int kind, Vector3 monpos)
	{
		float delay = 0f;
		grab_ing = true;
		script_cam.LookTarget(bip01, 0, 0f);
		myanimation.Stop();
		mycollider.enabled = false;
		Spcharge(-50f);
		if (!story_scene)
		{
			script_ui.ComboPlus(0.1f);
		}
		chamovestat = 112;
		Vector3 vector = monpos - mytransform.position;
		if (vector != Vector3.zero)
		{
			mytransform.forward = new Vector3(vector.x, 0f, vector.z);
		}
		SwingStart();
		switch (kind)
		{
		case 0:
			delay = 1.6f;
			myanimation.Play("grab10");
			grabfinish = myanimation.PlayQueued("grab10_finish", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab10_finish"].speed;
			grabfinish = myanimation.PlayQueued("grab10_finish2", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab10_finish2"].speed;
			break;
		case 1:
			delay = 1.6f;
			myanimation.Play("grab11");
			grabfinish = myanimation.PlayQueued("grab11_finish", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab11_finish"].speed;
			grabfinish = myanimation.PlayQueued("grab11_finish2", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab11_finish2"].speed;
			break;
		case 2:
			delay = 2f;
			myanimation.Play("grab12");
			grabfinish = myanimation.PlayQueued("grab12_finish", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab12_finish"].speed;
			grabfinish = myanimation.PlayQueued("grab12_finish2", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab12_finish2"].speed;
			break;
		case 3:
			delay = 1.6f;
			myanimation.Play("grab20");
			grabfinish = myanimation.PlayQueued("grab20_finish", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab20_finish"].speed;
			grabfinish = myanimation.PlayQueued("grab20_finish2", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab20_finish2"].speed;
			break;
		case 4:
			delay = 1.6f;
			myanimation.Play("grab21");
			grabfinish = myanimation.PlayQueued("grab21_finish", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab21_finish"].speed;
			grabfinish = myanimation.PlayQueued("grab21_finish2", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab21_finish2"].speed;
			break;
		case 5:
			delay = 3f;
			myanimation.Play("grab22");
			grabfinish = myanimation.PlayQueued("grab22_finish", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab22_finish"].speed;
			grabfinish = myanimation.PlayQueued("grab22_finish2", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab22_finish2"].speed;
			break;
		case 6:
			delay = 1.2f;
			myanimation.Play("grab30");
			grabfinish = myanimation.PlayQueued("grab30_finish", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab30_finish"].speed;
			grabfinish = myanimation.PlayQueued("grab30_finish2", QueueMode.CompleteOthers);
			grabfinish.speed = myanimation["grab30_finish2"].speed;
			break;
		}
		script_cam.ZoomIn(5, 13, delay);
	}

	public void Grabfinish(Vector3 monpos, Vector3 lookat)
	{
		monpos.y = 0f;
		mytransform.position = Vector3.Lerp(base.transform.position, monpos, Time.deltaTime * 15f);
		mytransform.forward = -new Vector3(lookat.x, 0f, lookat.z);
	}

	private void StepOn()
	{
		if (chamovestat <= 10)
		{
			myaudio.clip = footstep;
			myaudio.Play();
		}
	}

	public void SkillStart()
	{
		skill_ing = true;
		myaudio.PlayOneShot(snd_skillstart);
		if (!general)
		{
			myaudio.PlayOneShot(skillyell[1]);
		}
		else
		{
			myaudio.PlayOneShot(g_skillyell[0]);
		}
	}

	public void Damaged(Vector3 _damageVector)
	{
		if (isinvincibility || chamovestat > 50)
		{
			return;
		}
		int num = Random.Range(0, 100);
		damage = (int)_damageVector.y;
		_damageVector[1] = 0f;
		if (evasion >= num && chamovestat >= 0)
		{
			chamovestat = -1;
			script_txtef.TxtEfOn(1);
			Invincibility(1f);
			if (_damageVector != Vector3.zero)
			{
				mytransform.rotation = Quaternion.LookRotation(_damageVector);
			}
			myrigidbody.AddForce(_damageVector * 140f);
			script_cam.ZoomIn(5, 22, 0.2f);
			myanimation.Play("evade");
			myanimation.PlayQueued("idle");
		}
		else
		{
			chamovestat = -1;
			screen_effect.gameObject.active = true;
			if (cur_difficulty == 2)
			{
				script_ui.Damaged();
			}
			else if (general)
			{
				g_hp -= Mathf.Max(1, damage - defence);
				hp = g_hp;
			}
			else
			{
				cha_hp -= Mathf.Max(1, damage - defence);
				hp = cha_hp;
				if (infinitymode)
				{
					script_ui.Damaged_Extreme();
				}
			}
			int num2 = Random.Range(0, 100);
			script_cam.ZoomIn(5, 22, 0.2f);
			myrigidbody.mass = 2f;
			combotime = 0f;
			if (hp <= 0)
			{
				if (!story_scene)
				{
					if (general)
					{
						chamovestat = 12;
						g_hp = 0;
						Spcharge(100f);
						script_ui.CallGeneral(1);
					}
					else
					{
						life = false;
						hp = 0;
						myanimation.Play("dead");
						Invincibility(10f);
						script_ui.GameOver();
					}
				}
				else
				{
					hp = 95;
				}
			}
			else if (num2 > endurance)
			{
				myanimation.Stop();
				myanimation.Play("behitdown");
				Invincibility(2f);
				myrigidbody.AddForce(_damageVector * 180f);
			}
			else
			{
				Invincibility(0.5f);
				myrigidbody.AddForce(_damageVector * 20f);
			}
			if (!story_scene)
			{
				script_ui.StatUpdate_hp(hp, maxhp);
			}
			else
			{
				script_ui_story.StatUpdate(hp, maxhp, sp);
			}
		}
		mycollider.enabled = false;
		CancelAtk();
		ResetPower();
	}

	public void CancelAtk()
	{
		switch (battlestyle)
		{
		case 0:
			script_swing.SwingOff();
			script_thrust.SwingOff();
			break;
		case 1:
			script_swing.SwingOff();
			script_swing_sub.SwingOff();
			break;
		case 2:
			script_swing.SwingOff();
			script_splash.SplashOff();
			break;
		case 3:
			CancelInvoke("Arrow_shoot");
			break;
		case 4:
			script_thrust.SwingOff();
			script_punch.PunchOff();
			script_splash.SplashOff();
			break;
		case 5:
			script_splash.SplashOff();
			CancelInvoke("Magic_shoot");
			CancelInvoke("FireSplash");
			break;
		}
	}

	public void Blocked(Vector3 _monpos)
	{
		if (chamovestat >= 0 && !rideon)
		{
			chamovestat = -1;
			directionVector = Vector3.Normalize(_monpos - mytransform.position);
			mytransform.rotation = Quaternion.LookRotation(directionVector);
			if (guard_break > Random.Range(0, 100))
			{
				script_sound.SoundOn(-1);
				ef_blockbreak.gameObject.active = true;
				ef_blockbreak.position = _monpos + Vector3.up * 0.05f;
				ef_block.rotation = mytransform.rotation;
				myrigidbody.AddForce(directionVector * -10f);
			}
			else
			{
				script_sound.SoundOn(0);
				ef_block.gameObject.active = true;
				ef_block.position = mytransform.position;
				ef_block.rotation = mytransform.rotation;
				CancelAtk();
				script_txtef.TxtEfOn(3);
				myanimation.Stop();
				myanimation.Play("blocked");
				myrigidbody.AddForce(directionVector * -80f);
			}
		}
	}

	public void GetItem(int _kind, int _level)
	{
		script_getitem.TwirlOn(4, 4, 20, loop: false);
		myaudio.PlayOneShot(snd_getitem);
		switch (_kind)
		{
		case 0:
		{
			int a = (short)(1.34f * (float)_level + 2f);
			script_ui.GainCoin(a);
			break;
		}
		case 1:
			Spcharge(10f + itemboolst);
			break;
		case 2:
			Heal((int)((float)(100 + (level - 1)) * 0.1f * itemboolst));
			break;
		case 3:
			script_ui.GainSoul(1f);
			break;
		case 4:
			script_ui.GainJade(1);
			break;
		default:
			script_ui.SkillPlus(_kind - 100);
			break;
		}
	}

	public void GainExp(float _amount)
	{
		if (level >= 199)
		{
			exp = 0f;
			return;
		}
		exp += _amount * expboost;
		if (exp > (float)(level * 100))
		{
			level++;
			exp = 0f;
			cha_maxhp += 5;
			Crypto.Save_int_key("n47", level);
			PlayerPrefs.SetInt("updateRank", 1);
			Crypto.Save_int_key("n11", (int)exp);
		}
	}

	public void Invincibility(float t)
	{
		if (life)
		{
			ef_super.gameObject.active = true;
		}
		isinvincibility = true;
		target_invincibility = t;
		hitrate = 200;
	}

	public void Heal(int a)
	{
		{
			var emission = pt_heal.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		StartCoroutine(Sk_Heal());
		hp = Mathf.Min(hp + a, maxhp);
		if (general)
		{
			g_hp = hp;
		}
		else
		{
			cha_hp = hp;
		}
		if (!story_scene)
		{
			script_ui.StatUpdate_hp(hp, maxhp);
		}
		else
		{
			script_ui_story.StatUpdate(hp, maxhp, sp);
		}
	}

	public IEnumerator Sk_Heal()
	{
		yield return new WaitForSeconds(1.2f);
		{
			var emission = pt_heal.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
	}

	public void AttakUp(float _factor)
	{
		maxatk = (int)((float)(minatk + maxatk) * 0.5f * _factor);
		minatk = maxatk - 1;
		hitrate = 200;
	}

	public void ResetAtk()
	{
		if (!general)
		{
			maxatk = origin_maxattack;
			minatk = origin_minattack;
			hitrate = origin_hitrate;
		}
		else
		{
			maxatk = g_atk;
			minatk = (int)((float)g_atk * 0.8f);
			hitrate = 100;
		}
	}

	public void DefenceUp()
	{
		defence = (int)((float)defence * 2.4f);
		endurance = 200;
	}

	public void ResetDef()
	{
		defence = origin_defence;
		endurance = origin_endurance;
	}

	public void Spcharge(float a)
	{
		sp = Mathf.Min(sp + a, maxsp);
		if (!story_scene)
		{
			script_ui.StatUpdate_sp(sp);
		}
		else
		{
			script_ui_story.StatUpdate(hp, maxhp, sp);
		}
	}

	public void KillEnemy()
	{
		if (special_kind == 5)
		{
			Spcharge(special_amount * 2);
		}
		if (!general && cur_weapon >= 6)
		{
			Heal(special_amount - 1);
		}
	}

	public void Exstart()
	{
		if (!general)
		{
			CancelAtk();
			base.GetComponent<AudioSource>().clip = skillyell[0];
			base.GetComponent<AudioSource>().Play();
			exstart = true;
			myanimation.Play("attackex2");
			if (directionVector != Vector3.zero)
			{
				mytransform.forward = directionVector;
			}
			commendDelay = 0f;
			helper = false;
			if (!story_scene)
			{
				script_ui.PowerCharge();
			}
			else
			{
				script_ui_story.PowerCharge();
			}
		}
	}

	public void Eximpact()
	{
		script_swingex2.SwingOn(0f, 2, 2, 20, 1, 0);
		myanimation.Play("attackex2_impact");
		exstart = false;
		myaudio.clip = boom;
		myaudio.Play();
		myrigidbody.AddForce(mytransform.forward * 30f);
		keydown = false;
		prevPoint = currentTouchPos;
	}

	public void ResetPower()
	{
		if (!general && exstart)
		{
			if (!story_scene)
			{
				script_ui.ResetPower();
			}
			else
			{
				script_ui_story.ResetPower();
			}
			pressdelay = 0f;
			prevPoint = currentTouchPos;
			exstart = false;
			script_swingex2.SwingOff();
			myanimation.CrossFade("evade");
		}
	}

	private void Update()
	{
		chapos = mytransform.position;
		if (isinvincibility)
		{
			if (delay_invincibility < target_invincibility)
			{
				delay_invincibility += Time.deltaTime;
			}
			else
			{
				hitrate = origin_hitrate;
				ef_super.gameObject.active = false;
				isinvincibility = false;
				delay_invincibility = 0f;
			}
		}
		if (!isplaycha)
		{
			if (targetpos != Vector3.zero)
			{
				if (infinitymode)
				{
					if (dunenter)
					{
						chapos = Vector3.MoveTowards(chapos, targetpos, Time.deltaTime);
						directionVector = targetpos - chapos;
						if (chapos == targetpos)
						{
							dunenter = false;
							targetpos = Vector3.zero;
							script_spawn.SetMapExtreme(_clear: true);
							EnterDun();
						}
					}
					else if (!myanimation.IsPlaying("change_in"))
					{
						myanimation.Play("land_changeweapon");
						script_weapon.ReturnBlade();
						targetpos = Vector3.zero;
						isplaycha = true;
						script_spawn.RegenStart();
						script_ui.ResetTime();
						if (extremeStageHeal)
						{
							Heal((int)((float)cha_maxhp * 0.1f));
						}
					}
				}
				else if (!storyrun)
				{
					directionVector = targetpos - chapos;
					chamovestat = 1;
					mytransform.position = Vector3.MoveTowards(mytransform.position, targetpos, Time.deltaTime * 0.1f);
					if (!myanimation.IsPlaying("ride_s"))
					{
						if (myanimation.IsPlaying("ride1"))
						{
							GameObject.Find("scene_trans").GetComponent<Story_trans>().ScreenOn(2f);
							targetpos = Vector3.zero;
						}
						else
						{
							if (!(directionVector.magnitude < 0.05f))
							{
								mytransform.rotation = Quaternion.LookRotation(directionVector);
								return;
							}
							RideHorse();
						}
					}
				}
				else
				{
					speedfactor = 6.5f;
					movespeed = movespeed_origin;
					if (directionVector != Vector3.zero)
					{
						mytransform.rotation = Quaternion.LookRotation(directionVector);
					}
					if (Vector3.Distance(mytransform.position, targetpos) < 0.35f)
					{
						targetpos = Vector3.zero;
						isplaycha = true;
						script_cam.ResetCam();
						GameObject.Find("scene_trans").GetComponent<Story_trans>().ScreenOn(0f);
						GameObject.FindWithTag("Respawn").GetComponent<Spawn_story>().RegenStart();
					}
				}
			}
			longdash = 0f;
		}
		else if (Input.anyKeyDown)
		{
			savetouchposition = false;
			ray = guicam.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out casthit, 4f))
			{
				if (chamovestat < 100 && casthit.transform.gameObject.GetComponent<Icon_Skill_p>().SkillColliderOn())
				{
					mycollider.enabled = false;
					CancelAtk();
				}
				skillicondown = true;
			}
			pressdelay = 0f;
			keydown = true;
		}
		else if (Input.anyKey)
		{
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (!skillicondown && mapcollider.Raycast(ray, out casthit, 4f))
			{
				currentTouchPos = casthit.point;
				if (!savetouchposition)
				{
					prevPoint = currentTouchPos;
					savetouchposition = true;
				}
				tempdir = currentTouchPos - mytransform.position;
				tempdir[1] = 0f;
				magnitude_temp = tempdir.magnitude;
				tempdir /= magnitude_temp;
				if (dubbleclick < 0.3f && chamovestat < 50 && chamovestat >= 0 && !(sp < weaponweight * 2f) && Vector3.Distance(currentTouchPos, prevPoint) < 0.1f)
				{
					directionVector = tempdir;
					mytransform.rotation = Quaternion.LookRotation(directionVector);
					speedfactor = 6.5f;
					myrigidbody.mass = 2f;
					Spcharge((0f - weaponweight) * 2f);
					CancelAtk();
					base.GetComponent<AudioSource>().clip = snd_dodge;
					base.GetComponent<AudioSource>().Play();
					myanimation.Stop();
					myanimation.Play("dodge");
					chamovestat = 53;
					myrigidbody.AddForce(directionVector * 220f);
					mycollider.enabled = false;
					pressdelay = 1f;
				}
				if (magnitude_temp < 0.05f)
				{
					directionVector = Vector3.zero;
					if (rideon)
					{
						script_horse.RunStart(a: false);
						if (clone_ride != null)
						{
							clone_ride.speed = 0.1f;
						}
					}
				}
				else if (magnitude_temp > 0.08f)
				{
					directionVector = tempdir;
					speedfactor = 6.5f;
				}
			}
			if (chamovestat < 19)
			{
				pressdelay += Time.deltaTime;
				if (pressdelay > 0.3f)
				{
					Exstart();
				}
				else if (pressdelay < 0.1f && !general && !grabstart && (!(sp < 50f) || superMode != 0) && Vector3.Distance(prevPoint, currentTouchPos) > 0.2f)
				{
					myaudio.clip = yell[2];
					base.GetComponent<AudioSource>().Play();
					chamovestat = 53;
					myaudio.PlayOneShot(snd_skillstart);
					Vector3 vector = Vector3.Normalize(currentTouchPos - prevPoint);
					if (vector != Vector3.zero)
					{
						mytransform.rotation = Quaternion.LookRotation(vector);
					}
					if (superMode == 0)
					{
						myrigidbody.AddForce(vector * 300f);
						myanimation.Play("grabstart");
						grabstart = true;
					}
					else if (superMode == 1)
					{
						myrigidbody.mass = 6f;
						base.GetComponent<Rigidbody>().drag = 15f;
						myanimation.Stop();
						skill_ing = true;
						Time.timeScale = 0.1f;
						superMode = 2;
						myanimation["attackex3"].speed = 1.35f;
						myanimation.Play("attackex3");
						AnimationState animationState = myanimation.PlayQueued("attackex3_impact");
						animationState.speed = 0.25f;
						Invincibility(1f);
						script_ui.SuperModeOn();
						ef_swordExtreme.position = mytransform.position + Vector3.up * 0.1f;
						ef_swordExtreme.gameObject.active = true;
						mytransform.forward = vector;
					}
					mycollider.radius = 0.1f;
					mycollider.center = collider_dodge;
					mycollider.enabled = false;
					mycollider.enabled = true;
					CancelAtk();
					pressdelay = 1f;
				}
			}
		}
		else if (keydown)
		{
			savetouchposition = false;
			dubbleclick = 0f;
			keydown = false;
			skillicondown = false;
			ResetPower();
		}
		dubbleclick += 2f * Time.deltaTime;
		if (myanimation.IsPlaying("dead"))
		{
			mycollider.enabled = false;
			chamovestat = 500;
			directionVector = Vector3.zero;
			combotime = 0f;
			return;
		}
		if (change_cha)
		{
			if (!myanimation.isPlaying || myanimation.IsPlaying("1t"))
			{
				change_cha = false;
				chamovestat = 5;
			}
		}
		else if (pet_ing)
		{
			if (myanimation.IsPlaying("land_changeweapon"))
			{
				if (chamovestat != 182)
				{
					script_weapon.ReturnBlade();
					chamovestat = 182;
					pet_ing = false;
				}
			}
			else if (myanimation.IsPlaying("jump_changeweapon"))
			{
				chamovestat = 180;
			}
			else if (myanimation.IsPlaying("jump_changeweapon_i"))
			{
				if (chamovestat != 181)
				{
					script_weapon.SwitchWepon(currentPet, 3, 0f, 0f);
				}
				chamovestat = 181;
			}
			else if (myanimation.IsPlaying("eagle_shoot"))
			{
				chamovestat = 192;
				chapos[1] = 0.02f * Mathf.Sin(Time.time * 8f) + 0.02f;
			}
			else if (myanimation.IsPlaying("eagle_start"))
			{
				chamovestat = 200;
				chapos[1] = 0.02f * Mathf.Sin(Time.time * 8f) + 0.02f;
			}
			else if (myanimation.IsPlaying("eagle"))
			{
				if (chamovestat != 203)
				{
					mycollider.enabled = true;
					movespeed = 0.3f;
					chamovestat = 203;
				}
				chapos[1] = 0.02f * Mathf.Sin(Time.time * 8f) + 0.02f;
			}
			else if (myanimation.IsPlaying("eagle_finish") || myanimation.IsPlaying("horse_finish"))
			{
				chamovestat = 184;
			}
			else if (myanimation.IsPlaying("horse_chacry"))
			{
				if (chamovestat != 185)
				{
					rideon = true;
					script_horse.RideandCry();
					chamovestat = 185;
				}
			}
			else if (myanimation.IsPlaying("ride_s") || myanimation.IsPlaying("horse_start"))
			{
				chamovestat = 184;
				directionVector = Vector3.zero;
				chapos = Vector3.Lerp(mytransform.position, pet_horse.position, Time.deltaTime * 5f);
				mytransform.rotation = Quaternion.Slerp(mytransform.rotation, pet_horse.rotation, Time.deltaTime * 5f);
			}
			else if (myanimation.IsPlaying("attack_ride1") || myanimation.IsPlaying("attack_ride2"))
			{
				chamovestat = 214;
				combotime = 1f;
			}
			else if (myanimation.IsPlaying("ride1") && chamovestat != 212)
			{
				chamovestat = 212;
				mycollider.enabled = true;
			}
		}
		else if (skill_ing)
		{
			if (myanimation.IsPlaying("cast1") || myanimation.IsPlaying("cast3") || myanimation.IsPlaying("cast4"))
			{
				if (directionVector != Vector3.zero)
				{
					mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(directionVector), Time.deltaTime * 80f);
				}
				chamovestat = 110;
				combotime = 0f;
			}
			else if (myanimation.IsPlaying("cast2"))
			{
				chamovestat = 110;
				combotime = 0f;
			}
			else if (myanimation.IsPlaying("skill01") || myanimation.IsPlaying("skill02") || myanimation.IsPlaying("skill03") || myanimation.IsPlaying("skill04") || myanimation.IsPlaying("rapidstab") || myanimation.IsPlaying("skill_chargesmash"))
			{
				chamovestat = 111;
				combotime = 0f;
				skill_ing = false;
			}
			else if (myanimation.IsPlaying("wing_i"))
			{
				if (chamovestat != 112)
				{
					script_skill.DelaySkill(19);
					chamovestat = 112;
					myaudio.clip = boom;
					myaudio.Play();
				}
				combotime = 0f;
				skill_ing = false;
			}
			else if (myanimation.IsPlaying("wing"))
			{
				if (fogdelay >= 0.1f)
				{
					FogOn(2);
					fogdelay = 0f;
				}
				else
				{
					fogdelay += Time.deltaTime;
				}
				chamovestat = 111;
				combotime = 0f;
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.identity, Time.deltaTime * 3f);
			}
			else if (myanimation.IsPlaying("wheelwind"))
			{
				movespeed = 0.5f;
				chamovestat = 222;
				combotime = 0f;
			}
			else if (myanimation.IsPlaying("jumpattack_air"))
			{
				chapos += directionVector * Time.deltaTime * 0.6f;
				chamovestat = 151;
				combotime = 0f;
			}
			else if (myanimation.IsPlaying("jumpattack_impact"))
			{
				if (chamovestat != 112)
				{
					script_skill.DelaySkill(2);
					chamovestat = 112;
					myaudio.clip = boom;
					myaudio.Play();
				}
				combotime = 0f;
				skill_ing = false;
			}
			else if (myanimation.IsPlaying("attackex3"))
			{
				chapos += directionVector * 0.03f * Time.deltaTime;
				if (directionVector != Vector3.zero)
				{
					mytransform.forward = directionVector;
				}
				chamovestat = 103;
			}
			else if (myanimation.IsPlaying("attackex3_impact"))
			{
				if (fogdelay >= 0.1f)
				{
					FogOn(0);
					fogdelay = 0f;
				}
				if (chamovestat != 102)
				{
					myaudio.clip = boom;
					myaudio.Play();
					if (superMode == 2)
					{
						superMode = 0;
						ef_linehit.position = mytransform.position + Vector3.up * 0.02f;
						ef_linehit.right = -base.transform.forward;
						ef_linehit.gameObject.active = true;
						Time.timeScale = 1f;
						myrigidbody.AddForce(mytransform.forward * 4000f);
					}
					else
					{
						myrigidbody.AddForce(mytransform.forward * 380f);
					}
				}
				chamovestat = 102;
			}
			else
			{
				skill_ing = false;
			}
		}
		else if (grab_ing)
		{
			if (myanimation.IsPlaying("grab10_finish2") || myanimation.IsPlaying("grab20_finish2") || myanimation.IsPlaying("grab22_finish2") || myanimation.IsPlaying("grab30_finish2"))
			{
				if (chamovestat != 114)
				{
					script_cam.ResetCam();
					script_txtef.TxtEfOn(0);
					Heal((int)((float)cha_maxhp * grabheal_factor));
				}
				chamovestat = 114;
				combotime = 0f;
			}
			else if (myanimation.IsPlaying("grab11_finish2") || myanimation.IsPlaying("grab12_finish2") || myanimation.IsPlaying("grab21_finish2"))
			{
				if (chamovestat != 114)
				{
					Heal((int)((float)cha_maxhp * 0.1f));
					script_splash.SplashOn(col: true, 0.07f, 0f);
					ef_rotfog.position = mytransform.position;
					script_rotfog.RotfogOn(4f, 5f, 80, 8, 2f);
					script_cam.ResetCam();
					script_txtef.TxtEfOn(0);
				}
				chamovestat = 114;
			}
			else if (myanimation.IsPlaying("grab10_finish") || myanimation.IsPlaying("grab11_finish") || myanimation.IsPlaying("grab12_finish") || myanimation.IsPlaying("grab20_finish") || myanimation.IsPlaying("grab21_finish") || myanimation.IsPlaying("grab22_finish") || myanimation.IsPlaying("grab30_finish"))
			{
				chamovestat = 113;
				combotime = 0f;
			}
			else if (myanimation.IsPlaying("grab10") || myanimation.IsPlaying("grab11") || myanimation.IsPlaying("grab12") || myanimation.IsPlaying("grab20") || myanimation.IsPlaying("grab21") || myanimation.IsPlaying("grab22") || myanimation.IsPlaying("grab30"))
			{
				chamovestat = 112;
				combotime = 0f;
			}
			else
			{
				grab_ing = false;
			}
		}
		else if (myanimation.IsPlaying("behitdown"))
		{
			chamovestat = -1;
		}
		else if (myanimation.IsPlaying("blocked") || myanimation.IsPlaying("evade"))
		{
			chamovestat = -1;
			combotime = 0f;
		}
		else if (myanimation.IsPlaying("grabstart"))
		{
			chamovestat = 53;
		}
		else if (myanimation.IsPlaying("dodge"))
		{
			chamovestat = 53;
			combotime = 0f;
		}
		else if (myanimation.IsPlaying("attackex2"))
		{
			pressdelay = 1f;
			chamovestat = 19;
			commendDelay = 0f;
			helper = false;
		}
		else if (myanimation.IsPlaying("attackex2_impact"))
		{
			if (!general)
			{
				commendDelay += Time.deltaTime;
				if (!helper && commendDelay > 0.3f)
				{
					guide_circle.gameObject.active = true;
					helper = true;
				}
				if (Input.anyKeyDown)
				{
					if (commendDelay < 0.7f && commendDelay > 0.5f)
					{
						script_weapon.SwitchWepon(0, 8, 0.6f, 1f);
						myanimation.Play("attackex1");
						impact = myanimation.PlayQueued("attackex1_impact");
						impact.speed = 0.25f;
						myaudio.PlayOneShot(snd_skillstart);
						commendDelay = 1f;
					}
					else
					{
						commendDelay = 1f;
					}
				}
				pressdelay = 1f;
			}
			if (chamovestat != 102)
			{
				myaudio.clip = boom;
				myaudio.Play();
				script_splash.SplashOn(col: true, 0.1f, 0f);
			}
			chamovestat = 102;
		}
		else if (myanimation.IsPlaying("attackex1"))
		{
			if (fogdelay >= 0.15f)
			{
				FogOn(0);
				fogdelay = 0f;
			}
			else
			{
				fogdelay += Time.deltaTime;
			}
			chapos += directionVector * 0.5f * Time.deltaTime;
			if (directionVector != Vector3.zero)
			{
				mytransform.forward = directionVector;
			}
			chamovestat = 101;
			commendDelay = 0f;
			helper = false;
		}
		else if (myanimation.IsPlaying("attackex1_impact"))
		{
			fogdelay = 0.15f;
			commendDelay += Time.deltaTime;
			if (!helper && commendDelay > 0.3f)
			{
				guide_circle.gameObject.active = true;
				helper = true;
			}
			if (Input.anyKeyDown)
			{
				if (commendDelay < 0.7f && commendDelay > 0.5f)
				{
					script_cam.Topview();
					pt_rising.position = mytransform.position;
					pt_rising.gameObject.active = true;
					myanimation.Play("riseattack");
					script_weapon.SwitchWepon(1, 8, 1.4f, 1f);
					myaudio.clip = snd_riseattack;
					myaudio.PlayOneShot(snd_skillstart);
					exattack++;
					commendDelay = 1f;
				}
				else
				{
					commendDelay = 1f;
				}
			}
			if (chamovestat != 102)
			{
				script_cam.Topview();
				ef_riseattack.position = mytransform.position;
				ef_riseattack.gameObject.active = true;
				myaudio.clip = boom;
				myaudio.Play();
			}
			chamovestat = 102;
		}
		else if (myanimation.IsPlaying("riseattack"))
		{
			chamovestat = 102;
			myaudio.Play();
			pt_rising.Rotate(Vector3.up * Time.deltaTime * 2000f);
		}
		else if (myanimation.IsPlaying("attack3"))
		{
			combotime = 1f;
			chamovestat = 11;
			fogdelay = 0f;
		}
		else if (myanimation.IsPlaying("attack4"))
		{
			if (weapon_kind == 0)
			{
				if (fogdelay >= 0.15f)
				{
					FogOn(1);
					fogdelay = 0f;
				}
				else
				{
					fogdelay += Time.deltaTime;
				}
			}
			combotime = 1f;
			movespeed = movespeed_origin;
			chamovestat = 12;
			commendDelay = 0f;
			helper = false;
		}
		else if (myanimation.IsPlaying("attack5"))
		{
			chamovestat = 13;
		}
		else if (myanimation.IsPlaying("attack2"))
		{
			chamovestat = 13;
			combotime = 1f;
		}
		else if (myanimation.IsPlaying("attack1"))
		{
			commendDelay = 0f;
			chamovestat = 11;
			combotime = 1f;
		}
		else if (myanimation.IsPlaying("dash_attack"))
		{
			chapos += mytransform.forward * Time.deltaTime * 0.5f;
			commendDelay = 0f;
			chamovestat = 19;
			combotime = 1f;
		}
		else if (myanimation.IsPlaying("run"))
		{
			movespeed = movespeed_origin;
			if (chamovestat == 2)
			{
				if (longdash < 3f)
				{
					longdash += Time.deltaTime;
				}
				else if (chamovestat != 3)
				{
					ef_blur.gameObject.active = true;
					if (!noeagle)
					{
						pet_eagle.gameObject.active = true;
					}
					chamovestat = 3;
					mycollider.center = collider_horse;
					mycollider.radius = 0.2f;
				}
			}
			else if (chamovestat != 3)
			{
				pet_ing = false;
				skill_ing = false;
				grabstart = false;
				myrigidbody.mass = 2f;
				base.GetComponent<Rigidbody>().drag = 5f;
				longdash = 0f;
				chamovestat = 2;
				mycollider.radius = collider_radius;
				mycollider.center = collider_center;
				mycollider.enabled = false;
				mycollider.enabled = true;
			}
		}
		else if (myanimation.IsPlaying("walk"))
		{
			if (chamovestat != 1)
			{
				mycollider.enabled = true;
				chamovestat = 1;
			}
		}
		else if (myanimation.IsPlaying("idle"))
		{
			chamovestat = 0;
		}
		if (!myanimation.isPlaying || chamovestat <= 10 || chamovestat > 200)
		{
			combotime -= 2f * Time.deltaTime;
			if (directionVector == Vector3.zero || chamovestat < 0)
			{
				myanimation.CrossFade("idle");
			}
			else
			{
				rotate = Quaternion.LookRotation(directionVector);
				if (rideon)
				{
					if (!(clone_ride == null) && clone_ride.speed < 0.6f)
					{
						clone_ride.speed = 0.6f;
						movespeed = 0.8f;
						clone_ride.layer = 3;
						script_horse.RunStart(a: true);
					}
					speedfactor = 7f;
				}
				else if (speedfactor > 4.5f)
				{
					speedfactor -= 2f * Time.deltaTime;
					myanimation.CrossFade("run");
				}
				else if (speedfactor > 0.2f)
				{
					speedfactor -= 2f * Time.deltaTime;
					myanimation.CrossFade("walk");
					myanimation["walk"].speed = speedfactor / 7.5f + 0.2f;
					movespeed = speedfactor / 12f;
				}
				else
				{
					directionVector = Vector3.zero;
				}
				chapos += directionVector * Time.deltaTime * movespeed;
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, rotate, Time.deltaTime * 8f);
			}
		}
		if (!infinitymode)
		{
			chapos[0] = Mathf.Clamp(chapos[0], 0f - limit_x, limit_x);
			chapos[2] = Mathf.Clamp(chapos[2], limit_y_b, limit_y_f);
		}
		else
		{
			Vector3 vector2 = chapos;
			vector2.y = 0f;
			if (Vector3.SqrMagnitude(vector2) > restrictArea)
			{
				directionVector = -vector2;
				chapos -= vector2 * Time.deltaTime;
			}
		}
		mytransform.position = chapos;
	}

	public void SetRestrictArea(float _area)
	{
		restrictArea = _area;
	}
}
