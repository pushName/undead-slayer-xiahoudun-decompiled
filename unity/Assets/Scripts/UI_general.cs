using UnityEngine;

public class UI_general : MonoBehaviour
{
	private const int MAXGENERAL = 12;

	private const int MAXGENERALPOOL = 30;

	private const int SLOTPRICE = 3;

	private const int MAXWORK = 2;

	private const int MAXTEMPGENERAL = 50;

	public GUISkin basicSkin;

	private int cur_general = -1;

	private int sel_general = -1;

	private int comb_general = -1;

	private int jade;

	private int coin;

	private int getslot;

	private int slot_general;

	private Texture2D[] prt_general = new Texture2D[12];

	private int language;

	private int[] general_seed = new int[12];

	private int[] general_hp = new int[12];

	private int[] general_cur_hp = new int[12];

	private int[] general_exp = new int[12];

	private short[] general_grade = new short[12];

	private short[] general_level = new short[12];

	private bool[] ismaxhp = new bool[12];

	private int[] cur_workdelay = new int[2];

	private int[] temp_general = new int[50];

	private int[] cardtype_count = new int[3];

	private int[] temp_general_cardtype = new int[50];

	public Texture2D[] general_weapon = new Texture2D[5];

	public Texture2D[] icon_popgeneralkind = new Texture2D[3];

	public Texture2D bg_titlename;

	public Texture2D bg_general;

	public Texture2D star_grade;

	public Texture2D gauge_hp;

	public Texture2D titlebase;

	public Texture2D titlebase3;

	public Texture2D hpbase;

	public Texture2D icon_jade;

	public Texture2D icon_coin;

	public Texture2D gauge_exp;

	public Texture2D black_all;

	public Texture2D black;

	public Texture2D arrow_combine;

	public Texture2D bg_inherit;

	public Texture2D please_touch;

	public Texture2D pop_blank;

	public Texture2D pop_blank2;

	public Texture2D icon_unlock;

	public Texture2D bg_speech;

	public Texture2D sel_mark;

	public Texture2D ico_lock;

	public Texture2D bg_red;

	public Texture2D empty;

	public Texture2D bt_general_small;

	public Texture2D[] card_general = new Texture2D[3];

	public Texture2D icon_alarm;

	public Texture2D bg_asset;

	public GUIStyle pausemenu;

	public GUIStyle bt_dismiss;

	public GUIStyle bt_general;

	public GUIStyle bt_roll;

	public GUIStyle bt_empty;

	public GUIStyle bt_back;

	public GUIStyle bt_yesno;

	public Transform fortune_teller;

	public Transform sound_dummy;

	public Transform bg_popui;

	public Transform bg_card;

	public Transform bg_combine;

	public Transform ef_card_l;

	public Transform ef_card_r;

	public Transform ef_card_dis;

	public Transform ani_walk;

	private Transform sound_UI;

	private GameObject cashshop;

	private Vector2 iconmove;

	private short pop_general;

	private short general_index;

	private short general_kind;

	private short g_maxatk;

	private short g_def;

	private short g_maxhp;

	private short g_grade;

	private short g_level;

	private short comb_grade;

	private short comb_level;

	private short comb_maxatk;

	private short comb_def;

	private short comb_maxhp;

	private short comb_get_level;

	private short show_fortune_teller;

	private short confirm;

	private float g_atkspd;

	private float comb_atkspd;

	private float g_exp_length;

	private float comb_exp_length;

	private float comb_exp;

	private float get_exp;

	private float bg_posX_r = 480f;

	private float icon_posY = 340f;

	private float posX1;

	private float posX2;

	private float cur_generalmaxhp = 1500f;

	private float f_delay;

	private float cardposY = -100f;

	private float showcard_delay;

	private bool gaugeup;

	private bool changemode;

	private bool b_delay;

	private bool giftmode;

	private Language_Name script_name;

	private General_Stat script_generalstat;

	private SoundEf_UI script_soundUI;

	private bool dragOn;

	private bool scrollOn;

	private float prevposX;

	private float currentX;

	private float dragposX;

	private float dragrange;

	private int tutorial = -1;

	private int icon_size;

	private int rnd_speech;

	private int typeslot = -1;

	private int totalcount;

	private short showcard;

