using System;
using System.Collections;
using UnityEngine;

public class UI_Ingame : MonoBehaviour
{
	private int chamaxhp = 100;

	private int chahp = 100;

	private float chasp = 100f;

	private float chamaxsp = 100f;

	private float chaexp;

	private int chalv = 1;

	private int atk;

	private int def;

	private int hit;

	private int spr;

	private bool chargeon;

	private bool infinitymode;

	private bool returnmap;

	private int soul = 1;

	private float f_soul;

	private int jade;

	private int coin;

	private int extreme_point;

	private int dropcoin;

	private int dropjade;

	private int total_star;

	private float soulgain;

	public Transform pack;

	public Transform pt_levelup;

	public Transform num_level;

	public Transform bg_popui;

	public Transform[] cut_extreme = new Transform[2];

	public Texture2D[] card = new Texture2D[3];

	public Transform ui_ingame_gui;

	public Transform txt_maxcombo;

	private Transform txt_result;

	private Transform txt_star;

	private Transform g_mp;

	private Transform g_hp;

	private Transform g_power;

	private TextMesh stage_num;

	private TextMesh point_num;

	private TextMesh extreme_num;

	private Transform getcoin_num;

	private Transform giftbox;

	private Transform cha1;

	private Transform g_combo;

	private int chaprevlv;

	private int damagecount;

	private float finishdelay;

	private float hp_length;

	private float sp_length;

	private float mp_length;

	private float exp_height;

	private float combo_length;

	private float playtime;

	private bool general;

	private bool generaldead;

	private bool gauge_scalechange;

	private int cur_general = -1;

	public int cur_difficulty;

	public bool angelOn;

	private int cur_angel;

	private int play_kind;

	private float f_charge;

	private bool loadingfinish;

	private bool supermode;

	private int wave;

	private int enemykill;

	private int grappling;

	private int finalstage;

	private int exattack;

	private int max_stage_index;

	private int cur_stage_index;

	private int tutorial = -1;

	private int getcoin;

	private short killcount;

	private short rewardkind;

	private int totalkillcount;

	private int tutorial_maxcombo;

	private short stage_progress;

	private Cha_Control script_cha;

	private MakeUI script_pack;

	private Spawn script_spawn;

	private UI_Ingame_GUI script_OnGUI;

	private Icon_Skill script_icon;

	private Gauge_UV gauge_hp;

	private Gauge_UV gauge_sp;

	private Gauge_UV gauge_mp;

	private Gauge_UV gauge_exp;

	private Gauge_UV gauge_power;

	private Gauge_UV gauge_stage;

	private Gauge_UV gauge_combo;

	private bool stageRegen = true;

	private float extreme_point_cur;

	private void OnApplicationPause(bool pocus)
	{
		if (pocus && script_OnGUI != null && !ui_ingame_gui.gameObject.active)
		{
			PauseOn();
		}
	}

