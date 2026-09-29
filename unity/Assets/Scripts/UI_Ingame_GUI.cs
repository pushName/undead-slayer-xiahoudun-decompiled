using System;
using UnityEngine;

public class UI_Ingame_GUI : MonoBehaviour
{
	public GUIStyle pausemenu;

	public Texture2D bg_pause;

	public Texture2D bg_option;

	public Texture2D ico_slide;

	public Texture2D icon_jade;

	public Texture2D pop_blank;

	public Texture2D touchthis;

	private Texture2D guide_img;

	private bool guidestart;

	private int guide_index;

	private int guide_maxindex = 1;

	private int guide_curent_subindex;

	private short guide_txt;

	private bool guide_point;

	private Vector2 guide_point_pos = Vector2.zero;

	private Rect guide_img_rect;

	private int getAngelIndex;

	public GUIStyle zoom;

	public GUIStyle sel_lang;

	public GUIStyle bar_slide;

	public GUIStyle bt_yesno;

	private bool infinitymode;

	public Transform angel;

	private float vol_bgm;

	private float vol_master;

	private float color_alpha;

	private bool slideon;

	private bool slideon2;

	private bool angelOn;

	private bool showAds;

	private Cha_Control script_cha;

	private Spawn script_spawn;

	private General_Stat script_generalstat;

	private GameObject cashshop;

	private bool cashshopOn;

	private int count = 10;

	private int require_jade = 1;

	private int jade;

	private bool pause;

	private bool option;

	private bool chance;

	private int language;

	private UI_Ingame script_UI;

	public GUISkin basicSkin;

	private int tempcamfov;

	private void OnEnable()
	{
		jade = Crypto.Load_int_key("n24");
	}

	private void Start()
	{
		Input.multiTouchEnabled = false;
		language = PlayerPrefs.GetInt("language");
		script_UI = GameObject.FindWithTag("ui").GetComponent<UI_Ingame>();
		script_cha = GameObject.FindWithTag("Player").GetComponent<Cha_Control>();
		script_spawn = GameObject.FindWithTag("Respawn").GetComponent<Spawn>();
		vol_bgm = PlayerPrefs.GetFloat("vol_bgm");
		vol_master = PlayerPrefs.GetFloat("vol_master");
		AudioListener.volume = vol_master;
		infinitymode = script_spawn.infinitymode;
		if (infinitymode)
		{
			require_jade = 3;
		}
		tempcamfov = Crypto.Load_int_key("camfov");
		Camera.main.GetComponent<Cam_Move>().FovTest(tempcamfov * -2 + 30);
		base.gameObject.active = false;
		AndroidScript.i.hideCCPlay();
	}

	public void CountDown()
	{
		count--;
		if (count <= 0)
		{
			chance = false;
			Time.timeScale = 1f;
			script_UI.WaveSet(-1);
		}
	}

	public void GetAngel(int _index)
	{
		getAngelIndex = _index;
		Crypto.Save_int_key("n61", getAngelIndex);
		angelOn = true;
		Invoke("GetAngelFinish", 2.4f);
		angel.gameObject.active = true;
		angel.GetComponent<AI_Asist>().SetIndex(getAngelIndex);
	}

	private void GetAngelFinish()
	{
		angelOn = false;
		base.gameObject.active = false;
		GC.Collect();
	}

	public void ChanceOn()
	{
		Time.timeScale = 0.5f;
		chance = true;
		InvokeRepeating("CountDown", 0.5f, 0.5f);
	}

	public void PauseOn()
	{
		if (cashshopOn)
		{
			base.gameObject.active = false;
			return;
		}
		base.gameObject.active = true;
		Time.timeScale = 0f;
		script_cha.StopControl();
		pause = true;
		string channelId = ChannelMgr.GetInstance().getChannelId();
		if (channelId == "000116")
		{
			AndroidScript.i.loadWDJAds();
			showAds = false;
		}
	}