	private void Awake()
	{
		script_name = GetComponent<Language_Name>();
		script_generalstat = GetComponent<General_Stat>();
		language = PlayerPrefs.GetInt("language");
		cur_general = Crypto.Load_int_key("cur_general");
		general_seed = PlayerPrefsX.GetIntArray("n13");
		general_exp = PlayerPrefsX.GetIntArray("n29");
		general_hp = PlayerPrefsX.GetIntArray("n33");
		slot_general = Crypto.Load_int_key("n09");
		tutorial = Crypto.Load_int_key("tutorial");
		temp_general = PlayerPrefsX.GetIntArray("n53");
	}

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
	}

	private void Start()
	{
		dragrange = (float)Screen.height * 0.375f;
		if (GameObject.FindWithTag("sound") == null)
		{
			sound_UI = (Transform)Object.Instantiate(sound_dummy, Vector3.zero, Quaternion.identity);
		}
		else
		{
			sound_UI = GameObject.FindWithTag("sound").transform;
		}
		script_soundUI = sound_UI.GetComponent<SoundEf_UI>();
		GeneralKindOnly();
		CurGeneralStat(_start: true);
		SetServerTime();
		InvokeRepeating("SetServerTime", 29f, 30f);
		int num = PlayerPrefs.GetInt("n57");
		if (num == 1)
		{
			confirm = 2;
			PlayerPrefs.SetInt("n57", 0);
		}
		else if (cur_general >= 0)
		{
			sel_general = cur_general;
			GeneralStat(general_seed[sel_general], _exist: true);
			pop_general = 1;
			bg_popui.localPosition = new Vector3(0f, -0.051f, 0.4f);
			bg_popui.localScale = Vector3.one * 5.2f;
		}
		InvokeRepeating("Warning_iconsize", 1f, 0.34f);
		SetGiftCount();
		PlayerPrefs.SetInt("currentSence", 8);
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
		for (int j = 0; j < 12; j++)
		{
			if (!ismaxhp[j])
			{
				general_cur_hp[j] = general_hp[j] + (int)((float)cur_workdelay[1] * 0.2f);
			}
		}
	}

	public void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
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
			general_hp[getslot] = g_maxhp;
			general_cur_hp[getslot] = g_maxhp;
			PlayerPrefsX.SetIntArray("n33", general_cur_hp);
			TimeControl.SetDelay(1);
			int[] intArray = PlayerPrefsX.GetIntArray("staff");
			intArray[general_index] = Mathf.Max(intArray[general_index], g_grade + 1);
			PlayerPrefsX.SetIntArray("staff", intArray);
		}
		else
		{
			if (general_cur_hp[sel_general] >= g_maxhp)
			{
				general_cur_hp[sel_general] = g_maxhp;
				ismaxhp[sel_general] = true;
			}
			else if (general_cur_hp[sel_general] <= 0)
			{
				general_cur_hp[sel_general] = 1;
			}
			g_exp_length = (float)(general_exp[sel_general] * 58) / ((float)(100 + g_level * 6) * (1f + (float)g_grade * 0.2f));
		}
	}

	public void FortuneTell()
	{
		int[] array = new int[4];
		int[] array2 = new int[4];
		int num = general_seed[sel_general];
		for (int i = 0; i < 4; i++)
		{
			array2[i] = num % 10;
			num /= 10;
			array[(array2[i % 4] + general_index) % 4]++;
		}
		fortune_teller.gameObject.active = true;
		fortune_teller.GetComponent<FortuneTeller>().FortuneMesh(array[0] + 1, array[1] + 1, array[2] + 1, array[3] + 1);
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

	public void Delay(float t)
	{
		b_delay = true;
		f_delay = t;
	}

	public void GeneralKindOnly()
	{
		int num = 0;
		for (int i = 0; i < slot_general; i++)
		{
			if (general_seed[i] > 0)
			{
				num = general_seed[i] % 10000000 / 100000;
				general_grade[i] = (short)(general_seed[i] % 100000 / 10000);
				general_level[i] = (short)((float)general_seed[i] / 10000000f);
				prt_general[i] = Resources.Load("prt_general" + (num + 1)) as Texture2D;
			}
		}
	}

	private void Update()
	{
		if (b_delay)
		{
			f_delay -= Time.deltaTime;
			if (f_delay <= 0f)
			{
				b_delay = false;
				f_delay = 0f;
			}
		}
		if (showcard > 0)
		{
			if (showcard == 3)
			{
				showcard_delay -= Time.deltaTime;
				if (showcard_delay < 0f)
				{
					int num = temp_general[typeslot];
					general_seed[getslot] = num;
					GeneralKindOnly();
					GeneralStat(num, _exist: false);
					temp_general[typeslot] = 0;
					PlayerPrefsX.SetIntArray("n53", temp_general);
					PlayerPrefsX.SetIntArray("n13", general_seed);
					SetGiftCount();
					sel_general = getslot;
					showcard = 4;
					bg_popui.localScale = Vector3.one * 5.2f;
				}
				bg_popui.localScale = Vector3.MoveTowards(bg_popui.localScale, Vector3.one * 5.2f, Time.deltaTime * 20f);
			}
			else if (showcard == 2)
			{
				showcard_delay -= Time.deltaTime;
				if (showcard_delay < 0f)
				{
					showcard = 3;
					bg_card.gameObject.active = false;
					showcard_delay = 0.5f;
				}
			}
			else if (showcard == 1)
			{
				bg_card.position = Vector3.MoveTowards(bg_card.position, new Vector3(0f, 0.05f, 0.5f), Time.deltaTime * 2f);
				showcard_delay -= Time.deltaTime;
				if (showcard_delay < 0f)
				{
					bg_card.GetChild(0).GetComponent<ParticleSystem>().Emit(1);
					bg_card.GetChild(1).GetComponent<ParticleSystem>().Emit(1);
					showcard = 2;
					showcard_delay = 0.5f;
				}
			}
		}
		else if (giftmode)
		{
			cardposY = Mathf.MoveTowards(cardposY, 110f, Time.deltaTime * 800f);
		}
		if (Input.GetMouseButtonDown(0))
		{
			if (Input.mousePosition.y < dragrange)
			{
				dragOn = true;
				prevposX = Input.mousePosition.x;
				currentX = dragposX;
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			dragOn = false;
			scrollOn = false;
		}
		if (dragOn)
		{
			if (Mathf.Abs(Input.mousePosition.x - prevposX) > 8f)
			{
				scrollOn = true;
			}
			dragposX = (Input.mousePosition.x - prevposX) * (480f / (float)Screen.width) + currentX;
			dragposX = Mathf.Min(dragposX, 0f);
			dragposX = Mathf.Max(dragposX, -452f);
		}
		if (pop_general >= 8)
		{
			bg_popui.localScale = Vector3.MoveTowards(bg_popui.localScale, Vector3.one * 5.2f, Time.deltaTime * 30f);
			posX1 = Mathf.MoveTowards(posX1, 140f, Time.deltaTime * 100f);
			posX2 = Mathf.MoveTowards(posX2, 276f, Time.deltaTime * 100f);
			if (pop_general == 9 && f_delay == 0f)
			{
				general_exp[sel_general] = 0;
				general_seed[sel_general] = 0;
				PlayerPrefsX.SetIntArray("n13", general_seed);
				PlayerPrefsX.SetIntArray("n29", general_exp);
				if (sel_general == cur_general)
				{
					cur_general = -1;
					Crypto.Save_int_key("cur_general", cur_general);
				}
				pop_general = 10;
				sel_general = comb_general;
				g_exp_length = comb_exp_length;
				GeneralStat(general_seed[sel_general], _exist: true);
				general_level[sel_general] = g_level;
				if (tutorial == 4)
				{
					tutorial = -3;
					Crypto.Save_int_key("tutorial", -3);
					confirm = 17;
					Delay(1f);
				}
			}
			if (pop_general == 8 && f_delay == 0f)
			{
				pop_general = 9;
				ef_card_dis.gameObject.active = true;
				Delay(0.8f);
				confirm = 0;
			}
		}
		else if (pop_general >= 6)
		{
			posX1 = Mathf.MoveTowards(posX1, 100f, Time.deltaTime * 100f);
			posX2 = Mathf.MoveTowards(posX2, 316f, Time.deltaTime * 100f);
			iconmove = Vector2.MoveTowards(iconmove, Vector2.right * posX2 + Vector2.up * 97f, Time.deltaTime * 1000f);
			bg_posX_r = Mathf.MoveTowards(bg_posX_r, 480f, Time.deltaTime * 1500f);
			icon_posY = Mathf.MoveTowards(icon_posY, 340f, Time.deltaTime * 500f);
			bg_popui.localScale = Vector3.MoveTowards(bg_popui.localScale, Vector3.zero, Time.deltaTime * 30f);
			bg_combine.localScale = Vector3.MoveTowards(bg_combine.localScale, new Vector3(2.5f, 10f, 1f), Time.deltaTime * 30f);
			if (pop_general == 6)
			{
				if (f_delay == 0f)
				{
					pop_general = 7;
					{
						var emission = bg_combine.GetChild(0).GetComponent<ParticleSystem>().emission;
						emission.enabled = true;
					}
					ef_card_l.gameObject.active = true;
					ef_card_r.gameObject.active = true;
					Delay(2f);
				}
			}
			else if (pop_general == 7)
			{
				if (!gaugeup)
				{
					get_exp -= (float)(100 + comb_level * 6) * Time.deltaTime;
					comb_exp += (float)(100 + comb_level * 6) * Time.deltaTime;
					if (comb_exp >= (float)(100 + comb_level * 6) * (1f + (float)comb_grade * 0.2f))
					{
						comb_level++;
						comb_exp = 0f;
						comb_get_level++;
						if (comb_level >= (comb_grade + 1) * 20)
						{
							get_exp = 0f;
						}
					}
					if (get_exp <= 0f)
					{
						general_exp[comb_general] = (int)comb_exp;
						general_seed[comb_general] += 10000000 * comb_get_level;
						gaugeup = true;
					}
					comb_exp_length = comb_exp * 58f / ((float)(100 + comb_level * 6) * (1f + (float)comb_grade * 0.2f));
				}
				if (f_delay == 0f && gaugeup)
				{
					bg_combine.gameObject.active = false;
					{
						var emission = bg_combine.GetChild(0).GetComponent<ParticleSystem>().emission;
						emission.enabled = false;
					}
					ef_card_l.gameObject.active = false;
					ef_card_r.gameObject.active = false;
					pop_general = 8;
					Delay(1.5f);
					confirm = 7;
					rnd_speech = Random.Range(96, 101);
				}
			}
		}
		else if (pop_general == 5)
		{
			iconmove = Vector2.MoveTowards(iconmove, Vector2.right * posX2 + Vector2.up * 97f, Time.deltaTime * 1000f);
		}
		else
		{
			bg_posX_r = Mathf.MoveTowards(bg_posX_r, 0f, Time.deltaTime * 1500f);
			icon_posY = Mathf.MoveTowards(icon_posY, 250f, Time.deltaTime * 500f);
		}
		int num2 = PlayerPrefs.GetInt("updateJAndG");
		if (num2 == 1)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetInt("updateJAndG", 0);
		}
	}

	private void SetGiftCount()
	{
		totalcount = 0;
		temp_general_cardtype = new int[50];
		cardtype_count = new int[3];
		for (int i = 0; i < 50; i++)
		{
			if (temp_general[i] != 0)
			{
				int num = temp_general[i] / 10000000;
				temp_general_cardtype[i] = num;
				cardtype_count[num - 1]++;
				totalcount++;
			}
		}
	}

	private int FindCardType(int _type)
	{
		typeslot = -1;
		for (int i = 0; i < 50; i++)
		{
			if (temp_general_cardtype[i] == _type)
			{
				typeslot = i;
				break;
			}
		}
		return typeslot;
	}

	private void GiftMode()
	{
		GUI.Label(Crypto.Rect2(110f, cardposY - 40f, 260f, 34f), Language.intxt[language, 421] + "\r\n(" + totalcount + " / 50)", "blank_white");
		for (int i = 0; i < 3; i++)
		{
			GUI.DrawTexture(Crypto.Rect2(90 + i * 110, cardposY, 80f, 80f), card_general[i]);
			if (GUI.Button(Crypto.Rect2(90 + i * 110, cardposY, 80f, 80f), string.Empty, bt_empty))
			{
				typeslot = FindCardType(i + 1);
				if (typeslot != -1)
				{
					if (GetGeneral() != -1)
					{
						bg_card.GetComponent<Renderer>().material.mainTexture = card_general[i];
						bg_card.gameObject.active = true;
						bg_card.position = new Vector3((float)(i - 1) * -0.5f, 0.05f, 0.5f);
						cardposY = -100f;
						showcard = 1;
						showcard_delay = 0.5f;
						break;
					}
					if (slot_general >= 12)
					{
						confirm = 9;
						Delay(1f);
					}
					else
					{
						Delay(1f);
						confirm = 3;
					}
				}
			}
			GUI.Box(Crypto.Rect2(94 + i * 110, cardposY + 80f, 72f, 24f), string.Empty, "blank_box");
			GUI.Label(Crypto.Rect2(90 + i * 110, cardposY + 76f, 80f, 32f), string.Empty + cardtype_count[i], "imagenum");
		}
	}

	private int GetGeneral()
	{
		getslot = -1;
		for (int i = 0; i < slot_general; i++)
		{
			if (general_seed[i] == 0)
			{
				getslot = i;
				break;
			}
		}
		return getslot;
	}

	private void GeneralMode()
	{
		if (sel_general < 0)
		{
			sel_general = cur_general;
		}
		else if (general_seed[sel_general] == 0)
		{
			pop_general = 0;
			bg_popui.localScale = Vector3.zero;
			return;
		}
		if (pop_general < 4)
		{
			GUI.DrawTexture(Crypto.Rect2(130f, 54f, 220f, 18f), bg_titlename);
			GUI.DrawTexture(Crypto.Rect2(140f, 97f, 64f, 64f), prt_general[sel_general]);
			GUI.DrawTexture(Crypto.Rect2(140f, 82f, 64f, 110f), bg_general);
			GUI.Label(Crypto.Rect2(144f, 163f, 70f, 16f), string.Empty + g_level + " / " + (g_grade + 1) * 20, "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(143f, 184f, g_exp_length, 5f), gauge_exp);
			for (int i = 0; i < g_grade + 1; i++)
			{
				GUI.DrawTexture(Crypto.Rect2(166 - g_grade * 5 + i * 10, 84f, 12f, 12f), star_grade);
			}
			for (int j = 0; j < 3; j++)
			{
				if (GUI.Button(Crypto.Rect2(364f, 42 + j * 64, 64f, 64f), string.Empty, bt_empty))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (j == 2)
					{
						if (tutorial == 4)
						{
							if (general_seed[2] == 0)
							{
								Delay(1f);
								confirm = 16;
								return;
							}
							if (cur_general != 2 || sel_general != 2)
							{
								Delay(1f);
								confirm = 16;
								return;
							}
						}
						comb_general = sel_general;
						comb_grade = g_grade;
						comb_level = g_level;
						comb_exp_length = g_exp_length;
						comb_exp = general_exp[sel_general];
						comb_get_level = 0;
						comb_atkspd = g_atkspd;
						comb_def = g_def;
						comb_maxatk = g_maxatk;
						comb_maxhp = g_maxhp;
						gaugeup = false;
						posX1 = 140f;
						posX2 = 276f;
					}
					pop_general = (short)(j + 2);
					bg_popui.localScale = Vector3.one * 5.2f;
					bg_popui.localPosition = new Vector3(0f, -0.051f, 0.4f);
				}
				GUI.DrawTexture(Crypto.Rect2(364f, 42 + j * 64, 64f, 64f), icon_popgeneralkind[j]);
				GUI.Box(Crypto.Rect2(386f, 80 + j * 64, 64f, 20f), Language.intxt[language, 178 + j]);
			}
			if (pop_general == 1)
			{
				GUI.Label(Crypto.Rect2(130f, 54f, 220f, 16f), script_name.txt_name[language, 21 + general_index], "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(322f, 50f, 26f, 26f), general_weapon[general_kind]);
				for (int k = 0; k < 3; k++)
				{
					GUI.DrawTexture(Crypto.Rect2(220f, 114 + k * 20, 128f, 16f), titlebase);
				}
				GUI.DrawTexture(Crypto.Rect2(220f, 174f, 128f, 16f), titlebase3);
				GUI.DrawTexture(Crypto.Rect2(220f, 82f, 128f, 32f), hpbase);
				GUI.DrawTexture(Crypto.Rect2(225f, 101f, 112f * (float)general_cur_hp[sel_general] / (float)g_maxhp, 5f), gauge_hp);
				GUI.Label(Crypto.Rect2(190f, 81f, 128f, 16f), "HP", "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 114f, 128f, 16f), Language.intxt[language, 141], "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 134f, 128f, 16f), Language.intxt[language, 142], "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 154f, 128f, 16f), Language.intxt[language, 295], "txt12_0");
				GUI.Label(Crypto.Rect2(260f, 81f, 100f, 16f), general_cur_hp[sel_general] + " / " + g_maxhp, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 114f, 64f, 16f), string.Empty + g_maxatk, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 134f, 64f, 16f), string.Empty + g_def, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 154f, 64f, 16f), string.Empty + (g_atkspd * 100f).ToString("F1"), "txt12_0");
				if (g_grade >= 2)
				{
					GUI.Label(Crypto.Rect2(220f, 174f, 128f, 16f), script_name.txt_name[language, 1 + general_index], "txt12_b");
				}
				else
				{
					GUI.Label(Crypto.Rect2(220f, 174f, 128f, 16f), script_name.txt_name[language, 0], "txt12_0");
				}
				if (cur_general != sel_general)
				{
					if (GUI.Button(Crypto.Rect2(208f, 200f, 64f, 32f), Language.intxt[language, 5], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						cur_general = sel_general;
						Crypto.Save_int_key("cur_general", cur_general);
						CurGeneralStat(_start: false);
						if (tutorial == 14)
						{
							tutorial = -3;
							Crypto.Save_int_key("tutorial", -3);
						}
					}
				}
				else if (GUI.Button(Crypto.Rect2(208f, 200f, 64f, 32f), Language.intxt[language, 175], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					cur_general = -1;
					Crypto.Save_int_key("cur_general", cur_general);
					pop_general = 0;
					bg_popui.localScale = Vector3.zero;
					if (tutorial == -3)
					{
						tutorial = 14;
						Crypto.Save_int_key("tutorial", 14);
					}
				}
				if (GUI.Button(Crypto.Rect2(280f, 200f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					pop_general = 0;
					bg_popui.localScale = Vector3.zero;
					bg_posX_r = 480f;
					bg_posX_r = 480f;
					icon_posY = 340f;
				}
				else if (GUI.Button(Crypto.Rect2(124f, 200f, 32f, 32f), string.Empty, bt_dismiss))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (tutorial == 4 || tutorial == 14)
					{
						Delay(1f);
						confirm = 16;
					}
					else
					{
						confirm = 4;
					}
				}
				if (GUI.Button(Crypto.Rect2(164f, 200f, 32f, 32f), string.Empty, bt_roll))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					changemode = true;
					bg_popui.localScale = Vector3.zero;
					pop_general = 0;
				}
				if (tutorial == 4 && general_seed[2] != 0 && sel_general == 2)
				{
					if (cur_general != 2)
					{
						GUI.DrawTexture(Crypto.Rect2(212f, 182f, icon_size * 64, icon_size * 64), please_touch);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(370f, 170f, icon_size * 64, icon_size * 64), please_touch);
					}
				}
			}
			else if (pop_general == 2)
			{
				GUI.Label(Crypto.Rect2(130f, 54f, 220f, 16f), Language.intxt[language, 178], "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(220f, 82f, 128f, 32f), hpbase);
				GUI.DrawTexture(Crypto.Rect2(225f, 101f, 112f * (float)general_cur_hp[sel_general] / (float)g_maxhp, 5f), gauge_hp);
				GUI.Label(Crypto.Rect2(190f, 81f, 128f, 16f), "HP", "txt12_0");
				GUI.Label(Crypto.Rect2(260f, 81f, 100f, 16f), general_cur_hp[sel_general] + " / " + g_maxhp, "txt12_0");
				GUI.Label(Crypto.Rect2(202f, 118f, 150f, 16f), Language.intxt[language, 388], "txt12_0");
				GUI.Label(Crypto.Rect2(270f, 170f, 32f, 16f), string.Empty + g_level * 10, "txt12_0");
				GUI.DrawTexture(Crypto.Rect2(256f, 170f, 16f, 16f), icon_coin);
				if (GUI.Button(Crypto.Rect2(210f, 134f, 140f, 40f), Language.intxt[language, 178], pausemenu))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (general_cur_hp[sel_general] >= g_maxhp)
					{
						Delay(1f);
						confirm = 9;
						ismaxhp[sel_general] = true;
					}
					else if (coin >= g_level * 10)
					{
						if (Crypto.Property_change(-g_level * 10, _jade: false))
						{
							coin -= g_level * 10;
							general_cur_hp[sel_general] = g_maxhp;
							TimeControl.SetDelay(1);
							PlayerPrefsX.SetIntArray("n33", general_cur_hp);
							general_hp = general_cur_hp;
						}
					}
					else
					{
						Delay(1f);
						confirm = 8;
					}
				}
				else if (GUI.Button(Crypto.Rect2(280f, 200f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					pop_general = 1;
				}
			}
			else
			{
				if (pop_general != 3)
				{
					return;
				}
				GUI.Label(Crypto.Rect2(130f, 54f, 220f, 16f), Language.intxt[language, 179], "txt12_w");
				GUI.Label(Crypto.Rect2(202f, 100f, 150f, 16f), Language.intxt[language, 184], "txt12_0");
				GUI.Label(Crypto.Rect2(270f, 160f, 32f, 16f), string.Empty + 100, "txt12_0");
				GUI.DrawTexture(Crypto.Rect2(256f, 160f, 16f, 16f), icon_coin);
				GUI.Label(Crypto.Rect2(202f, 180f, 150f, 16f), Language.intxt[language, 350], "txt12_b");
				if (GUI.Button(Crypto.Rect2(210f, 120f, 140f, 40f), Language.intxt[language, 176], pausemenu))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (jade >= 3)
					{
						if (Crypto.Property_change(-100, _jade: false))
						{
							FortuneTell();
							coin -= 100;
							pop_general = 0;
							bg_popui.localScale = Vector3.zero;
							show_fortune_teller = 1;
							Delay(3f);
							bg_posX_r = 480f;
						}
					}
					else
					{
						Delay(1f);
						confirm = 6;
					}
				}
				else if (GUI.Button(Crypto.Rect2(280f, 200f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					pop_general = 1;
				}
			}
			return;
		}
		GUI.DrawTexture(Crypto.Rect2(130f, 54f, 220f, 18f), bg_titlename);
		GUI.Label(Crypto.Rect2(130f, 54f, 220f, 16f), Language.intxt[language, 180], "txt12_w");
		GUI.DrawTexture(Crypto.Rect2(posX1, 97f, 64f, 64f), black_all);
		GUI.DrawTexture(Crypto.Rect2(posX1, 97f, 64f, 64f), prt_general[comb_general]);
		GUI.DrawTexture(Crypto.Rect2(posX1, 82f, 64f, 110f), bg_general);
		GUI.Label(Crypto.Rect2(posX1 + 4f, 163f, 70f, 16f), string.Empty + comb_level + " / " + (comb_grade + 1) * 20, "txt12_w");
		GUI.DrawTexture(Crypto.Rect2(posX1 + 3f, 184f, comb_exp_length, 5f), gauge_exp);
		for (int l = 0; l < comb_grade + 1; l++)
		{
			GUI.DrawTexture(Crypto.Rect2(posX1 + 26f - (float)(comb_grade * 5) + (float)(l * 10), 84f, 12f, 12f), star_grade);
		}
		if (pop_general == 10)
		{
			GUI.DrawTexture(Crypto.Rect2(130f, 54f, 220f, 18f), bg_titlename);
			GUI.Label(Crypto.Rect2(130f, 54f, 220f, 16f), script_name.txt_name[language, 21 + general_index], "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(322f, 50f, 26f, 26f), general_weapon[general_kind]);
			for (int m = 0; m < 3; m++)
			{
				GUI.DrawTexture(Crypto.Rect2(220f, 114 + m * 20, 128f, 16f), titlebase);
			}
			GUI.DrawTexture(Crypto.Rect2(220f, 174f, 128f, 16f), titlebase3);
			GUI.DrawTexture(Crypto.Rect2(220f, 82f, 128f, 32f), hpbase);
			GUI.DrawTexture(Crypto.Rect2(225f, 101f, 112f * (float)general_cur_hp[sel_general] / (float)g_maxhp, 5f), gauge_hp);
			GUI.Label(Crypto.Rect2(190f, 81f, 128f, 16f), "HP", "txt12_0");
			GUI.Label(Crypto.Rect2(190f, 114f, 128f, 16f), Language.intxt[language, 141], "txt12_0");
			GUI.Label(Crypto.Rect2(190f, 134f, 128f, 16f), Language.intxt[language, 142], "txt12_0");
			GUI.Label(Crypto.Rect2(190f, 154f, 128f, 16f), Language.intxt[language, 295], "txt12_0");
			GUI.Label(Crypto.Rect2(260f, 81f, 100f, 16f), general_cur_hp[sel_general] + " / " + g_maxhp, "txt12_0");
			GUI.Label(Crypto.Rect2(290f, 114f, 64f, 16f), string.Empty + g_maxatk, "txt12_0");
			GUI.Label(Crypto.Rect2(290f, 134f, 64f, 16f), string.Empty + g_def, "txt12_0");
			GUI.Label(Crypto.Rect2(290f, 154f, 64f, 16f), string.Empty + (g_atkspd * 100f).ToString("F1"), "txt12_0");
			if (g_maxhp - comb_maxhp > 0)
			{
				GUI.Box(Crypto.Rect2(346f, 79f, 46f, 20f), "(+" + (g_maxhp - comb_maxhp) + ")");
			}
			if (g_maxatk - comb_maxatk > 0)
			{
				GUI.Box(Crypto.Rect2(346f, 112f, 46f, 20f), "(+" + (g_maxatk - comb_maxatk) + ")");
			}
			if (g_def - comb_def > 0)
			{
				GUI.Box(Crypto.Rect2(346f, 132f, 46f, 20f), "(+" + (g_def - comb_def) + ")");
			}
			if (g_atkspd * 100f - comb_atkspd * 100f > 0f)
			{
				GUI.Box(Crypto.Rect2(346f, 152f, 46f, 20f), "(+" + (g_atkspd * 100f - comb_atkspd * 100f).ToString("F1") + ")");
			}
			if (g_grade >= 2)
			{
				GUI.Label(Crypto.Rect2(220f, 174f, 128f, 16f), script_name.txt_name[language, 1 + general_index], "txt12_b");
			}
			else
			{
				GUI.Label(Crypto.Rect2(220f, 174f, 128f, 16f), script_name.txt_name[language, 0], "txt12_0");
			}
			if (GUI.Button(Crypto.Rect2(208f, 200f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				pop_general = 1;
			}
		}
		else if (pop_general == 4)
		{
			GUI.Label(Crypto.Rect2(202f, 90f, 150f, 16f), Language.intxt[language, 181], "txt12_0");
			GUI.Label(Crypto.Rect2(120f, 206f, 150f, 16f), Language.intxt[language, 185], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(208f, 120f, 64f, 64f), arrow_combine);
			GUI.DrawTexture(Crypto.Rect2(280f, 114f, 64f, 64f), bg_inherit);
			if (GUI.Button(Crypto.Rect2(280f, 200f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				sel_general = comb_general;
				g_exp_length = comb_exp_length;
				GeneralStat(general_seed[sel_general], _exist: true);
				pop_general = 1;
			}
		}
		else if (pop_general < 7)
		{
			GUI.DrawTexture(Crypto.Rect2(posX2, 97f, 64f, 64f), black_all);
			GUI.DrawTexture(Crypto.Rect2(iconmove.x, iconmove.y, 64f, 64f), prt_general[sel_general]);
			GUI.DrawTexture(Crypto.Rect2(posX2, 82f, 64f, 110f), bg_general);
			GUI.Label(Crypto.Rect2(posX2 + 4f, 163f, 70f, 16f), string.Empty + g_level + " / " + (g_grade + 1) * 20, "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(posX2 + 3f, 184f, g_exp_length, 5f), gauge_exp);
			for (int n = 0; n < g_grade + 1; n++)
			{
				GUI.DrawTexture(Crypto.Rect2(posX2 + 26f - (float)(g_grade * 5) + (float)(n * 10), 84f, 12f, 12f), star_grade);
			}
			if (pop_general != 5)
			{
				return;
			}
			GUI.DrawTexture(Crypto.Rect2(208f, 120f, 64f, 64f), arrow_combine);
			if (GUI.Button(Crypto.Rect2(208f, 200f, 64f, 32f), Language.intxt[language, 177], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (comb_level < (comb_grade + 1) * 20)
				{
					confirm = 10;
				}
				else
				{
					Delay(1f);
					confirm = 9;
				}
			}
			else if (GUI.Button(Crypto.Rect2(280f, 200f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				pop_general = 4;
			}
			if (tutorial == 4 && confirm == 0)
			{
				GUI.DrawTexture(Crypto.Rect2(210f, 184f, icon_size * 64, icon_size * 64), please_touch);
			}
		}
		else if (pop_general != 9)
		{
			GUI.DrawTexture(Crypto.Rect2(posX2, 97f, 64f, 64f), prt_general[sel_general]);
		}
	}

	public void CashshopOpen()
	{
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
		bg_popui.localScale = Vector3.zero;
	}

	public void jumpSence(int type)
	{
		switch (type)
		{
		case 0:
			Application.LoadLevel("Map");
			break;
		case 1:
			Application.LoadLevel("Extreme");
			break;
		}
	}

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 240f, 480f, 100f), black);
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
		GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
		GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
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
		if (show_fortune_teller > 0)
		{
			GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 256f, 490f, 74f), black_all);
			if (f_delay == 0f)
			{
				if (GUI.Button(Crypto.Rect2(390f, 274f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					fortune_teller.position = Vector3.one * 5f;
					fortune_teller.gameObject.active = false;
					show_fortune_teller = 0;
					pop_general = 1;
					bg_popui.localScale = Vector3.one * 5.2f;
					bg_popui.localPosition = new Vector3(0f, -0.051f, 0.5f);
				}
				GUI.Label(Crypto.Rect2(0f, 270f, 480f, 32f), Language.intxt[language, 186], "txt12_w");
				GUI.Label(Crypto.Rect2(208f, 112f, 64f, 16f), Language.intxt[language, 141], "txt12_w");
				GUI.Label(Crypto.Rect2(208f, 232f, 64f, 16f), "HP", "txt12_w");
				GUI.Label(Crypto.Rect2(140f, 172f, 64f, 16f), Language.intxt[language, 142], "txt12_w");
				GUI.Label(Crypto.Rect2(276f, 172f, 64f, 16f), Language.intxt[language, 295], "txt12_w");
			}
			else
			{
				GUI.Label(Crypto.Rect2(0f, 270f, 480f, 32f), Language.intxt[language, 188], "txt12_w");
			}
			GUI.Box(Crypto.Rect2(96f, 84f, 80f, 24f), Language.intxt[language, 99]);
			return;
		}
		for (int i = 0; i < 12; i++)
		{
			if (GUI.Button(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY, 64f, 64f), string.Empty, bt_general))
			{
				if (scrollOn)
				{
					break;
				}
				if (giftmode)
				{
					if (showcard > 0)
					{
						break;
					}
					giftmode = false;
				}
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (pop_general >= 4)
				{
					if (pop_general >= 6 || comb_general == i)
					{
						break;
					}
					if (general_seed[i] != 0)
					{
						iconmove = new Vector2((float)(i * 76 + 18) + dragposX, icon_posY);
						sel_general = i;
						GeneralStat(general_seed[i], _exist: true);
						pop_general = 5;
						break;
					}
				}
				else if (changemode)
				{
					if (i < slot_general)
					{
						int num = general_seed[i];
						int num2 = general_hp[i];
						int num3 = general_cur_hp[i];
						int num4 = general_exp[i];
						Texture2D texture2D = prt_general[i];
						general_seed[i] = general_seed[sel_general];
						general_hp[i] = general_hp[sel_general];
						general_cur_hp[i] = general_cur_hp[sel_general];
						general_exp[i] = general_exp[sel_general];
						prt_general[i] = prt_general[sel_general];
						general_seed[sel_general] = num;
						general_hp[sel_general] = num2;
						general_cur_hp[sel_general] = num3;
						general_exp[sel_general] = num4;
						prt_general[sel_general] = texture2D;
						if (sel_general == cur_general)
						{
							cur_general = i;
						}
						GeneralKindOnly();
						sel_general = i;
						GeneralStat(general_seed[sel_general], _exist: true);
						changemode = false;
						Crypto.Save_int_key("cur_general", cur_general);
						PlayerPrefsX.SetIntArray("n13", general_seed);
						PlayerPrefsX.SetIntArray("n29", general_exp);
						PlayerPrefsX.SetIntArray("n33", general_hp);
						pop_general = 1;
						bg_popui.localScale = Vector3.one * 5.2f;
					}
					else
					{
						changemode = false;
						pop_general = 1;
						bg_popui.localScale = Vector3.one * 5.2f;
					}
				}
				else if (general_seed[i] != 0)
				{
					sel_general = i;
					GeneralStat(general_seed[i], _exist: true);
					pop_general = 1;
					bg_popui.localScale = Vector3.one * 5.2f;
					bg_popui.localPosition = new Vector3(0f, -0.051f, 0.5f);
				}
				else if (i >= slot_general)
				{
					pop_general = 0;
					bg_popui.localScale = Vector3.zero;
					confirm = 2;
				}
			}
			if ((pop_general >= 5 && sel_general == i) || general_seed[i] <= 0)
			{
				continue;
			}
			if (cur_general == i)
			{
				GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY, 64f, 64f), sel_mark);
			}
			GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY, 64f, 64f), prt_general[i]);
			for (int j = 0; j < general_grade[i] + 1; j++)
			{
				GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 44) + dragposX - (float)(general_grade[i] * 5) + (float)(j * 10), icon_posY, 12f, 12f), star_grade);
			}
			if (general_level[i] >= (general_grade[i] + 1) * 20)
			{
				GUI.Box(Crypto.Rect2((float)(i * 76 + 28) + dragposX, icon_posY + 52f, 44f, 18f), "MAX");
			}
			else
			{
				GUI.Box(Crypto.Rect2((float)(i * 76 + 28) + dragposX, icon_posY + 52f, 44f, 18f), "LV " + general_level[i]);
			}
			if (tutorial != 4 || general_seed[2] == 0)
			{
				continue;
			}
			if (pop_general == 4)
			{
				if (i == 1)
				{
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 22) + dragposX, icon_posY + 4f, icon_size * 64, icon_size * 64), please_touch);
				}
			}
			else if (pop_general == 1)
			{
				if (i == 2 && sel_general != 2)
				{
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 22) + dragposX, icon_posY + 4f, icon_size * 64, icon_size * 64), please_touch);
				}
			}
			else if (pop_general == 0 && i == 2)
			{
				GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 22) + dragposX, icon_posY + 4f, icon_size * 64, icon_size * 64), please_touch);
			}
		}
		for (int k = slot_general; k < 12; k++)
		{
			GUI.DrawTexture(Crypto.Rect2((float)(k * 76 + 18) + dragposX, icon_posY, 64f, 64f), ico_lock);
			GUI.DrawTexture(Crypto.Rect2((float)(k * 76 + 24) + dragposX, icon_posY + 6f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2((float)(k * 76 + 44) + dragposX, icon_posY + 6f, 32f, 16f), string.Empty + 3, "txt12_w");
		}
		if (showcard > 0)
		{
			if (showcard == 4)
			{
				GUI.DrawTexture(Crypto.Rect2(130f, 54f, 220f, 18f), bg_titlename);
				GUI.DrawTexture(Crypto.Rect2(140f, 97f, 64f, 64f), prt_general[getslot]);
				GUI.DrawTexture(Crypto.Rect2(140f, 82f, 64f, 110f), bg_general);
				GUI.Label(Crypto.Rect2(144f, 163f, 70f, 16f), string.Empty + g_level + " / " + (g_grade + 1) * 20, "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(143f, 184f, g_exp_length, 5f), gauge_exp);
				for (int l = 0; l < g_grade + 1; l++)
				{
					GUI.DrawTexture(Crypto.Rect2(166 - g_grade * 5 + l * 10, 84f, 12f, 12f), star_grade);
				}
				GUI.Label(Crypto.Rect2(130f, 54f, 220f, 16f), script_name.txt_name[language, 21 + general_index], "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(322f, 50f, 26f, 26f), general_weapon[general_kind]);
				for (int m = 0; m < 3; m++)
				{
					GUI.DrawTexture(Crypto.Rect2(220f, 114 + m * 20, 128f, 16f), titlebase);
				}
				GUI.DrawTexture(Crypto.Rect2(220f, 174f, 128f, 16f), titlebase3);
				GUI.DrawTexture(Crypto.Rect2(220f, 82f, 128f, 32f), hpbase);
				GUI.DrawTexture(Crypto.Rect2(225f, 101f, 112f * (float)general_cur_hp[sel_general] / (float)g_maxhp, 5f), gauge_hp);
				GUI.Label(Crypto.Rect2(190f, 81f, 128f, 16f), "HP", "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 114f, 128f, 16f), Language.intxt[language, 141], "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 134f, 128f, 16f), Language.intxt[language, 142], "txt12_0");
				GUI.Label(Crypto.Rect2(190f, 154f, 128f, 16f), Language.intxt[language, 295], "txt12_0");
				GUI.Label(Crypto.Rect2(260f, 81f, 100f, 16f), general_cur_hp[sel_general] + " / " + g_maxhp, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 114f, 64f, 16f), string.Empty + g_maxatk, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 134f, 64f, 16f), string.Empty + g_def, "txt12_0");
				GUI.Label(Crypto.Rect2(290f, 154f, 64f, 16f), string.Empty + (g_atkspd * 100f).ToString("F1"), "txt12_0");
				if (g_grade >= 2)
				{
					GUI.Label(Crypto.Rect2(220f, 174f, 128f, 16f), script_name.txt_name[language, 1 + general_index], "txt12_b");
				}
				else
				{
					GUI.Label(Crypto.Rect2(220f, 174f, 128f, 16f), script_name.txt_name[language, 0], "txt12_0");
				}
				if (GUI.Button(Crypto.Rect2(280f, 200f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					showcard = 0;
					bg_popui.localScale = Vector3.zero;
				}
			}
		}
		else if (giftmode)
		{
			GiftMode();
			if (GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				giftmode = false;
				bg_popui.localScale = Vector3.zero;
				pop_general = 0;
			}
		}
		else
		{
			if (GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				if (Crypto.Load_int_key("gamemode") == 0)
				{
					Application.LoadLevel("Map");
				}
				else
				{
					Application.LoadLevel("Extreme");
				}
			}
			if (pop_general < 4)
			{
				GUI.DrawTexture(Crypto.Rect2(16f, 182f, 40f, 40f), card_general[2]);
				if (GUI.Button(Crypto.Rect2(4f, 170f, 64f, 64f), string.Empty, bt_empty))
				{
					giftmode = true;
					bg_popui.localScale = Vector3.zero;
				}
				if (totalcount > 0)
				{
					GUI.DrawTexture(Crypto.Rect2(34f, 196f, icon_size * 32, 32f), icon_alarm);
				}
			}
			GUI.DrawTexture(Crypto.Rect2(4f, 106f, 64f, 64f), bt_general_small);
			if (cur_general != -1)
			{
				GUI.DrawTexture(Crypto.Rect2(16f, 118f, 40f, 40f), prt_general[cur_general]);
				GUI.DrawTexture(Crypto.Rect2(18f, 161f, 36f * Mathf.Min(1f, (float)general_cur_hp[cur_general] / cur_generalmaxhp), 4f), gauge_hp);
				if (general_cur_hp[cur_general] < 20)
				{
					GUI.DrawTexture(Crypto.Rect2(16f, 118f, icon_size * 40, 40f), bg_red);
				}
				if (pop_general < 4 && !changemode && GUI.Button(Crypto.Rect2(4f, 106f, 64f, 64f), string.Empty, bt_empty))
				{
					sel_general = cur_general;
					GeneralStat(general_seed[cur_general], _exist: true);
					pop_general = 1;
					bg_popui.localScale = Vector3.one * 5.2f;
					bg_popui.localPosition = new Vector3(0f, -0.051f, 0.5f);
				}
			}
			else
			{
				GUI.DrawTexture(Crypto.Rect2(16f, 118f, icon_size * 40, 40f), empty);
				if (pop_general == 0)
				{
					GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
					GUI.Label(Crypto.Rect2(112f, 120f, 256f, 32f), Language.intxt[language, 297], "txt12_0");
				}
			}
			if (pop_general > 0)
			{
				GeneralMode();
			}
			if (confirm == 0 && changemode)
			{
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(112f, 120f, 256f, 16f), Language.intxt[language, 394], "txt12_0");
				if (GUI.Button(Crypto.Rect2(208f, 138f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					bg_popui.localScale = Vector3.one * 5.2f;
					changemode = false;
					pop_general = 1;
				}
			}
		}
		if (confirm <= 0)
		{
			return;
		}
		GUI.enabled = true;
		switch (confirm)
		{
		case 2:
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.DrawTexture(Crypto.Rect2(160f, 98f, 160f, 18f), bg_titlename);
			GUI.Label(Crypto.Rect2(120f, 98f, 240f, 16f), Language.intxt[language, 236] + " +1", "txt12_w");
			GUI.Label(Crypto.Rect2(120f, 120f, 240f, 16f), Language.intxt[language, 300], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(186f, 134f, 32f, 32f), icon_unlock);
			GUI.DrawTexture(Crypto.Rect2(244f, 142f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(264f, 142f, 16f, 16f), string.Empty + 3, "txt12_b");
			GUI.Label(Crypto.Rect2(112f, 162f, 256f, 16f), Language.intxt[language, 350], "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (jade >= 3)
				{
					if (Crypto.Property_change(-3, _jade: true))
					{
						Delay(1f);
						confirm = 18;
						jade -= 3;
						slot_general++;
						Crypto.Save_int_key("n09", slot_general);
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + 3 + ")\t" + Language.intxt[language, 236]);
					}
				}
				else
				{
					confirm = 6;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
				if (cur_general >= 0)
				{
					sel_general = cur_general;
					GeneralStat(general_seed[sel_general], _exist: true);
					pop_general = 1;
					bg_popui.localScale = Vector3.one * 5.2f;
					bg_popui.localPosition = new Vector3(0f, -0.051f, 0.5f);
				}
				else
				{
					pop_general = 0;
				}
			}
			break;
		case 3:
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(120f, 130f, 230f, 16f), Language.intxt[language, 298], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 2;
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
		case 4:
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(120f, 120f, 240f, 14f), Language.intxt[language, 299], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 138f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				pop_general = 0;
				bg_popui.localScale = Vector3.zero;
				general_seed[sel_general] = 0;
				general_exp[sel_general] = 0;
				if (sel_general == cur_general)
				{
					cur_general = -1;
					Crypto.Save_int_key("cur_general", cur_general);
				}
				PlayerPrefsX.SetIntArray("n13", general_seed);
				PlayerPrefsX.SetIntArray("n29", general_exp);
				confirm = 5;
				rnd_speech = Random.Range(101, 106);
				Delay(1.5f);
			}
			else if (GUI.Button(Crypto.Rect2(240f, 138f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
			}
			break;
		case 5:
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.DrawTexture(Crypto.Rect2(130f, 105f, 64f, 64f), prt_general[sel_general]);
			GUI.Label(Crypto.Rect2(192f, 110f, 176f, 64f), script_name.txt_name[language, rnd_speech], "txt12_0");
			if (f_delay == 0f)
			{
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
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
				if (cur_general >= 0)
				{
					sel_general = cur_general;
					GeneralStat(general_seed[sel_general], _exist: true);
					pop_general = 1;
					bg_popui.localScale = Vector3.one * 5.2f;
				}
				else
				{
					pop_general = 0;
				}
			}
			break;
		case 7:
			GUI.DrawTexture(Crypto.Rect2(146f, 188f, 192f, 48f), bg_speech);
			GUI.Label(Crypto.Rect2(154f, 190f, 176f, 48f), script_name.txt_name[language, rnd_speech], "txt12_0");
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
			GUI.Label(Crypto.Rect2(120f, 110f, 240f, 64f), Language.intxt[language, 267], "txt12_0");
			if (f_delay == 0f)
			{
				confirm = 0;
			}
			break;
		case 10:
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(120f, 120f, 240f, 14f), Language.intxt[language, 182], "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 138f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
				pop_general = 6;
				bg_combine.gameObject.active = true;
				Delay(1f);
				get_exp = 110 + g_grade * 100 + general_exp[sel_general];
				for (int n = 0; n < g_level - 1; n++)
				{
					get_exp += (float)(100 + n * 6) * (1f + (float)g_grade * 0.2f) + 10f;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 138f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
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
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(120f, 110f, 240f, 64f), Language.intxt[language, 392], "txt12_0");
			if (f_delay == 0f)
			{
				if (!giftmode && cur_general >= 0)
				{
					sel_general = cur_general;
					GeneralStat(general_seed[sel_general], _exist: true);
					pop_general = 1;
					bg_popui.localScale = Vector3.one * 5.2f;
					bg_popui.localPosition = new Vector3(0f, -0.051f, 0.5f);
				}
				confirm = 0;
			}
			break;
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
			break;
		}
	}
}
