using System.Collections;
using UnityEngine;

public class UI_skill : MonoBehaviour
{
	private const int MAXSKILL = 20;

	public Texture2D bg_skill;

	public Texture2D txt_name;

	public Texture2D bg_black;

	public Texture2D bg_black100;

	public Texture2D pop_detail;

	public Texture2D pop_blank;

	public Texture2D pop_blank2;

	private Texture2D cost_icon;

	public Texture2D bg_asset;

	public Texture2D titlebase;

	public Texture2D titlebase_w;

	public Texture2D arrow;

	public Texture2D active_slot;

	public Texture2D icon_jade;

	public Texture2D icon_coin;

	public Texture2D icon_soul;

	public Texture2D please_touch;

	public Texture2D[] skillicon = new Texture2D[20];

	public Texture2D[] icon_diamond = new Texture2D[3];

	public GUIStyle bt_skill;

	public GUIStyle bt_yesno;

	public GUIStyle bt_back;

	public GUIStyle bt_skill_slot;

	public GUIStyle bt_empty;

	public GUISkin basicSkin;

	public Transform sound_dummy;

	private GameObject cashshop;

	private Transform sound_UI;

	private SoundEf_UI script_soundUI;

	private float bg_posX_l;

	private float bg_posX_r;

	private float icon_posY;

	private int selectskill;

	private bool popupOn;

	private bool b_delay;

	private float f_delay;

	private short confirm;

	private bool dragOn;

	private bool scrollOn;

	private float dragposX;

	private float prevposX;

	private float currentX;

	private float dragrange;

	private int chalevel;

	private int coin;

	private int jade;

	private string updatestring;

	private bool imagemovefinish;

	public int[] cur_skill_grade = new int[20];

	private skillset[,] ss = new skillset[20, 5];

	private int[] skill_slot = new int[5];

	private int language;

	private int emptyslot = -1;

	private int tutorial;

	private int icon_size;

	private string[] ss_url = new string[20];

	private Texture2D ss_skill;

	private bool imageloading;

	private bool showimg;

	private WWW www;

