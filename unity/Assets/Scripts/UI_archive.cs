using System.Collections.Generic;
using UnityEngine;

public class UI_archive : MonoBehaviour
{
	private const int MAXSKILL = 20;

	private const int MAXARCHIVE = 76;

	private const int MAXGENERALPOOL = 30;

	private const int MAXTREASURE = 24;

	public Texture2D icon_warning;

	public Texture2D bg_archive;

	public Texture2D txt_name;

	public Texture2D archive_complete;

	public Texture2D archive_gift;

	public Texture2D bg_archiverate;

	public Texture2D gauge_rate;

	public Texture2D bg_black;

	public Texture2D bg_title;

	public Texture2D pop_blank2;

	public Texture2D icon_jade;

	public Texture2D icon_coin;

	public Texture2D bg_asset;

	public Texture2D bg_scroll;

	public Texture2D titlebase;

	public Texture2D numberbase;

	public Texture2D please_touch;

	public Texture2D bg_general_detail;

	public Texture2D bt_roullete;

	public Texture2D bt_roullete_active;

	public Texture2D[] img_kind = new Texture2D[3];

	public Texture2D[] prt_general = new Texture2D[30];

	public Texture2D[] icon_treasure = new Texture2D[24];

	public Texture2D[] gifticon = new Texture2D[6];

	private bool[] treasure_gift = new bool[6];

	public GUIStyle bt_yesno;

	public GUIStyle bt_back;

	public GUIStyle bt_kind;

	public GUIStyle bt_general;

	public GUIStyle bt_empty;

	public GUISkin basicSkin;

	private int select_index = -1;

	private Color inactive = new Color(0f, 0f, 0f, 0.5f);

	private GameObject cashshop;

	private GameObject sound_UI;

	private SoundEf_UI script_soundUI;

	private Language_Archive script_archname;

	private Language_Name script_name;

	private DB_Archive script_archive;

	private Vector2 scrollPosition = Vector2.zero;

	private Vector2 prev_scrollPosition = Vector2.zero;

	private float startMousePosY;

	private float curMousePosY;

	private float bg_posX_l;

	private float bg_posX_r;

	private float icon_posY;

	private bool imagemovefinish;

	private bool b_delay;

	private float f_delay;

	private short confirm;

	private int selectgeneral = -1;

	private int coin;

	private int jade;

	private int selectblank = -1;

	private int expand_height;

	private int expand_height2;

	private int language;

	private int step;

	private int count_staff;

	private int count_treasure;

	private int[] temp_treasure = new int[24];

	private int[] temp_staff = new int[30];

	private int[] complete_archive = new int[76];

	private int[] current_archive = new int[76];

	private int[] rate_archive = new int[76];

	private bool[] gift_archive = new bool[76];

	private bool warning_newarchive;

	private bool warning_newtreasure;

	private int[] unlock_costume = new int[20];

	private int[] bosskill = new int[12];

	private int[] skill_use = new int[20];

	private int[] pet_skill_use = new int[2];

	private int enemykill;

	private int grappling;

	private int exattack;

	private int death;

	private int resurrection;

	private int cashing;

	private int max_stage_index = -1;

	private int remain_arch;

	private int icon_size;

	private int tutorial;

	private int generalsearch;

	private int[] cur_skill_grade = new int[20];

	private int caveplay;

