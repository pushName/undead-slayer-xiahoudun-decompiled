using UnityEngine;

public class UI_map : MonoBehaviour
{
	private const int MAXSTAGE = 90;

	private const int MAXPET = 2;

	private const int MAXWORK = 2;

	private const int MAXGENERAL = 12;

	private const int SLOTPRICE = 3;

	private const int MAXRANKING = 10;

	public GUISkin basicSkin;

	public Texture tex_eventicon;

	public Texture2D pop_blank;

	public Texture2D pop_blank2;

	public Texture2D pop_stage;

	public Texture2D icon_clear;

	public Texture2D bg_option;

	public Texture2D ico_slide;

	public Texture2D ico_warn;

	public Texture2D ico_new;

	public Texture2D please_touch;

	public Texture2D bg_mission;

	public Texture2D bg_mission_clear;

	public Texture2D mission_lock;

	public Texture2D bt_lockmenu;

	public Texture2D[] icon_mission = new Texture2D[3];

	public Texture2D bg_titlename;

	public Texture2D icon_coin;

	public Texture2D icon_jade;

	private Texture2D icon_treasure;

	private Texture2D prt_general;

	private Texture2D prt_general_get;

	private Texture2D[] skillicon = new Texture2D[5];

	public Texture2D titlebase;

	public Texture2D titlebase3;

	public Texture2D hpbase;

	public Texture2D txt_start;

	public Texture2D bg_general;

	public Texture2D star_grade;

	public Texture2D black_all;

	public Texture2D black;

	public Texture2D gauge_hp;

	public Texture2D gauge_exp;

	public Texture2D bg_asset;

	public Texture2D bg_asset_lv;

	public Texture2D bg_asset_key;

	public Texture2D[] general_weapon = new Texture2D[5];

	public Texture2D[] icon_stagekind = new Texture2D[5];

	public Texture2D icon_key;

	public Texture2D img_skill_slot;

	public Texture2D[] icon_explorekind = new Texture2D[2];

	public Texture2D bg_red;

	public Texture2D empty;

	public Texture2D txt_story;

	public Texture2D txt_loading;

	public Texture2D gift_title;

	public Texture2D gift_five_percnt;

	public Texture2D gift_skill;

	public Texture2D gift_coin;

	public Texture2D gift_bg_item;

	public Texture2D gift_skill_bg;

	public Texture2D[] img_menu = new Texture2D[5];

	public Transform icon_ef;

	public Transform icon_active;

	public Transform icon_mainbattle;

	public Transform[] icon_mainbattle_clear = new Transform[3];

	public Transform sound_dummy;

	public Transform ef_creaticon;

	public Transform ef_breakicon;

	public Transform bg_popui;

	public Transform ani_walk;

	private Transform bg_worldmap;

	public GUIStyle bt_stagestart;

	public GUIStyle bt_menu;

	public GUIStyle pausemenu;

	public GUIStyle bar_slide;

	public GUIStyle bt_yesno;

	public GUIStyle sel_lang;

	public GUIStyle bt_explore;

	public GUIStyle bt_sqbox;

	public GUIStyle bt_general;

	public GUIStyle bt_general_small;

	public GUIStyle bt_empty;

	public GUIStyle bt_bonus;

	public GUIStyle bt_gift;

	public GUIStyle bt_shop;

	private Transform c_battleicon;

	private Transform c_iconef;

	private Transform c_eficon_creat;

	private Transform c_eficon_break;

	private Transform sound_UI;

	private Transform uicam;

	private SoundEf_UI script_soundUI;

	private CamScroll script_cam;

	private Language_Name script_name;

	private General_Stat script_generalstat;

	private LoopArchive script_archive;

	private Vector3 cur_stage_iconpos;

	private Vector3 battleposition;

	private short stageon;

	private bool iconactive = true;

	private bool bosscutin;

	private bool pause;

	private bool option;

	private bool slideon;

	private bool slideon2;

	private bool warning_hungry;

	private bool warning_new;

	private bool newarchive;

	private bool newtreasure;

	private short zoom_total;

	private short transmode;

	private bool showAds;

	private int storycutin;

	private bool b_scenario;

	private bool exploreMode;

	private bool changescene;

	private short explore_progress;

	private short confirm;

	private short treasurekind;

	private int max_stage_index;

	private int on_stage_index;

	private int sel_stage_index;

	private int cur_general = -1;

	private int sel_general = -1;

	private string cur_stage_name;

	private int cur_stage_kind;

	private int cutinXpos = 480;

	private int getslot;

	private float cur_generalmaxhp = 1500f;

	private float trans_delay;

	private int explorekind;

	private int treasureindex;

	private int icon_size;

	private int jade;

	private int coin;

	private int key_cave;

	private int rnd_speech;

	private short explore_method;

	private int cur_difficulty;

	private int slot_general;

	private int language;

	private bool b_delay;

	private float f_delay;

	private float vol_bgm;

	private float vol_master;

	private Vector2 iconmove;

	private short general_index;

	private short general_kind;

	private short g_maxatk;

	private short g_def;

	private short g_maxhp;

	private short g_grade;

	private short g_level;

	private float g_atkspd;

	private int[] stage_clear_grade = new int[90];

	private int[] general_seed = new int[12];

	private int general_hp;

	private int general_cur_hp;

	private int extra_info;

	private int chalv;

	private int event_stage = -1;

	private float length_exp;

	private short menuopen = 1;

	private int[] menuarray = new int[5] { 1, 2, 3, 4, 6 };

	private int[] cur_workdelay = new int[2];

	private int[] pet_hunger = new int[2];

	private int[] skill_slot = new int[5];

	public Transform icon_folder;

	public Transform icon_explore;

	public Transform icon_dun;

	public Transform obj_scenario;

	public Transform total_collider;

	private Transform eventicon;

	private int play_kind;

	private int tutorial = -1;

	private int haveGiftBox;

	private GameObject cashshop;

	private Transform ranking;

	private int chaexp;

	private Rect poprect;

	private bool overui;

	private bool clickButton;

	private Vector2 v2;

	private void Awake()
	{
		vol_bgm = PlayerPrefs.GetFloat("vol_bgm");
		vol_master = PlayerPrefs.GetFloat("vol_master");
		AudioListener.volume = vol_master;
		TimeControl.InitStart();
		uicam = Camera.main.transform;
		script_cam = uicam.GetComponent<CamScroll>();
		poprect = Crypto.Rect2(112f, 204f, 256f, 100f);
		script_archive = GameObject.FindWithTag("Finish").GetComponent<LoopArchive>();
		haveGiftBox = PlayerPrefs.GetInt("haveDrawedGift", 0);
	}

	private void OnEnable()
	{
		key_cave = Crypto.Load_int_key("n10");
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
		if (explore_progress >= 2)
		{
			bg_popui.localScale = Vector3.one * 5.2f;
			bg_popui.localPosition = new Vector3(0f, -0.24f, 0.5f);
		}
		else
		{
			script_cam.EnableMove();
		}
	}

