using UnityEngine;

public class UI_pet : MonoBehaviour
{
	private const int MAXPET = 2;

	private const int MAXANGEL = 8;

	public Texture2D bg_pet;

	public Texture2D txt_name;

	public Texture2D bg_black;

	public Texture2D pop_detail;

	public Texture2D pop_blank;

	public Texture2D pop_blank2;

	public Texture2D c_equip;

	public Texture2D[] item_stat = new Texture2D[2];

	public Texture2D[] skill_stat = new Texture2D[4];

	public Texture2D[] angel = new Texture2D[8];

	public GUIStyle bt_yesno;

	public GUIStyle bt_back;

	public GUIStyle bt_petkind;

	public GUIStyle bt_info;

	public GUIStyle bt_empty;

	public Texture2D bg_asset;

	public Texture2D cost_icon;

	public Texture2D jade_icon;

	public Texture2D titlebase;

	public Texture2D icon_unlock;

	public Texture2D ico_feed;

	public Texture2D ico_arrow;

	public Texture2D ico_stamina;

	public Texture2D ico_stamina_empty;

	public Texture2D ico_warn;

	public Texture2D[] petkind = new Texture2D[3];

	public GUISkin basicSkin;

	private Cha_Costume script_costume;

	private DB_angel script_dbangel;

	private GameObject cashshop;

	private GameObject sound_UI;

	private SoundEf_UI script_soundUI;

	private petset[,] ps = new petset[2, 10];

	private int bg_posX_l;

	private int bg_posX_r;

	private int icon_posY;

	private int currentpet;

	private int confirm;

	private int icon_size;

	private int coin;

	private int jade;

	private int cost_feed;

	private int menu_kind;

	public Transform[] pet_mesh = new Transform[2];

	private int language;

	private int[] pet_activeskill = new int[2];

	private int[] pet_passiveskill = new int[2];

	private int[] pet_hunger = new int[2];

	private bool b_delay;

	private float f_delay;

	private bool warning_hungry;

	private int gamemode;

	private bool dragOn;

	private bool scrollOn;

	private float prevposX;

	private float currentX;

	private float dragrange;

	private float dragposX;

	private int maxExtremeStage = 1;

	private int cur_angel;

	private int sel_angel;

	private float angel_firerate;

	private string angel_name = string.Empty;

