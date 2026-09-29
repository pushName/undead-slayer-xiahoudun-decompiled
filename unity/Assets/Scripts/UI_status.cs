using UnityEngine;

public class UI_status : MonoBehaviour
{
	public Texture2D bg_shop;

	public Texture2D bg_black;

	public Texture2D pop_detail;

	public Texture2D pop_blank;

	public Texture2D pop_blank2;

	public Texture2D cost_icon;

	public Texture2D bg_asset;

	public Texture2D arrow_upgrade;

	public Texture2D titlebase;

	public Texture2D gauge_exp;

	public Texture2D[] equipicon = new Texture2D[6];

	public Texture2D[] img_acce = new Texture2D[6];

	public GUIStyle bt_equip;

	public GUIStyle bt_yesno;

	public GUIStyle bt_back;

	public GUIStyle titlebase2;

	public GUIStyle bt_prev;

	public GUIStyle bt_next;

	public GUIStyle bt_empty;

	public GUIStyle bt_acce;

	public GUISkin basicSkin;

	private GameObject cashshop;

	private Transform sound_UI;

	public Transform sound_dummy;

	private SoundEf_UI script_soundUI;

	private float bg_posX_l;

	private float bg_posX_r;

	private float icon_posY;

	private int selectequip;

	private short confirm;

	private bool popup_equip_select;

	private bool imagemovefinish;

	private int coin;

	private int jade;

	private int rollstat;

	private int level;

	private int maxhp;

	private float exp;

	private int select_itemidx;

	private string[] thisitemStat = new string[6];

	private bool b_delay;

	private float f_delay;

	private int[] cur_equip_grade = new int[6];

	private equipitem[,] ei = new equipitem[6, 11];

	private int language;

	private int statinfo = 161;

	private int statname_idx;

	private int hpplus;

	private int hitrate;

	private int evasion;

	private int endurance;

	private int critical;

	private int[] accessory = new int[6];