	private void Start()
	{
		max_stage_index = Crypto.Load_int_key("n06");
		chalv = Crypto.Load_int_key("n47");
		chaprevlv = chalv;
		chaexp = Crypto.Load_int_key("n11");
		tutorial_maxcombo = PlayerPrefs.GetInt("tutorial2");
		script_OnGUI = ui_ingame_gui.GetComponent<UI_Ingame_GUI>();
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha = cha1.GetComponent<Cha_Control>();
		script_icon = GameObject.FindWithTag("icon_skill").GetComponent<Icon_Skill>();
		script_spawn = GameObject.FindWithTag("Respawn").GetComponent<Spawn>();
		script_pack = pack.GetComponent<MakeUI>();
		infinitymode = script_spawn.infinitymode;
		finalstage = script_spawn.finalstage;
		soulgain = (float)chalv * 0.003f + 0.05f;
		play_kind = Crypto.Load_int_key("play_kind");
		cur_difficulty = Crypto.Load_int_key("n14");
		GameObject gameObject = Resources.Load("stagenum_cave") as GameObject;
		if (infinitymode)
		{
			wave = Crypto.Load_int_key("n49");
			Transform transform = (Transform)UnityEngine.Object.Instantiate(gameObject.transform, gameObject.transform.localPosition + new Vector3(0.03f, -0.02f, 0f), Quaternion.identity);
			extreme_num = transform.GetComponent<TextMesh>();
			extreme_num.characterSize = 0.03f;
			extreme_num.text = wave.ToString();
			GameObject gameObject2 = Resources.Load("getcoin_num") as GameObject;
			getcoin_num = (Transform)UnityEngine.Object.Instantiate(gameObject2.transform, gameObject2.transform.localPosition, Quaternion.identity);
			cur_difficulty = 0;
			script_pack.CreatCustomPlane(new Vector2(0.72f, 0.16f), 0f, new Vector3(0.9f, 2.81f, 2.1f), new Vector2(0f, 0.75f), new Vector2(0.25f, 0.8125f), "bg_point", null, 0f, 0);
			script_pack.CreatCustomPlane(Vector2.one * 0.15f, 0f, new Vector3(0f, 2.9f, 2.1f), new Vector2(0.375f, 0.625f), new Vector2(0.4375f, 0.6875f), "icon_clock", null, 0f, 0);
			Transform transform2 = (Transform)UnityEngine.Object.Instantiate(gameObject.transform, gameObject.transform.localPosition, Quaternion.identity);
			transform2.position -= new Vector3(0.26f, 0.05f, 0f);
			stage_num = transform2.GetComponent<TextMesh>();
			stage_num.characterSize = 0.02f;
			Transform transform3 = (Transform)UnityEngine.Object.Instantiate(gameObject.transform, gameObject.transform.localPosition, Quaternion.identity);
			transform3.position -= new Vector3(-0.55f, 0.13f, 0f);
			point_num = transform3.GetComponent<TextMesh>();
			point_num.characterSize = 0.03f;
			point_num.text = "0";
			txt_star = script_pack.Creat_star(new Vector2(1.2f, 0.3f));
			txt_star.gameObject.active = false;
			GameObject gameObject3 = Resources.Load("giftbox") as GameObject;
			giftbox = (Transform)UnityEngine.Object.Instantiate(gameObject3.transform, Vector3.up * 60f, Quaternion.identity);
			InvokeRepeating("CountDown", 0.1f, 1f);
		}
		else
		{
			tutorial = Crypto.Load_int_key("tutorial");
			g_mp = script_pack.CreatCustomPlane(new Vector2(1.06f, 0.1f), 0f, new Vector3(-0.6775f, 2.78f, 1.8f), new Vector2(0f, 0.588f), new Vector2(0.5f, 0.625f), "gauge_mp", "Gauge_UV", 0f, 0);
			gauge_mp = g_mp.GetComponent<Gauge_UV>();
			g_mp.localScale = Vector3.zero;
			if (cur_difficulty == 1)
			{
				if (play_kind == 5)
				{
					playtime = 300f;
				}
				else if (play_kind == 6)
				{
					playtime = 240f;
				}
				else if (play_kind == 7)
				{
					playtime = 240f;
				}
				else
				{
					playtime = 240f;
				}
				script_pack.CreatCustomPlane(Vector2.one * 0.15f, 0f, new Vector3(0.2f, 2.9f, 2.1f), new Vector2(0.375f, 0.625f), new Vector2(0.4375f, 0.6875f), "icon_clock", null, 0f, 0);
				Transform transform4 = (Transform)UnityEngine.Object.Instantiate(gameObject.transform, gameObject.transform.localPosition, Quaternion.identity);
				transform4.position -= new Vector3(0.04f, 0.02f, 0f);
				stage_num = transform4.GetComponent<TextMesh>();
				stage_num.characterSize = 0.03f;
				InvokeRepeating("CountDown", 0.1f, 1f);
			}
		}
		jade = Crypto.Load_int_key("n24");
		coin = Crypto.Load_int_key("n17");
		f_soul = soul;
		cur_general = script_spawn.cur_general;
		chamaxsp = script_cha.maxsp;
		script_pack.CreatCustomPlane(new Vector2(0.25f, 0.25f), 0f, new Vector3(-1.35f, 2.85f, 2f), new Vector2(0.5f, 0.625f), new Vector2(0.625f, 0.75f), "bg_hp", null, 0f, 0);
		if (cur_difficulty != 2)
		{
			g_hp = script_pack.CreatCustomPlane(new Vector2(0.9f, 0.1f), 0f, new Vector3(-0.757f, 2.92f, 1.8f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5625f), "gauge_hp", "Gauge_UV", 0.2f, 0);
		}
		else
		{
			g_hp = script_pack.CreatCustomPlane(new Vector2(0.4f, 0.12f), 0f, new Vector3(-0.9f, 2.94f, 1.8f), new Vector2(0f, 0.625f), new Vector2(0.1875f, 0.6875f), "gauge_hp", "Gauge_UV", 0f, 0);
		}
		g_power = script_pack.CreatCustomPlane(new Vector2(0.4f, 0.05f), 0f, new Vector3(0f, 2.4f, 2.1f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5625f), "gauge_power", "Gauge_UV", 0f, 0);
		g_combo = script_pack.CreatCustomPlane(new Vector2(0.6f, 0.05f), 0f, new Vector3(0f, 2.74f, 2.1f), new Vector2(0.5f, 0.4375f), new Vector2(1f, 0.47f), "gauge_combo", "Gauge_UV", 0f, 0);
		Transform transform5 = script_pack.CreatCustomPlane(new Vector2(0.3f, 0.1f), 0f, new Vector3(0f, 2.79f, 2.1f), new Vector2(0.0625f, 0.8125f), new Vector2(0.25f, 0.875f), "txt_combo", null, 0f, 0);
		transform5.parent = g_combo;
		Transform transform6 = script_pack.CreatCustomPlane(new Vector2(1.06f, 0.05f), 0f, new Vector3(-0.6775f, 2.85f, 1.8f), new Vector2(0f, 0.47f), new Vector2(0.5f, 0.498f), "gauge_sp", "Gauge_UV", 0.12f, 0);
		Transform transform7 = script_pack.CreatCustomPlane(Vector2.one * 0.186f, 0f, new Vector3(-1.352f, 2.85f, 2.1f), new Vector2(0.875f, 0.75f), new Vector2(1f, 0.875f), "gauge_exp", "Gauge_UV", 0f, 0);
		if (play_kind <= 5)
		{
			script_pack.CreatCustomPlane(new Vector2(0.24f, 0.08f), 0f, new Vector3(0.63f, 2.91f, 2.1f), new Vector2(0.8125f, 0.6875f), new Vector2(1f, 0.75f), "stage", null, 0f, 0);
			Transform transform8 = script_pack.CreatCustomPlane(new Vector2(0.5f, 0.12f), 0f, new Vector3(1f, 2.91f, 2.1f), new Vector2(0.25f, 0.6875f), new Vector2(0.5f, 0.75f), "cur_stage", "Gauge_UV", 0f, 0);
			gauge_stage = transform8.GetComponent<Gauge_UV>();
		}
		gauge_hp = g_hp.GetComponent<Gauge_UV>();
		gauge_sp = transform6.GetComponent<Gauge_UV>();
		gauge_exp = transform7.GetComponent<Gauge_UV>();
		gauge_power = g_power.GetComponent<Gauge_UV>();
		gauge_combo = g_combo.GetComponent<Gauge_UV>();
		script_icon.SoulMeasure(soul);
		GameObject gameObject4 = GameObject.Find("Loading_f");
		if (gameObject4 == null)
		{
			LoadingFinish();
			Camera.main.transform.GetChild(0).gameObject.active = true;
		}
		g_power.gameObject.active = false;
		txt_result = script_pack.Creat_result(new Vector2(1.6f, 0.4f), new Vector3(0f, 2.2f, 2f), "txt_result");
		txt_result.gameObject.active = false;
	}