	private string angel_info = string.Empty;

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
	}

	private void Start()
	{
		maxExtremeStage = Crypto.Load_int_key("n48");
		maxExtremeStage = (maxExtremeStage - 1) / 8;
		language = PlayerPrefs.GetInt("language");
		ps = GetComponent<DB_PetSkill>().ps;
		script_dbangel = GetComponent<DB_angel>();
		cur_angel = Crypto.Load_int_key("n61");
		if (cur_angel != 0)
		{
			sel_angel = cur_angel;
			SetAngel(sel_angel - 1);
		}
		bg_posX_l = -380;
		bg_posX_r = 480;
		icon_posY = 340;
		cost_feed = 20;
		dragrange = (float)Screen.height * 0.375f;
		pet_activeskill = PlayerPrefsX.GetIntArray("n23");
		pet_passiveskill = PlayerPrefsX.GetIntArray("n27");
		pet_hunger = PlayerPrefsX.GetIntArray("n25");
		for (int i = 0; i < 2; i++)
		{
			if (pet_hunger[i] <= 0)
			{
				warning_hungry = true;
			}
		}
		InvokeRepeating("Warning_iconsize", 0.5f, 0.34f);
		sound_UI = GameObject.FindWithTag("sound");
		if (sound_UI != null)
		{
			script_soundUI = sound_UI.GetComponent<SoundEf_UI>();
		}
		gamemode = Crypto.Load_int_key("gamemode");
		PlayerPrefs.SetInt("currentSence", 7);
	}

	private void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
	}

	public void Delay(float t)
	{
		b_delay = true;
		f_delay = t;
	}

	private void SetAngel(int _index)
	{
		angel_name = Language.intxt[language, script_dbangel.ag[_index]._name];
		angel_info = Language.intxt[language, script_dbangel.ag[_index]._info];
		angel_firerate = 10f - script_dbangel.ag[_index]._firerate;
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
		if (bg_posX_l < -64)
		{
			bg_posX_l = (int)((float)bg_posX_l + Mathf.Min(-bg_posX_l, Time.deltaTime * 600f));
		}
		else
		{
			bg_posX_l = -64;
		}
		if (menu_kind > 0)
		{
			bg_posX_r = (int)((float)bg_posX_r - Mathf.Min(bg_posX_r, Time.deltaTime * 1500f));
			if (bg_posX_r <= 0)
			{
				bg_posX_r = 0;
				icon_posY = (int)((float)icon_posY - Mathf.Min(icon_posY, Time.deltaTime * 500f));
				if (icon_posY <= 230)
				{
					icon_posY = 230;
				}
			}
		}
		else
		{
			bg_posX_r = 480;
			icon_posY = 340;
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
			dragposX = Mathf.Max(dragposX, -150f);
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
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.DrawTexture(Crypto.Rect2(bg_posX_l, 0f, 320f, 320f), bg_pet);
		GUI.DrawTexture(Crypto.Rect2(126f, 30f, 100f, 50f), txt_name);
		GUI.Box(Crypto.Rect2(216f, 52f, 80f, 24f), Language.intxt[language, 74]);
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
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		if (menu_kind == 0)
		{
			if (warning_hungry)
			{
				GUI.Label(Crypto.Rect2(212f, 157f, 230f, 36f), Language.intxt[language, 268], "txt12_0");
			}
			else if (gamemode != 0)
			{
				GUI.Label(Crypto.Rect2(212f, 157f, 230f, 36f), Language.intxt[language, 423], "txt12_0");
			}
			else
			{
				GUI.Label(Crypto.Rect2(212f, 157f, 230f, 36f), Language.intxt[language, 20], "txt12_0");
			}
			for (int i = 0; i < 3; i++)
			{
				if (GUI.Button(Crypto.Rect2(220 + 76 * i, 90f, 64f, 64f), string.Empty, bt_petkind))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					if (i < 2)
					{
						menu_kind = 1;
						currentpet = i;
					}
					else
					{
						menu_kind = 2;
					}
				}
				GUI.DrawTexture(Crypto.Rect2(220 + 76 * i, 90f, 64f, 64f), petkind[i]);
				if (i < 2 && pet_hunger[i] <= 0)
				{
					GUI.DrawTexture(Crypto.Rect2(220 + 76 * i, 88f, icon_size * 32, 32f), ico_warn);
				}
			}
			if (GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				if (gamemode == 0)
				{
					Application.LoadLevel("Map");
				}
				else
				{
					Application.LoadLevel("Extreme");
				}
			}
		}
		else if (menu_kind == 1)
		{
			pet_mesh[currentpet].position = new Vector3(pet_mesh[currentpet].position.x, pet_mesh[currentpet].position.y, -1f);
			if (GUI.Button(Crypto.Rect2(370f, 158f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				menu_kind = 0;
				for (int j = 0; j < 2; j++)
				{
					pet_mesh[j].position = new Vector3(pet_mesh[j].position.x, pet_mesh[j].position.y, 1f);
				}
			}
			else if (GUI.Button(Crypto.Rect2(320f, 158f, 32f, 32f), string.Empty, bt_info))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 10;
			}
			GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 212f, 480f, 100f), bg_black);
			for (int k = 0; k < 2; k++)
			{
				if (GUI.Button(Crypto.Rect2(k * 76 + 94, icon_posY, 64f, 64f), string.Empty, bt_petkind))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					switch (k)
					{
					case 0:
						if (coin >= cost_feed)
						{
							if (pet_hunger[currentpet] < 5)
							{
								confirm = 1;
								break;
							}
							confirm = 7;
							Delay(1f);
						}
						else
						{
							confirm = 5;
							Delay(1f);
						}
						break;
					case 1:
						if (coin >= cost_feed)
						{
							if (pet_hunger[currentpet] < 5)
							{
								confirm = 2;
								break;
							}
							confirm = 7;
							Delay(1f);
						}
						else
						{
							confirm = 5;
							Delay(1f);
						}
						break;
					}
				}
				GUI.DrawTexture(Crypto.Rect2(k * 76 + 94, icon_posY, 64f, 64f), item_stat[k]);
			}
			if (cost_feed > 0)
			{
				GUI.DrawTexture(Crypto.Rect2(98f, icon_posY + 66, 16f, 16f), cost_icon);
				GUI.Label(Crypto.Rect2(94f, icon_posY + 66, 64f, 14f), "\t  " + cost_feed, "txt12_w");
			}
			int num = cost_feed * (5 - pet_hunger[currentpet]);
			if (num > 0)
			{
				GUI.DrawTexture(Crypto.Rect2(174f, icon_posY + 66, 16f, 16f), cost_icon);
				GUI.Label(Crypto.Rect2(170f, icon_posY + 66, 64f, 14f), "\t  " + num, "txt12_w");
			}
			for (int l = 0; l < 2; l++)
			{
				if (GUI.Button(Crypto.Rect2(l * 76 + 246, icon_posY, 64f, 64f), string.Empty, bt_petkind))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					switch (l)
					{
					case 0:
						if (pet_passiveskill[currentpet] < 9)
						{
							confirm = 3;
							break;
						}
						confirm = 7;
						Delay(1f);
						break;
					case 1:
						if (pet_activeskill[currentpet] < 9)
						{
							confirm = 4;
							break;
						}
						confirm = 7;
						Delay(1f);
						break;
					}
				}
				GUI.DrawTexture(Crypto.Rect2(l * 76 + 246, icon_posY, 64f, 64f), skill_stat[l + currentpet * 2]);
			}
			if (pet_passiveskill[currentpet] < 9)
			{
				GUI.DrawTexture(Crypto.Rect2(248f, icon_posY + 66, 16f, 16f), jade_icon);
				GUI.Label(Crypto.Rect2(246f, icon_posY + 66, 64f, 14f), "\t  " + ps[currentpet, pet_passiveskill[currentpet]]._price, "txt12_w");
				GUI.Label(Crypto.Rect2(246f, icon_posY - 14, 64f, 14f), "Level " + (pet_passiveskill[currentpet] + 1), "txt12_w");
			}
			else
			{
				GUI.Label(Crypto.Rect2(246f, icon_posY - 14, 64f, 14f), Language.intxt[language, 60], "txt12_w");
			}
			if (pet_activeskill[currentpet] < 9)
			{
				GUI.DrawTexture(Crypto.Rect2(324f, icon_posY + 66, 16f, 16f), jade_icon);
				GUI.Label(Crypto.Rect2(322f, icon_posY + 66, 64f, 14f), "\t  " + ps[currentpet, pet_activeskill[currentpet]]._price, "txt12_w");
				GUI.Label(Crypto.Rect2(322f, icon_posY - 14, 64f, 14f), "Level " + (pet_activeskill[currentpet] + 1), "txt12_w");
			}
			else
			{
				GUI.Label(Crypto.Rect2(322f, icon_posY - 14, 64f, 14f), Language.intxt[language, 60], "txt12_w");
			}
			GUI.DrawTexture(Crypto.Rect2(310f, 85f, 128f, 16f), titlebase);
			GUI.Label(Crypto.Rect2(246f, 86f, 256f, 14f), Language.intxt[language, ps[currentpet, 0]._info], "txt12_w");
			GUI.Label(Crypto.Rect2(246f, 110f, 256f, 14f), Language.intxt[language, 265] + "  " + pet_hunger[currentpet], "txt12_0");
			for (int m = 0; m < 5; m++)
			{
				if (m >= pet_hunger[currentpet])
				{
					GUI.DrawTexture(Crypto.Rect2(324 + 20 * m, 130f, 16f, 16f), ico_stamina_empty);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(320 + 20 * m, 126f, 24f, 24f), ico_stamina);
				}
			}
		}
		else if (menu_kind == 2)
		{
			GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 212f, 480f, 100f), bg_black);
			for (int n = 0; n < 8; n++)
			{
				if (GUI.Button(Crypto.Rect2((float)(n * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), string.Empty, bt_petkind) && !scrollOn)
				{
					sel_angel = n + 1;
					SetAngel(n);
				}
				if (maxExtremeStage > n)
				{
					GUI.DrawTexture(Crypto.Rect2((float)(n * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), angel[n]);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2((float)(n * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), icon_unlock);
					GUI.Label(Crypto.Rect2((float)(n * 76 + 18) + dragposX, icon_posY + 57, 64f, 14f), "???", "txt12_w");
				}
				if (n == cur_angel - 1)
				{
					GUI.DrawTexture(Crypto.Rect2((float)(n * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), c_equip);
				}
			}
			if (sel_angel > 0)
			{
				if (maxExtremeStage >= sel_angel)
				{
					GUI.DrawTexture(Crypto.Rect2(216f, 104f, 64f, 64f), angel[sel_angel - 1]);
					if (cur_angel == sel_angel)
					{
						if (GUI.Button(Crypto.Rect2(300f, 158f, 64f, 32f), Language.intxt[language, 175], bt_yesno))
						{
							cur_angel = 0;
							Crypto.Save_int_key("n61", cur_angel);
						}
					}
					else if (GUI.Button(Crypto.Rect2(300f, 158f, 64f, 32f), Language.intxt[language, 10], bt_yesno))
					{
						cur_angel = sel_angel;
						Crypto.Save_int_key("n61", cur_angel);
					}
					GUI.DrawTexture(Crypto.Rect2(310f, 85f, 128f, 16f), titlebase);
					GUI.Label(Crypto.Rect2(246f, 86f, 256f, 14f), angel_name, "txt12_w");
					GUI.Label(Crypto.Rect2(246f, 106f, 256f, 14f), Language.intxt[language, 153] + "   " + angel_firerate, "txt12_0");
					GUI.Label(Crypto.Rect2(300f, 134f, 140f, 14f), angel_info, "txt12_b");
				}
				else
				{
					GUI.Label(Crypto.Rect2(216f, 110f, 220f, 14f), Language.intxt[language, 456], "txt12_0");
					GUI.Label(Crypto.Rect2(216f, 140f, 220f, 14f), "Stage  " + sel_angel * 8 + "  " + Language.intxt[language, 87], "txt12_b");
				}
			}
			else
			{
				GUI.Label(Crypto.Rect2(216f, 116f, 220f, 14f), Language.intxt[language, 457], "txt12_0");
			}
			if (GUI.Button(Crypto.Rect2(370f, 158f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				menu_kind = 0;
			}
		}
		if (confirm <= 0)
		{
			return;
		}
		GUI.enabled = true;
		switch (confirm)
		{
		case 1:
			GUI.DrawTexture(Crypto.Rect2(112f, 100f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 269], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(144f, 150f, 192f, 16f), titlebase);
			GUI.DrawTexture(Crypto.Rect2(180f, 150f, 16f, 16f), ico_stamina);
			GUI.DrawTexture(Crypto.Rect2(236f, 150f, 16f, 16f), ico_arrow);
			GUI.Label(Crypto.Rect2(180f, 150f, 128f, 14f), string.Empty + pet_hunger[currentpet] + "            " + Mathf.Min(5, pet_hunger[currentpet] + 1), "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(214f, 168f, 16f, 16f), cost_icon);
			GUI.Label(Crypto.Rect2(214f, 168f, 64f, 16f), "20", "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 190f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (Crypto.Property_change(-cost_feed, _jade: false))
				{
					coin -= cost_feed;
					pet_hunger[currentpet] = Mathf.Min(5, pet_hunger[currentpet] + 1);
					PlayerPrefsX.SetIntArray("n25", pet_hunger);
					warning_hungry = false;
					confirm = 0;
				}
				for (int num2 = 0; num2 < 2; num2++)
				{
					if (pet_hunger[num2] <= 0)
					{
						warning_hungry = true;
					}
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 190f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
			break;
		case 2:
			GUI.DrawTexture(Crypto.Rect2(112f, 100f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 270], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(144f, 150f, 192f, 16f), titlebase);
			GUI.DrawTexture(Crypto.Rect2(180f, 150f, 16f, 16f), ico_stamina);
			GUI.DrawTexture(Crypto.Rect2(236f, 150f, 16f, 16f), ico_arrow);
			GUI.Label(Crypto.Rect2(180f, 150f, 128f, 14f), string.Empty + pet_hunger[currentpet] + "            5", "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(214f, 168f, 16f, 16f), cost_icon);
			GUI.Label(Crypto.Rect2(214f, 168f, 64f, 16f), string.Empty + (5 - pet_hunger[currentpet]) * 20, "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 190f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				if (Crypto.Property_change(-cost_feed * (5 - pet_hunger[currentpet]), _jade: false))
				{
					coin -= cost_feed * (5 - pet_hunger[currentpet]);
					pet_hunger[currentpet] = 5;
					PlayerPrefsX.SetIntArray("n25", pet_hunger);
					confirm = 0;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 190f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
			break;
		case 3:
			GUI.DrawTexture(Crypto.Rect2(112f, 100f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 16f), Language.intxt[language, 271], "txt12_0");
			GUI.Label(Crypto.Rect2(128f, 132f, 224f, 16f), Language.intxt[language, ps[currentpet, 2]._info], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(144f, 156f, 192f, 16f), titlebase);
			GUI.DrawTexture(Crypto.Rect2(260f, 156f, 16f, 16f), ico_arrow);
			GUI.Label(Crypto.Rect2(160f, 156f, 80f, 14f), Language.intxt[language, ps[currentpet, 3]._info], "txt12_w");
			GUI.Label(Crypto.Rect2(204f, 156f, 128f, 14f), string.Empty + ps[currentpet, pet_passiveskill[currentpet]]._passive + "            " + ps[currentpet, pet_passiveskill[currentpet] + 1]._passive, "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(214f, 174f, 16f, 16f), jade_icon);
			GUI.Label(Crypto.Rect2(214f, 174f, 64f, 16f), "2", "txt12_0");
			GUI.Label(Crypto.Rect2(240f, 174f, 128f, 16f), Language.intxt[language, 350], "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 190f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (jade >= ps[currentpet, pet_passiveskill[currentpet]]._price)
				{
					if (Crypto.Property_change(-ps[currentpet, pet_passiveskill[currentpet]]._price, _jade: true))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						jade -= ps[currentpet, pet_passiveskill[currentpet]]._price;
						pet_passiveskill[currentpet]++;
						PlayerPrefsX.SetIntArray("n27", pet_passiveskill);
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (2)\t" + Language.intxt[language, 231]);
					}
					confirm = 0;
				}
				else
				{
					confirm = 6;
					Delay(1f);
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 190f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
			break;
		case 4:
			GUI.DrawTexture(Crypto.Rect2(112f, 100f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 16f), Language.intxt[language, 272], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(144f, 132f, 192f, 16f), titlebase);
			GUI.DrawTexture(Crypto.Rect2(144f, 152f, 192f, 16f), titlebase);
			GUI.DrawTexture(Crypto.Rect2(260f, 132f, 16f, 16f), ico_arrow);
			GUI.DrawTexture(Crypto.Rect2(260f, 152f, 16f, 16f), ico_arrow);
			GUI.Label(Crypto.Rect2(152f, 132f, 80f, 14f), Language.intxt[language, 274], "txt12_w");
			GUI.Label(Crypto.Rect2(152f, 152f, 80f, 14f), Language.intxt[language, 273], "txt12_w");
			GUI.Label(Crypto.Rect2(204f, 132f, 128f, 14f), string.Empty + ps[currentpet, pet_activeskill[currentpet]]._attackpoint + "            " + ps[currentpet, pet_activeskill[currentpet] + 1]._attackpoint, "txt12_w");
			GUI.Label(Crypto.Rect2(204f, 152f, 128f, 14f), string.Empty + ps[currentpet, pet_activeskill[currentpet]]._duration + "            " + ps[currentpet, pet_activeskill[currentpet] + 1]._duration, "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(214f, 170f, 16f, 16f), jade_icon);
			GUI.Label(Crypto.Rect2(214f, 170f, 64f, 16f), "2", "txt12_0");
			GUI.Label(Crypto.Rect2(240f, 170f, 128f, 16f), Language.intxt[language, 350], "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 190f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (jade >= ps[currentpet, pet_activeskill[currentpet]]._price)
				{
					if (Crypto.Property_change(-ps[currentpet, pet_activeskill[currentpet]]._price, _jade: true))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						jade -= ps[currentpet, pet_activeskill[currentpet]]._price;
						pet_activeskill[currentpet]++;
						PlayerPrefsX.SetIntArray("n23", pet_activeskill);
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (2)\t" + Language.intxt[language, 232]);
					}
					confirm = 0;
				}
				else
				{
					confirm = 6;
					Delay(1f);
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 190f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
			break;
		case 5:
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
			break;
		case 7:
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 267], "txt12_0");
			if (f_delay == 0f)
			{
				confirm = 0;
			}
			break;
		case 10:
			GUI.DrawTexture(Crypto.Rect2(112f, 100f, 256f, 128f), pop_blank2);
			GUI.Box(Crypto.Rect2(176f, 116f, 128f, 22f), Language.intxt[language, 265]);
			GUI.Label(Crypto.Rect2(128f, 130f, 224f, 42f), Language.intxt[language, 279], "txt12_0");
			GUI.Label(Crypto.Rect2(128f, 156f, 224f, 42f), Language.intxt[language, 280], "txt12_0");
			if (GUI.Button(Crypto.Rect2(286f, 188f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
			break;
		case 8:
		case 9:
			break;
		}
	}
}
