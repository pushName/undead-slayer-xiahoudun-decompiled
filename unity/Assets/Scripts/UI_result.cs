using UnityEngine;

public class UI_result : MonoBehaviour
{
	private float show_delay;

	private bool openbox;

	private bool getpoint = true;

	private bool startexpgain;

	public int isclear;

	private int clear_grade;

	private int cur_stage_index;

	public int max_stage_index;

	private int coin;

	private int dropcoin;

	private int jade;

	private int dropjade;

	private int getcoin;

	private int getexp;

	private int chalv;

	private int chaexp;

	private int language;

	private float getcoin_f;

	private float getexp_f;

	private bool gonext;

	private bool starmove;

	public GUISkin basicSkin;

	public Texture2D bg_asset;

	public Texture2D gauge_exp;

	public Texture2D bg_list;

	public Texture2D bg_gauge;

	public Texture2D icon_coin;

	public Texture2D icon_jade;

	public Texture2D star_big;

	public Texture2D bg_speech2;

	public Texture2D[] img_bts = new Texture2D[5];

	public Texture2D[] icon_mission = new Texture2D[3];

	public GUIStyle bt_empty;

	public Transform pt_star;

	public Transform inven_weapon;

	private float[] icon_size = new float[3] { 300f, 300f, 300f };

	private float posX = 1000f;

	private GameObject cashshop;

	private bool movefinish;

	private bool b_getgem;

	private int changelevel;

	private short icon_pluspos;

	private Transform ranking;

	private bool b_openshop;

	private short[] mission_archive_current = new short[3];

	private short[] mission_archive_already = new short[3];

	private bool[] efon = new bool[3];

	private int[] curtraining = new int[6];

	private bool trainmax;

	public AudioClip bgm_victory;

	public AudioClip bgm_defeat;

	private int cur_difficulty;