	private void Start()
	{
		int num = Crypto.Load_int_key("n58");
		int num2 = Crypto.Load_int_key("n59");
		if (num != 0)
		{
			Crypto.Property_change(num, _jade: true);
			Crypto.Save_int_key("n58", 0);
		}
		else if (num2 != 0)
		{
			Crypto.Property_change(num2, _jade: false);
			Crypto.Save_int_key("n59", 0);
		}
		GameObject gameObject = GameObject.Find("bg_worldmap");
		if (gameObject == null)
		{
			GameObject gameObject2 = Resources.Load("bg_worldmap") as GameObject;
			bg_worldmap = (Transform)Object.Instantiate(gameObject2.transform, Vector3.zero, Quaternion.identity);
			bg_worldmap.name = "bg_worldmap";
		}
		else
		{
			bg_worldmap = gameObject.transform;
		}
		stage_clear_grade = PlayerPrefsX.GetIntArray("n15");
		script_name = GetComponent<Language_Name>();
		script_generalstat = GetComponent<General_Stat>();
		language = PlayerPrefs.GetInt("language");
		tutorial = Crypto.Load_int_key("tutorial");
		chalv = Crypto.Load_int_key("n47");
		chaexp = Crypto.Load_int_key("n11");
		skill_slot = PlayerPrefsX.GetIntArray("skill_slot");
		slot_general = Crypto.Load_int_key("n09");
		pet_hunger = PlayerPrefsX.GetIntArray("n25");
		max_stage_index = Crypto.Load_int_key("n06");
		storycutin = Crypto.Load_int_key("storycutin");
		cur_general = Crypto.Load_int_key("cur_general");
		general_seed = PlayerPrefsX.GetIntArray("n13");
		if (cur_general >= 0)
		{
			int[] intArray = PlayerPrefsX.GetIntArray("n33");
			general_hp = intArray[cur_general];
			GeneralKindOnly();
			CurGeneralStat(_start: true);
		}
		length_exp = (float)chaexp / (float)(chalv * 100) * 50f;
		for (int i = 0; i < 5; i++)
		{
			if (skill_slot[i] >= 0)
			{
				skillicon[i] = Resources.Load("sk_" + (skill_slot[i] + 1)) as Texture2D;
			}
		}
		if (max_stage_index > 6)
		{
			for (int j = 0; j < 2; j++)
			{
				if (pet_hunger[j] == 0)
				{
					warning_hungry = true;
				}
			}
		}
		if (GameObject.FindWithTag("sound") == null)
		{
			sound_UI = (Transform)Object.Instantiate(sound_dummy, Vector3.zero, Quaternion.identity);
		}
		else
		{
			sound_UI = GameObject.FindWithTag("sound").transform;
		}
		if (storycutin > 0)
		{
			Invoke("SceneStart_Delay", 1f);
			b_scenario = true;
			script_cam.DisableMove();
		}
		else if (max_stage_index >= 1)
		{
			newarchive = script_archive.FindArchive();
		}
		event_stage = Crypto.Load_int_key("n37");
		if (tutorial == 5)
		{
			event_stage = 2;
		}
		short num3 = 0;
		int num4 = 0;
		for (int k = 0; k < 90; k++)
		{
			battleposition = icon_folder.GetChild(k).position + Vector3.forward * 0.2f + Vector3.up * 0.09f;
			if (k == max_stage_index)
			{
				icon_folder.GetChild(max_stage_index).GetComponent<Collider>().enabled = true;
				c_battleicon = (Transform)Object.Instantiate(icon_mainbattle, battleposition - Vector3.up * 0.08f, Quaternion.identity);
				c_battleicon.parent = icon_folder;
				c_iconef = (Transform)Object.Instantiate(icon_ef, battleposition + new Vector3(0f, -0.1f, -0.3f), Quaternion.identity);
				c_iconef.parent = icon_folder;
				icon_folder.GetChild(max_stage_index).localScale = Vector3.one;
			}
			else if (k == event_stage)
			{
				eventicon = icon_folder.GetChild(event_stage);
				eventicon.GetComponent<Renderer>().material.mainTexture = tex_eventicon;
				eventicon.localScale = Vector3.one * 1.4f;
				eventicon.GetComponent<Collider>().enabled = true;
			}
			else if (k < max_stage_index)
			{
				icon_folder.GetChild(k).GetComponent<Collider>().enabled = true;
				if (stage_clear_grade[k] > 0)
				{
					if (stage_clear_grade[k] != 111)
					{
						num3 = ((stage_clear_grade[k] > 1) ? ((short)1) : ((short)0));
					}
					else
					{
						num4++;
						num3 = 2;
					}
					c_battleicon = (Transform)Object.Instantiate(icon_mainbattle_clear[num3], battleposition, Quaternion.identity);
					c_battleicon.parent = icon_folder;
				}
				icon_folder.GetChild(k).localScale = Vector3.one;
			}
			else
			{
				icon_folder.GetChild(k).localScale = Vector3.zero;
				icon_folder.GetChild(k).GetComponent<Collider>().enabled = false;
			}
		}
		Crypto.Save_int_key("perfectplay", num4);
		SetMenuOpen();
		script_soundUI = sound_UI.GetComponent<SoundEf_UI>();
		newtreasure = script_archive.FindTreasure();
		InvokeRepeating("Warning_iconsize", 1f, 0.34f);
		SetServerTime();
		InvokeRepeating("SetServerTime", 29f, 30f);
		AndroidScript.i.showCCPlayAndForce();
		PlayerPrefs.SetInt("currentSence", 1);
	}

	public void jumpSence(int type)
	{
		if (type == 1)
		{
			Crypto.Save_int_key("gamemode", 1);
			Object.Destroy(bg_worldmap.gameObject);
			changescene = true;
			Application.LoadLevel("Extreme");
		}
	}

	public void CurGeneralStat(bool _start)
	{
		if (_start)
		{
			if (cur_general != -1)
			{
				script_generalstat.SetGeneral(general_seed[cur_general]);
				cur_generalmaxhp = script_generalstat.g_maxhp;
			}
		}
		else
		{
			cur_generalmaxhp = g_maxhp;
		}
	}

	public void GeneralKindOnly()
	{
		int num = general_seed[cur_general] % 10000000 / 100000;
		prt_general = Resources.Load("prt_general" + (num + 1)) as Texture2D;
	}

	public void GeneralStat(int _seed, bool _exist)
	{
		script_generalstat.SetGeneral(_seed);
		general_index = script_generalstat.general_index;
		general_kind = script_generalstat.general_kind;
		g_maxatk = script_generalstat.g_maxatk;
		g_def = script_generalstat.g_def;
		g_maxhp = script_generalstat.g_maxhp;
		g_atkspd = script_generalstat.g_atkspd;
		g_grade = script_generalstat.g_grade;
		g_level = script_generalstat.g_level;
		if (!_exist)
		{
			int num = general_seed[getslot] % 10000000 / 100000;
			prt_general_get = Resources.Load("prt_general" + (num + 1)) as Texture2D;
			int[] intArray = PlayerPrefsX.GetIntArray("n33");
			intArray[getslot] = g_maxhp;
			PlayerPrefsX.SetIntArray("n33", intArray);
			int[] intArray2 = PlayerPrefsX.GetIntArray("staff");
			intArray2[general_index] = Mathf.Max(intArray2[general_index], g_grade + 1);
			PlayerPrefsX.SetIntArray("staff", intArray2);
		}
	}

	private void SetServerTime()
	{
		TimeControl.RequestServerTime();
		DelayWorkTime();
	}

	public void DelayWorkTime()
	{
		for (int i = 0; i < 2; i++)
		{
			cur_workdelay[i] = TimeControl.SubtractDelay(i);
		}
		general_cur_hp = general_hp + (int)((float)cur_workdelay[1] * 0.2f);
		general_cur_hp = Mathf.Min(general_cur_hp, (int)cur_generalmaxhp);
	}

	public void Zoom_finger(bool _zoomout, Vector3 _pos)
	{
		if (_zoomout)
		{
		}
		stageon = 0;
		script_cam.Zoom(_zoomout, _pos);
	}

	public void ExploreIconArray()
	{
		exploreMode = true;
		icon_folder.position = Vector3.one * 10f;
		icon_dun.position = Vector3.one * 20f;
		Zoom_finger(_zoomout: true, Vector3.zero);
		icon_explore.position = Vector3.zero;
	}

