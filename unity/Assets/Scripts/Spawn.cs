using UnityEngine;

public class Spawn : MonoBehaviour
{
	private const int ENEMY_POOL = 16;

	private const int BOSS_POOL = 11;

	public Transform[] stage = new Transform[10];

	private float[] restrictArea = new float[8] { 0.9f, 0.9f, 0.9f, 0.8f, 0.9f, 0.9f, 0.9f, 0.9f };

	public Transform[] mon_destroy = new Transform[4];

	public Transform[] enemy_source = new Transform[16];

	public Transform[] boss = new Transform[11];

	public Transform general_portrait;

	public Transform collider_ground;

	public Transform cart;

	public Transform tower;

	public Transform barrack;

	public Transform basecamp;

	public Transform tank;

	public Transform plane_staff;

	public Transform pt_soul;

	private Texture prt_general;

	private Texture prt_cha;

	private Transform c_basecamp;

	private Transform[] c_tower = new Transform[6];

	private Transform[] c_barrack = new Transform[6];

	private Transform c_tank;

	private Transform spawnenemy;

	private Transform clone_enemy;

	private Transform clone_Boss;

	private Transform c_stage;

	private Transform c_extraunit;

	private Transform[] enemyset = new Transform[4];

	private Transform[] clone_destroy = new Transform[6];

	private Transform[] clone_soul = new Transform[3];

	public Transform cut_boss;

	public int monnum;

	public short rewardkind;

	public Transform pt_summonfog;

	private Transform c_dun_door;

	private int WAVEEMEMYNUM = 40;

	private int totalEnemyNum;

	private int maxEnemyNum = 15;

	private int regen = -1;

	public int wave = 1;

	public int finalstage = 3;

	private float spawndelay = 1f;

	private float set_spawndelay = 1f;

	private float g_hp_length;

	private bool countdown;

	private bool stagefinish;

	private int play_kind;

	private int bosscount;

	private short cur_stage_index;

	private short infinity_stage_index;

	private int destroy_human_kind;

	private int destroy_beast_kind = 3;

	private int destroy_last_kind;

	private int enemykind;

	private int cur_difficulty;

	private int oldstageindex = -1;

	private int summon_amount;

	public int enemykill;

	public int grappling;

	private Vector3 summonpos = Vector3.zero;

	private Vector3 rndpos;

	private Vector3 temprndpos;

	private int rndpoint;

	private int rndoldpoint = -1;

	private short allycount;

	private short general_index;

	private short g_grade;

	private int soulcount;

	private short barrack_count = 6;

	private short tower_count = 6;

	private GameObject last_mon;

	public bool infinitymode;

	private DB_Stage script_stageDB;

	private UI_Ingame script_IngameUI;

	private General_Stat script_generalstat;

	private Gauge_UV script_g_hpgauge;

	private DB_General script_DBgeneral;

	private Icon_Skill script_iconskill;

	private Cha_Control script_cha;

	private Transform g_hpgauge;

	public Transform icon_dead;

	private Vector3[] spawpoint = new Vector3[8];

	private int[] general_hp = new int[12];

	private short[] bossindex = new short[3];

	private short negativefactor = 1;

	public Texture lightmap_tank;

	private int[] bosskill = new int[12];

	public int cur_general = -1;

	private short bossremain;

	private float restrict_Factor = 100f;