	public void SkillPlus(int _index)
	{
		script_icon.SkillPlus(_index, general);
	}

	public void ResetTime()
	{
		extreme_num.text = wave.ToString();
		playtime = 0f;
		damagecount = 0;
		InvokeRepeating("CountDown", 0.1f, 1f);
		stageRegen = true;
	}

	private void CountDown()
	{
		int num = (int)(playtime / 60f);
		int num2 = (int)playtime % 60;
		string text = " : ";
		if (num2 < 10)
		{
			text = " : 0";
		}
		stage_num.text = num + text + num2;
	}

	public void SoulStartplus()
	{
		soul = 5;
	}

	public bool CallGeneral(short _dead)
	{
		if (cur_general < 0)
		{
			return false;
		}
		if (!script_cha.life || generaldead)
		{
			return false;
		}
		if (chasp >= 10f)
		{
			if (!general)
			{
				general = true;
				script_icon.SkillIcon_Move(_move: true);
				script_icon.PetIcon_Move(_move: true);
				script_icon.GeneralSkillFillbox(_active: true);
			}
			else
			{
				general = false;
				script_icon.SkillIcon_Move(_move: false);
				script_icon.PetIcon_Move(_move: false);
				script_icon.GeneralSkillFillbox(_active: false);
			}
			g_hp.localScale = -Vector3.one;
			gauge_scalechange = true;
			script_spawn.CallGeneral(general, chahp, chamaxhp);
			script_cha.GeneralOnOff(general, _dead);
			return true;
		}
		return false;
	}