	public void SetTutorial(int _index, int _subindex)
	{
		Time.timeScale = 0f;
		script_cha.StopControl();
		guidestart = true;
		guide_txt = 0;
		guide_curent_subindex = _subindex;
		guide_index = _index;
		int num = guide_index * 10 + guide_curent_subindex;
		guide_img = Resources.Load("guide" + num) as Texture2D;
		switch (num)
		{
		case 10:
			guide_txt = 338;
			guide_maxindex = 3;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 11:
			guide_txt = 339;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 12:
			guide_txt = 340;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 20:
			guide_txt = 341;
			guide_maxindex = 2;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 21:
			guide_txt = 342;
			guide_img_rect = Crypto.Rect2(150f, 110f, 40f, 80f);
			break;
		case 30:
			guide_txt = 36;
			guide_maxindex = 2;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 31:
			guide_txt = 359;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 40:
			guide_txt = 346;
			guide_maxindex = 2;
			guide_point = true;
			guide_point_pos = new Vector2(10f, 40f);
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 41:
			guide_txt = 347;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 50:
			guide_txt = 343;
			guide_maxindex = 3;
			guide_point = false;
			guide_point_pos = new Vector2(10f, 40f);
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 51:
			guide_txt = 344;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 52:
			guide_txt = 345;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 60:
			guide_txt = 348;
			guide_maxindex = 2;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 61:
			guide_txt = 349;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		case 70:
			guide_txt = 433;
			guide_maxindex = 1;
			guide_point = false;
			guide_img_rect = Crypto.Rect2(170f, 134f, 128f, 64f);
			break;
		}
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.depth = 0;
		if (chance)
		{
			if (infinitymode && require_jade > 15)
			{
				chance = false;
				Time.timeScale = 1f;
				script_UI.WaveSet(-1);
				return;
			}
			color_alpha += Time.deltaTime;
			color_alpha = Mathf.Clamp(color_alpha, 0f, 1f);
			GUI.color = Color.black * color_alpha;
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), bg_pause);
			if (count <= 9)
			{
				GUI.color = Color.white;
				GUI.Label(Crypto.Rect2(0f, 130f, 480f, 16f), Language.intxt[language, 203] + "...  " + count, "txt14_w");
				GUI.Label(Crypto.Rect2(246f, 150f, 32f, 16f), string.Empty + require_jade, "txt14_w");
				GUI.DrawTexture(Crypto.Rect2(230f, 150f, 16f, 16f), icon_jade);
				if (jade >= require_jade)
				{
					GUI.Label(Crypto.Rect2(112f, 170f, 256f, 16f), "(" + Language.intxt[language, 204] + "    " + jade + ")", "txt12_w");
					if (GUI.Button(Crypto.Rect2(170f, 190f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
					{
						if (Crypto.Property_change(-require_jade, _jade: true))
						{
							chance = false;
							color_alpha = 0f;
							count = 10;
							Time.timeScale = 1f;
							script_cha.Resurection();
							script_UI.Resurrection();
							base.gameObject.active = false;
							cashshopOn = false;
							CancelInvoke();
							int num = require_jade;
							PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + require_jade + ")\t" + Language.intxt[language, 239]);
							jade -= require_jade;
							if (infinitymode)
							{
								require_jade += 3;
							}
							else
							{
								require_jade *= 2;
							}
							string jsonValue = "{jade:" + num + ",type: revive}";
							AndroidScript.i.LogEvent("consumeJade", jsonValue);
						}
						GC.Collect();
					}
				}
				else
				{
					GUI.Label(Crypto.Rect2(112f, 170f, 256f, 16f), "(" + Language.intxt[language, 204] + "    " + jade + ")", "txt12_r");
					if (GUI.Button(Crypto.Rect2(170f, 190f, 64f, 32f), Language.intxt[language, 207], bt_yesno))
					{
						Crypto.Save_int_key("cashshopkind", 1);
						if (cashshop == null)
						{
							cashshop = Resources.Load("CashShop") as GameObject;
						}
						UnityEngine.Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
						cashshopOn = true;
					}
				}
				if (GUI.Button(Crypto.Rect2(246f, 190f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
				{
					chance = false;
					Time.timeScale = 1f;
					script_UI.WaveSet(-1);
				}
			}
		}
		else if (angelOn)
		{
			GUI.Box(Crypto.Rect2(112f, 90f, 256f, 14f), Language.intxt[language, 434]);
			GUI.Box(Crypto.Rect2(112f, 176f, 256f, 36f), Language.intxt[language, 435]);
		}
		else if (option)
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
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), bg_pause);
			GUI.DrawTexture(Crypto.Rect2(112f, 60f, 256f, 256f), bg_option);
			GUI.Label(Crypto.Rect2(128f, 83f, 70f, 16f), Language.intxt[language, 302], "txt12_w");
			GUI.Label(Crypto.Rect2(128f, 118f, 70f, 16f), Language.intxt[language, 244], "txt12_w");
			GUI.Label(Crypto.Rect2(128f, 155f, 70f, 16f), Language.intxt[language, 90], "txt12_w");
			GUI.Label(Crypto.Rect2(128f, 194f, 70f, 16f), Language.intxt[language, 245], "txt12_w");
			if (GUI.Button(Crypto.Rect2(212f, 186f, 128f, 32f), Language.intxt[language, 246], sel_lang))
			{
				short num2 = 2;
				language = (language + 1) % num2;
				PlayerPrefs.SetInt("language", language);
			}
			if (GUI.RepeatButton(Crypto.Rect2(200f, 76f, 152f, 32f), string.Empty, bar_slide))
			{
				slideon2 = true;
			}
			else if (slideon2)
			{
				AudioListener.volume = vol_master;
				slideon2 = false;
			}
			GUI.DrawTexture(Crypto.Rect2(vol_master * 126f + 206f, 84f, 16f, 16f), ico_slide);
			if (GUI.RepeatButton(Crypto.Rect2(200f, 111f, 152f, 32f), string.Empty, bar_slide))
			{
				slideon = true;
			}
			else if (slideon)
			{
				script_spawn.SetBGM(vol_bgm);
				slideon = false;
			}
			GUI.DrawTexture(Crypto.Rect2(vol_bgm * 126f + 206f, 119f, 16f, 16f), ico_slide);
			if (GUI.Button(Crypto.Rect2(208f, 223f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				option = false;
				PlayerPrefs.SetFloat("vol_bgm", vol_bgm);
				PlayerPrefs.SetFloat("vol_master", vol_master);
			}
			if (GUI.Button(Crypto.Rect2(214f, 147f, 32f, 32f), "-", zoom))
			{
				if (tempcamfov > 0)
				{
					tempcamfov--;
					Camera.main.GetComponent<Cam_Move>().FovTest(tempcamfov * -2 + 30);
					Crypto.Save_int_key("camfov", tempcamfov);
				}
			}
			else if (GUI.Button(Crypto.Rect2(308f, 147f, 32f, 32f), "+", zoom) && tempcamfov < 5)
			{
				tempcamfov++;
				Camera.main.GetComponent<Cam_Move>().FovTest(tempcamfov * -2 + 30);
				Crypto.Save_int_key("camfov", tempcamfov);
			}
			GUI.Label(Crypto.Rect2(260f, 147f, 32f, 32f), "x " + tempcamfov, "txt12_w");
		}
		else if (pause)
		{
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), bg_pause);
			if (GUI.Button(Crypto.Rect2(112f, 80f, 256f, 64f), Language.intxt[language, 30], pausemenu))
			{
				Time.timeScale = 1f;
				pause = false;
				script_cha.StartControl();
				base.gameObject.active = false;
				GC.Collect();
			}
			else if (GUI.Button(Crypto.Rect2(112f, 130f, 256f, 64f), Language.intxt[language, 31], pausemenu))
			{
				option = true;
			}
			else if (GUI.Button(Crypto.Rect2(112f, 180f, 256f, 64f), Language.intxt[language, 59], pausemenu))
			{
				Time.timeScale = 1f;
				pause = false;
				script_UI.WaveSet(-1);
			}
		}
		if (!guidestart)
		{
			return;
		}
		GUI.DrawTexture(Crypto.Rect2(112f, 96f, 256f, 128f), pop_blank);
		GUI.Label(Crypto.Rect2(120f, 102f, 240f, 36f), string.Empty + Language.intxt[language, guide_txt], "txt12_0");
		GUI.Label(Crypto.Rect2(118f, 200f, 244f, 14f), string.Empty + (guide_curent_subindex + 1) + " / " + guide_maxindex, "txt12_0");
		GUI.DrawTexture(guide_img_rect, guide_img);
		if (guide_point)
		{
			GUI.DrawTexture(Crypto.Rect2(guide_point_pos.x, guide_point_pos.y, 64f, 64f), touchthis);
		}
		if (guide_maxindex - 1 <= guide_curent_subindex)
		{
			if (GUI.Button(Crypto.Rect2(290f, 184f, 64f, 32f), Language.intxt[language, 4], bt_yesno))
			{
				guidestart = false;
				Time.timeScale = 1f;
				base.gameObject.active = false;
				script_cha.StartControl();
			}
		}
		else if (GUI.Button(Crypto.Rect2(290f, 184f, 64f, 32f), Language.intxt[language, 47], bt_yesno))
		{
			guide_curent_subindex++;
			SetTutorial(guide_index, guide_curent_subindex);
		}
	}
}