	private void Awake()
	{
		ss_url[0] = "http://ozd.dn.hangame.com/smart/ozd/udi/01_Skill_1.jpg";
		ss_url[1] = "http://ozd.dn.hangame.com/smart/ozd/udi/02_Skill_2.jpg";
		ss_url[2] = "http://ozd.dn.hangame.com/smart/ozd/udi/03_Skill_1.jpg";
		ss_url[3] = "http://ozd.dn.hangame.com/smart/ozd/udi/04_Skill_1.jpg";
		ss_url[4] = "http://ozd.dn.hangame.com/smart/ozd/udi/05_Skill_1.jpg";
		ss_url[5] = "http://ozd.dn.hangame.com/smart/ozd/udi/06_Skill_1.jpg";
		ss_url[6] = "http://ozd.dn.hangame.com/smart/ozd/udi/07_Skill_1.jpg";
		ss_url[7] = "http://ozd.dn.hangame.com/smart/ozd/udi/08_Skill_1.jpg";
		ss_url[8] = "http://ozd.dn.hangame.com/smart/ozd/udi/09_Skill_1.jpg";
		ss_url[9] = "http://ozd.dn.hangame.com/smart/ozd/udi/10_Skill_1.jpg";
		ss_url[10] = "http://ozd.dn.hangame.com/smart/ozd/udi/11_Skill_2.jpg";
		ss_url[11] = "http://ozd.dn.hangame.com/smart/ozd/udi/12_Skill_1.jpg";
		ss_url[12] = "http://ozd.dn.hangame.com/smart/ozd/udi/13_Skill_1.jpg";
		ss_url[13] = "http://ozd.dn.hangame.com/smart/ozd/udi/14_Skill_1.jpg";
		ss_url[14] = "http://ozd.dn.hangame.com/smart/ozd/udi/15_Skill_2.jpg";
		ss_url[15] = "http://ozd.dn.hangame.com/smart/ozd/udi/16_Skill_1.jpg";
		ss_url[16] = "http://ozd.dn.hangame.com/smart/ozd/udi/17_Skill_2.jpg";
		ss_url[17] = "http://ozd.dn.hangame.com/smart/ozd/udi/18_Skill_2.jpg";
		ss_url[18] = "http://ozd.dn.hangame.com/smart/ozd/udi/19_Skill_2.jpg";
		ss_url[19] = "http://ozd.dn.hangame.com/smart/ozd/udi/20_Skill_2.jpg";
	}

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
	}

	private IEnumerator ShotLoading(int _index)
	{
		www = new WWW(ss_url[_index]);
		yield return www;
		if (www.error == null)
		{
			ss_skill = www.texture;
		}
		imageloading = true;
	}

	private void Start()
	{
		skill_slot = PlayerPrefsX.GetIntArray("skill_slot");
		chalevel = Crypto.Load_int_key("n47");
		dragrange = (float)Screen.height * 0.375f;
		language = PlayerPrefs.GetInt("language");
		bg_posX_l = -380f;
		bg_posX_r = 480f;
		icon_posY = 340f;
		ss = GetComponent<DB_Skill>().ss;
		tutorial = Crypto.Load_int_key("tutorial");
		cur_skill_grade = PlayerPrefsX.GetIntArray("n22");
		if (GameObject.FindWithTag("sound") == null)
		{
			sound_UI = (Transform)Object.Instantiate(sound_dummy, Vector3.zero, Quaternion.identity);
		}
		else
		{
			sound_UI = GameObject.FindWithTag("sound").transform;
		}
		if (sound_UI != null)
		{
			script_soundUI = sound_UI.GetComponent<SoundEf_UI>();
		}
		for (int i = 0; i < 20; i++)
		{
			if (cur_skill_grade[i] == -2 && chalevel >= ss[i, 0]._requireLV)
			{
				cur_skill_grade[i] = -1;
			}
		}
		PlayerPrefsX.SetIntArray("n22", cur_skill_grade);
		PlayerPrefs.SetInt("updateRank", 1);
		InvokeRepeating("Warning_iconsize", 0.1f, 0.34f);
		PlayerPrefs.SetInt("currentSence", 3);
	}

	public void UpdateSkillStat(int _skillindex)
	{
		short txtkind = ss[_skillindex, 0]._txtkind;
		if (cur_skill_grade[_skillindex] < 4)
		{
			switch (txtkind)
			{
			case 1:
				updatestring = Language.intxt[language, 273];
				break;
			case 2:
				updatestring = Language.intxt[language, 97];
				break;
			default:
				updatestring = Language.intxt[language, 274];
				break;
			}
		}
	}

	public void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
	}

	public void Delay(float t)
	{
		b_delay = true;
		f_delay = t;
	}

	public int FindEmptySlot()
	{
		emptyslot = -1;
		for (int i = 0; i < 5; i++)
		{
			if (skill_slot[i] == -1)
			{
				emptyslot = i;
				break;
			}
		}
		return emptyslot;
	}

	public void ShortCut(int _index)
	{
		if (selectskill == skill_slot[_index])
		{
			skill_slot[_index] = -1;
			if (tutorial == -2)
			{
				tutorial = 0;
				Crypto.Save_int_key("tutorial", 0);
			}
		}
		else
		{
			for (int i = 0; i < 5; i++)
			{
				if (skill_slot[i] == selectskill)
				{
					skill_slot[i] = -1;
				}
			}
			skill_slot[_index] = selectskill;
			if (tutorial == 0)
			{
				tutorial = -2;
				Crypto.Save_int_key("tutorial", -2);
				Delay(1f);
				confirm = 7;
			}
		}
		PlayerPrefsX.SetIntArray("skill_slot", skill_slot);
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
		if (!imagemovefinish)
		{
			if (bg_posX_l < -64f)
			{
				bg_posX_l += Mathf.Min(0f - bg_posX_l, Time.deltaTime * 600f);
			}
			else
			{
				bg_posX_l = -64f;
				bg_posX_r -= Mathf.Min(bg_posX_r, Time.deltaTime * 1500f);
				if (bg_posX_r <= 0f)
				{
					bg_posX_r = 0f;
					icon_posY -= Mathf.Min(icon_posY, Time.deltaTime * 500f);
					if (icon_posY <= 230f)
					{
						icon_posY = 230f;
						imagemovefinish = true;
					}
				}
			}
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
			dragposX = Mathf.Max(dragposX, -1060f);
		}
		int num = PlayerPrefs.GetInt("updateJAndG");
		if (num == 1)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetInt("updateJAndG", 0);
		}
	}

	public void CashshopOpen()
	{
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
	}

	public void jumpSence(int type)
	{
		popupOn = false;
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
		if (confirm > 0 || showimg)
		{
			GUI.enabled = false;
		}
		GUI.DrawTexture(Crypto.Rect2(bg_posX_l, 0f, 320f, 320f), bg_skill);
		GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 224f, 480f, 100f), bg_black);
		GUI.DrawTexture(Crypto.Rect2(120f, 30f, 100f, 50f), txt_name);
		GUI.Box(Crypto.Rect2(190f, 70f, 80f, 24f), Language.intxt[language, 68]);
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
		for (int i = 0; i < 20; i++)
		{
			if (!((float)(i * 152) + dragposX * 2f > -152f) || !((float)(i * 152) + dragposX * 2f < 1100f))
			{
				continue;
			}
			if (cur_skill_grade[i] == -2)
			{
				GUI.Label(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY + 74f, 64f, 14f), Language.intxt[language, 266] + ss[i, 0]._requireLV, "txt12_r");
				GUI.color = Color.gray;
				GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY + 8f, 64f, 64f), skillicon[i]);
				GUI.color = Color.white;
			}
			else if (cur_skill_grade[i] >= 4)
			{
				GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY + 8f, 64f, 64f), skillicon[i]);
				GUI.Label(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY - 6f, 64f, 14f), Language.intxt[language, 60], "txt12_w");
			}
			else
			{
				if (ss[i, cur_skill_grade[i] + 1]._pricekind == 1)
				{
					cost_icon = icon_jade;
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 22) + dragposX, icon_posY + 74f, 16f, 16f), cost_icon);
					GUI.Label(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY + 74f, 64f, 14f), "\t  " + ss[i, cur_skill_grade[i] + 1]._price, "txt12_w");
				}
				else if (cur_skill_grade[i] != -1)
				{
					cost_icon = icon_coin;
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 22) + dragposX, icon_posY + 74f, 16f, 16f), cost_icon);
					GUI.Label(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY + 74f, 64f, 14f), "\t  " + ss[i, cur_skill_grade[i] + 1]._price, "txt12_w");
				}
				GUI.DrawTexture(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY + 8f, 64f, 64f), skillicon[i]);
				if (cur_skill_grade[i] >= 0)
				{
					GUI.Label(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY - 6f, 64f, 14f), "Level " + (cur_skill_grade[i] + 1), "txt12_w");
				}
				else
				{
					GUI.Label(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY - 6f, 64f, 14f), Language.intxt[language, 296], "txt12_w");
				}
			}
			if (GUI.Button(Crypto.Rect2((float)(i * 76 + 18) + dragposX, icon_posY + 8f, 64f, 64f), string.Empty, bt_skill))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (!scrollOn)
				{
					selectskill = i;
					UpdateSkillStat(selectskill);
					popupOn = true;
				}
			}
		}
		if (popupOn)
		{
			GUI.Box(Crypto.Rect2(180f, 44f, 256f, 24f), Language.intxt[language, 94]);
			GUI.DrawTexture(Crypto.Rect2(180f, 70f, 256f, 128f), pop_detail);
			GUI.DrawTexture(Crypto.Rect2(196f, 104f, 64f, 64f), skillicon[selectskill]);
			GUI.Label(Crypto.Rect2(180f, 81f, 256f, 14f), Language.intxt[language, ss[selectskill, 0]._name], "txt12_w");
			GUI.Label(Crypto.Rect2(264f, 90f, 160f, 64f), Language.intxt[language, ss[selectskill, 0]._info], "txt12_0");
			GUI.Box(Crypto.Rect2(194f, 79f, 32f, 20f), "    " + ss[selectskill, 0]._soulprice);
			GUI.DrawTexture(Crypto.Rect2(196f, 81f, 16f, 16f), icon_soul);
			if (cur_skill_grade[selectskill] == -2)
			{
				GUI.Label(Crypto.Rect2(196f, 169f, 64f, 14f), Language.intxt[language, 266] + ss[selectskill, 0]._requireLV, "txt12_0");
			}
			else
			{
				if (cur_skill_grade[selectskill] < 4)
				{
					if (ss[selectskill, cur_skill_grade[selectskill] + 1]._pricekind == 1)
					{
						cost_icon = icon_jade;
					}
					else
					{
						cost_icon = icon_coin;
					}
					GUI.DrawTexture(Crypto.Rect2(196f, 169f, 16f, 16f), cost_icon);
					GUI.Label(Crypto.Rect2(206f, 169f, 58f, 14f), string.Empty + ss[selectskill, cur_skill_grade[selectskill] + 1]._price, "txt12_0");
				}
				string text = ((cur_skill_grade[selectskill] < 0) ? Language.intxt[language, 209] : Language.intxt[language, 7]);
				if (cur_skill_grade[selectskill] < 4)
				{
					GUI.Label(Crypto.Rect2(280f, 140f, 128f, 16f), Language.intxt[language, 95] + "   " + (cur_skill_grade[selectskill] + 1), "txt12_b");
					if (GUI.Button(Crypto.Rect2(280f, 158f, 64f, 32f), text, bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						confirm = 1;
					}
				}
				else
				{
					GUI.Label(Crypto.Rect2(196f, 169f, 64f, 14f), Language.intxt[language, 60], "txt12_b");
				}
			}
			if (GUI.Button(Crypto.Rect2(350f, 158f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				popupOn = false;
			}
			for (int j = 0; j < 5; j++)
			{
				if (skill_slot[j] >= 0)
				{
					GUI.DrawTexture(Crypto.Rect2(446f, 42 + j * 36, 28f, 28f), skillicon[skill_slot[j]]);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(446f, 42 + j * 36, 28f, 28f), bg_black);
				}
				if (GUI.Button(Crypto.Rect2(444f, 40 + j * 36, 32f, 32f), string.Empty + (j + 1), bt_skill_slot))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (cur_skill_grade[selectskill] < 0)
					{
						confirm = 4;
						Delay(1f);
					}
					else
					{
						ShortCut(j);
					}
				}
				if (selectskill == skill_slot[j])
				{
					GUI.DrawTexture(Crypto.Rect2(446f, 42 + j * 36, icon_size * 28, icon_size * 28), active_slot);
				}
			}
			if (showimg)
			{
				GUI.enabled = true;
				GUI.DrawTexture(Crypto.Rect2(97f, 24f, 286f, 286f), bg_black100);
				if (imageloading)
				{
					if (www.error == null && ss_skill != null)
					{
						GUI.DrawTexture(Crypto.Rect2(112f, 40f, 256f, 256f), ss_skill);
					}
					else
					{
						GUI.Label(Crypto.Rect2(60f, 40f, 360f, 240f), Language.intxt[language, 304], "txt12_w");
					}
				}
				else
				{
					GUI.Label(Crypto.Rect2(60f, 40f, 360f, 240f), Language.intxt[language, 391], "txt12_w");
				}
				if (GUI.Button(Crypto.Rect2(208f, 280f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					showimg = false;
					imageloading = false;
				}
			}
		}
		else
		{
			GUI.DrawTexture(Crypto.Rect2(180f, 90f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(188f, 104f, 240f, 32f), Language.intxt[language, 18], "txt12_0");
			if (GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				popupOn = false;
				if (Crypto.Load_int_key("gamemode") == 0)
				{
					Application.LoadLevel("Map");
				}
				else
				{
					Application.LoadLevel("Extreme");
				}
			}
		}
		if (confirm > 0)
		{
			GUI.enabled = true;
			if (confirm == 1)
			{
				if (cur_skill_grade[selectskill] >= 0)
				{
					GUI.DrawTexture(Crypto.Rect2(112f, 80f, 256f, 148f), pop_blank2);
					GUI.DrawTexture(Crypto.Rect2(160f, 89f, 160f, 20f), titlebase);
					GUI.Label(Crypto.Rect2(112f, 92f, 256f, 14f), Language.intxt[language, ss[selectskill, 0]._name], "txt12_w");
					GUI.DrawTexture(Crypto.Rect2(140f, 113f, 200f, 16f), titlebase_w);
					GUI.Label(Crypto.Rect2(130f, 112f, 128f, 16f), Language.intxt[language, 95], "txt12_0");
					GUI.Label(Crypto.Rect2(212f, 112f, 128f, 16f), cur_skill_grade[selectskill] + 1 + "           " + (cur_skill_grade[selectskill] + 2), "txt12_0");
					GUI.DrawTexture(Crypto.Rect2(268f, 112f, 16f, 16f), arrow);
					GUI.DrawTexture(Crypto.Rect2(140f, 133f, 200f, 16f), titlebase_w);
					GUI.Label(Crypto.Rect2(130f, 132f, 128f, 16f), updatestring, "txt12_0");
					GUI.Label(Crypto.Rect2(212f, 132f, 128f, 16f), ss[selectskill, cur_skill_grade[selectskill]]._attackpoint + "           " + ss[selectskill, cur_skill_grade[selectskill] + 1]._attackpoint, "txt12_0");
					GUI.DrawTexture(Crypto.Rect2(268f, 132f, 16f, 16f), arrow);
					GUI.DrawTexture(Crypto.Rect2(140f, 153f, 200f, 16f), titlebase_w);
					GUI.Label(Crypto.Rect2(130f, 152f, 128f, 16f), Language.intxt[language, 200], "txt12_0");
					GUI.Label(Crypto.Rect2(212f, 152f, 128f, 16f), ss[selectskill, cur_skill_grade[selectskill]]._cooltime + "           " + ss[selectskill, cur_skill_grade[selectskill] + 1]._cooltime, "txt12_0");
					GUI.DrawTexture(Crypto.Rect2(268f, 152f, 16f, 16f), arrow);
					GUI.DrawTexture(Crypto.Rect2(200f, 172f, 16f, 16f), cost_icon);
					GUI.Label(Crypto.Rect2(224f, 172f, 48f, 16f), string.Empty + ss[selectskill, cur_skill_grade[selectskill] + 1]._price, "txt12_b");
					if (ss[selectskill, cur_skill_grade[selectskill] + 1]._pricekind == 1)
					{
						GUI.Label(Crypto.Rect2(180f, 172f, 256f, 16f), Language.intxt[language, 350], "txt12_b");
					}
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(112f, 100f, 256f, 128f), pop_blank2);
					GUI.DrawTexture(Crypto.Rect2(160f, 109f, 160f, 20f), titlebase);
					GUI.Label(Crypto.Rect2(112f, 112f, 256f, 14f), Language.intxt[language, ss[selectskill, 0]._name], "txt12_w");
					if (ss[selectskill, cur_skill_grade[selectskill] + 1]._pricekind == 1)
					{
						GUI.Label(Crypto.Rect2(112f, 132f, 256f, 14f), Language.intxt[language, 91], "txt12_0");
						GUI.DrawTexture(Crypto.Rect2(200f, 152f, 16f, 16f), cost_icon);
						GUI.Label(Crypto.Rect2(224f, 152f, 48f, 16f), string.Empty + ss[selectskill, cur_skill_grade[selectskill] + 1]._price, "txt12_b");
						GUI.Label(Crypto.Rect2(112f, 172f, 256f, 16f), Language.intxt[language, 350], "txt12_b");
					}
					else
					{
						GUI.Label(Crypto.Rect2(112f, 152f, 256f, 14f), Language.intxt[language, 91], "txt12_0");
					}
				}
				if (GUI.Button(Crypto.Rect2(170f, 188f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (ss[selectskill, cur_skill_grade[selectskill] + 1]._pricekind == 1)
					{
						if (!Crypto.Property_change(-ss[selectskill, cur_skill_grade[selectskill] + 1]._price, _jade: true))
						{
							confirm = 3;
							return;
						}
						jade -= ss[selectskill, cur_skill_grade[selectskill] + 1]._price;
						cur_skill_grade[selectskill]++;
						PlayerPrefsX.SetIntArray("n22", cur_skill_grade);
						PlayerPrefs.SetInt("updateRank", 1);
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + ss[selectskill, cur_skill_grade[selectskill]]._price + ")\t" + Language.intxt[language, ss[selectskill, 0]._name]);
					}
					else
					{
						if (!Crypto.Property_change(-ss[selectskill, cur_skill_grade[selectskill] + 1]._price, _jade: false))
						{
							confirm = 2;
							return;
						}
						coin -= ss[selectskill, cur_skill_grade[selectskill] + 1]._price;
						cur_skill_grade[selectskill]++;
						PlayerPrefsX.SetIntArray("n22", cur_skill_grade);
						PlayerPrefs.SetInt("updateRank", 1);
					}
					if (tutorial == 0)
					{
						confirm = 0;
					}
					else if (cur_skill_grade[selectskill] < 0)
					{
						if (FindEmptySlot() >= 0)
						{
							confirm = 5;
						}
						else
						{
							confirm = 0;
						}
					}
					else
					{
						confirm = 0;
					}
				}
				else if (GUI.Button(Crypto.Rect2(240f, 188f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					confirm = 0;
				}
			}
			else if (confirm == 2)
			{
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
					if (cashshop == null)
					{
						cashshop = Resources.Load("CashShop") as GameObject;
					}
					Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
				}
				else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					confirm = 0;
				}
			}
			else if (confirm == 3)
			{
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
					if (cashshop == null)
					{
						cashshop = Resources.Load("CashShop") as GameObject;
					}
					Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
				}
				else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					confirm = 0;
				}
			}
			else if (confirm == 4)
			{
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 210], "txt12_0");
				if (f_delay == 0f)
				{
					confirm = 0;
				}
			}
			else if (confirm == 5)
			{
				GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
				GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 92], "txt12_0");
				if (GUI.Button(Crypto.Rect2(170f, 138f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					confirm = 0;
					ShortCut(emptyslot);
				}
				else if (GUI.Button(Crypto.Rect2(240f, 138f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					confirm = 0;
				}
			}
			else if (confirm == 6)
			{
				GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 98]);
				if (f_delay == 0f)
				{
					confirm = 0;
				}
			}
			else if (confirm == 7)
			{
				GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 256]);
				if (f_delay == 0f)
				{
					confirm = 0;
				}
			}
		}
		if (tutorial != 0)
		{
			return;
		}
		if (!popupOn)
		{
			GUI.DrawTexture(Crypto.Rect2(24f + dragposX, icon_posY + 16f, icon_size * 64, icon_size * 64), please_touch);
		}
		else
		{
			if (showimg)
			{
				return;
			}
			if (selectskill == 0)
			{
				if (cur_skill_grade[0] >= 0)
				{
					GUI.DrawTexture(Crypto.Rect2(432f, 24f, icon_size * 64, icon_size * 64), please_touch);
				}
				else if (confirm == 1)
				{
					GUI.DrawTexture(Crypto.Rect2(174f, 174f, icon_size * 64, icon_size * 64), please_touch);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(284f, 142f, icon_size * 64, icon_size * 64), please_touch);
				}
			}
			else if (selectskill > 0)
			{
				Delay(1f);
				popupOn = false;
				confirm = 6;
			}
			else
			{
				GUI.DrawTexture(Crypto.Rect2(24f + dragposX, icon_posY + 16f, icon_size * 64, icon_size * 64), please_touch);
			}
		}
	}
}