	private void Awake()
	{
		bosskill = PlayerPrefsX.GetIntArray("bosskill");
		cur_difficulty = Crypto.Load_int_key("n14");
		script_stageDB = GetComponent<DB_Stage>();
		int num = Crypto.Load_int_key("cur_stage_kind");
		cur_stage_index = (short)Crypto.Load_int_key("cur_stage_index");
		infinity_stage_index = (short)(cur_stage_index % 90);
		play_kind = Crypto.Load_int_key("play_kind");
		cur_general = Crypto.Load_int_key("cur_general");
		GameObject gameObject = GameObject.FindWithTag("Player");
		script_cha = gameObject.GetComponent<Cha_Control>();
		script_generalstat = GetComponent<General_Stat>();
		script_iconskill = GameObject.FindWithTag("icon_skill").GetComponent<Icon_Skill>();
		if (num == 11)
		{
			cur_difficulty = 0;
			infinitymode = true;
			wave = Crypto.Load_int_key("n49");
			cur_stage_index = (short)(2 + 3 * (wave - 1));
			infinity_stage_index = (short)(cur_stage_index % 90);
			GameObject gameObject2 = Resources.Load("dun_gate") as GameObject;
			c_dun_door = (Transform)Object.Instantiate(gameObject2.transform, Vector3.up * 55f, Quaternion.identity);
			set_spawndelay = 0.4f;
			finalstage = 1000;
			WAVEEMEMYNUM = 80;
		}
		else
		{
			if (play_kind == 5)
			{
				SetMapStory(num, _islinemap: false);
				bossremain = script_stageDB.st[cur_stage_index]._bosscount;
				finalstage = 1;
				WAVEEMEMYNUM = 120;
			}
			if (play_kind == 6)
			{
				SetMapStory(num, _islinemap: true);
				collider_ground.position = new Vector3(0f, -0.01f, 3f);
				collider_ground.localScale = new Vector3(1f, 0f, 6f);
				cur_general = -1;
				c_extraunit = (Transform)Object.Instantiate(cart, Vector3.forward * 0.5f, Quaternion.identity);
				general_portrait.gameObject.active = true;
				general_portrait.GetComponent<Renderer>().material.mainTexture = Resources.Load("prt_cart") as Texture;
				set_spawndelay = 6.5f - (float)cur_stage_index * 0.03f;
				finalstage = 1;
				WAVEEMEMYNUM = 250;
			}
			else if (play_kind == 7)
			{
				SetMapStory(num, _islinemap: true);
				collider_ground.position = new Vector3(0f, -0.01f, 3f);
				collider_ground.localScale = new Vector3(1f, 0f, 6f);
				for (int i = 0; i < 3; i++)
				{
					c_tower[2 * i] = (Transform)Object.Instantiate(tower, Vector3.forward * (i * 3) + Vector3.forward * 3f - Vector3.right * 0.5f, Quaternion.Euler(0f, 45f, 0f));
					c_tower[2 * i + 1] = (Transform)Object.Instantiate(tower, Vector3.forward * (i * 3) + Vector3.forward * 3f + Vector3.right * 0.5f, Quaternion.Euler(0f, 45f, 0f));
					c_barrack[2 * i] = (Transform)Object.Instantiate(barrack, Vector3.forward * (i * 3) + Vector3.forward * 2f + Vector3.right * 0.4f, Quaternion.Euler(0f, 225f, 0f));
					c_barrack[2 * i + 1] = (Transform)Object.Instantiate(barrack, Vector3.forward * (i * 3) + Vector3.forward * 1f - Vector3.right * 0.4f, Quaternion.Euler(0f, -225f, 0f));
				}
				c_basecamp = (Transform)Object.Instantiate(basecamp, Vector3.forward * 10f, Quaternion.identity);
				c_tank = (Transform)Object.Instantiate(tank, Vector3.forward * -0.5f, Quaternion.Euler(0f, 180f, 0f));
				set_spawndelay = 3f;
				c_stage.Find("amap").GetComponent<Renderer>().material.SetTexture("_LightMap", lightmap_tank);
				InvokeRepeating("RegenAlly", 2f, 2f);
				finalstage = 1;
				WAVEEMEMYNUM = 250;
			}
			else
			{
				SetMapStory(num, _islinemap: false);
				finalstage = script_stageDB.st[cur_stage_index]._stagenum;
				WAVEEMEMYNUM = 120;
				if (cur_stage_index == 0)
				{
					WAVEEMEMYNUM = 40;
				}
			}
			rewardkind = script_stageDB.st[infinity_stage_index]._reward;
		}
		if (cur_general != -1)
		{
			script_DBgeneral = GetComponent<DB_General>();
			int[] array = new int[6];
			array = PlayerPrefsX.GetIntArray("n13");
			general_hp = PlayerPrefsX.GetIntArray("n33");
			GeneralHP(_isstart: true);
			int general = array[cur_general];
			script_generalstat.SetGeneral(general);
			short general_kind = script_generalstat.general_kind;
			short g_maxhp = script_generalstat.g_maxhp;
			g_grade = script_generalstat.g_grade;
			int num2 = general_hp[cur_general];
			if (num2 > g_maxhp)
			{
				num2 = g_maxhp;
			}
			general_index = script_generalstat.general_index;
			short g_maxatk = script_generalstat.g_maxatk;
			float g_atkspd = script_generalstat.g_atkspd;
			float cooltime = script_DBgeneral.gs[general_index]._cooltime - g_atkspd * 40f;
			short soulcost = script_DBgeneral.gs[general_index]._soulcost;
			if (infinitymode)
			{
				soulcost = 0;
			}
			short voice = script_DBgeneral.gs[general_index]._voice;
			script_cha.Set_General(script_DBgeneral.gs[general_index]._weapon, general_kind, g_maxhp, g_maxatk, script_generalstat.g_def, g_atkspd, script_generalstat.g_unique, num2, voice);
			if (g_grade < 2)
			{
				Object.Destroy(general_portrait.GetChild(0).gameObject);
			}
			else
			{
				gameObject.GetComponent<Cha_Skill>().Set_General_Skill(g_maxatk, script_DBgeneral.gs[general_index]._skillatk, script_DBgeneral.gs[general_index]._skillkind);
				script_iconskill.Set_General(soulcost, cooltime, general_kind, infinitymode);
			}
			general_portrait.gameObject.active = true;
			prt_general = Resources.Load("prt_general" + (general_index + 1)) as Texture;
			prt_cha = Resources.Load("prt_cha") as Texture;
			general_portrait.GetComponent<Renderer>().material.mainTexture = prt_general;
			general_portrait.GetChild(0).GetComponent<Icon_Skill_p>().SkillKind(0, general_index, 0, soulcost);
			if (cur_difficulty != 2)
			{
				g_hpgauge = GameObject.FindWithTag("p_plan").GetComponent<MakeUI>().CreatCustomPlane(new Vector2(0.24f, 0.08f), 0f, new Vector3(-1.39f, 2.36f, 1.8f), new Vector2(0.75f, 0.625f), new Vector2(0.875f, 0.6875f), "general_hp", "Gauge_UV", 0.1f, 0);
				script_g_hpgauge = g_hpgauge.GetComponent<Gauge_UV>();
				g_hp_length = (1f - (float)num2 / (float)g_maxhp) * 0.125f;
				script_g_hpgauge.UvMove(Vector2.right * g_hp_length);
			}
		}
	}