	public void GeneralDead()
	{
		general = false;
		generaldead = true;
		script_icon.SkillIcon_Move(_move: false);
		script_icon.PetIcon_Move(_move: false);
		script_icon.GeneralSkillFillbox(_active: false);
		script_icon.GeneralDead();
	}

	public void WaveSet(int _count)
	{
		enemykill = script_spawn.enemykill;
		grappling = script_spawn.grappling;
		exattack = script_cha.exattack;
		Crypto.Save_int_key("enemykill", enemykill);
		Crypto.Save_int_key("grappling", grappling);
		Crypto.Save_int_key("exattack", exattack);
		Crypto.Save_int_key("n58", dropjade);
		Crypto.Save_int_key("n59", dropcoin);
		Crypto.Save_int_key("n47", chalv);
		PlayerPrefs.SetInt("updateRank", 1);
		Crypto.Save_int_key("n11", (int)chaexp);
		script_icon.SaveSkillUse();
		script_spawn.GeneralHP(_isstart: false);
		if (_count == -1)
		{
			if (infinitymode)
			{
				Crypto.Save_int_key("stage_clear", -1000);
				int num = Crypto.Load_int_key("n54");
				if (extreme_point > num)
				{
					Crypto.Save_int_key("n54", extreme_point);
				}
				int num2 = Crypto.Load_int_key("n48");
				if (wave > num2)
				{
					Crypto.Save_int_key("n48", wave);
				}
			}
			else
			{
				Crypto.Save_int_key("stage_clear", 0);
			}
			Stagefinish();
			ShowTxt(-1);
			total_star = 0;
			return;
		}
		if (_count >= finalstage)
		{
			cur_stage_index = Crypto.Load_int_key("cur_stage_index");
			rewardkind = script_spawn.rewardkind;
			if (cur_stage_index == max_stage_index)
			{
				Crypto.Save_int_key("storycutin", 1);
				if (max_stage_index < 6)
				{
					Crypto.Save_int_key("tutorial", max_stage_index + 1);
					if (max_stage_index == 4)
					{
						Crypto.Save_int_key("n37", 2);
					}
				}
			}
			short num3 = 1;
			if (cur_difficulty == 2)
			{
				num3 += 100;
			}
			else if (cur_difficulty == 1)
			{
				num3 += 10;
			}
			if (infinitymode)
			{
				Crypto.Save_int_key("stage_clear", -1000);
			}
			else
			{
				Crypto.Save_int_key("stage_clear", rewardkind * 1000 + num3);
			}
			Stagefinish();
			ShowTxt(1);
			if (max_stage_index > 4)
			{
				int num4 = UnityEngine.Random.Range(0, 100);
				if (num4 < 20)
				{
					Crypto.Save_int_key("n37", UnityEngine.Random.Range(0, max_stage_index - 1));
				}
			}
			return;
		}
		if (infinitymode)
		{
			int num5 = 90 - (int)playtime + (5 - damagecount) * 10;
			if (num5 > 100)
			{
				total_star = 3;
			}
			else if (num5 > 60)
			{
				total_star = 2;
			}
			else
			{
				total_star = 1;
			}
			int[] array = new int[100];
			array = PlayerPrefsX.GetIntArray("n52");
			if (array[_count - 1] < total_star)
			{
				array[_count - 1] = total_star;
				PlayerPrefsX.SetIntArray("n52", array);
			}
			stage_progress = 0;
			killcount = 0;
			wave = _count + 1;
			int num6 = Crypto.Load_int_key("n48");
			if (wave > num6)
			{
				if ((wave - 1) % 8 == 0)
				{
					if (wave != 1)
					{
						angelOn = true;
						cur_angel = (wave - 1) / 8;
					}
				}
				else
				{
					angelOn = false;
				}
				Crypto.Save_int_key("n48", wave);
			}
			ShowTxt(0);
			CancelInvoke("CountDown");
			stageRegen = false;
		}
		StartCoroutine(WaveClear());
	}