	private void Awake()
	{
		int num = Crypto.Load_int_key("stage_clear");
		cur_difficulty = Crypto.Load_int_key("n14");
		isclear = (short)(num / 1000);
		clear_grade = num % 1000;
		for (int i = 0; i < 3; i++)
		{
			mission_archive_current[i] = (short)(clear_grade % 10);
			clear_grade /= 10;
		}
		int[] intArray = PlayerPrefsX.GetIntArray("n15");
		cur_stage_index = Crypto.Load_int_key("cur_stage_index");
		max_stage_index = Crypto.Load_int_key("n06");
		int num2 = 0;
		if (max_stage_index < 5)
		{
			Object.Destroy(GameObject.Find("pet_eagle_UI"));
		}
		num2 = intArray[cur_stage_index];
		for (int j = 0; j < 3; j++)
		{
			mission_archive_already[j] = (short)(num2 % 10);
			num2 /= 10;
		}
		int num3 = 0;
		if (mission_archive_already[2] + mission_archive_current[2] > 0)
		{
			num3 += 100;
		}
		if (mission_archive_already[1] + mission_archive_current[1] > 0)
		{
			num3 += 10;
		}
		if (mission_archive_already[0] + mission_archive_current[0] > 0)
		{
			num3++;
		}
		intArray[cur_stage_index] = num3;
		PlayerPrefsX.SetIntArray("n15", intArray);
		Time.timeScale = 1f;
	}

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
		inven_weapon.gameObject.GetComponent<Inventory_Weapon>().EnableGUI();
	}

	private void Start()
	{
		language = PlayerPrefs.GetInt("language");
		base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
		AudioListener.volume = PlayerPrefs.GetFloat("vol_master");
		dropjade = Crypto.Load_int_key("n58");
		dropcoin = Crypto.Load_int_key("n59");
		Crypto.Save_int_key("n58", 0);
		Crypto.Save_int_key("n59", 0);
		coin -= dropcoin;
		jade -= dropjade;
		chalv = Crypto.Load_int_key("n47");
		chaexp = Crypto.Load_int_key("n11");
		getexp_f = chaexp;
		if (chalv < 19)
		{
			changelevel = Crypto.Load_int_key("changelevel");
			if (changelevel == 1 && chalv % 2 == 1)
			{
				changelevel = 1;
			}
			else
			{
				changelevel = 0;
			}
		}
		Crypto.Save_int_key("changelevel", 0);
		if (isclear == 0)
		{
			getcoin = 0;
			getexp = 0;
			base.GetComponent<AudioSource>().clip = bgm_defeat;
			base.GetComponent<AudioSource>().Play();
			curtraining = PlayerPrefsX.GetIntArray("n20");
			int num = 0;
			for (int i = 0; i < 6 && curtraining[i] >= 10; i++)
			{
				num++;
			}
			if (num == 6)
			{
				trainmax = true;
			}
			return;
		}
		float num2 = 1f;
		switch (cur_difficulty)
		{
		case 0:
			num2 = 1f;
			break;
		case 1:
			num2 = 1.1f;
			break;
		case 2:
			num2 = 1.5f;
			break;
		}
		getcoin = (int)((float)(cur_stage_index * 2 + 100) * num2);
		getexp = (cur_stage_index + 5) * 20;
		base.GetComponent<AudioSource>().clip = bgm_victory;
		base.GetComponent<AudioSource>().Play();
	}

	public void GoNext()
	{
		gonext = true;
	}

	public void AmountJade(int _jade)
	{
		jade = _jade;
	}

	public void AmountCoin(int _coin)
	{
		coin = _coin;
	}

	public void GetGem()
	{
		b_getgem = true;
	}

	private void Update()
	{
		if (!movefinish)
		{
			posX = Mathf.MoveTowards(posX, 270f, Time.deltaTime * 1200f);
			if (posX == 270f && isclear > 0)
			{
				icon_size[0] = Mathf.MoveTowards(icon_size[0], 0f, Time.deltaTime * 800f);
				if (icon_size[0] == 0f)
				{
					if (!efon[0])
					{
						if (mission_archive_current[0] > 0)
						{
							Object.Instantiate(pt_star, new Vector3(-0.44f, 0.55f, 0.1f), Quaternion.identity);
						}
						icon_size[0] = 80f;
						efon[0] = true;
					}
					icon_size[1] = Mathf.MoveTowards(icon_size[1], 0f, Time.deltaTime * 800f);
					if (icon_size[1] == 0f)
					{
						if (!efon[1])
						{
							if (mission_archive_current[1] > 0)
							{
								Object.Instantiate(pt_star, new Vector3(-0.74f, 0.55f, 0.1f), Quaternion.identity);
							}
							icon_size[1] = 80f;
							efon[1] = true;
						}
						icon_size[2] = Mathf.MoveTowards(icon_size[2], 0f, Time.deltaTime * 800f);
						if (icon_size[2] == 0f)
						{
							if (!efon[2])
							{
								if (mission_archive_current[2] > 0)
								{
									Object.Instantiate(pt_star, new Vector3(-1.04f, 0.55f, 0.1f), Quaternion.identity);
								}
								icon_size[2] = 80f;
								efon[2] = true;
							}
							else
							{
								movefinish = true;
							}
						}
					}
				}
				starmove = true;
			}
		}
		if (!openbox)
		{
			if (isclear > 0)
			{
				if (show_delay > 3f)
				{
					GameObject.Find("cha_ui").GetComponent<Cha_Costume>().OpenGiftBox();
					openbox = true;
				}
				else
				{
					show_delay += Time.deltaTime;
				}
			}
			else if (show_delay > 3f)
			{
				GoNext();
				openbox = true;
			}
			else
			{
				show_delay += Time.deltaTime;
			}
		}
		if (getpoint)
		{
			getcoin_f += Time.deltaTime * 50f;
			if (getcoin_f >= (float)getcoin)
			{
				getcoin_f = getcoin;
				startexpgain = true;
				getpoint = false;
				coin += getcoin + dropcoin;
				jade += dropjade;
				Crypto.Property_change(getcoin + dropcoin, _jade: false);
			}
		}
		else
		{
			if (!startexpgain)
			{
				return;
			}
			if (chalv >= 199)
			{
				startexpgain = false;
				chaexp = 0;
				getexp_f = 0f;
				getexp = 0;
				Crypto.Save_int_key("n11", 0);
				return;
			}
			getexp_f += Time.deltaTime * (float)(cur_stage_index * 20 + 50);
			if (getexp_f >= (float)(getexp + chaexp))
			{
				getexp_f = (float)getexp + (float)chaexp;
				startexpgain = false;
				Crypto.Save_int_key("n11", (int)getexp_f);
			}
			else if (getexp_f >= (float)(chalv * 100))
			{
				getexp_f = 0f;
				getexp -= chalv * 100;
				chalv++;
				Crypto.Save_int_key("n47", chalv);
				PlayerPrefs.SetInt("updateRank", 1);
				if (chalv >= 19)
				{
					changelevel = 0;
				}
				else if (changelevel == 0 && chalv % 2 == 1)
				{
					changelevel = 1;
				}
			}
		}
	}

	public void CashshopOpen(int _kind)
	{
		Crypto.Save_int_key("cashshopkind", _kind);
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
	}

	public void OpenShop()
	{
		b_openshop = true;
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.Box(Crypto.Rect2(380f, 4f, 80f, 20f), Language.intxt[language, 64] + " " + (cur_stage_index + 1));
		if (b_openshop)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
			GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
			GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
			if (GUI.Button(Crypto.Rect2(144f, 2f, 100f, 24f), string.Empty, bt_empty))
			{
				CashshopOpen(2);
			}
			if (GUI.Button(Crypto.Rect2(272f, 2f, 88f, 24f), string.Empty, bt_empty))
			{
				CashshopOpen(1);
			}
		}
		if (isclear > -1)
		{
			GUI.DrawTexture(Crypto.Rect2(posX, 44f, 180f, 190f), bg_list);
			GUI.DrawTexture(Crypto.Rect2(posX + 26f, 137f, 16f, 16f), icon_coin);
			GUI.Label(Crypto.Rect2(posX + 76f, 138f, 124f, 14f), string.Empty + (int)getcoin_f, "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(posX + 55f, 118f, 114f, 14f), bg_gauge);
			GUI.Label(Crypto.Rect2(posX + 4f, 118f, 50f, 16f), "      " + chalv, "txt14_w");
			GUI.DrawTexture(Crypto.Rect2(posX + 57f, 120f, getexp_f * 112f / (float)(chalv * 100), 10f), gauge_exp);
			GUI.Label(Crypto.Rect2(posX, 98f, 180f, 16f), Language.intxt[language, 262], "txt14_w");
			GUI.Label(Crypto.Rect2(posX, 167f, 180f, 16f), Language.intxt[language, 263], "txt14_w");
			GUI.DrawTexture(Crypto.Rect2(posX + 26f, 186f, 16f, 16f), icon_coin);
			GUI.DrawTexture(Crypto.Rect2(posX + 26f, 206f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(posX + 76f, 187f, 124f, 14f), string.Empty + dropcoin, "txt12_0");
			GUI.Label(Crypto.Rect2(posX + 76f, 207f, 124f, 14f), string.Empty + dropjade, "txt12_0");
			for (int i = 0; i < 3; i++)
			{
				if (mission_archive_already[i] > 0)
				{
					GUI.DrawTexture(Crypto.Rect2(277 + 50 * i, 34f, 64f, 64f), star_big);
				}
			}
			if (starmove)
			{
				for (int j = 0; j < 3; j++)
				{
					if (mission_archive_current[j] > 0)
					{
						if (!efon[j])
						{
							GUI.DrawTexture(Crypto.Rect2((float)(277 + 50 * j) - icon_size[j] * 3f, 34f, icon_size[j] + 64f, icon_size[j] + 64f), star_big);
						}
						else
						{
							GUI.DrawTexture(Crypto.Rect2((float)(277 + 50 * j) - icon_size[j] * 0.5f, 34f - icon_size[j] * 0.5f, icon_size[j] + 64f, icon_size[j] + 64f), star_big);
						}
					}
					GUI.DrawTexture(Crypto.Rect2(293 + 50 * j, 50f, 32f, 32f), icon_mission[j]);
				}
			}
		}
		if (!gonext)
		{
			return;
		}
		GUI.DrawTexture(Crypto.Rect2(400f, 250f, 64f, 64f), img_bts[0]);
		if (GUI.Button(Crypto.Rect2(400f, 250f, 64f, 64f), Language.intxt[language, 202], bt_empty))
		{
			Application.LoadLevel("Map");
		}
		if (isclear == 0)
		{
			icon_pluspos = 76;
			GUI.DrawTexture(Crypto.Rect2(324f, 250f, 64f, 64f), img_bts[1]);
			if (GUI.Button(Crypto.Rect2(324f, 250f, 64f, 64f), Language.intxt[language, 201], bt_empty))
			{
				Application.LoadLevel("Stage");
			}
		}
		else if ((isclear == 2 || b_getgem) && max_stage_index >= 3)
		{
			icon_pluspos = 76;
			GUI.DrawTexture(Crypto.Rect2(324f, 250f, 64f, 64f), img_bts[3]);
			if (GUI.Button(Crypto.Rect2(324f, 250f, 64f, 64f), Language.intxt[language, 398], bt_empty))
			{
				Application.LoadLevel("Forge");
			}
		}
		if (changelevel == 1)
		{
			GUI.DrawTexture(Crypto.Rect2(324 - icon_pluspos, 250f, 64f, 64f), img_bts[2]);
			if (GUI.Button(Crypto.Rect2(324 - icon_pluspos, 250f, 64f, 64f), Language.intxt[language, 395], bt_empty))
			{
				Application.LoadLevel("Skill");
			}
			GUI.DrawTexture(Crypto.Rect2(198 - icon_pluspos, 256f, 128f, 64f), bg_speech2);
			GUI.Label(Crypto.Rect2(206 - icon_pluspos, 264f, 112f, 32f), Language.intxt[language, 261], "txt12_0");
		}
		else if (isclear == 0 && max_stage_index >= 3 && !trainmax)
		{
			GUI.DrawTexture(Crypto.Rect2(324 - icon_pluspos, 250f, 64f, 64f), img_bts[4]);
			if (GUI.Button(Crypto.Rect2(324 - icon_pluspos, 250f, 64f, 64f), Language.intxt[language, 397], bt_empty))
			{
				Application.LoadLevel("Status");
			}
			GUI.DrawTexture(Crypto.Rect2(198 - icon_pluspos, 256f, 128f, 64f), bg_speech2);
			GUI.Label(Crypto.Rect2(206 - icon_pluspos, 264f, 112f, 32f), Language.intxt[language, 301], "txt12_0");
		}
	}
}
