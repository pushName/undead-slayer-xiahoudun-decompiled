using System.Collections;
using System.Text;
using LitJson;
using UnityEngine;

public class UI_Extreme : MonoBehaviour
{
	private const int MAXSTAGE = 64;

	private const int MAXWORK = 2;

	private short MAXRANKING = 50;

	public Transform sound_dummy;

	private Transform bg_dun;

	private Transform bg_dun_cover;

	private Transform bg_dun_below;

	public Transform[] flag_icon = new Transform[3];

	private int[] grade_extreme = new int[100];

	public Transform icon_stage;

	public Transform plane_friend;

	public Transform num_stage_index;

	public Transform ring_active;

	public Transform gifticon;

	public Texture2D pop_blank;

	public Texture2D pop_blank2;

	public Texture2D bg_title;

	public Texture2D bg_title_me;

	public Texture2D icon_key;

	public Texture2D toggle_active;

	public Texture2D bg_asset;

	public Texture2D bg_asset_lv;

	public Texture2D bg_asset_key;

	public Texture2D gauge_exp;

	public Texture2D black;

	public Texture2D black_all;

	public Texture2D bg_option;

	public Texture2D ico_slide;

	public Texture2D img_skill_slot;

	public Texture2D bg_item;

	public Texture2D icon_coin;

	public Texture2D[] img_menu = new Texture2D[5];

	public Texture2D[] img_consumeItem = new Texture2D[6];

	public Texture2D bt_lockmenu;

	public Texture2D bt_lockmenu2;

	public Texture2D[] bt_unlockstage = new Texture2D[3];

	public Texture2D menutray;

	public Texture2D[] icon_ranktab = new Texture2D[3];

	public Texture2D errorimg;

	public Texture2D icon_info;

	public Texture2D txt_start;

	public Texture2D arrow_next;

	public Texture2D icon_table;

	public Texture2D icon_friend_plus;

	public Texture2D bg_speech;

	public Texture2D bg_red;

	public Texture2D icon_jade;

	public Texture2D tex_currentStage;

	public Texture2D txt_extreme;

	public Texture2D txt_loading;

	public Texture2D ico_warn;

	public Texture2D gift_title;

	public Texture2D gift_five_percnt;

	public Texture2D gift_skill;

	public Texture2D gift_coin;

	public Texture2D gift_bg_item;

	public Texture2D gift_skill_bg;

	public Texture2D rank_bg;

	public Texture2D rank_item_bg;

	public Texture2D[] icon_toprank = new Texture2D[3];

	public TextAsset blockName;

	private Texture2D myimage;

	private Texture2D[] skillicon = new Texture2D[5];

	private int[] menuarray = new int[5] { 1, 2, 3, 4, 6 };

	private int[] item_price = new int[6] { 100, 120, 130, 140, 150, 155 };

	private int[] skill_slot = new int[5];

	private int[] item_slot = new int[3];

	public GUIStyle bar_slide;

	public GUIStyle bt_empty;

	public GUIStyle bt_start;

	public GUIStyle bt_back;

	public GUIStyle sel_lang;

	public GUIStyle pausemenu;

	public GUIStyle bt_yesno;

	public GUIStyle bt_menu;

	public GUIStyle bt_shop;

	public GUIStyle bt_arrow_r;

	public GUIStyle bt_arrow_l;

	public GUIStyle bt_general_small;

	public GUIStyle bt_plus;

	public GUIStyle bt_gift;

	public GUIStyle bt_rank;

	public GUIStyle rank_name;

	public GUIStyle bg_ownRanking;

	private string username = string.Empty;

	private bool readyOn;

	private bool option;

	private bool pause;

	private bool slideon;

	private bool slideon2;

	private bool isSend = true;

	private bool changescene;

	private bool loadfinish;

	private bool newarchive;

	private bool newtreasure;

	private float vol_master;

	private float vol_bgm;

	private LoopArchive script_archive;

	public GUISkin basicSkin;

	private float curMousePosY;

	private float startMousePosY;

	private float startMousePosY_rank;

	private float curMousePosY_rank;

	private Vector2 scrollPosition;

	private Vector2 prev_scrollPosition;

	private Vector2 rank_scrollPosition = Vector2.zero;

	private Vector2 rank_prev_scrollPosition = Vector2.zero;

	private Rect inputrect = new Rect(84f, 118f, 320f, 180f);

	private int language;

	private float posX;

	private float posX_r;