	public void GetAngel()
	{
		Texture mainTexture = Resources.Load("petkind_angel" + cur_angel) as Texture;
		bg_popui.GetComponent<Renderer>().material.mainTexture = mainTexture;
		bg_popui.gameObject.active = true;
		bg_popui.localPosition = new Vector3(0f, 5f, 1.5f);
		Invoke("GetAngel_GUI", 1f);
	}

	private void GetAngel_GUI()
	{
		ui_ingame_gui.gameObject.active = true;
		script_OnGUI.GetAngel(cur_angel);
	}

	public void ComboPlus(float _amount)
	{
		if (supermode || !(combo_length < 0.5f))
		{
			return;
		}
		combo_length += _amount;
		gauge_combo.UvMove(Vector2.right * (0f - combo_length));
		if (combo_length >= 0.5f)
		{
			supermode = true;
			txt_maxcombo.gameObject.active = true;
			txt_maxcombo.GetChild(0).gameObject.active = true;
			g_combo.position = Vector3.up * 5f;
			script_cha.MaxCombo();
			if (tutorial_maxcombo == 0)
			{
				ui_ingame_gui.gameObject.active = true;
				script_OnGUI.SetTutorial(7, 0);
				tutorial_maxcombo = 1;
				PlayerPrefs.SetInt("tutorial2", 1);
			}
		}
	}

	public void SuperModeOn()
	{
		cut_extreme[0].gameObject.active = true;
		cut_extreme[1].gameObject.active = true;
		supermode = false;
		combo_length = 0f;
		gauge_combo.UvMove(Vector2.right * (0f - combo_length));
		txt_maxcombo.gameObject.active = false;
		txt_maxcombo.GetChild(0).gameObject.active = true;
	}

	public void GetCoin()
	{
		getcoin += (int)((float)(wave * 4 + 80) * ((float)total_star * 0.1f + 1f));
		Crypto.Property_change(getcoin, _jade: false);
		getcoin_num.GetComponent<CaveGetCoinNum>().GetOn(getcoin);
	}

	public void GetGeneral()
	{
		int[] intArray = PlayerPrefsX.GetIntArray("n53");
		int num = -1;
		int num2 = UnityEngine.Random.Range(0, 720 + wave * 12);
		num2 = ((num2 > 990) ? 4 : ((num2 > 970) ? 3 : ((num2 > 800) ? 2 : ((num2 > 500) ? 1 : 0))));
		int num3 = 0;
		num3 = ((num2 > 850) ? 3 : ((num2 <= 600) ? 1 : 2));
		int num4 = UnityEngine.Random.Range(0, 90);
		num4 = ((num2 < 2) ? (num4 % 30) : (num4 % 20));
		num4 *= 100000;
		int num5 = UnityEngine.Random.Range(0, 10000) + num2 * 10000 + num3 * 10000000 + num4;
		for (int i = 0; i < 50; i++)
		{
			if (intArray[i] == 0)
			{
				num = i;
				intArray[i] = num5;
				PlayerPrefsX.SetIntArray("n53", intArray);
				break;
			}
		}
		bg_popui.GetComponent<Renderer>().material.mainTexture = card[num3 - 1];
		bg_popui.gameObject.active = true;
		bg_popui.localPosition = new Vector3(0f, 5f, 1.5f);
		if (num == -1)
		{
		}
	}

	public void IntermissionOff()
	{
		GC.Collect();
		script_spawn.OpenDundoor();
	}

	public void ResetExtreme()
	{
		giftbox.transform.position = Vector3.up * 60f;
		giftbox.gameObject.active = false;
		gauge_stage.UvMove(Vector2.zero);
		script_icon.RegenGeneralSkill();
	}

	public void ShowTxt(int isSuccess)
	{
		Vector2 startUV = Vector2.up * 0.5f + Vector2.up * ((float)isSuccess * 0.25f);
		if (isSuccess == 0)
		{
			txt_star.gameObject.active = true;
			txt_star.GetComponent<Txt_star>().SetStar(total_star);
			int num = wave * total_star * 15;
			extreme_point_cur = extreme_point;
			extreme_point += num;
			InvokeRepeating("ScorePlus", 0.5f, 0.05f);
			Crypto.Save_string_key("lastSatge", extreme_num.text);
			Crypto.Save_string_key("highScore", point_num.text);
			Crypto.Save_string_key("useTime", stage_num.text);
			PlayerPrefs.SetInt("needInit", 1);
			PlayerPrefs.SetInt("updateRank", 1);
		}
		txt_result.gameObject.active = true;
		txt_result.GetComponent<Txt_result>().UvMove(startUV);
	}