	public void ExploreIconFinish()
	{
		icon_folder.position = Vector3.zero;
		icon_dun.position = Vector3.zero;
		exploreMode = false;
		Zoom_finger(_zoomout: false, icon_folder.GetChild(max_stage_index).position);
		icon_explore.position = Vector3.one * 50f;
	}

	public void AniFinish(bool _success)
	{
		if (_success)
		{
			Delay(1.5f);
			explore_progress = 5;
			rnd_speech = Random.Range(92, 96);
		}
		else
		{
			explore_progress = 7;
		}
	}

	public void SetStage(int index, int kind, Vector3 pos, int _play)
	{
		if (tutorial == 5 && !exploreMode && event_stage != index)
		{
			return;
		}
		cur_difficulty = 0;
		sel_stage_index = index;
		cur_stage_kind = kind;
		cur_stage_iconpos = pos;
		play_kind = _play;
		if (sel_stage_index != 100)
		{
			if (exploreMode)
			{
				cur_stage_name = script_name.txt_name[language, 66 + sel_stage_index];
				explore_progress = 1;
			}
			else if (event_stage == index)
			{
				cur_stage_name = script_name.txt_name[language, 64];
				play_kind = 4;
				stageon = 2;
			}
			else if (tutorial < 0)
			{
				cur_stage_name = script_name.txt_name[language, 61];
				switch (play_kind)
				{
				case 5:
					cur_stage_name = script_name.txt_name[language, 65];
					break;
				case 6:
					cur_stage_name = script_name.txt_name[language, 62];
					break;
				case 7:
					cur_stage_name = script_name.txt_name[language, 63];
					break;
				}
				stageon = 1;
			}
		}
		else
		{
			changescene = true;
			Object.Destroy(bg_worldmap.gameObject);
			Crypto.Save_int_key("gamemode", 1);
			Application.LoadLevel("Extreme");
		}
	}

	public bool GetGeneral()
	{
		int num = Crypto.Load_int_key("generalsearch");
		num++;
		Crypto.Save_int_key("generalsearch", num);
		int num2 = Random.Range(0, 1000);
		int num3 = 0;
		if (tutorial == 4)
		{
			num3 = ((general_seed[1] != 0) ? 10020123 : 12610123);
		}
		else
		{
			if (explore_method == 1)
			{
				num2 = ((num2 > 990) ? 4 : ((num2 <= 850) ? 2 : 3));
			}
			else if (explore_method == 0)
			{
				if (num2 > 970)
				{
					num2 = 2;
				}
				else if (num2 > 770)
				{
					num2 = 1;
				}
				else
				{
					if (num2 <= 200)
					{
						return false;
					}
					num2 = 0;
				}
			}
			int num4 = Random.Range(0, 90);
			num4 = ((explore_method == 1) ? (num4 % 20) : ((num2 < 2) ? (num4 % 30) : (num4 % 20)));
			num4 *= 100000;
			num3 = Random.Range(0, 10000) + num2 * 10000 + 10000000 + num4;
		}
		for (int i = 0; i < slot_general; i++)
		{
			if (general_seed[i] == 0)
			{
				getslot = i;
				general_seed[i] = num3;
				PlayerPrefsX.SetIntArray("n13", general_seed);
				GeneralStat(num3, _exist: false);
				break;
			}
		}
		newarchive = script_archive.FindArchive();
		return true;
	}

	public void GetTreasure()
	{
		int num = Random.Range(0, 100);
		if (num < 55)
		{
			treasurekind = 1;
			treasureindex = Random.Range(0, 24);
			icon_treasure = Resources.Load("treasure" + (treasureindex + 1)) as Texture2D;
			int[] intArray = PlayerPrefsX.GetIntArray("treasure");
			intArray[treasureindex]++;
			PlayerPrefsX.SetIntArray("treasure", intArray);
			explore_progress = 4;
			ani_walk.GetComponent<Ef_ani_walk>().AniStart(4);
			newtreasure = script_archive.FindTreasure();
		}
		else if (num < 57)
		{
			treasurekind = 2;
			treasureindex = 1;
			jade += treasureindex;
			Crypto.Property_change(1, _jade: true);
			explore_progress = 4;
			ani_walk.GetComponent<Ef_ani_walk>().AniStart(4);
		}
		else if (num < 70)
		{
			treasurekind = 3;
			treasureindex = 1;
			key_cave++;
			Crypto.Save_int_key("n10", key_cave);
			explore_progress = 4;
			ani_walk.GetComponent<Ef_ani_walk>().AniStart(4);
		}
		else
		{
			explore_progress = 8;
			ani_walk.gameObject.active = false;
		}
	}

	public void Delay(float t)
	{
		b_delay = true;
		f_delay = t;
	}

	public void ScenarioFinish(int _txt)
	{
		b_scenario = false;
		extra_info = _txt;
		if (storycutin == 1)
		{
			transmode = 1;
		}
		else
		{
			battleposition = icon_folder.GetChild(0).position + Vector3.forward * 0.2f + Vector3.up * 0.09f;
			icon_folder.GetChild(0).GetComponent<Collider>().enabled = true;
			c_battleicon = (Transform)Object.Instantiate(icon_mainbattle, battleposition - Vector3.up * 0.08f, Quaternion.identity);
			icon_folder.GetChild(0).localScale = Vector3.one;
			script_cam.MoveTarget(icon_folder.GetChild(0), _isevent: false);
			c_iconef = (Transform)Object.Instantiate(icon_ef, battleposition + new Vector3(0f, -0.1f, -0.3f), Quaternion.identity);
			transmode = 0;
			script_cam.EnableMove();
			max_stage_index++;
			Crypto.Save_int_key("n06", max_stage_index);
			Crypto.Save_int_key("storycutin", 0);
			SetMenuOpen();
			confirm = 14;
			Delay(2f);
		}
		storycutin = 0;
	}

	private void SceneStart_Delay()
	{
		obj_scenario.GetComponent<Scenario>().SceneStart(max_stage_index + 1);
	}

	public void SetMenuOpen()
	{
		if (max_stage_index >= 7)
		{
			menuopen = 5;
			warning_new = false;
		}
		else if (max_stage_index >= 6)
		{
			menuopen = 5;
			warning_new = true;
		}
		else if (max_stage_index >= 4)
		{
			menuopen = 4;
			warning_new = false;
		}
		else if (max_stage_index >= 3)
		{
			menuopen = 4;
			warning_new = false;
		}
		else if (max_stage_index >= 2)
		{
			menuopen = 3;
			warning_new = true;
		}
		else if (max_stage_index >= 1)
		{
			menuopen = 2;
			warning_new = false;
		}
		else
		{
			menuopen = 1;
			warning_new = false;
		}
	}