	private void Start()
	{
		int num = Random.Range(1, 4);
		base.GetComponent<AudioSource>().clip = Resources.Load("bgm_stage" + num) as AudioClip;
		base.GetComponent<AudioSource>().Play();
		base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
		for (int i = 0; i < 3; i++)
		{
			clone_destroy[i] = (Transform)Object.Instantiate(mon_destroy[i], Vector3.one * 6f, Quaternion.identity);
			clone_destroy[i + 3] = (Transform)Object.Instantiate(mon_destroy[3], Vector3.one * 6f, Quaternion.identity);
			clone_soul[i] = (Transform)Object.Instantiate(pt_soul, Vector3.one * 31f, Quaternion.identity);
		}
		if (infinitymode)
		{
			SetMapExtreme(_clear: false);
		}
		monnum = 0;
		totalEnemyNum = WAVEEMEMYNUM;
		script_IngameUI = GameObject.FindWithTag("ui").GetComponent<UI_Ingame>();
		enemykill = Crypto.Load_int_key("enemykill");
		grappling = Crypto.Load_int_key("grappling");
	}

	public void SetMapStory(int _cur_stage_kind, bool _islinemap)
	{
		int num = 0;
		if (_islinemap)
		{
			num = 5;
		}
		c_stage = (Transform)Object.Instantiate(stage[_cur_stage_kind - 1 + num], Vector3.zero, Quaternion.identity);
		c_stage.name = "stage";
		if (!_islinemap)
		{
			for (int i = 0; i < 8; i++)
			{
				ref Vector3 reference = ref spawpoint[i];
				reference = c_stage.GetChild(0).GetChild(i).position;
			}
		}
	}