	private int perfectplay;

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
	}

	private void Start()
	{
		language = PlayerPrefs.GetInt("language");
		script_archive = GetComponent<DB_Archive>();
		bg_posX_l = -380f;
		bg_posX_r = 480f;
		icon_posY = 340f;
		temp_treasure = PlayerPrefsX.GetIntArray("treasure");
		temp_staff = PlayerPrefsX.GetIntArray("staff");
		tutorial = Crypto.Load_int_key("tutorial");
		sound_UI = GameObject.FindWithTag("sound");
		if (sound_UI != null)
		{
			script_soundUI = sound_UI.GetComponent<SoundEf_UI>();
		}
		script_name = GetComponent<Language_Name>();
		script_archname = GetComponent<Language_Archive>();
		complete_archive = PlayerPrefsX.GetIntArray("archive");
		bosskill = PlayerPrefsX.GetIntArray("bosskill");
		skill_use = PlayerPrefsX.GetIntArray("skill_use");
		pet_skill_use = PlayerPrefsX.GetIntArray("pet_skill_use");
		enemykill = Crypto.Load_int_key("enemykill");
		grappling = Crypto.Load_int_key("grappling");
		exattack = Crypto.Load_int_key("exattack");
		death = Crypto.Load_int_key("death");
		resurrection = Crypto.Load_int_key("resurrection");
		max_stage_index = Crypto.Load_int_key("n06");
		unlock_costume = PlayerPrefsX.GetIntArray("n18");
		cashing = Crypto.Load_int_key("cashing");
		generalsearch = Crypto.Load_int_key("generalsearch");
		cur_skill_grade = PlayerPrefsX.GetIntArray("n22");
		caveplay = Crypto.Load_int_key("caveplay");
		perfectplay = Crypto.Load_int_key("perfectplay");
		for (int i = 0; i < 76; i++)
		{
			if (complete_archive[i] != 0)
			{
				continue;
			}
			remain_arch++;
			switch (script_archive.aci[i]._require_kind)
			{
			case 1:
				if (enemykill >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)enemykill / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 2:
				if (grappling >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)grappling / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 3:
				if (death >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)death / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 4:
				if (resurrection >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)resurrection / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 5:
				if (exattack >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)exattack / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 6:
			{
				for (int m = 0; m < 30; m++)
				{
					if (temp_staff[m] == 5)
					{
						gift_archive[i] = true;
						warning_newarchive = true;
						break;
					}
				}
				break;
			}
			case 7:
				if (max_stage_index >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)max_stage_index / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 8:
				if (bosskill[script_archive.aci[i]._require_kind_sub] >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)bosskill[script_archive.aci[i]._require_kind_sub] / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 9:
				if (pet_skill_use[script_archive.aci[i]._require_kind_sub] >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)pet_skill_use[script_archive.aci[i]._require_kind_sub] / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 10:
				if (skill_use[script_archive.aci[i]._require_kind_sub] >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)skill_use[script_archive.aci[i]._require_kind_sub] / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 11:
				if (generalsearch >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)generalsearch / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 12:
			{
				int num3 = 0;
				for (int l = 0; l < 30; l++)
				{
					if (temp_staff[l] >= 4)
					{
						num3++;
					}
				}
				if (num3 >= 5)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)num3 / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			}
			case 13:
			{
				int num2 = 0;
				for (int k = 0; k < 20; k++)
				{
					if (cur_skill_grade[k] >= 0)
					{
						num2++;
					}
				}
				if (num2 >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)num2 / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			}
			case 14:
			{
				int num4 = 0;
				for (int n = 0; n < 20; n++)
				{
					if (cur_skill_grade[n] >= 4)
					{
						num4++;
					}
				}
				if (num4 >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)num4 / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			}
			case 15:
				if (caveplay >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)caveplay / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 16:
				if (perfectplay >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)perfectplay / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 18:
				if (cashing >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)cashing / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			case 19:
			{
				int num = 0;
				for (int j = 0; j < 20; j++)
				{
					if (unlock_costume[j] > 0)
					{
						num++;
					}
				}
				if (num >= script_archive.aci[i]._require_amount)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)num / (float)script_archive.aci[i]._require_amount * 100f);
				}
				break;
			}
			case 20:
				if (remain_arch <= 0)
				{
					gift_archive[i] = true;
					warning_newarchive = true;
				}
				else
				{
					rate_archive[i] = (int)((float)(76 - remain_arch) / 76f * 100f);
				}
				break;
			}
		}
		FindNewTreasure();
		InvokeRepeating("Warning_iconsize", 0.5f, 0.34f);
		PlayerPrefs.SetInt("currentSence", 4);
	}

	public void FindNewTreasure()
	{
		bool flag = false;
		for (int i = 0; i < 6; i++)
		{
			int num = 0;
			if (temp_treasure[4 * i] > 0)
			{
				num++;
			}
			if (temp_treasure[4 * i + 1] > 0)
			{
				num++;
			}
			if (temp_treasure[4 * i + 2] > 0)
			{
				num++;
			}
			if (temp_treasure[4 * i + 3] > 0)
			{
				num++;
			}
			if (num == 4)
			{
				treasure_gift[i] = true;
				flag = true;
			}
			else
			{
				treasure_gift[i] = false;
			}
		}
		warning_newtreasure = flag;
	}

	public void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
	}

	public bool IsWarning()
	{
		for (int i = 0; i < 76; i++)
		{
			if (gift_archive[i])
			{
				return true;
			}
		}
		return false;
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

	public void CashshopOpen()
	{
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
	}

	public void sortAchieve()
	{
		int[] array = new int[76];
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		for (int i = 0; i < 76; i++)
		{
			if (gift_archive[i])
			{
				list.Add(i);
			}
			else if (complete_archive[i] == 1)
			{
				list2.Add(i);
			}
			else
			{
				list3.Add(i);
			}
		}
		list.AddRange(list2);
		list.AddRange(list3);
		list2.Clear();
		list3.Clear();
		current_archive = list.ToArray();
		list.Clear();
	}

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		GUI.DrawTexture(Crypto.Rect2(bg_posX_l, 0f, 320f, 320f), bg_archive);
		GUI.DrawTexture(Crypto.Rect2(120f, 30f, 100f, 50f), txt_name);
		GUI.Box(Crypto.Rect2(216f, 52f, 80f, 24f), Language.intxt[language, 195]);
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
		if (step == 0)
		{
			GUI.DrawTexture(Crypto.Rect2(200f, 70f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(200f, 164f, 256f, 32f), Language.intxt[language, 174], "txt12_0");
			for (int i = 0; i < 3; i++)
			{
				if (GUI.Button(Crypto.Rect2(216 + 80 * i, 90f, 64f, 64f), string.Empty, bt_kind))
				{
					scrollPosition = Vector2.zero;
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					step = i + 1;
					switch (step)
					{
					case 1:
					{
						count_treasure = 0;
						for (int k = 0; k < 24; k++)
						{
							if (temp_treasure[k] > 0)
							{
								count_treasure++;
							}
						}
						break;
					}
					case 2:
					{
						count_staff = 0;
						for (int j = 0; j < 30; j++)
						{
							if (temp_staff[j] > 0)
							{
								count_staff++;
							}
						}
						break;
					}
					case 3:
						sortAchieve();
						break;
					}
				}
				GUI.DrawTexture(Crypto.Rect2(216 + 80 * i, 90f, 64f, 64f), img_kind[i]);
				GUI.Label(Crypto.Rect2(216 + 80 * i, 92f, 64f, 14f), string.Empty + Language.intxt[language, 171 + i], "txt12_w");
			}
			if (warning_newarchive)
			{
				GUI.DrawTexture(Crypto.Rect2(416f, 82f, icon_size * 32, 32f), icon_warning);
			}
			if (warning_newtreasure)
			{
				GUI.DrawTexture(Crypto.Rect2(256f, 82f, icon_size * 32, 32f), icon_warning);
			}
			if (tutorial == 1)
			{
				GUI.DrawTexture(Crypto.Rect2(380f, 94f, icon_size * 64, icon_size * 64), please_touch);
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
		else if (step > 0)
		{
			GUI.DrawTexture(Crypto.Rect2(200f, 34f, 256f, 64f), bg_title);
			if (GUI.Button(Crypto.Rect2(384f, 36f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				if (step == 3)
				{
					warning_newarchive = IsWarning();
				}
				step = 0;
			}
			GUI.DrawTexture(Crypto.Rect2(200f, 70f, 256f, 256f), bg_scroll);
			if (Input.GetMouseButtonDown(0))
			{
				startMousePosY = Input.mousePosition.y;
				prev_scrollPosition = scrollPosition;
			}
			if (Input.GetMouseButton(0) && Crypto.Rect2(200f, 80f, 256f, 224f).Contains(Event.current.mousePosition))
			{
				curMousePosY = Input.mousePosition.y;
				scrollPosition = prev_scrollPosition + Vector2.up * (curMousePosY - startMousePosY) * (640f / (float)Screen.height);
			}
			if (step == 1)
			{
				GUI.Label(Crypto.Rect2(220f, 44f, 64f, 16f), count_treasure + " / " + 24, "txt12_w");
				scrollPosition = GUI.BeginScrollView(Crypto.Rect2(200f, 80f, 272f, 224f), scrollPosition, Crypto.Rect2(200f, -4f, 256f, 366f));
				for (int l = 0; l < 24; l++)
				{
					GUI.DrawTexture(Crypto.Rect2(220 + l % 4 * 44, l / 4 * 60 + 30, 32f, 16f), numberbase);
					GUI.Label(Crypto.Rect2(219 + l % 4 * 44, l / 4 * 60 + 29, 32f, 16f), string.Empty + temp_treasure[l], "txt12_w");
					if (temp_treasure[l] == 0)
					{
						GUI.color = inactive;
					}
					GUI.DrawTexture(Crypto.Rect2(220 + l % 4 * 44, l / 4 * 60, 32f, 32f), icon_treasure[l]);
					GUI.color = Color.white;
				}
				for (int m = 0; m < 6; m++)
				{
					GUI.DrawTexture(Crypto.Rect2(208f, m * 60 + 50, 240f, 1f), bg_black);
					if (treasure_gift[m])
					{
						GUI.Box(Crypto.Rect2(208f, m * 60 - 8, icon_size * 240, icon_size * 56), string.Empty, "glow_box");
						GUI.DrawTexture(Crypto.Rect2(394f, m * 60 - 4, 48f, 48f), bt_roullete_active);
						if (GUI.Button(Crypto.Rect2(394f, m * 60 - 4, 48f, 48f), string.Empty, bt_empty))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							int[] intArray = PlayerPrefsX.GetIntArray("n39");
							switch (m)
							{
							case 0:
							{
								int num = Crypto.Load_int_key("n10");
								num += 10;
								Crypto.Save_int_key("n10", num);
								break;
							}
							case 1:
								coin += 1000;
								Crypto.Property_change(1000, _jade: false);
								break;
							case 2:
								jade += 3;
								Crypto.Property_change(3, _jade: true);
								break;
							case 3:
								intArray[2] += 5;
								PlayerPrefsX.SetIntArray("n39", intArray);
								break;
							case 4:
								intArray[3] += 4;
								PlayerPrefsX.SetIntArray("n39", intArray);
								break;
							case 5:
								intArray[4] += 3;
								PlayerPrefsX.SetIntArray("n39", intArray);
								break;
							}
							temp_treasure[4 * m]--;
							temp_treasure[4 * m + 1]--;
							temp_treasure[4 * m + 2]--;
							temp_treasure[4 * m + 3]--;
							PlayerPrefsX.SetIntArray("treasure", temp_treasure);
							FindNewTreasure();
						}
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(394f, m * 60 - 4, 48f, 48f), bt_roullete);
					}
					GUI.DrawTexture(Crypto.Rect2(390f, m * 60 - 8, 56f, 56f), gifticon[m]);
				}
				GUI.EndScrollView();
			}
			else if (step == 2)
			{
				GUI.Label(Crypto.Rect2(220f, 44f, 64f, 16f), count_staff + " / " + 30, "txt12_w");
				scrollPosition = GUI.BeginScrollView(Crypto.Rect2(200f, 80f, 272f, 224f), scrollPosition, Crypto.Rect2(200f, 0f, 256f, 920f));
				for (int n = 0; n < 30; n++)
				{
					if (temp_staff[n] == 0)
					{
						GUI.color = inactive;
						GUI.DrawTexture(Crypto.Rect2(218 + n % 3 * 76, n / 3 * 90 + 64, 68f, 16f), titlebase);
						GUI.Label(Crypto.Rect2(220 + n % 3 * 76, n / 3 * 90 + 64, 64f, 16f), "???", "txt12_w");
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(218 + n % 3 * 76, n / 3 * 90 + 64, 68f, 16f), titlebase);
						GUI.Label(Crypto.Rect2(220 + n % 3 * 76, n / 3 * 90 + 64, 64f, 16f), script_name.txt_name[language, 21 + n], "txt12_w");
					}
					GUI.DrawTexture(Crypto.Rect2(220 + n % 3 * 76, n / 3 * 90, 64f, 64f), prt_general[n]);
					GUI.color = Color.white;
				}
				GUI.EndScrollView();
			}
			else if (step == 3)
			{
				GUI.Label(Crypto.Rect2(220f, 44f, 64f, 16f), 76 - remain_arch + " / " + 76, "txt12_w");
				scrollPosition = GUI.BeginScrollView(Crypto.Rect2(200f, 80f, 272f, 224f), scrollPosition, Crypto.Rect2(200f, 0f, 256f, 2474f));
				for (int num2 = 0; num2 < 76; num2++)
				{
					if (selectblank == num2)
					{
						expand_height = 40;
					}
					else
					{
						expand_height = 0;
					}
					if (selectblank != -1 && num2 > selectblank)
					{
						expand_height2 = 40;
					}
					else
					{
						expand_height2 = 0;
					}
					if (GUI.Button(Crypto.Rect2(212f, 32 * num2 + expand_height2, 232f, 32 + expand_height), string.Empty))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(1);
						}
						if (Mathf.Abs(curMousePosY - startMousePosY) < 10f * (640f / (float)Screen.height))
						{
							if (gift_archive[current_archive[num2]])
							{
								if (script_archive.aci[current_archive[num2]]._reward_kind == 1)
								{
									coin += script_archive.aci[current_archive[num2]]._reward_amount;
									Crypto.Property_change(script_archive.aci[current_archive[num2]]._reward_amount, _jade: false);
								}
								else if (script_archive.aci[current_archive[num2]]._reward_kind == 2)
								{
									jade += script_archive.aci[current_archive[num2]]._reward_amount;
									Crypto.Property_change(script_archive.aci[current_archive[num2]]._reward_amount, _jade: true);
								}
								complete_archive[current_archive[num2]] = 1;
								PlayerPrefsX.SetIntArray("archive", complete_archive);
								gift_archive[current_archive[num2]] = false;
								remain_arch--;
								if (tutorial == 1 && current_archive[num2] == 0)
								{
									tutorial = -4;
									Crypto.Save_int_key("tutorial", -4);
									Delay(1f);
									confirm = 2;
								}
								if (num2 + 4 < 10)
								{
									string text = "0" + (num2 + 4);
								}
								else
								{
									string text = (num2 + 4).ToString();
								}
								sortAchieve();
							}
							else if (selectblank != num2)
							{
								select_index = current_archive[num2];
								selectblank = num2;
							}
							else
							{
								selectblank = -1;
							}
						}
					}
					if (!((float)(64 * num2 + expand_height2) - scrollPosition.y < 426f) || !((float)(64 * num2 + expand_height2) - scrollPosition.y > -48f))
					{
						continue;
					}
					if (complete_archive[current_archive[num2]] != 0)
					{
						GUI.DrawTexture(Crypto.Rect2(216f, 32 * num2 + expand_height2, 32f, 32f), archive_complete);
					}
					else if (gift_archive[current_archive[num2]])
					{
						GUI.DrawTexture(Crypto.Rect2(216f, 32 * num2 + expand_height2, 32f, 32f), archive_gift);
						if (script_archive.aci[current_archive[num2]]._reward_kind == 1)
						{
							GUI.DrawTexture(Crypto.Rect2(424f, 32 * num2 + expand_height2 + 2, 16f, 16f), icon_coin);
						}
						else
						{
							GUI.DrawTexture(Crypto.Rect2(424f, 32 * num2 + expand_height2 + 2, 16f, 16f), icon_jade);
						}
						GUI.Label(Crypto.Rect2(406f, 32 * num2 + expand_height2 + 16, 40f, 16f), string.Empty + script_archive.aci[current_archive[num2]]._reward_amount, "txt12_0");
					}
					else
					{
						if (script_archive.aci[current_archive[num2]]._reward_kind == 1)
						{
							GUI.DrawTexture(Crypto.Rect2(424f, 32 * num2 + expand_height2 + 2, 16f, 16f), icon_coin);
						}
						else
						{
							GUI.DrawTexture(Crypto.Rect2(424f, 32 * num2 + expand_height2 + 2, 16f, 16f), icon_jade);
						}
						GUI.Label(Crypto.Rect2(406f, 32 * num2 + expand_height2 + 16, 40f, 16f), string.Empty + script_archive.aci[current_archive[num2]]._reward_amount, "txt12_0");
						GUI.DrawTexture(Crypto.Rect2(216f, 32 * num2 + expand_height2, 32f, 32f), bg_archiverate);
						GUI.DrawTexture(Crypto.Rect2(219f, 32 * num2 + expand_height2 + 20, (float)rate_archive[current_archive[num2]] * 0.25f, 5f), gauge_rate);
						GUI.Label(Crypto.Rect2(212f, 32 * num2 + expand_height2 + 5, 32f, 16f), string.Empty + rate_archive[current_archive[num2]], "txt12_w");
					}
					GUI.Label(Crypto.Rect2(208f, 32 * num2 + expand_height2 + 8, 240f, 16f), script_archname.txt_arch[language, script_archive.aci[current_archive[num2]]._name], "txt12_0");
				}
				if (selectblank != -1)
				{
					GUI.Label(Crypto.Rect2(208f, 32 * selectblank + 33, 240f, 32f), script_archname.txt_arch[language, script_archive.aci[select_index]._info], "txt12_0");
				}
				if (tutorial == 1)
				{
					GUI.DrawTexture(Crypto.Rect2(206f, -14f, icon_size * 64, icon_size * 64), please_touch);
				}
				GUI.EndScrollView();
			}
		}
		if (confirm <= 0)
		{
			return;
		}
		GUI.enabled = true;
		if (confirm == 1)
		{
			GUI.enabled = true;
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), bg_general_detail);
			GUI.DrawTexture(Crypto.Rect2(128f, 123f, 64f, 64f), prt_general[selectgeneral]);
			if (GUI.Button(Crypto.Rect2(284f, 180f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				selectgeneral = -1;
				confirm = 0;
			}
		}
		else if (confirm == 2)
		{
			GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 257]);
			if (f_delay == 0f)
			{
				confirm = 0;
			}
		}
	}
}