	private void ScorePlus()
	{
		float num = (float)extreme_point - extreme_point_cur;
		if (num > 1f)
		{
			extreme_point_cur = Mathf.Lerp(extreme_point_cur, extreme_point, Time.deltaTime * 5f);
			point_num.text = ((int)extreme_point_cur).ToString();
			Crypto.Save_string_key("highScore", point_num.text);
		}
		else
		{
			CancelInvoke("ScorePlus");
		}
	}

	public void Stagefinish()
	{
		returnmap = true;
	}

	public IEnumerator WaveClear()
	{
		yield return new WaitForSeconds(2f);
		if (!infinitymode)
		{
			script_spawn.RegenStart();
		}
		else
		{
			IntermissionOn();
		}
		script_cha.Spcharge(100f);
		if (tutorial == -10)
		{
			ui_ingame_gui.gameObject.active = true;
			script_OnGUI.SetTutorial(3, 0);
			tutorial = -20;
		}
		else if (tutorial == -20)
		{
			ui_ingame_gui.gameObject.active = true;
			script_OnGUI.SetTutorial(4, 0);
			tutorial = -1;
		}
		GC.Collect();
	}

	public void IntermissionOn()
	{
		Vector3 vector = cha1.position + cha1.forward * 0.05f;
		if (Vector3.SqrMagnitude(vector) > 1f)
		{
			vector = Vector3.Normalize(vector) * 0.95f;
		}
		giftbox.gameObject.transform.position = vector;
		giftbox.gameObject.active = true;
	}

	public void LoadingFinish()
	{
		loadingfinish = true;
		script_cha.LoadingFinish();
		exp_height = (1f - chaexp / (float)(chalv * 100)) * 0.125f;
		gauge_exp.UvMove(Vector2.up * exp_height);
		mp_length = (1f - f_soul / 8f) * 0.5f;
		num_level.GetComponent<TextMesh>().text = chalv.ToString();
		if (!infinitymode)
		{
			g_mp.localScale = Vector3.one;
			gauge_mp.UvMove(Vector2.right * mp_length);
			script_spawn.RegenStart();
			switch (tutorial)
			{
			case -2:
				ui_ingame_gui.gameObject.active = true;
				script_OnGUI.SetTutorial(1, 0);
				break;
			case -4:
				ui_ingame_gui.gameObject.active = true;
				script_OnGUI.SetTutorial(5, 0);
				break;
			case -3:
				ui_ingame_gui.gameObject.active = true;
				script_OnGUI.SetTutorial(6, 0);
				break;
			}
			Crypto.Save_int_key("tutorial", -1);
		}
	}

	public void ResetPower()
	{
		chargeon = false;
		f_charge = 0f;
		gauge_power.UvMove(Vector2.zero);
		g_power.gameObject.active = false;
	}

	public void PowerCharge()
	{
		chargeon = true;
		g_power.gameObject.active = true;
	}

	public void GrabCharge()
	{
		g_power.gameObject.active = true;
		f_charge += 0.02f;
		gauge_power.UvMove(Vector2.right * (0f - f_charge));
	}

	public void GainCoin(int a)
	{
		coin += a;
		dropcoin += a;
		Crypto.Property_change(a, _jade: false);
	}

	public void GainJade(int a)
	{
		jade += a;
		dropjade += a;
		Crypto.Property_change(a, _jade: true);
	}

	public bool GainSoul(float _amount)
	{
		bool result = false;
		f_soul += _amount;
		if (!infinitymode)
		{
			f_soul = Mathf.Clamp(f_soul, 0f, 8f);
		}
		if ((int)f_soul != soul)
		{
			soul = (int)f_soul;
			mp_length = (1f - (float)soul / 8f) * 0.5f;
			if (!infinitymode)
			{
				gauge_mp.UvMove(Vector2.right * mp_length);
			}
			result = true;
		}
		script_icon.SoulMeasure(soul);
		return result;
	}