	private void Awake()
	{
		level = Crypto.Load_int_key("n47");
		hpplus = Crypto.Load_int_key("n21");
		hitrate = Crypto.Load_int_key("n08");
		evasion = Crypto.Load_int_key("n30");
		endurance = Crypto.Load_int_key("n28");
		critical = Crypto.Load_int_key("n32");
	}

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
		accessory = PlayerPrefsX.GetIntArray("n36");
	}

	private void Start()
	{
		language = PlayerPrefs.GetInt("language");
		bg_posX_l = -380f;
		bg_posX_r = 480f;
		icon_posY = 340f;
		ei = GetComponent<DB_EquipItem>().ei;
		maxhp = 95 + level * 5;
		exp = Crypto.Load_int_key("n11");
		exp /= level * 100;
		cur_equip_grade = PlayerPrefsX.GetIntArray("n20");
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
		thisitemStat[0] = Language.intxt[language, 143];
		thisitemStat[1] = Language.intxt[language, 147];
		thisitemStat[2] = Language.intxt[language, 148];
		thisitemStat[3] = Language.intxt[language, 146];
		thisitemStat[4] = Language.intxt[language, 154];
		thisitemStat[5] = Language.intxt[language, 145];
		PlayerPrefs.SetInt("currentSence", 5);
	}

	public void Delay(float t)
	{
		b_delay = true;
		f_delay = t;
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
		int num = PlayerPrefs.GetInt("updateJAndG");
		if (num == 1)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetInt("updateJAndG", 0);
		}
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

	public void StatInfo_detail(int _index)
	{
		switch (_index)
		{
		case 0:
			statinfo = 163;
			break;
		case 1:
			statinfo = 162;
			break;
		case 2:
			statinfo = 167;
			break;
		case 3:
			statinfo = 165;
			break;
		case 4:
			statinfo = 161;
			break;
		case 5:
			statinfo = 170;
			break;
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

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		GUI.DrawTexture(Crypto.Rect2(bg_posX_l, 0f, 320f, 320f), bg_shop);
		GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 212f, 480f, 100f), bg_black);
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
		for (int i = 0; i < 6; i++)
		{
			if (cur_equip_grade[i] >= 10)
			{
				GUI.Label(Crypto.Rect2(i * 76 + 18, icon_posY - 14f, 64f, 14f), Language.intxt[language, 60], "txt12_w");
			}
			else
			{
				GUI.Label(Crypto.Rect2(i * 76 + 18, icon_posY - 14f, 64f, 14f), "Level " + (cur_equip_grade[i] + 1), "txt12_w");
			}
			GUI.DrawTexture(Crypto.Rect2(i * 76 + 18, icon_posY, 64f, 64f), equipicon[i]);
			if (GUI.Button(Crypto.Rect2(i * 76 + 18, icon_posY, 64f, 64f), string.Empty, bt_equip))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				selectequip = i;
				StatInfo_detail(selectequip);
				popup_equip_select = true;
			}
			if (cur_equip_grade[i] < 10)
			{
				GUI.DrawTexture(Crypto.Rect2(i * 76 + 22, icon_posY + 66f, 16f, 16f), cost_icon);
				GUI.Label(Crypto.Rect2(i * 76 + 18, icon_posY + 66f, 64f, 14f), "    " + ei[i, cur_equip_grade[i] + 1]._price, "txt12_w");
			}
		}
		if (!popup_equip_select)
		{
			GUI.DrawTexture(Crypto.Rect2(200f, 70f, 256f, 128f), pop_blank2);
			if (rollstat == 0)
			{
				GUI.DrawTexture(Crypto.Rect2(256f, 82f, 146f, 16f), titlebase);
				GUI.DrawTexture(Crypto.Rect2(258f, 84f, 142f * exp, 12f), gauge_exp);
				GUI.Label(Crypto.Rect2(266f, 83f, 128f, 14f), "LEVEL " + level, "txt14_w");
				GUI.Label(Crypto.Rect2(202f, 98f, 256f, 14f), "( " + Language.intxt[language, 61] + "   " + (0.03f * (float)level + 0.5f) * 10f + " % )", "txt12_b");
				GUI.Label(Crypto.Rect2(202f, 120f, 256f, 14f), Language.intxt[language, 154] + "   " + maxhp + " + " + ei[4, cur_equip_grade[4]]._capacity, "txt12_0");
				GUI.Label(Crypto.Rect2(202f, 138f, 256f, 14f), Language.intxt[language, 145] + "   " + ei[5, cur_equip_grade[5]]._capacity, "txt12_0");
				for (int j = 0; j < 6; j++)
				{
					if (GUI.Button(Crypto.Rect2(217 + j * 38, 158f, 32f, 32f), string.Empty, bt_acce))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						if (accessory[j] == 0)
						{
							confirm = 4;
						}
						else
						{
							select_itemidx = j;
							confirm = 5;
						}
					}
					if (accessory[j] == 0)
					{
						GUI.color = new Color(0f, 0f, 0f, 0.5f);
					}
					GUI.DrawTexture(Crypto.Rect2(217 + j * 38, 158f, 32f, 32f), img_acce[j]);
					GUI.color = Color.white;
				}
			}
			else if (rollstat == 1)
			{
				for (int k = 0; k < 4; k++)
				{
					if (GUI.Button(Crypto.Rect2(254f, 95 + k * 20, 156f, 16f), string.Empty, titlebase2))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(1);
						}
						statname_idx = k;
						StatInfo_detail(statname_idx);
						confirm = 3;
					}
					GUI.Label(Crypto.Rect2(174f, 96 + 20 * k, 256f, 14f), thisitemStat[k], "txt12_0");
				}
				GUI.Label(Crypto.Rect2(248f, 96f, 256f, 14f), string.Empty + hitrate, "txt12_0");
				GUI.Label(Crypto.Rect2(248f, 116f, 256f, 14f), string.Empty + endurance + "%", "txt12_0");
				GUI.Label(Crypto.Rect2(248f, 136f, 256f, 14f), critical + "%", "txt12_0");
				GUI.Label(Crypto.Rect2(248f, 156f, 256f, 14f), evasion + "%", "txt12_0");
			}
			else if (rollstat == 2)
			{
				for (int l = 0; l < 4; l++)
				{
					if (GUI.Button(Crypto.Rect2(254f, 85 + l * 20, 156f, 16f), string.Empty, titlebase2))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(1);
						}
						statinfo = l + (rollstat - 1) * 5 + 161;
						statname_idx = l + 5;
						confirm = 3;
					}
					GUI.Label(Crypto.Rect2(174f, 86 + 20 * l, 256f, 14f), thisitemStat[l + 5], "txt12_0");
				}
			}
			if (GUI.Button(Crypto.Rect2(212f, 116f, 32f, 32f), string.Empty, bt_prev))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				rollstat--;
				if (rollstat < 0)
				{
					rollstat = 1;
				}
			}
			else if (GUI.Button(Crypto.Rect2(412f, 116f, 32f, 32f), string.Empty, bt_next))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				rollstat = (rollstat + 1) % 2;
			}
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
		}
		else if (popup_equip_select)
		{
			GUI.DrawTexture(Crypto.Rect2(200f, 70f, 256f, 128f), pop_detail);
			GUI.DrawTexture(Crypto.Rect2(216f, 104f, 64f, 64f), equipicon[selectequip]);
			GUI.Label(Crypto.Rect2(200f, 81f, 256f, 14f), string.Empty + Language.intxt[language, ei[selectequip, cur_equip_grade[selectequip]]._name], "txt12_w");
			GUI.Label(Crypto.Rect2(298f, 108f, 124f, 14f), string.Empty + thisitemStat[selectequip] + " + " + ei[selectequip, cur_equip_grade[selectequip]]._capacity, "txt12_b");
			GUI.Label(Crypto.Rect2(282f, 134f, 160f, 14f), Language.intxt[language, statinfo], "txt12_0");
			if (cur_equip_grade[selectequip] < 10)
			{
				GUI.DrawTexture(Crypto.Rect2(216f, 169f, 16f, 16f), cost_icon);
				GUI.Label(Crypto.Rect2(226f, 169f, 58f, 14f), string.Empty + ei[selectequip, cur_equip_grade[selectequip] + 1]._price, "txt12_0");
				if (GUI.Button(Crypto.Rect2(300f, 158f, 64f, 32f), Language.intxt[language, 66], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (coin < ei[selectequip, cur_equip_grade[selectequip] + 1]._price)
					{
						confirm = 2;
						Delay(1f);
					}
					else
					{
						confirm = 1;
					}
				}
			}
			else
			{
				GUI.Label(Crypto.Rect2(216f, 169f, 64f, 14f), string.Empty + Language.intxt[language, 60], "txt12_b");
			}
			if (GUI.Button(Crypto.Rect2(370f, 158f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				popup_equip_select = false;
			}
		}
		if (confirm <= 0)
		{
			return;
		}
		GUI.enabled = true;
		if (confirm == 1)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 106f, 256f, 14f), Language.intxt[language, 14], "txt12_0");
			GUI.Label(Crypto.Rect2(156f, 132f, 128f, 14f), string.Empty + thisitemStat[selectequip] + " + " + ei[selectequip, cur_equip_grade[selectequip]]._capacity, "txt12_0");
			GUI.Label(Crypto.Rect2(200f, 150f, 128f, 14f), string.Empty + thisitemStat[selectequip] + " + " + ei[selectequip, cur_equip_grade[selectequip] + 1]._capacity, "txt12_b");
			GUI.DrawTexture(Crypto.Rect2(182f, 149f, 16f, 16f), arrow_upgrade);
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (Crypto.Property_change(-ei[selectequip, cur_equip_grade[selectequip] + 1]._price, _jade: false))
				{
					coin -= ei[selectequip, cur_equip_grade[selectequip] + 1]._price;
					cur_equip_grade[selectequip]++;
					hitrate = ei[0, cur_equip_grade[0]]._capacity;
					endurance = ei[1, cur_equip_grade[1]]._capacity;
					critical = ei[2, cur_equip_grade[2]]._capacity;
					evasion = ei[3, cur_equip_grade[3]]._capacity;
					hpplus = ei[4, cur_equip_grade[4]]._capacity;
					PlayerPrefsX.SetIntArray("n20", cur_equip_grade);
					Crypto.Save_int_key("n08", hitrate);
					PlayerPrefs.SetInt("updateRank", 1);
					Crypto.Save_int_key("n28", endurance);
					Crypto.Save_int_key("n32", critical);
					Crypto.Save_int_key("n30", evasion);
					Crypto.Save_int_key("n21", hpplus);
					Crypto.Save_int_key("n26", ei[5, cur_equip_grade[5]]._capacity);
					confirm = 0;
				}
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
			GUI.DrawTexture(Crypto.Rect2(160f, 106f, 160f, 16f), titlebase);
			GUI.Label(Crypto.Rect2(120f, 110f, 240f, 64f), Language.intxt[language, statinfo], "txt12_0");
			GUI.Label(Crypto.Rect2(120f, 106f, 240f, 16f), thisitemStat[statname_idx], "txt12_w");
			if (GUI.Button(Crypto.Rect2(208f, 180f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
			}
		}
		else if (confirm == 4)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(120f, 130f, 230f, 16f), Language.intxt[language, 337], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				Crypto.Save_int_key("cashshopkind", 6);
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
		else
		{
			if (confirm != 5)
			{
				return;
			}
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Box(Crypto.Rect2(176f, 100f, 128f, 22f), Language.intxt[language, 331 + select_itemidx]);
			GUI.Label(Crypto.Rect2(120f, 136f, 230f, 16f), Language.intxt[language, 351 + select_itemidx], "txt12_0");
			if (GUI.Button(Crypto.Rect2(208f, 180f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
	}
}