	private void Update()
	{
		int num = PlayerPrefs.GetInt("updateJAndG");
		if (num == 1)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetInt("updateJAndG", 0);
		}
		if (b_delay)
		{
			f_delay -= Time.deltaTime;
			if (f_delay <= 0f)
			{
				b_delay = false;
				f_delay = 0f;
			}
		}
		if (bosscutin)
		{
			cutinXpos = Mathf.Max(224, (int)((float)cutinXpos - Time.deltaTime * 800f));
		}
		if (!b_scenario)
		{
			if (transmode > 0)
			{
				if (trans_delay < 1f)
				{
					trans_delay += Time.deltaTime;
				}
				else
				{
					trans_delay = 0f;
					transmode++;
				}
				if (transmode == 2)
				{
					battleposition = icon_folder.GetChild(max_stage_index).position + Vector3.forward * 0.2f + Vector3.up * 0.09f;
					Object.Destroy(c_battleicon.gameObject);
					Object.Destroy(c_iconef.gameObject);
					short num2 = 0;
					num2 = ((stage_clear_grade[max_stage_index] == 111) ? ((short)2) : ((stage_clear_grade[max_stage_index] > 1) ? ((short)1) : ((short)0)));
					c_battleicon = (Transform)Object.Instantiate(icon_mainbattle_clear[num2], battleposition, Quaternion.identity);
					c_battleicon.parent = icon_folder;
					c_eficon_break = (Transform)Object.Instantiate(ef_breakicon, battleposition, Quaternion.identity);
					Object.Destroy(c_eficon_break.gameObject, 1f);
					max_stage_index++;
					Crypto.Save_int_key("storycutin", 0);
					Crypto.Save_int_key("n06", max_stage_index);
					if (max_stage_index >= 1)
					{
						newarchive = script_archive.FindArchive();
					}
					if (max_stage_index >= 90)
					{
						transmode = 0;
						Object.Destroy(sound_UI.gameObject);
						Object.Destroy(bg_worldmap.gameObject);
						Application.LoadLevel("Ending");
						return;
					}
					script_cam.MoveTarget(icon_folder.GetChild(max_stage_index), _isevent: false);
					SetMenuOpen();
					transmode = 3;
				}
				else if (transmode == 3)
				{
					battleposition = icon_folder.GetChild(max_stage_index).position + Vector3.forward * 0.2f + Vector3.up * 0.09f;
					c_eficon_creat = (Transform)Object.Instantiate(ef_creaticon, battleposition, Quaternion.identity);
					Object.Destroy(c_eficon_creat.gameObject, 1f);
					transmode = 4;
				}
				else if (transmode == 5)
				{
					battleposition = icon_folder.GetChild(max_stage_index).position + Vector3.forward * 0.2f + Vector3.up * 0.09f;
					icon_folder.GetChild(max_stage_index).GetComponent<Collider>().enabled = true;
					c_battleicon = (Transform)Object.Instantiate(icon_mainbattle, battleposition - Vector3.up * 0.08f, Quaternion.identity);
					c_battleicon.parent = icon_folder;
					icon_folder.GetChild(max_stage_index).localScale = Vector3.one;
					c_iconef = (Transform)Object.Instantiate(icon_ef, battleposition + new Vector3(0f, -0.1f, -0.3f), Quaternion.identity);
					c_iconef.parent = icon_folder;
					transmode = 0;
					script_cam.EnableMove();
					if (extra_info > 0 && extra_info < 1000)
					{
						confirm = 14;
						Delay(2f);
					}
				}
			}
			else
			{
				if (pause)
				{
					return;
				}
				if (Input.touchCount == 2 && !exploreMode)
				{
					float num3 = 0f;
					float num4 = 0f;
					if (Input.GetTouch(1).phase == TouchPhase.Began)
					{
						num3 = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
					}
					else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
					{
						num4 = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
						if (Mathf.Abs(num4 - num3) > 1.5f)
						{
							zoom_total = 1;
							Zoom_finger(_zoomout: true, Vector3.one);
						}
					}
				}
				else if (zoom_total == 1)
				{
					if (Input.GetMouseButtonDown(0))
					{
						zoom_total = 2;
					}
				}
				else if (Input.GetMouseButtonUp(0))
				{
					if (script_cam.drag)
					{
						return;
					}
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hitInfo;
					if (zoom_total == 2)
					{
						total_collider.gameObject.active = true;
						Physics.Raycast(ray, out hitInfo);
						Zoom_finger(_zoomout: false, hitInfo.point);
						zoom_total = 0;
						total_collider.gameObject.active = false;
					}
					else if (Physics.Raycast(ray, out hitInfo) && iconactive)
					{
						Transform transform = hitInfo.transform;
						transform.GetComponent<Icon_Stage>().IconDown();
						iconactive = false;
					}
					else
					{
						if (stageon == 1 && overui)
						{
							return;
						}
						iconactive = true;
						stageon = 0;
					}
				}
			}
		}
		if (stageon > 0)
		{
			icon_active.position = cur_stage_iconpos;
		}
		else
		{
			icon_active.position = Vector3.forward * -1f;
		}
		haveGiftBox = PlayerPrefs.GetInt("haveDrawedGift", 0);
		if (haveGiftBox == 1)
		{
			coin = Crypto.Load_int_key("n17");
		}
	}

	public void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
	}

	public void CashshopOpen()
	{
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
		script_cam.DisableMove();
		bg_popui.localScale = Vector3.zero;
	}

	private void OnGUI()
	{
		if (b_scenario || transmode > 0)
		{
			return;
		}
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		if (zoom_total > 0)
		{
			return;
		}
		if (confirm > 0)
		{
			iconactive = false;
			GUI.enabled = false;
		}
		GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
		GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
		GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
		GUI.DrawTexture(Crypto.Rect2(50f, 0f, 64f, 32f), bg_asset_lv);
		GUI.Label(Crypto.Rect2(52f, 3f, 80f, 14f), string.Empty + chalv, "txt12_w");
		GUI.DrawTexture(Crypto.Rect2(57f, 20f, length_exp, 3f), gauge_exp);
		if (option)
		{
			if (slideon)
			{
				vol_bgm = (Input.mousePosition.x * (480f / (float)Screen.width) - 214f) / 126f;
				vol_bgm = Mathf.Clamp(vol_bgm, 0f, 1f);
			}
			else if (slideon2)
			{
				vol_master = (Input.mousePosition.x * (480f / (float)Screen.width) - 214f) / 126f;
				vol_master = Mathf.Clamp(vol_master, 0f, 1f);
			}
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), black);
			GUI.DrawTexture(Crypto.Rect2(112f, 85f, 256f, 256f), bg_option);
			GUI.Label(Crypto.Rect2(128f, 108f, 70f, 16f), Language.intxt[language, 302], "txt12_w");
			GUI.Label(Crypto.Rect2(128f, 143f, 70f, 16f), Language.intxt[language, 244], "txt12_w");
			GUI.Label(Crypto.Rect2(128f, 180f, 70f, 16f), Language.intxt[language, 245], "txt12_w");
			if (GUI.Button(Crypto.Rect2(212f, 172f, 128f, 32f), Language.intxt[language, 246], sel_lang))
			{
				short num = 2;
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				language = (language + 1) % num;
				PlayerPrefs.SetInt("language", language);
			}
			if (GUI.RepeatButton(Crypto.Rect2(200f, 101f, 152f, 32f), string.Empty, bar_slide))
			{
				slideon2 = true;
			}
			else if (slideon2)
			{
				AudioListener.volume = vol_master;
				slideon2 = false;
			}
			GUI.DrawTexture(Crypto.Rect2(vol_master * 126f + 206f, 109f, 16f, 16f), ico_slide);
			if (GUI.RepeatButton(Crypto.Rect2(200f, 136f, 152f, 32f), string.Empty, bar_slide))
			{
				slideon = true;
			}
			else if (slideon)
			{
				script_soundUI.SetBGM(vol_bgm);
				slideon = false;
			}
			GUI.DrawTexture(Crypto.Rect2(vol_bgm * 126f + 206f, 144f, 16f, 16f), ico_slide);
			if (GUI.Button(Crypto.Rect2(208f, 210f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				option = false;
				PlayerPrefs.SetFloat("vol_bgm", vol_bgm);
				PlayerPrefs.SetFloat("vol_master", vol_master);
			}
		}
		else if (pause)
		{
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), black);
			stageon = 0;
			if (GUI.Button(Crypto.Rect2(112f, 80f, 256f, 64f), Language.intxt[language, 30], pausemenu))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				pause = false;
				Input.multiTouchEnabled = true;
				script_cam.EnableMove();
			}
			if (GUI.Button(Crypto.Rect2(112f, 140f, 256f, 64f), Language.intxt[language, 31], pausemenu))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				option = true;
			}
			if (GUI.Button(Crypto.Rect2(112f, 200f, 256f, 64f), Language.intxt[language, 32], pausemenu))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				pause = false;
				changescene = true;
				Application.LoadLevel("Intro");
				PlayerPrefsX.SetVector3("uiCampos", uicam.position);
				iconactive = false;
				Object.Destroy(sound_UI.gameObject);
				Object.Destroy(bg_worldmap.gameObject);
			}
		}
		else if (exploreMode)
		{
			if (explore_progress == 0)
			{
				GUI.DrawTexture(Crypto.Rect2(112f, 250f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(112f, 260f, 256f, 14f), Language.intxt[language, 282], "txt12_0");
				if (GUI.Button(Crypto.Rect2(208f, 276f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					ExploreIconFinish();
					stageon = 0;
				}
			}
			else if (explore_progress == 1)
			{
				iconactive = true;
				GUI.DrawTexture(Crypto.Rect2(112f, 234f, 256f, 64f), pop_blank);
				GUI.DrawTexture(Crypto.Rect2(130f, 242f, 220f, 18f), bg_titlename);
				GUI.Label(Crypto.Rect2(112f, 243f, 256f, 16f), string.Empty + cur_stage_name, "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(106f, 260f, 128f, 32f), icon_stagekind[cur_stage_kind - 1]);
				if (GUI.Button(Crypto.Rect2(218f, 260f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					explore_progress = 2;
					bg_popui.localScale = Vector3.one * 5.2f;
					bg_popui.localPosition = new Vector3(0f, -0.24f, 0.5f);
					icon_explore.position = Vector3.one * 50f;
				}
				else if (GUI.Button(Crypto.Rect2(286f, 260f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					explore_progress = 0;
					stageon = 0;
				}
			}
			else if (explore_progress == 2)
			{
				iconactive = false;
				GUI.DrawTexture(Crypto.Rect2(130f, 78f, 220f, 18f), bg_titlename);
				GUI.Label(Crypto.Rect2(112f, 79f, 256f, 16f), string.Empty + cur_stage_name, "txt12_w");
				if (explorekind == 1)
				{
					GUI.Label(Crypto.Rect2(120f, 106f, 240f, 16f), Language.intxt[language, 293], "txt12_0");
					if (GUI.Button(Crypto.Rect2(112f, 124f, 256f, 64f), Language.intxt[language, 284], pausemenu))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						if (tutorial == 4)
						{
							if (general_seed[1] != 0)
							{
								Delay(1f);
								confirm = 16;
							}
							else
							{
								explore_progress = 3;
								ani_walk.GetComponent<Ef_ani_walk>().AniStart(0);
								explore_method = 0;
							}
						}
						else if (coin >= 200)
						{
							if (Crypto.Property_change(-200, _jade: false))
							{
								coin -= 200;
								explore_progress = 3;
								ani_walk.GetComponent<Ef_ani_walk>().AniStart(0);
								explore_method = 0;
							}
						}
						else
						{
							Delay(1f);
							confirm = 8;
						}
					}
					else if (GUI.Button(Crypto.Rect2(112f, 176f, 256f, 64f), Language.intxt[language, 285], pausemenu))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						if (tutorial == 4)
						{
							if (general_seed[1] == 0)
							{
								Delay(1f);
								confirm = 16;
							}
							else
							{
								explore_progress = 3;
								ani_walk.GetComponent<Ef_ani_walk>().AniStart(0);
								explore_method = 1;
							}
						}
						else if (jade >= 5)
						{
							if (Crypto.Property_change(-5, _jade: true))
							{
								jade -= 5;
								ani_walk.GetComponent<Ef_ani_walk>().AniStart(0);
								explore_method = 1;
								explore_progress = 3;
								PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + 5 + ")\t" + Language.intxt[language, 285]);
							}
						}
						else
						{
							Delay(1f);
							confirm = 6;
						}
					}
					if (tutorial == 4)
					{
						GUI.Box(Crypto.Rect2(112f, 276f, 256f, 22f), Language.intxt[language, 387]);
						GUI.Box(Crypto.Rect2(158f, 146f, 38f, 20f), "0");
						GUI.Box(Crypto.Rect2(158f, 198f, 38f, 20f), "0");
					}
					else
					{
						GUI.Box(Crypto.Rect2(158f, 146f, 38f, 20f), "200");
						GUI.Box(Crypto.Rect2(158f, 198f, 38f, 20f), string.Empty + 5);
					}
					GUI.DrawTexture(Crypto.Rect2(140f, 148f, 16f, 16f), icon_coin);
					GUI.DrawTexture(Crypto.Rect2(140f, 200f, 16f, 16f), icon_jade);
					GUI.DrawTexture(Crypto.Rect2(284f, 148f, 16f, 16f), star_grade);
					GUI.DrawTexture(Crypto.Rect2(284f, 200f, 16f, 16f), star_grade);
					GUI.Label(Crypto.Rect2(286f, 148f, 64f, 16f), "1 ~ 3", "txt12_w");
					GUI.Label(Crypto.Rect2(286f, 200f, 64f, 16f), "3 ~ 5", "txt12_w");
				}
				else
				{
					GUI.Label(Crypto.Rect2(112f, 120f, 256f, 16f), Language.intxt[language, 292], "txt12_0");
					if (GUI.Button(Crypto.Rect2(112f, 150f, 256f, 64f), Language.intxt[language, 284], pausemenu))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						if (coin >= 100)
						{
							if (Crypto.Property_change(-100, _jade: false))
							{
								coin -= 100;
								explore_progress = 3;
								ani_walk.GetComponent<Ef_ani_walk>().AniStart(1);
							}
						}
						else
						{
							Delay(1f);
							confirm = 8;
						}
					}
					GUI.DrawTexture(Crypto.Rect2(140f, 174f, 16f, 16f), icon_coin);
					GUI.Box(Crypto.Rect2(158f, 172f, 38f, 20f), "100");
				}
				if (GUI.Button(Crypto.Rect2(208f, 234f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					explore_progress = 1;
					icon_explore.position = Vector3.zero;
					bg_popui.localScale = Vector3.zero;
				}
			}
			else if (explore_progress == 3)
			{
				iconactive = false;
				if (explorekind == 1)
				{
					GUI.Label(Crypto.Rect2(140f, 204f, 200f, 32f), Language.intxt[language, 187], "txt12_0");
				}
				else
				{
					GUI.Label(Crypto.Rect2(140f, 204f, 200f, 32f), Language.intxt[language, 283], "txt12_0");
				}
			}
			else if (explore_progress == 4)
			{
				iconactive = false;
				GUI.Label(Crypto.Rect2(112f, 204f, 256f, 16f), Language.intxt[language, 287], "txt12_0");
				switch (treasurekind)
				{
				case 1:
					GUI.DrawTexture(Crypto.Rect2(224f, 120f, 32f, 32f), icon_treasure);
					break;
				case 2:
					GUI.DrawTexture(Crypto.Rect2(224f, 120f, 32f, 32f), icon_jade);
					break;
				case 3:
					GUI.DrawTexture(Crypto.Rect2(224f, 120f, 32f, 32f), icon_key);
					break;
				case 4:
					GUI.DrawTexture(Crypto.Rect2(224f, 120f, 32f, 32f), icon_coin);
					break;
				}
				if (GUI.Button(Crypto.Rect2(208f, 230f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					ani_walk.gameObject.active = false;
					bg_popui.localScale = Vector3.zero;
					explore_progress = 0;
					ExploreIconFinish();
				}
			}
			else if (explore_progress == 5)
			{
				GUI.DrawTexture(Crypto.Rect2(196f, 100f, 88f, 88f), prt_general_get);
				GUI.Label(Crypto.Rect2(140f, 204f, 200f, 32f), script_name.txt_name[language, rnd_speech], "txt12_0");
				if (f_delay == 0f)
				{
					explore_progress = 6;
					ani_walk.gameObject.active = false;
				}
			}
			else if (explore_progress == 6)
			{
				iconactive = false;
				GUI.Label(Crypto.Rect2(112f, 86f, 256f, 16f), Language.intxt[language, 294], "txt12_0");
				GUI.DrawTexture(Crypto.Rect2(130f, 84f, 220f, 18f), bg_titlename);
				GUI.Label(Crypto.Rect2(130f, 84f, 220f, 16f), script_name.txt_name[language, 21 + general_index], "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(322f, 80f, 26f, 26f), general_weapon[general_kind]);
				GUI.DrawTexture(Crypto.Rect2(140f, 127f, 64f, 64f), prt_general_get);
				GUI.DrawTexture(Crypto.Rect2(140f, 112f, 64f, 110f), bg_general);
				GUI.Label(Crypto.Rect2(144f, 193f, 70f, 16f), " " + g_level + " / " + (g_grade + 1) * 20, "txt12_w");
				for (int i = 0; i < g_grade + 1; i++)
				{
					GUI.DrawTexture(Crypto.Rect2(166 - g_grade * 5 + i * 10, 114f, 12f, 12f), star_grade);
				}
				for (int j = 0; j < 3; j++)
				{
					GUI.DrawTexture(Crypto.Rect2(220f, 144 + j * 20, 128f, 16f), titlebase);
				}
				GUI.DrawTexture(Crypto.Rect2(220f, 204f, 128f, 16f), titlebase3);
				GUI.DrawTexture(Crypto.Rect2(220f, 112f, 128f, 32f), hpbase);
				GUI.DrawTexture(Crypto.Rect2(225f, 131f, 112f, 5f), gauge_hp);
				GUI.Label(Crypto.Rect2(190f, 111f, 128f, 16f), "HP", "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 144f, 128f, 16f), Language.intxt[language, 141], "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 164f, 128f, 16f), Language.intxt[language, 142], "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 184f, 128f, 16f), Language.intxt[language, 295], "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 111f, 64f, 16f), string.Empty + g_maxhp, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 144f, 64f, 16f), string.Empty + g_maxatk, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 164f, 64f, 16f), string.Empty + g_def, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 184f, 64f, 16f), string.Empty + (g_atkspd * 100f).ToString("F1"), "txt12_0");
				if (g_grade >= 2)
				{
					GUI.Label(Crypto.Rect2(220f, 204f, 128f, 16f), script_name.txt_name[language, 1 + general_index], "txt12_b");
				}
				else
				{
					GUI.Label(Crypto.Rect2(220f, 204f, 128f, 16f), script_name.txt_name[language, 0], "txt12_0");
				}
				if (GUI.Button(Crypto.Rect2(208f, 230f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					bg_popui.localScale = Vector3.zero;
					explore_progress = 0;
					ExploreIconFinish();
				}
			}
			else if (explore_progress == 7)
			{
				iconactive = false;
				GUI.Label(Crypto.Rect2(140f, 204f, 200f, 32f), Language.intxt[language, 288], "txt12_0");
				if (GUI.Button(Crypto.Rect2(208f, 230f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					bg_popui.localScale = Vector3.zero;
					explore_progress = 0;
					ExploreIconFinish();
					ani_walk.gameObject.active = false;
				}
			}
			else if (explore_progress == 8)
			{
				iconactive = false;
				GUI.Label(Crypto.Rect2(140f, 120f, 200f, 32f), Language.intxt[language, 240], "txt12_0");
				if (GUI.Button(Crypto.Rect2(208f, 230f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					bg_popui.localScale = Vector3.zero;
					explore_progress = 0;
					ExploreIconFinish();
				}
			}
		}
		else if (GUI.Button(Crypto.Rect2(448f, -1f, 32f, 32f), string.Empty, bt_menu))
		{
			if (sound_UI != null)
			{
				script_soundUI.SoundOn(0);
			}
			pause = true;
			Input.multiTouchEnabled = false;
			script_cam.DisableMove();
			string channelId = ChannelMgr.GetInstance().getChannelId();
			if (channelId == "000116")
			{
				AndroidScript.i.loadWDJAds();
				showAds = false;
			}
		}
		else
		{
			if (haveGiftBox == 0 && GUI.Button(Crypto.Rect2(4f, 175f, 64f, 64f), string.Empty, bt_gift))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 18;
			}
			if (!exploreMode)
			{
				if (GUI.Button(Crypto.Rect2(144f, 2f, 100f, 24f), string.Empty, bt_empty))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					Crypto.Save_int_key("cashshopkind", 2);
					CashshopOpen();
				}
				if (GUI.Button(Crypto.Rect2(272f, 2f, 88f, 24f), string.Empty, bt_empty))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					Crypto.Save_int_key("cashshopkind", 1);
					CashshopOpen();
				}
				if (GUI.Button(Crypto.Rect2(366f, 0f, 64f, 28f), string.Empty, bt_empty))
				{
					Crypto.Save_int_key("gamemode", 1);
					Object.Destroy(bg_worldmap.gameObject);
					changescene = true;
					Application.LoadLevel("Extreme");
				}
				GUI.DrawTexture(Crypto.Rect2(366f, 1f, 64f, 26f), txt_story);
			}
			if (stageon == 0 && event_stage >= 0)
			{
				if (GUI.Button(Crypto.Rect2(340f, 254f, 64f, 64f), string.Empty, bt_bonus))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					iconactive = false;
					stageon = 0;
					script_cam.MoveTarget(eventicon, _isevent: true);
				}
				if (tutorial != 5)
				{
					GUI.DrawTexture(Crypto.Rect2(370f, 254f, icon_size * 32, icon_size * 32), ico_warn);
				}
			}
			if (max_stage_index >= 4 && GUI.Button(Crypto.Rect2(410f, 254f, 64f, 64f), Language.intxt[language, 401], bt_explore))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (tutorial == 4)
				{
					if (general_seed[2] != 0)
					{
						confirm = 16;
						Delay(1f);
					}
					else
					{
						iconactive = false;
						confirm = 1;
					}
				}
				else
				{
					iconactive = false;
					confirm = 1;
				}
				script_cam.MoveTargetCancel();
				stageon = 0;
			}
			if (GUI.Button(Crypto.Rect2(4f, 106f, 64f, 64f), string.Empty, bt_general_small))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				PlayerPrefsX.SetVector3("uiCampos", uicam.position);
				iconactive = false;
				stageon = 0;
				Application.LoadLevel("General");
			}
			if (cur_general != -1)
			{
				GUI.DrawTexture(Crypto.Rect2(16f, 118f, 40f, 40f), prt_general);
				GUI.DrawTexture(Crypto.Rect2(18f, 161f, 36f * ((float)general_cur_hp / cur_generalmaxhp), 4f), gauge_hp);
				if (general_cur_hp < 20)
				{
					GUI.DrawTexture(Crypto.Rect2(16f, 118f, icon_size * 40, 40f), bg_red);
				}
			}
			else
			{
				GUI.DrawTexture(Crypto.Rect2(16f, 118f, icon_size * 40, 40f), empty);
			}
			if (GUI.Button(Crypto.Rect2(4f, 254f, 64f, 64f), Language.intxt[language, 400], bt_shop))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				Crypto.Save_int_key("cashshopkind", 0);
				CashshopOpen();
			}
			for (int k = 0; k < 5; k++)
			{
				if (k < menuopen)
				{
					GUI.DrawTexture(Crypto.Rect2(10 + 80 * k, 30f, 64f, 64f), img_menu[k]);
					if (GUI.Button(Crypto.Rect2(10 + 80 * k, 30f, 64f, 64f), Language.intxt[language, 395 + k], bt_empty))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						PlayerPrefsX.SetVector3("uiCampos", uicam.position);
						iconactive = false;
						stageon = 0;
						switch (k)
						{
						case 0:
							Application.LoadLevel("Skill");
							break;
						case 1:
							Application.LoadLevel("Archive");
							break;
						case 2:
							Application.LoadLevel("Status");
							break;
						case 3:
							Application.LoadLevel("Forge");
							break;
						case 4:
							Application.LoadLevel("Pet");
							break;
						}
					}
					if (warning_new && k == menuopen - 1)
					{
						GUI.DrawTexture(Crypto.Rect2(10 + 80 * k, 30f, icon_size * 32, icon_size * 16), ico_new);
					}
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(10 + 80 * k, 30f, 64f, 64f), bt_lockmenu);
					GUI.Label(Crypto.Rect2(10 + 80 * k, 77f, 64f, 16f), Language.intxt[language, 64] + menuarray[k] + Language.intxt[language, 431], "txt12_r");
				}
			}
			if (warning_hungry)
			{
				GUI.DrawTexture(Crypto.Rect2(370f, 22f, icon_size * 32, icon_size * 32), ico_warn);
				GUI.Box(Crypto.Rect2(80f, 100f, 320f, 24f), Language.intxt[language, 281]);
			}
			if (newarchive || newtreasure)
			{
				GUI.DrawTexture(Crypto.Rect2(130f, 22f, icon_size * 32, icon_size * 32), ico_warn);
			}
			for (int l = 0; l < 5; l++)
			{
				if (skill_slot[l] >= 0)
				{
					GUI.DrawTexture(Crypto.Rect2(446f, 68 + l * 36, 28f, 28f), skillicon[l]);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(446f, 68 + l * 36, 28f, 28f), black);
				}
				GUI.DrawTexture(Crypto.Rect2(444f, 66 + l * 36, 32f, 32f), img_skill_slot);
			}
			if (GUI.Button(Crypto.Rect2(440f, 60f, 40f, 186f), string.Empty, bt_empty))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 15;
				Delay(1f);
			}
			if (tutorial >= 0)
			{
				switch (tutorial)
				{
				case 0:
					GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 221]);
					GUI.DrawTexture(Crypto.Rect2(14f, 34f, icon_size * 64, icon_size * 64), please_touch);
					break;
				case 1:
					GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 222]);
					GUI.DrawTexture(Crypto.Rect2(94f, 34f, icon_size * 64, icon_size * 64), please_touch);
					break;
				case 2:
					tutorial = -1;
					Crypto.Save_int_key("tutorial", -1);
					break;
				case 3:
					GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 224]);
					GUI.DrawTexture(Crypto.Rect2(254f, 34f, icon_size * 64, icon_size * 64), please_touch);
					break;
				case 4:
					if (general_seed[1] == 0)
					{
						GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 225]);
						if (confirm == 0)
						{
							GUI.DrawTexture(Crypto.Rect2(414f, 252f, icon_size * 64, icon_size * 64), please_touch);
						}
					}
					else if (general_seed[2] == 0)
					{
						GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 223]);
						if (confirm == 0)
						{
							GUI.DrawTexture(Crypto.Rect2(414f, 252f, icon_size * 64, icon_size * 64), please_touch);
						}
					}
					else
					{
						GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 226]);
						GUI.DrawTexture(Crypto.Rect2(4f, 110f, icon_size * 64, icon_size * 64), please_touch);
					}
					break;
				case 5:
					if (stageon == 0)
					{
						GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 229]);
						GUI.DrawTexture(Crypto.Rect2(344f, 256f, icon_size * 64, icon_size * 64), please_touch);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(310f, 236f, icon_size * 64, icon_size * 64), please_touch);
					}
					break;
				case 6:
					PlayerPrefsX.SetIntArray("n25", 3, 3);
					tutorial = -1;
					Crypto.Save_int_key("tutorial", -1);
					break;
				case 14:
					GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 227]);
					GUI.DrawTexture(Crypto.Rect2(4f, 110f, icon_size * 64, icon_size * 64), please_touch);
					break;
				}
			}
		}
		if (confirm > 0)
		{
			GUI.enabled = true;
			switch (confirm)
			{
			case 1:
			{
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 128f), pop_blank2);
				GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 289], "txt12_0");
				for (int m = 0; m < 2; m++)
				{
					if (GUI.Button(Crypto.Rect2(166 + 84 * m, 136f, 64f, 64f), string.Empty, bt_general))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						if (m == 1)
						{
							int num2 = 0;
							for (int n = 0; n < slot_general; n++)
							{
								if (general_seed[n] == 0)
								{
									num2++;
								}
							}
							if (num2 == 0)
							{
								if (slot_general >= 12)
								{
									confirm = 9;
									Delay(1f);
								}
								else
								{
									confirm = 3;
								}
								return;
							}
						}
						else if (tutorial == 4)
						{
							confirm = 16;
							Delay(1f);
							return;
						}
						explorekind = m;
						ExploreIconArray();
						confirm = 0;
					}
					GUI.DrawTexture(Crypto.Rect2(166 + 84 * m, 136f, 64f, 64f), icon_explorekind[m]);
					GUI.Label(Crypto.Rect2(166 + 84 * m, 137f, 64f, 16f), Language.intxt[language, 290 + m], "txt12_w");
				}
				if (GUI.Button(Crypto.Rect2(208f, 200f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					confirm = 0;
				}
				break;
			}
			case 3:
				GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
				GUI.Label(Crypto.Rect2(120f, 130f, 230f, 16f), Language.intxt[language, 298], "txt12_0");
				if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					PlayerPrefs.SetInt("n57", 1);
					Application.LoadLevel("General");
				}
				else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					confirm = 0;
				}
				break;
			case 6:
				GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
				GUI.Label(Crypto.Rect2(120f, 130f, 230f, 16f), Language.intxt[language, 21], "txt12_0");
				if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					Crypto.Save_int_key("cashshopkind", 1);
					confirm = 0;
					CashshopOpen();
				}
				else
				{
					if (!GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
					{
						break;
					}
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					confirm = 0;
					if (cur_general >= 0)
					{
						sel_general = cur_general;
						GeneralStat(general_seed[sel_general], _exist: true);
						bg_popui.localScale = Vector3.one * 5.2f;
						if (exploreMode)
						{
							bg_popui.localPosition = new Vector3(0f, -0.24f, 0.5f);
						}
					}
				}
				break;
			case 8:
				GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
				GUI.Label(Crypto.Rect2(120f, 130f, 240f, 16f), Language.intxt[language, 22], "txt12_0");
				if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					Crypto.Save_int_key("cashshopkind", 2);
					confirm = 0;
					CashshopOpen();
				}
				else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					confirm = 0;
				}
				break;
			case 9:
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 267], "txt12_0");
				if (f_delay == 0f)
				{
					confirm = 0;
				}
				break;
			case 13:
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 208], "txt12_0");
				if (f_delay == 0f)
				{
					confirm = 0;
				}
				break;
			case 14:
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(120f, 110f, 240f, 64f), Language.intxt[language, extra_info], "txt12_0");
				if (f_delay == 0f)
				{
					confirm = 0;
				}
				break;
			case 15:
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(120f, 110f, 240f, 64f), Language.intxt[language, 93], "txt12_0");
				if (f_delay == 0f)
				{
					confirm = 0;
				}
				break;
			case 16:
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(120f, 110f, 240f, 64f), Language.intxt[language, 98], "txt12_0");
				if (f_delay == 0f)
				{
					confirm = 0;
				}
				break;
			case 17:
				GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 259]);
				if (f_delay == 0f)
				{
					confirm = 0;
				}
				break;
			case 18:
				GUI.DrawTexture(Crypto.Rect2(90f, 80f, 300f, 230f), pop_blank2);
				GUI.DrawTexture(Crypto.Rect2(140f, 56f, 200f, 42f), gift_title);
				GUI.DrawTexture(Crypto.Rect2(350f, 70f, 40f, 48f), gift_five_percnt);
				GUI.DrawTexture(Crypto.Rect2(240f, 110f, 64f, 84f), gift_skill);
				GUI.DrawTexture(Crypto.Rect2(240f, 110f, 64f, 84f), gift_skill_bg);
				GUI.DrawTexture(Crypto.Rect2(170f, 110f, 64f, 84f), gift_bg_item);
				GUI.DrawTexture(Crypto.Rect2(170f, 110f, 64f, 64f), gift_coin);
				GUI.DrawTexture(Crypto.Rect2(170f, 174f, 16f, 16f), icon_coin);
				GUI.Label(Crypto.Rect2(176f, 172f, 64f, 20f), "8000", "txt12_w");
				GUI.Label(Crypto.Rect2(120f, 220f, 240f, 16f), Language.intxt[language, 466], "txt12_0");
				if (GUI.Button(Crypto.Rect2(255f, 256f, 80f, 32f), Language.intxt[language, 467], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					confirm = 0;
				}
				else if (GUI.Button(Crypto.Rect2(140f, 256f, 80f, 32f), Language.intxt[language, 468], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					AndroidScript.i.pay("100", "0", 15f, 0, "pay_offline", "010", "5104767", "010", "30000744622508", "10");
					confirm = 0;
				}
				break;
			}
		}
		if (stageon > 0)
		{
			if (stageon == 1)
			{
				overui = true;
				if (Event.current.isMouse)
				{
					v2 = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);
				}
				if (!poprect.Contains(v2))
				{
					overui = false;
				}
				GUI.DrawTexture(poprect, pop_stage);
				GUI.Label(Crypto.Rect2(120f, 213f, 240f, 16f), string.Empty + (sel_stage_index + 1) + ".  " + cur_stage_name, "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(104f, 230f, 128f, 32f), icon_stagekind[cur_stage_kind - 1]);
				GUI.Box(Crypto.Rect2(210f, 234f, 84f, 24f), Language.intxt[language, 64] + "  " + (sel_stage_index + 1));
				GUI.DrawTexture(Crypto.Rect2(122 + 57 * cur_difficulty, 262f, 64f, 32f), bg_mission);
				int num3 = stage_clear_grade[sel_stage_index];
				for (int num4 = 0; num4 < 3; num4++)
				{
					if (num3 > 0 || num4 == 0)
					{
						GUI.DrawTexture(Crypto.Rect2(140 + num4 * 57, 262f, 32f, 32f), icon_mission[num4]);
						if (GUI.Button(Crypto.Rect2(126 + num4 * 57, 261f, 56f, 33f), string.Empty, bt_empty))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							cur_difficulty = num4;
						}
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(140 + num4 * 57, 262f, 32f, 32f), mission_lock);
					}
				}
				if (num3 > 0)
				{
					if (num3 > 100)
					{
						GUI.DrawTexture(Crypto.Rect2(236f, 262f, 64f, 32f), bg_mission_clear);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(238f, 252f, icon_size * 32, 32f), ico_warn);
					}
					if (num3 % 100 > 10)
					{
						GUI.DrawTexture(Crypto.Rect2(179f, 262f, 64f, 32f), bg_mission_clear);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(181f, 252f, icon_size * 32, 32f), ico_warn);
					}
					GUI.DrawTexture(Crypto.Rect2(122f, 262f, 64f, 32f), bg_mission_clear);
				}
				GUI.Box(Crypto.Rect2(112f, 166f, 256f, 32f), Language.intxt[language, 306 + cur_difficulty]);
				if (GUI.Button(Crypto.Rect2(296f, 234f, 64f, 64f), string.Empty, bt_stagestart))
				{
					Crypto.Save_int_key("n14", cur_difficulty);
					BattleStart();
					changescene = true;
					Application.LoadLevel("Loading");
				}
				GUI.DrawTexture(Crypto.Rect2(312f, 256f, 32f, 32f), icon_mission[cur_difficulty]);
				GUI.DrawTexture(Crypto.Rect2(296f, 234f, 64f, 32f), txt_start);
			}
			else if (stageon == 2)
			{
				GUI.DrawTexture(Crypto.Rect2(112f, 234f, 256f, 64f), pop_blank);
				GUI.DrawTexture(Crypto.Rect2(124f, 244f, 180f, 16f), bg_titlename);
				GUI.Label(Crypto.Rect2(144f, 243f, 128f, 16f), string.Empty + cur_stage_name, "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(144f, 260f, 128f, 32f), icon_stagekind[cur_stage_kind - 1]);
				if (GUI.Button(Crypto.Rect2(296f, 234f, 64f, 64f), string.Empty, bt_stagestart))
				{
					BattleStart();
					changescene = true;
					Application.LoadLevel("Loading");
				}
				GUI.DrawTexture(Crypto.Rect2(296f, 250f, 64f, 32f), txt_start);
				if (tutorial == 5)
				{
					GUI.DrawTexture(Crypto.Rect2(300f, 236f, icon_size * 64, icon_size * 64), please_touch);
				}
			}
		}
		else
		{
			bosscutin = false;
			cutinXpos = 480;
		}
		if (tutorial == 4)
		{
			if (explore_progress == 2)
			{
				if (confirm == 0)
				{
					if (general_seed[1] == 0)
					{
						GUI.DrawTexture(Crypto.Rect2(212f, 120f, icon_size * 64, icon_size * 64), please_touch);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(212f, 176f, icon_size * 64, icon_size * 64), please_touch);
					}
				}
			}
			else if (explore_progress == 1)
			{
				GUI.DrawTexture(Crypto.Rect2(222f, 242f, icon_size * 64, icon_size * 64), please_touch);
			}
			else if (confirm == 1)
			{
				if (general_seed[2] == 0)
				{
					GUI.DrawTexture(Crypto.Rect2(254f, 136f, icon_size * 64, icon_size * 64), please_touch);
				}
			}
			else if (confirm == 10)
			{
				GUI.DrawTexture(Crypto.Rect2(174f, 122f, icon_size * 64, icon_size * 64), please_touch);
			}
		}
		if (changescene)
		{
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), black_all);
			GUI.DrawTexture(Crypto.Rect2(370f, 292f, 100f, 25f), txt_loading);
		}
	}

	public void BattleStart()
	{
		PlayerPrefsX.SetVector3("uiCampos", uicam.position);
		TimeControl.SetDelay(1);
		Crypto.Save_int_key("cur_stage_index", sel_stage_index);
		Crypto.Save_int_key("cur_stage_kind", cur_stage_kind);
		Crypto.Save_int_key("n37", -1);
		Crypto.Save_int_key("play_kind", play_kind);
		Object.Destroy(sound_UI.gameObject);
		Object.Destroy(bg_worldmap.gameObject);
	}
}