	public bool GetExp(int _grab)
	{
		bool result = false;
		script_cha.KillEnemy();
		if (!infinitymode)
		{
			result = GainSoul(soulgain);
		}
		else
		{
			extreme_point += wave * 200;
			point_num.text = extreme_point.ToString();
		}
		chaexp = script_cha.exp;
		chalv = script_cha.level;
		killcount++;
		totalkillcount++;
		if (play_kind <= 5)
		{
			if (infinitymode)
			{
				if (killcount >= 10)
				{
					stage_progress--;
					gauge_stage.UvMove(Vector2.right * (1f / 32f) * stage_progress);
					killcount = 0;
				}
			}
			else if (killcount >= 15)
			{
				stage_progress--;
				gauge_stage.UvMove(Vector2.right * (1f / 32f) * stage_progress);
				killcount = 0;
			}
		}
		exp_height = (1f - chaexp / (float)(chalv * 100)) * 0.125f;
		gauge_exp.UvMove(Vector2.up * exp_height);
		if (chalv != chaprevlv)
		{
			UnityEngine.Object.Instantiate(pt_levelup, new Vector3(-1.35f, 2.84f, 1f), Quaternion.identity);
			num_level.GetComponent<TextMesh>().text = chalv.ToString();
			chaprevlv = chalv;
			Crypto.Save_int_key("changelevel", 1);
		}
		if (tutorial == -2)
		{
			ui_ingame_gui.gameObject.active = true;
			script_OnGUI.SetTutorial(2, 0);
			tutorial = -10;
		}
		return result;
	}

	public void PauseOn()
	{
		if (loadingfinish)
		{
			script_OnGUI.PauseOn();
		}
	}

	public void Resurrection()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
	}

	public void Damaged_Extreme()
	{
		damagecount++;
	}

	public void Damaged()
	{
		hp_length += 0.0625f;
		gauge_hp.UvMove(Vector2.right * hp_length);
		damagecount++;
		if (damagecount >= 3)
		{
			WaveSet(-1);
			script_cha.MissionFail();
			script_cha.StopControl();
		}
	}

	public void GameOver()
	{
		int num = Crypto.Load_int_key("death");
		num++;
		Crypto.Save_int_key("death", num);
		ui_ingame_gui.gameObject.active = true;
		script_OnGUI.ChanceOn();
	}

	public void StatUpdate_sp(float _sp)
	{
		chasp = _sp;
		sp_length = (1f - chasp / chamaxsp) * 0.5f;
		if (chasp >= 50f)
		{
			gauge_sp.UvMove(Vector2.right * sp_length);
		}
		else
		{
			gauge_sp.UvMove(Vector2.right * sp_length - Vector2.up * (1f / 32f));
		}
		if (cur_general != -1 && !generaldead)
		{
			script_icon.ShortSp(chasp);
		}
	}

	public void StatUpdate_hp(int _hp, int _maxhp)
	{
		float num = 0f;
		if (cur_difficulty != 2)
		{
			chahp = _hp;
			chamaxhp = _maxhp;
			num = 1f - (float)chahp / (float)chamaxhp;
			hp_length = num * 0.5f;
			gauge_hp.UvMove(Vector2.right * hp_length);
		}
	}

	private void Update()
	{
		if (!supermode)
		{
			if (combo_length > 0f)
			{
				if (stageRegen)
				{
					g_combo.position = new Vector3(0f, 2.74f, 2.1f);
					combo_length -= Time.deltaTime * 0.01f;
					gauge_combo.UvMove(Vector2.right * (0f - combo_length));
				}
			}
			else
			{
				g_combo.position = Vector3.up * 5f;
			}
		}
		if (chargeon)
		{
			if (f_charge < 0.5f)
			{
				f_charge += Time.deltaTime * 0.6f;
			}
			else
			{
				f_charge = 0f;
				script_cha.Eximpact();
				chargeon = false;
				g_power.gameObject.active = false;
			}
			gauge_power.UvMove(Vector2.right * (0f - f_charge));
		}
		if (returnmap)
		{
			if (finishdelay < 2f)
			{
				finishdelay += Time.deltaTime;
			}
			else if (!infinitymode)
			{
				Application.LoadLevel("Result");
			}
			else
			{
				Application.LoadLevel("Result_Extreme");
			}
		}
		if (cur_difficulty == 1)
		{
			playtime -= Time.deltaTime;
			if (playtime < 0f)
			{
				WaveSet(-1);
			}
		}
		else if (infinitymode)
		{
			playtime += Time.deltaTime;
		}
		if (gauge_scalechange)
		{
			g_hp.localScale = Vector3.MoveTowards(g_hp.localScale, Vector3.one, Time.deltaTime * 8f);
			if (g_hp.localScale == Vector3.one)
			{
				gauge_scalechange = false;
			}
		}
	}
}