	private float posY;

	private int coin;

	private int jade;

	private int chalv;

	private int maxStage = 1;

	private int currentStage;

	private int max_stage_index;

	private float length_exp;

	private Transform sound_UI;

	private Transform hiticon;

	private GameObject cashshop;

	private GameObject rankobj;

	private short scopemode = -1;

	private short confirm;

	private short menuopen = 1;

	private SoundEf_UI script_soundUI;

	private CamScroll_Extreme script_cam;

	private int key_cave;

	private int totalprice;

	private int selectItem = -1;

	private int selectblank = -1;

	private int expand_height;

	private int expand_height2;

	private int[] cur_workdelay = new int[2];

	private int time_hour;

	private int time_min;

	private int extreme_point;

	private int cur_general = -1;

	private float cur_generalmaxhp = 1500f;

	private int general_seed;

	private int general_hp;

	private int general_cur_hp;

	private Texture2D prt_general;

	public Texture2D gauge_hp;

	public Texture2D empty;

	private int icon_size;

	private bool uploadprofile = true;

	private bool invitefriend;

	private bool b_delay;

	private float f_delay;

	private int haveGiftBox;

	private GUIStyle bb = new GUIStyle();

	private General_Stat script_generalstat;

	private bool ban_on_drag;

	private bool showReward;

	private bool isSendGetReward = true;

	private bool canThrough = true;

	private bool control_rank_bt = true;