	public void SetMapExtreme(bool _clear)
	{
		if (_clear)
		{
			for (int i = 0; i < 6; i++)
			{
				clone_destroy[i].GetComponent<Mon_Destroy>().FinishNow();
			}
			GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().FinishEfs();
		}
		int num = 0;
		num = (wave - 1) / 8;
		if (num == oldstageindex)
		{
			c_stage.GetComponent<Map_extreme>().SetWave(wave);
		}
		else if (num < 8)
		{
			if (c_stage != null)
			{
				Object.Destroy(c_stage.gameObject);
			}
			GameObject gameObject = Resources.Load("Stage_dun" + (num + 1)) as GameObject;
			c_stage = (Transform)Object.Instantiate(gameObject.transform, Vector3.zero, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			c_stage.name = "stage";
			c_stage.GetComponent<Map_extreme>().SetWave(wave);
			float x = c_stage.localScale.x;
			restrict_Factor = restrictArea[num] * x;
			c_dun_door.position = -c_stage.forward * restrict_Factor;
			c_dun_door.rotation = Quaternion.LookRotation(-c_dun_door.position);
			c_dun_door.gameObject.active = false;
			c_stage.Find("light_dun1").rotation = Quaternion.Euler(0f, 120f, 0f);
			oldstageindex = num;
			script_cha.SetRestrictArea(restrict_Factor);
			for (int j = 0; j < 8; j++)
			{
				ref Vector3 reference = ref spawpoint[j];
				reference = c_stage.GetChild(0).GetChild(j).position;
			}
		}
	}

	public void TowerBreak(bool istower)
	{
		if (istower)
		{
			tower_count--;
		}
		else
		{
			barrack_count--;
		}
		if (tower_count <= 0 && barrack_count <= 0)
		{
			c_tank.GetComponent<Tank>().HurryUp();
		}
	}

	public Vector3 SetRndPoint()
	{
		if (play_kind == 6)
		{
			temprndpos = c_extraunit.position + Vector3.forward * Random.Range(-1.5f, 1.5f) + Vector3.right * ((float)Random.Range(-1, 1) + 0.5f) * 3f;
		}
		else if (play_kind == 7)
		{
			int num = 0;
			for (int i = 0; i < 6; i++)
			{
				rndpoint = (rndpoint + 1) % 6;
				if (c_barrack[rndpoint] != null)
				{
					temprndpos = c_barrack[rndpoint].position;
					num++;
					break;
				}
			}
			if (num == 0)
			{
				regen = -1;
			}
		}
		else if (infinitymode)
		{
			rndpoint = (rndpoint + 1) % 8;
			temprndpos = spawpoint[rndpoint];
		}
		else
		{
			rndpoint = Random.Range(0, 8);
			if (rndpoint == rndoldpoint)
			{
				rndpoint = (rndpoint + 1) % 8;
			}
			rndoldpoint = rndpoint;
			temprndpos = spawpoint[rndpoint];
		}
		return temprndpos;
	}

	public void OpenDundoor()
	{
		c_dun_door.gameObject.active = true;
	}

	public void EnemyDead(int _destroy, Vector3 _pos, Texture _tex, Vector3 _scale, Vector3 _forcedir)
	{
		monnum--;
		if (!stagefinish && monnum <= 0 && regen == -2)
		{
			stagefinish = true;
			if (infinitymode)
			{
				c_stage.GetComponent<Map_extreme>().TrapStop();
			}
			script_IngameUI.WaveSet(wave);
			wave++;
		}
		switch (_destroy)
		{
		case 2:
			clone_destroy[destroy_human_kind].position = _pos + _forcedir * 0.15f;
			clone_destroy[destroy_human_kind].rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			clone_destroy[destroy_human_kind].GetComponent<Mon_Destroy>().TextureChange(_tex, _scale, _destroy);
			clone_destroy[destroy_human_kind].gameObject.active = true;
			destroy_human_kind = (destroy_human_kind + 1) % 3;
			destroy_last_kind = destroy_human_kind;
			break;
		case 1:
			clone_destroy[destroy_beast_kind].position = _pos + _forcedir * 0.15f;
			clone_destroy[destroy_beast_kind].rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			clone_destroy[destroy_beast_kind].GetComponent<Mon_Destroy>().TextureChange(_tex, _scale, _destroy);
			clone_destroy[destroy_beast_kind].gameObject.active = true;
			destroy_beast_kind = (destroy_beast_kind + 1) % 3 + 3;
			destroy_last_kind = destroy_beast_kind;
			break;
		case 3:
			clone_destroy[destroy_last_kind].position = _pos;
			clone_destroy[destroy_last_kind].rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			_destroy = ((destroy_last_kind >= 3) ? 1 : 2);
			clone_destroy[destroy_last_kind].GetComponent<Mon_Destroy>().TextureChange(_tex, _scale, _destroy);
			clone_destroy[destroy_last_kind].gameObject.active = true;
			destroy_last_kind = (destroy_last_kind + 1) % 6;
			break;
		default:
			grappling++;
			break;
		}
		enemykill++;
		if (script_IngameUI.GetExp(_destroy))
		{
			clone_soul[soulcount].gameObject.active = true;
			clone_soul[soulcount].position = _pos;
			soulcount = (soulcount + 1) % 3;
		}
	}

	public void CallGeneral(bool _general, int _hp, int _maxhp)
	{
		if (!_general)
		{
			general_portrait.GetComponent<Renderer>().material.mainTexture = prt_general;
			if (g_grade >= 2)
			{
				general_portrait.GetChild(0).gameObject.active = false;
			}
		}
		else
		{
			general_portrait.GetComponent<Renderer>().material.mainTexture = prt_cha;
			if (g_grade >= 2)
			{
				general_portrait.GetChild(0).gameObject.active = true;
			}
		}
		if (cur_difficulty != 2)
		{
			g_hp_length = (1f - (float)_hp / (float)_maxhp) * 0.125f;
			script_g_hpgauge.UvMove(Vector2.right * g_hp_length);
		}
	}

	public void SetBGM(float _vol)
	{
		base.GetComponent<AudioSource>().volume = _vol;
		PlayerPrefs.SetFloat("vol_bgm", _vol);
	}

	public void ChangeBGM(bool _boss)
	{
		if (_boss)
		{
			base.GetComponent<AudioSource>().clip = Resources.Load("bgm_boss") as AudioClip;
		}
		else
		{
			int num = Random.Range(1, 4);
			base.GetComponent<AudioSource>().clip = Resources.Load("bgm_stage" + num) as AudioClip;
		}
		base.GetComponent<AudioSource>().Play();
	}

	public void RegenStart()
	{
		monnum = 0;
		stagefinish = false;
		bosscount = script_stageDB.st[infinity_stage_index]._bosscount;
		if (bosscount > 0)
		{
			bossindex[0] = script_stageDB.st[infinity_stage_index]._boss1;
			bossindex[1] = script_stageDB.st[infinity_stage_index]._boss2;
			bossindex[2] = script_stageDB.st[infinity_stage_index]._boss3;
		}
		totalEnemyNum = WAVEEMEMYNUM;
		regen = 0;
		enemyset[0] = enemy_source[script_stageDB.st[infinity_stage_index]._basemon1];
		enemyset[1] = enemy_source[script_stageDB.st[infinity_stage_index]._basemon2];
		enemyset[2] = enemy_source[script_stageDB.st[infinity_stage_index]._mainmon];
	}

	public void EnemyChange()
	{
		if (set_spawndelay > 0.5f)
		{
			set_spawndelay -= 0.3f;
		}
		if (wave < 5)
		{
			wave++;
		}
		for (int i = 0; i < 3; i++)
		{
			int num = wave + i + cur_stage_index % 10 - 1;
			enemyset[i] = enemy_source[num];
		}
	}

	public void ChangeBoss(Vector3 _pos, Quaternion _rot)
	{
		clone_Boss = (Transform)Object.Instantiate(boss[0], _pos, _rot);
		clone_Boss.GetComponent<AI_Boss01>().SetLevel(cur_stage_index, restrict_Factor);
	}

	public void BossAppear(short _kind)
	{
		rndpos = SetRndPoint();
		clone_Boss = (Transform)Object.Instantiate(boss[_kind], rndpos, Quaternion.identity);
		clone_Boss.GetComponent<AI_Boss01>().SetLevel(cur_stage_index, restrict_Factor);
		monnum++;
		countdown = true;
	}

	public void BossCutin(int _enemykind)
	{
		cut_boss.gameObject.active = true;
		cut_boss.GetComponent<Cutin_BossTexture>().SetCutinTexture(_enemykind);
		cut_boss.gameObject.active = true;
		cut_boss.GetComponent<Cutin01>().CutinOn(new Vector3(3f, 2f, 1f), new Vector3(0.5f, 2f, 1f), new Vector3(7f, 7f, 0f), 0.1f, 2.4f, 2.5f, cinema: true);
		ChangeBGM(_boss: true);
	}

	public void BossKill_Cheat()
	{
		if (clone_Boss != null)
		{
			Object.Destroy(clone_Boss.gameObject);
			monnum--;
		}
	}

	public void Summon(int _amount, Vector3 _summonpos)
	{
		if (summon_amount <= 0)
		{
			summon_amount = _amount;
			summonpos = _summonpos;
			InvokeRepeating("Summon_p", 0.1f, 0.5f);
		}
	}

	private void Summon_p()
	{
		if (monnum > 0)
		{
			Vector3 position = summonpos + Random.onUnitSphere * 0.3f;
			position[1] = 0f;
			pt_summonfog.position = position;
			pt_summonfog.GetComponent<ParticleSystem>().Play();
			spawnenemy = enemyset[Random.Range(0, 2)];
			clone_enemy = (Transform)Object.Instantiate(spawnenemy, position, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			clone_enemy.name = "enemy";
			clone_enemy.GetComponent<AI_Enemy01>().SetLevel(cur_stage_index, play_kind, _issummon: true, restrict_Factor);
			monnum++;
			summon_amount--;
		}
		else
		{
			summon_amount = 0;
			CancelInvoke("Summon_p");
		}
		if (summon_amount <= 0)
		{
			summon_amount = 0;
			CancelInvoke("Summon_p");
		}
	}

	public void BossKill(int _enemykind)
	{
		if (_enemykind == 0)
		{
			bosskill[9]++;
		}
		else
		{
			bosskill[_enemykind]++;
		}
		PlayerPrefsX.SetIntArray("bosskill", bosskill);
		if (!infinitymode && bosscount <= 0)
		{
			bossremain--;
			if (bossremain <= 0)
			{
				script_IngameUI.WaveSet(100);
			}
		}
		ChangeBGM(_boss: false);
	}

	public void FinalWave()
	{
		script_IngameUI.WaveSet(finalstage - 1);
		wave = finalstage;
	}

	public void GeneralHP(bool _isstart)
	{
		if (cur_general != -1)
		{
			int num = TimeControl.SubtractDelay(1);
			for (int i = 0; i < 12; i++)
			{
				general_hp[i] += (int)((float)num * 0.2f);
			}
			if (!_isstart)
			{
				general_hp[cur_general] = script_cha.g_hp;
			}
			PlayerPrefsX.SetIntArray("n33", general_hp);
			TimeControl.SetDelay(1);
		}
	}

	public void GeneralDead()
	{
		if (g_grade >= 2)
		{
			general_portrait.GetChild(0).gameObject.active = false;
		}
		GeneralHP(_isstart: false);
		if (cur_difficulty != 2)
		{
			g_hpgauge.gameObject.active = false;
		}
		icon_dead.gameObject.active = true;
	}

	public void RegenAlly()
	{
		if (allycount == 0)
		{
			if (c_tank.position.y >= 10f)
			{
				c_tank.position = Vector3.forward * -0.5f;
				c_tank.gameObject.active = true;
			}
		}
		else if (allycount < 8)
		{
			negativefactor *= -1;
			Object.Instantiate(plane_staff, Vector3.right * Random.Range(0.1f, 0.3f) * negativefactor - Vector3.forward * 0.5f, Quaternion.identity);
		}
		else if (allycount > 12)
		{
			allycount = -1;
		}
		allycount++;
	}

	private void Update()
	{
		if (regen == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				rndpos = SetRndPoint();
				spawnenemy = enemyset[i];
				clone_enemy = (Transform)Object.Instantiate(spawnenemy, rndpos, Quaternion.identity);
				clone_enemy.name = "enemy";
				clone_enemy.GetComponent<AI_Enemy01>().SetLevel(cur_stage_index, play_kind, infinitymode, restrict_Factor);
				if (infinitymode)
				{
					pt_summonfog.position = rndpos;
					pt_summonfog.GetComponent<ParticleSystem>().Play();
				}
				monnum++;
				totalEnemyNum--;
			}
			regen = 1;
			countdown = false;
		}
		else if (regen > 0)
		{
			if (spawndelay > 0f)
			{
				spawndelay -= Time.deltaTime;
			}
			else if (totalEnemyNum > 0)
			{
				if (monnum < maxEnemyNum)
				{
					rndpos = SetRndPoint();
					enemykind = Random.Range(0, 100);
					if (enemykind < 45)
					{
						enemykind = 0;
					}
					else if (enemykind < 80)
					{
						enemykind = 1;
					}
					else
					{
						enemykind = 2;
					}
					spawnenemy = enemyset[enemykind];
					clone_enemy = (Transform)Object.Instantiate(spawnenemy, rndpos, Quaternion.identity);
					clone_enemy.name = "enemy";
					clone_enemy.GetComponent<AI_Enemy01>().SetLevel(cur_stage_index, play_kind, infinitymode, restrict_Factor);
					if (infinitymode)
					{
						pt_summonfog.position = rndpos;
						pt_summonfog.GetComponent<ParticleSystem>().Play();
					}
					monnum++;
					totalEnemyNum--;
					spawndelay = set_spawndelay;
				}
			}
			else
			{
				if (totalEnemyNum > 0)
				{
					return;
				}
				if (wave >= finalstage)
				{
					if (bosscount > 0)
					{
						spawndelay = 1f;
						BossAppear(bossindex[script_stageDB.st[infinity_stage_index]._bosscount - bosscount]);
						bosscount--;
						if (bosscount <= 0)
						{
							regen = -2;
						}
					}
					else
					{
						regen = -2;
					}
				}
				else if (infinitymode)
				{
					if (bosscount > 0)
					{
						BossAppear(bossindex[script_stageDB.st[infinity_stage_index]._bosscount - bosscount]);
						bosscount--;
						if (bosscount <= 0)
						{
							cur_stage_index += 3;
							infinity_stage_index = (short)(cur_stage_index % 90);
							regen = -2;
						}
					}
					else
					{
						cur_stage_index += 3;
						infinity_stage_index = (short)(cur_stage_index % 90);
						regen = -2;
					}
				}
				else
				{
					regen = -2;
				}
			}
		}
		else if (monnum <= 5 && !countdown)
		{
			for (int j = 0; j < monnum; j++)
			{
				last_mon = GameObject.Find("enemy");
				last_mon.GetComponent<AI_Enemy01>().CountDown();
				last_mon.name = "enemy_confirm";
			}
			countdown = true;
		}
	}
}