	private void Awake()
	{
		language = PlayerPrefs.GetInt("language");
		cur_general = Crypto.Load_int_key("cur_general");
		if (cur_general >= 0)
		{
			int[] intArray = PlayerPrefsX.GetIntArray("n13");
			general_seed = intArray[cur_general];
			int[] intArray2 = PlayerPrefsX.GetIntArray("n33");
			general_hp = intArray2[cur_general];
		}
		script_generalstat = GetComponent<General_Stat>();
		grade_extreme = PlayerPrefsX.GetIntArray("n52");
		maxStage = Crypto.Load_int_key("n48");
		maxStage = Mathf.Clamp(maxStage, 1, 64);
		currentStage = Crypto.Load_int_key("n49");
		currentStage = Mathf.Max(currentStage, 1);
		extreme_point = Crypto.Load_int_key("n54");
		skill_slot = PlayerPrefsX.GetIntArray("skill_slot");
		posX = 84f;
		posX_r = 480f;
		posY = -100f;
		haveGiftBox = PlayerPrefs.GetInt("haveDrawedGift", 0);
		script_cam = Camera.main.GetComponent<CamScroll_Extreme>();
	}

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
		key_cave = Crypto.Load_int_key("n10");
		script_cam.EnableMove();
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
		script_archive = GameObject.FindWithTag("Finish").GetComponent<LoopArchive>();
		newarchive = script_archive.FindArchive();
		newtreasure = script_archive.FindTreasure();
		GameObject gameObject = GameObject.Find("bg_dun");
		Vector3 vector = Vector3.zero;
		bool flag = false;
		if (gameObject == null)
		{
			GameObject gameObject2 = Resources.Load("bg_dun") as GameObject;
			GameObject gameObject3 = Resources.Load("bg_dun_cover") as GameObject;
			GameObject gameObject4 = Resources.Load("bg_dun_below") as GameObject;
			bg_dun = (Transform)Object.Instantiate(gameObject2.transform, new Vector3(0f, -0.15f, -1f), Quaternion.identity);
			bg_dun_cover = (Transform)Object.Instantiate(gameObject3.transform, new Vector3(0f, -0.15f, -0.5f), Quaternion.identity);
			bg_dun_below = (Transform)Object.Instantiate(gameObject4.transform, new Vector3(0f, -0.15f, -1f), Quaternion.identity);
			bg_dun.name = "bg_dun";
			bg_dun_cover.name = "bg_dun_cover";
			bg_dun_below.name = "bg_dun_below";
			vector = ((maxStage >= 64) ? new Vector3(icon_stage.GetChild(maxStage - 1).position.x, 0f, 1f) : new Vector3(icon_stage.GetChild(maxStage).position.x, 0f, 1f));
			flag = true;
		}
		else
		{
			vector = PlayerPrefsX.GetVector3("uiCampos2");
			bg_dun = gameObject.transform;
			bg_dun_cover = GameObject.Find("bg_dun_cover").transform;
			bg_dun_below = GameObject.Find("bg_dun_below").transform;
			flag = false;
		}
		script_cam.SetCover(bg_dun_cover, bg_dun_below, vector.x, flag, 1.4f + (float)maxStage * 0.05f);
		rankobj = GameObject.FindWithTag("ranking");
		if (rankobj == null)
		{
			GameObject original = Resources.Load("RankingManager") as GameObject;
			rankobj = (GameObject)Object.Instantiate(original, Vector3.zero, Quaternion.identity);
		}
		if (GameObject.FindWithTag("sound") == null)
		{
			sound_UI = (Transform)Object.Instantiate(sound_dummy, Vector3.zero, Quaternion.identity);
		}
		else
		{
			sound_UI = GameObject.FindWithTag("sound").transform;
		}
		script_soundUI = sound_UI.GetComponent<SoundEf_UI>();
		vol_bgm = PlayerPrefs.GetFloat("vol_bgm");
		vol_master = PlayerPrefs.GetFloat("vol_master");
		AudioListener.volume = vol_master;
		TimeControl.InitStart();
		chalv = Crypto.Load_int_key("n47");
		int num3 = Crypto.Load_int_key("n11");
		length_exp = (float)num3 / (float)(chalv * 100) * 50f;
		for (int i = 0; i < maxStage && i < 64; i++)
		{
			Vector3 position = icon_stage.GetChild(i).position;
			Transform transform = (Transform)Object.Instantiate(num_stage_index, position, num_stage_index.rotation);
			transform.GetComponent<TextMesh>().text = (i + 1).ToString();
			if (grade_extreme[i] > 0)
			{
				Object.Instantiate(flag_icon[grade_extreme[i] - 1], position + new Vector3(0f, 0.12f, -0.1f), Quaternion.identity);
				continue;
			}
			icon_stage.GetChild(i).GetComponent<Renderer>().material.mainTexture = tex_currentStage;
			ring_active.position = position + new Vector3(0f, -0.044f, -0.1f);
			if ((i + 1) % 8 == 0)
			{
				Object.Instantiate(gifticon, position, Quaternion.identity);
			}
		}
		for (int j = maxStage; j < 64; j++)
		{
			icon_stage.GetChild(j).localScale = Vector3.zero;
			if ((j + 1) % 8 == 0)
			{
				Vector3 position2 = icon_stage.GetChild(j).position;
				Object.Instantiate(gifticon, position2, Quaternion.identity);
			}
		}
		GeneralKindOnly();
		CurGeneralStat(_start: true);
		SetMenuOpen();
		SetServerTime();
		InvokeRepeating("Warning_iconsize", 1f, 0.34f);
		InvokeRepeating("SetServerTime", 29f, 30f);
		for (int k = 0; k < 5; k++)
		{
			if (skill_slot[k] >= 0)
			{
				skillicon[k] = Resources.Load("sk_" + (skill_slot[k] + 1)) as Texture2D;
			}
		}
		SetMenuOpen();
		AndroidScript.i.showCCPlayAndForce();
	}

	private IEnumerator ImageLoading(string _url)
	{
		WWW www = new WWW(_url);
		yield return www;
		if (www.texture.height > 8)
		{
			myimage = www.texture;
		}
	}

	public void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
	}

	public void SetMenuOpen()
	{
		max_stage_index = Crypto.Load_int_key("n06");
		if (max_stage_index >= 7)
		{
			menuopen = 5;
		}
		else if (max_stage_index >= 6)
		{
			menuopen = 5;
		}
		else if (max_stage_index >= 4)
		{
			menuopen = 4;
		}
		else if (max_stage_index >= 3)
		{
			menuopen = 4;
		}
		else if (max_stage_index >= 2)
		{
			menuopen = 3;
		}
		else if (max_stage_index >= 1)
		{
			menuopen = 2;
		}
		else
		{
			menuopen = 1;
		}
	}

	public void jumpSence(int type)
	{
		if (type == 0)
		{
			Crypto.Save_int_key("gamemode", 0);
			Application.LoadLevel("Map");
			changescene = true;
			Object.Destroy(rankobj);
			Object.Destroy(bg_dun.gameObject);
			Object.Destroy(bg_dun_cover.gameObject);
			Object.Destroy(bg_dun_below.gameObject);
		}
	}

	public void PopUpOff()
	{
		confirm = 0;
	}

	public void CurGeneralStat(bool _start)
	{
		if (_start && cur_general != -1)
		{
			script_generalstat.SetGeneral(general_seed);
			cur_generalmaxhp = script_generalstat.g_maxhp;
		}
	}

	public void GeneralKindOnly()
	{
		int num = general_seed % 10000000 / 100000;
		prt_general = Resources.Load("prt_general" + (num + 1)) as Texture2D;
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
		if (cur_workdelay[0] > 360)
		{
			key_cave += cur_workdelay[0] / 360;
			key_cave = Mathf.Clamp(key_cave, 0, 5);
			TimeControl.SetDelay(0);
			cur_workdelay[0] = cur_workdelay[0] % 360;
		}
		time_hour = (360 - cur_workdelay[0]) / 60;
		time_min = (360 - cur_workdelay[0]) % 60;
	}

	public void BattleStart()
	{
		if (sound_UI != null)
		{
			Object.Destroy(sound_UI.gameObject);
		}
		Object.Destroy(rankobj);
		Object.Destroy(bg_dun.gameObject);
		Object.Destroy(bg_dun_cover.gameObject);
		Object.Destroy(bg_dun_below.gameObject);
		Crypto.Save_int_key("n49", currentStage);
		int num = Crypto.Load_int_key("caveplay");
		num++;
		Crypto.Save_int_key("caveplay", num);
		key_cave--;
		Crypto.Save_int_key("n10", key_cave);
		changescene = true;
		Application.LoadLevel("Loading2");
		if (key_cave >= 5)
		{
			TimeControl.SetDelay(0);
		}
		Crypto.Save_int_key("cur_stage_index", 100);
		Crypto.Save_int_key("cur_stage_kind", 11);
		Crypto.Save_int_key("play_kind", 0);
		PlayerPrefsX.SetIntArray("n50", item_slot);
	}

	public void CashshopOpen()
	{
		script_cam.DisableMove();
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
	}

	public bool ItemKeep(int _index)
	{
		bool result = false;
		for (int i = 0; i < 3; i++)
		{
			if (item_slot[i] == 0)
			{
				item_slot[i] = _index + 1;
				result = true;
				totalprice += item_price[_index];
				break;
			}
		}
		return result;
	}

	private bool ItemOver(int _index)
	{
		bool result = false;
		for (int i = 0; i < 3; i++)
		{
			if (item_slot[i] == _index + 1)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void Delay(float t)
	{
		b_delay = true;
		f_delay = t;
	}

	public void SetStage(int _index)
	{
		item_slot = new int[3];
		readyOn = true;
		inputrect = new Rect(10f, 120f, 242f, 180f);
		posX = -200f;
		posX_r = 550f;
		currentStage = _index + 1;
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
		if (!readyOn)
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (!script_cam.drag)
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					if (Physics.Raycast(ray, out var hitInfo))
					{
						hiticon = hitInfo.transform;
						hiticon.localScale = Vector3.one * 0.8f;
					}
				}
			}
			else if (Input.GetMouseButtonUp(0) && hiticon != null)
			{
				if (!script_cam.drag)
				{
					hiticon.GetComponent<Icon_Stage_Extreme>().IconDown();
				}
				hiticon.localScale = Vector3.one * 0.7f;
				hiticon = null;
			}
		}
		posX = Mathf.MoveTowards(posX, 84f, Time.deltaTime * 1000f);
		posX_r = Mathf.MoveTowards(posX_r, 280f, Time.deltaTime * 1000f);
		posY = Mathf.MoveTowards(posY, 114f, Time.deltaTime * 1000f);
		haveGiftBox = PlayerPrefs.GetInt("haveDrawedGift", 0);
		string text = PlayerPrefs.GetString("charge");
		if (text.CompareTo("true") == 0)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetString("charge", "false");
		}
	}

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
		GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
		GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
		GUI.DrawTexture(Crypto.Rect2(50f, 0f, 64f, 32f), bg_asset_lv);
		GUI.Label(Crypto.Rect2(52f, 3f, 80f, 14f), string.Empty + chalv, "txt12_w");
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
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
			return;
		}
		if (pause)
		{
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), black);
			if (GUI.Button(Crypto.Rect2(112f, 80f, 256f, 64f), Language.intxt[language, 30], pausemenu))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				pause = false;
				Input.multiTouchEnabled = true;
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
				Object.Destroy(sound_UI.gameObject);
				Object.Destroy(rankobj);
				Object.Destroy(bg_dun.gameObject);
				Object.Destroy(bg_dun_cover.gameObject);
				Object.Destroy(bg_dun_below.gameObject);
			}
			return;
		}
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
		if (Input.anyKeyDown)
		{
			if (Crypto.Rect2(inputrect.x, inputrect.y, inputrect.width, inputrect.height).Contains(Event.current.mousePosition))
			{
				startMousePosY = Input.mousePosition.y;
				prev_scrollPosition = scrollPosition;
			}
		}
		else if (Input.anyKey && Crypto.Rect2(inputrect.x, inputrect.y, inputrect.width, inputrect.height).Contains(Event.current.mousePosition))
		{
			curMousePosY = Input.mousePosition.y;
			scrollPosition = prev_scrollPosition + Vector2.up * (curMousePosY - startMousePosY) * (640f / (float)Screen.height);
		}
		if (readyOn && canThrough)
		{
			GUI.Box(Crypto.Rect2(posX_r + 4f, posY - 42f, 184f, 196f), string.Empty, "blank_box");
			GUI.Box(Crypto.Rect2(posX_r + 4f, 274f, 184f, 38f), string.Empty, "blank_box");
			GUI.Box(Crypto.Rect2(posX_r + 14f, 76f, 164f, 16f), "TOTAL  :  " + totalprice, "blank_red");
			GUI.DrawTexture(Crypto.Rect2(posX_r + 40f, 77f, 14f, 14f), icon_coin);
			GUI.Box(Crypto.Rect2(posX_r + 14f, 228f, 164f, 34f), string.Empty, "blank_white");
			if (selectItem >= 0)
			{
				GUI.DrawTexture(Crypto.Rect2(posX_r + 16f, 229f, 32f, 32f), img_consumeItem[selectItem]);
				GUI.Label(Crypto.Rect2(posX_r + 46f, 229f, 124f, 32f), Language.intxt[language, 411 + selectItem], "txt12_0");
			}
			else
			{
				GUI.DrawTexture(Crypto.Rect2(posX_r + 22f, 234f, 20f, 20f), icon_info);
				GUI.Label(Crypto.Rect2(posX_r + 46f, 229f, 124f, 32f), Language.intxt[language, 420], "txt12_0");
			}
			for (int i = 0; i < 6; i++)
			{
				if (ItemOver(i))
				{
					continue;
				}
				int num2 = i / 3 * 66;
				GUI.DrawTexture(Crypto.Rect2(posX_r + (float)(52 * (i % 3)) + 20f, 94 + num2, 48f, 63f), bg_item);
				GUI.DrawTexture(Crypto.Rect2(posX_r + (float)(52 * (i % 3)) + 20f, 94 + num2, 48f, 48f), img_consumeItem[i]);
				GUI.DrawTexture(Crypto.Rect2(posX_r + (float)(52 * (i % 3)) + 22f, 141 + num2, 14f, 14f), icon_coin);
				GUI.Label(Crypto.Rect2(posX_r + (float)(52 * (i % 3)) + 18f, 139 + num2, 64f, 16f), string.Empty + item_price[i], "txt12_w");
				if (!GUI.Button(Crypto.Rect2(posX_r + (float)(52 * (i % 3)) + 20f, 94 + num2, 48f, 48f), string.Empty, bt_empty))
				{
					continue;
				}
				if (coin >= item_price[i])
				{
					if (ItemKeep(i))
					{
						coin -= item_price[i];
					}
				}
				else
				{
					confirm = 1;
				}
				selectItem = i;
			}
			for (int j = 0; j < 3; j++)
			{
				GUI.Box(Crypto.Rect2(posX_r + 14f + (float)(40 * j), 36f, 32f, 32f), string.Empty, "blank_white");
				if (item_slot[j] > 0)
				{
					GUI.DrawTexture(Crypto.Rect2(posX_r + 14f + (float)(40 * j), 36f, 32f, 32f), img_consumeItem[item_slot[j] - 1]);
				}
				if (GUI.Button(Crypto.Rect2(posX_r + 14f + (float)(40 * j), 36f, 32f, 32f), string.Empty, bt_empty) && item_slot[j] > 0)
				{
					coin += item_price[item_slot[j] - 1];
					totalprice -= item_price[item_slot[j] - 1];
					item_slot[j] = 0;
				}
			}
			if (GUI.Button(Crypto.Rect2(0f, 30f, 280f, 280f), string.Empty, bt_back))
			{
				posX = 84f;
				posY = -100f;
				readyOn = false;
				inputrect = new Rect(84f, 118f, 320f, 180f);
				selectItem = -1;
				coin += totalprice;
				totalprice = 0;
				script_cam.EnableMove();
			}
			if (GUI.Button(Crypto.Rect2(posX_r + 10f, 276f, 172f, 48f), string.Empty, bt_start))
			{
				if (key_cave > 0)
				{
					if (Crypto.Property_change(-totalprice, _jade: false))
					{
						BattleStart();
					}
				}
				else
				{
					confirm = 4;
				}
			}
			GUI.Box(Crypto.Rect2(posX_r + 26f, 283f, 70f, 20f), "STAGE " + currentStage, "blank_box");
			GUI.DrawTexture(Crypto.Rect2(posX_r + 98f, 276f, 64f, 32f), txt_start);
		}
		else
		{
			for (int k = 0; k < 5; k++)
			{
				if (skill_slot[k] >= 0)
				{
					GUI.DrawTexture(Crypto.Rect2(446f, 68 + k * 36, 28f, 28f), skillicon[k]);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(446f, 68 + k * 36, 28f, 28f), black);
				}
				GUI.DrawTexture(Crypto.Rect2(444f, 66 + k * 36, 32f, 32f), img_skill_slot);
			}
			for (int l = 0; l < 5; l++)
			{
				if (l < menuopen)
				{
					GUI.DrawTexture(Crypto.Rect2(10 + 80 * l, posY - 84f, 64f, 64f), img_menu[l]);
					if (GUI.Button(Crypto.Rect2(10 + 80 * l, posY - 84f, 64f, 64f), Language.intxt[language, 395 + l], bt_empty))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						PlayerPrefsX.SetVector3("uiCampos2", Camera.main.transform.position);
						switch (l)
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
					if ((newarchive || newtreasure) && l == 1)
					{
						GUI.DrawTexture(Crypto.Rect2(130f, posY - 92f, icon_size * 32, icon_size * 32), ico_warn);
					}
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(10 + 80 * l, posY - 84f, 64f, 64f), bt_lockmenu);
					GUI.Label(Crypto.Rect2(10 + 80 * l, posY - 37f, 64f, 16f), Language.intxt[language, 64] + menuarray[l] + Language.intxt[language, 431], "txt12_r");
				}
			}
			GUI.Box(Crypto.Rect2(372f, 266f, 92f, 40f), string.Empty, "blank_box");
			GUI.DrawTexture(Crypto.Rect2(384f, 267f, 20f, 20f), icon_key);
			GUI.Label(Crypto.Rect2(392f, 268f, 48f, 20f), string.Empty + key_cave, "txt14_w");
			if (key_cave < 5)
			{
				GUI.Box(Crypto.Rect2(376f, 288f, 84f, 14f), string.Empty + time_hour + " h  " + time_min + " m", "blank_red");
			}
			else
			{
				GUI.Box(Crypto.Rect2(376f, 288f, 84f, 14f), Language.intxt[language, 60]);
			}
			if (GUI.Button(Crypto.Rect2(431f, 261f, 34f, 34f), string.Empty, bt_plus))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				Crypto.Save_int_key("cashshopkind", 3);
				CashshopOpen();
			}
			if (GUI.Button(Crypto.Rect2(448f, -1f, 32f, 32f), string.Empty, bt_menu))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				pause = true;
				string channelId = ChannelMgr.GetInstance().getChannelId();
				if (channelId == "000116")
				{
					AndroidScript.i.loadWDJAds();
				}
				Input.multiTouchEnabled = false;
			}
			if (GUI.Button(Crypto.Rect2(366f, 0f, 64f, 28f), string.Empty, bt_empty))
			{
				Crypto.Save_int_key("gamemode", 0);
				Application.LoadLevel("Map");
				changescene = true;
				Object.Destroy(rankobj);
				Object.Destroy(bg_dun.gameObject);
				Object.Destroy(bg_dun_cover.gameObject);
				Object.Destroy(bg_dun_below.gameObject);
			}
			GUI.DrawTexture(Crypto.Rect2(366f, 1f, 64f, 26f), txt_extreme);
			if (GUI.Button(Crypto.Rect2(375f, 180f, 64f, 64f), string.Empty, bt_rank))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				ban_on_drag = true;
				isSend = true;
				confirm = 19;
			}
		}
		if (ban_on_drag)
		{
			script_cam.DisableMove();
		}
		if (haveGiftBox == 0 && GUI.Button(Crypto.Rect2(4f, 175f, 64f, 64f), string.Empty, bt_gift))
		{
			if (sound_UI != null)
			{
				script_soundUI.SoundOn(0);
			}
			confirm = 18;
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
		if (confirm > 0)
		{
			GUI.enabled = true;
			switch (confirm)
			{
			case 1:
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
						script_soundUI.SoundOn(1);
					}
					confirm = 0;
				}
				break;
			case 2:
				GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
				GUI.Label(Crypto.Rect2(120f, 130f, 240f, 16f), Language.intxt[language, 21], "txt12_0");
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
				else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					confirm = 0;
				}
				break;
			case 3:
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(120f, 110f, 240f, 64f), Language.intxt[language, 267], "txt12_0");
				break;
			case 4:
				GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
				GUI.Label(Crypto.Rect2(120f, 130f, 240f, 16f), Language.intxt[language, 402], "txt12_0");
				if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					Crypto.Save_int_key("cashshopkind", 3);
					confirm = 0;
					CashshopOpen();
				}
				else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
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
			case 19:
			{
				if (PlayerPrefs.GetInt("doRank") == 0)
				{
					canThrough = false;
					GUI.DrawTexture(Crypto.Rect2(140f, 100f, 200f, 100f), pop_blank2);
					GUI.Label(Crypto.Rect2(163f, 125f, 70f, 30f), Language.intxt[language, 472], rank_name);
					username = GUI.TextField(new Rect(420f, 245f, 200f, 35f), username);
					username = Text_Length(username);
					if (GUI.Button(Crypto.Rect2(160f, 154f, 90f, 32f), Language.intxt[language, 470], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						HttpMsgManager httpMsgManager = (HttpMsgManager)Object.FindObjectOfType(typeof(HttpMsgManager));
						if ((bool)httpMsgManager)
						{
							if (string.Empty != username)
							{
								Crypto.Save_string_key("lastSatge", "0");
								Crypto.Save_string_key("highScore", "0");
								Crypto.Save_string_key("useTime", "0:00");
								string text = blockName.text;
								string[] array = text.Split('\n');
								bool flag = false;
								for (int m = 0; m < array.Length; m++)
								{
									if (array[m].CompareTo(username) == 0)
									{
										flag = true;
										break;
									}
								}
								if (flag)
								{
									AndroidScript.i.showNotice("4");
								}
								else
								{
									httpMsgManager.check(username);
								}
								username = string.Empty;
							}
							else
							{
								AndroidScript.i.showNotice("1");
							}
						}
						canThrough = true;
						confirm = 19;
					}
					if (GUI.Button(Crypto.Rect2(255f, 154f, 64f, 32f), Language.intxt[language, 471], bt_yesno))
					{
						canThrough = true;
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						confirm = 0;
					}
					break;
				}
				canThrough = false;
				if (isSend)
				{
					HttpMsgManager httpMsgManager2 = (HttpMsgManager)Object.FindObjectOfType(typeof(HttpMsgManager));
					if ((bool)httpMsgManager2)
					{
						string text2 = PlayerPrefs.GetString("rankName");
						httpMsgManager2.regist(text2);
					}
					isSend = false;
				}
				GUI.DrawTexture(Crypto.Rect2(50f, 20f, 360f, 290f), rank_bg);
				if (Input.GetMouseButtonDown(0))
				{
					startMousePosY = Input.mousePosition.y;
					prev_scrollPosition = rank_scrollPosition;
				}
				if (Input.GetMouseButton(0) && Crypto.Rect2(60f, 89f, 320f, 160f).Contains(Event.current.mousePosition))
				{
					curMousePosY = Input.mousePosition.y;
					rank_scrollPosition = prev_scrollPosition + Vector2.up * (curMousePosY - startMousePosY) * (640f / (float)Screen.height);
				}
				string[] stringArray = PlayerPrefsX.GetStringArray("n_r_list");
				int num3 = stringArray.Length;
				rank_scrollPosition = GUI.BeginScrollView(Crypto.Rect2(54f, 89f, 340f, 160f), rank_scrollPosition, Crypto.Rect2(54f, 89f, 330f, num3 * 32));
				for (int n = 0; n < num3; n++)
				{
					string aJSON = stringArray[n];
					JSONNode jSONNode = JSON.Parse(aJSON);
					GUI.DrawTexture(Crypto.Rect2(66f, 89 + n * 32, 320f, 32f), rank_item_bg);
					GUI.Label(Crypto.Rect2(53f, 99 + n * 32, 64f, 16f), string.Empty + (1 + n), "txt12_0");
					GUI.Label(Crypto.Rect2(107f, 99 + n * 32, 64f, 16f), (!(jSONNode != null)) ? string.Empty : jSONNode["username"].ToString(), "txt12_0");
					GUI.Label(Crypto.Rect2(157f, 99 + n * 32, 64f, 16f), (!(jSONNode != null)) ? string.Empty : jSONNode["level"].ToString(), "txt12_0");
					GUI.Label(Crypto.Rect2(204f, 99 + n * 32, 64f, 16f), (!(jSONNode != null)) ? string.Empty : jSONNode["fight"].ToString(), "txt12_0");
					GUI.Label(Crypto.Rect2(263f, 99 + n * 32, 64f, 16f), (!(jSONNode != null)) ? string.Empty : jSONNode["lastSatge"].ToString(), "txt12_0");
					GUI.Label(Crypto.Rect2(325f, 99 + n * 32, 64f, 16f), (!(jSONNode != null)) ? string.Empty : jSONNode["highScore"].ToString(), "txt12_0");
				}
				GUI.EndScrollView();
				int num4 = PlayerPrefs.GetInt("ownRanking");
				GUI.Label(Crypto.Rect2(323f, 35f, 64f, -16f), num4 + string.Empty, bg_ownRanking);
				if (GUI.Button(Crypto.Rect2(265f, 260f, 64f, 32f), Language.intxt[language, 473], bt_yesno) && control_rank_bt)
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					showReward = true;
					control_rank_bt = false;
				}
				if (GUI.Button(Crypto.Rect2(328f, 260f, 64f, 32f), Language.intxt[language, 0], bt_yesno) && control_rank_bt)
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					ban_on_drag = false;
					canThrough = true;
					script_cam.EnableMove();
					confirm = 0;
				}
				if (!showReward)
				{
					break;
				}
				GUI.DrawTexture(Crypto.Rect2(60f, 50f, 340f, 230f), pop_blank2);
				GUI.Label(Crypto.Rect2(80f, 80f, 240f, 16f), Language.intxt[language, 474], rank_name);
				GUI.Label(Crypto.Rect2(230f, 80f, 240f, 16f), Language.intxt[language, 475], rank_name);
				GUI.Label(Crypto.Rect2(80f, 105f, 240f, 16f), Language.intxt[language, 476], rank_name);
				GUI.Label(Crypto.Rect2(230f, 105f, 240f, 16f), Language.intxt[language, 477], rank_name);
				GUI.Label(Crypto.Rect2(80f, 130f, 240f, 16f), Language.intxt[language, 478], rank_name);
				GUI.Label(Crypto.Rect2(230f, 130f, 240f, 16f), Language.intxt[language, 479], rank_name);
				GUI.Label(Crypto.Rect2(80f, 155f, 240f, 16f), Language.intxt[language, 480], rank_name);
				GUI.Label(Crypto.Rect2(230f, 155f, 240f, 16f), Language.intxt[language, 481], rank_name);
				GUI.Label(Crypto.Rect2(80f, 180f, 240f, 16f), Language.intxt[language, 482], rank_name);
				GUI.Label(Crypto.Rect2(230f, 180f, 240f, 16f), Language.intxt[language, 483], rank_name);
				GUI.Label(Crypto.Rect2(80f, 205f, 240f, 16f), Language.intxt[language, 484], rank_name);
				if (GUI.Button(Crypto.Rect2(200f, 230f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					showReward = false;
					control_rank_bt = true;
				}
				break;
			}
			}
		}
		if (changescene)
		{
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), black_all);
			GUI.DrawTexture(Crypto.Rect2(370f, 292f, 100f, 25f), txt_loading);
		}
	}

	private string Text_Length(string Text)
	{
		string text = string.Empty;
		int num = 0;
		bool flag = false;
		int length = -1;
		for (int i = 0; i < Text.Length; i++)
		{
			byte[] bytes = Encoding.Default.GetBytes(Text.Substring(i, 1));
			num = ((bytes.Length <= 1) ? (num + 1) : (num + 2));
			length = i;
			if (num > 10)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return Text.Substring(0, length);
		}
		return Text;
	}
}
