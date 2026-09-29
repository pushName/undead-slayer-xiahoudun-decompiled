using UnityEngine;

public class Inventory_Weapon : MonoBehaviour
{
	private bool popupOn;

	public GUISkin basicSkin;

	public Texture2D bg_black;

	public Texture2D bg_weapon;

	public Texture2D bg_weapon_l;

	public Texture2D titlebase;

	public Texture2D titlebase2;

	public Texture2D icon_lock;

	public Texture2D icon_unlock;

	public Texture2D icon_coin;

	public Texture2D icon_jade;

	public Texture2D pop_blank2;

	public Texture2D[] weapon_kindicon = new Texture2D[3];

	public Texture2D[] weaponicon = new Texture2D[20];

	public Texture2D[] armoricon = new Texture2D[7];

	private int[] bottom_icon = new int[6];

	public Texture2D c_equip;

	public Texture2D pop_blank;

	public Texture2D star_grade;

	public Texture2D icon_new;

	public GUIStyle bt_yesno;

	public GUIStyle bt_equip;

	public GUIStyle erase;

	private bool imagemovefinish;

	private bool getitem;

	private bool nextstart;

	private int bg_posX_r = 480;

	private float icon_posY = 320f;

	private int selectslot = -1;

	private int getitem_seed;

	private int slot_item = 3;

	private int cur_difficulty;

	private int cur_stage_index;

	private int cur_item;

	private short confirm;

	private float nextdelay = 0.5f;

	private bool b_delay;

	private float f_delay;

	private bool stopgui;

	private short rewardkind;

	private int getweapon_meshkind;

	private int getitem_grade;

	private int getweapon_name;

	private int getweapon_maxatk;

	private int getweapon_minatk;

	private int getweapon_kind;

	private float getweapon_spd;

	private int getweapon_special;

	private int getweapon_special_txt;

	private int getweapon_index;

	private int getequip_hp;

	private int getequip_index;

	private int getequip_special;

	private int selequip_index;

	private int selequip_special;

	private int selequip_hp;

	private int selweapon_index;

	private int selweapon_grade;

	private int selweapon_name;

	private int selweapon_maxatk;

	private int selweapon_minatk;

	private int selweapon_kind;

	private float selweapon_spd;

	private int selweapon_special_txt;

	private int sellcost_get;

	private int sellcost_sel;

	private int icon_size = 32;

	private float icon_delay;

	private float posX = 112f;

	private int posY = 90;

	private int posX2;

	private int posY2;

	private int language;

	private int _getslot = -1;

	private UI_result script_ui;

	private Language_Costume script_name;

	private WeaponStat script_weaponstat;

	private int[] item_seed = new int[26];

	private int[] weapon_plusup = new int[26];

	private void Awake()
	{
		cur_stage_index = Crypto.Load_int_key("cur_stage_index");
		script_ui = GameObject.FindWithTag("ui").GetComponent<UI_result>();
		language = PlayerPrefs.GetInt("language");
		script_name = GetComponent<Language_Costume>();
		cur_difficulty = Crypto.Load_int_key("n14");
	}

	private void Start_Weapon()
	{
		script_weaponstat = GetComponent<WeaponStat>();
		cur_item = Crypto.Load_int_key("n44");
		slot_item = Crypto.Load_int_key("n03");
		item_seed = PlayerPrefsX.GetIntArray("n41");
		weapon_plusup = PlayerPrefsX.GetIntArray("n19");
		for (int i = 0; i < slot_item; i++)
		{
			bottom_icon[i] = script_weaponstat.IconImage_Only(item_seed[i]);
		}
	}

	private void Start_Armor()
	{
		cur_item = Crypto.Load_int_key("n05");
		slot_item = Crypto.Load_int_key("n42");
		item_seed = PlayerPrefsX.GetIntArray("n45");
		for (int i = 0; i < slot_item; i++)
		{
			bottom_icon[i] = item_seed[i] % 100;
		}
	}

	public void EnableGUI()
	{
		stopgui = false;
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
			bg_posX_r = (int)((float)bg_posX_r - Mathf.Min(bg_posX_r, Time.deltaTime * 1500f));
			if (bg_posX_r <= 0)
			{
				bg_posX_r = 0;
				icon_posY -= Mathf.Min(icon_posY, Time.deltaTime * 500f);
				if (icon_posY <= 240f)
				{
					icon_posY = 240f;
					imagemovefinish = true;
				}
			}
		}
		else if (getitem)
		{
			icon_delay += Time.deltaTime;
			if ((double)icon_delay > 0.8)
			{
				icon_size = 32;
				icon_delay = 0f;
			}
			else if (icon_delay > 0.4f)
			{
				icon_size = 0;
			}
		}
		else if (nextstart)
		{
			nextdelay -= Time.deltaTime;
			if (nextdelay <= 0f)
			{
				nextdelay = 0f;
				icon_posY += 500f * Time.deltaTime;
				if (icon_posY > 320f)
				{
					nextstart = false;
					script_ui.GoNext();
				}
			}
		}
		if (popupOn)
		{
			posX = Mathf.MoveTowards(posX, 20f, Time.deltaTime * 800f);
		}
		else
		{
			posX = Mathf.MoveTowards(posX, 112f, Time.deltaTime * 800f);
		}
	}

	public void SetUpArmor(int _index)
	{
		selequip_index = _index % 100;
		_index /= 100;
		int[] array = new int[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = _index % 10;
			_index /= 10;
		}
		selequip_special = array[1];
		selequip_hp = _index;
		sellcost_sel = selequip_hp * 5;
	}

	public void SetUpWeapon(int _index)
	{
		script_weaponstat.SetStat(_index);
		selweapon_index = script_weaponstat.weapon_index;
		selweapon_grade = script_weaponstat.weapon_grade;
		selweapon_name = script_weaponstat.weapon_name;
		selweapon_maxatk = script_weaponstat.weapon_maxatk + weapon_plusup[selectslot];
		selweapon_minatk = script_weaponstat.weapon_minatk + weapon_plusup[selectslot];
		selweapon_kind = script_weaponstat.weapon_kind;
		selweapon_spd = script_weaponstat.weapon_spd;
		selweapon_special_txt = script_weaponstat.weapon_special_txt;
		sellcost_sel = (int)(((float)selweapon_index + (float)selweapon_grade * 0.1f) * 10f);
	}

	public void GetArmor(int _index)
	{
		rewardkind = 2;
		Start_Armor();
		getequip_hp = (int)((float)(cur_stage_index + 1) * 0.4f + (float)Random.Range(0, 2) + (float)getitem_grade);
		getequip_index = Random.Range(1, 7);
		getitem_seed = getequip_index + 100 + getequip_special * 1000 + getequip_hp * 10000;
		getitem = true;
		script_ui.OpenShop();
		sellcost_get = getequip_hp * 5;
	}

	public void GetWeapon(int _index)
	{
		rewardkind = 1;
		Start_Weapon();
		getweapon_index = _index;
		getitem_grade = Random.Range(0, 1000);
		switch (cur_difficulty)
		{
		case 0:
			if (getitem_grade > 900)
			{
				getitem_grade = 4;
			}
			else if (getitem_grade > 700)
			{
				getitem_grade = 3;
			}
			else if (getitem_grade > 300)
			{
				getitem_grade = 2;
			}
			else
			{
				getitem_grade = 1;
			}
			break;
		case 1:
			if (getitem_grade > 900)
			{
				getitem_grade = 4;
			}
			else if (getitem_grade > 500)
			{
				getitem_grade = 3;
			}
			else if (getitem_grade > 200)
			{
				getitem_grade = 2;
			}
			else
			{
				getitem_grade = 1;
			}
			break;
		case 2:
			if (getitem_grade > 800)
			{
				getitem_grade = 4;
			}
			else if (getitem_grade > 300)
			{
				getitem_grade = 3;
			}
			else if (getitem_grade > 100)
			{
				getitem_grade = 2;
			}
			else
			{
				getitem_grade = 1;
			}
			break;
		}
		int num = Random.Range(10, 9987) + (getitem_grade - 1) * 3 + Random.Range(0, 4);
		getitem_seed = getweapon_index * 1000000 + getitem_grade * 10000 + num;
		script_weaponstat.SetStat(getitem_seed);
		getweapon_meshkind = script_weaponstat.weapon_meshkind;
		getitem_grade = script_weaponstat.weapon_grade;
		getweapon_name = script_weaponstat.weapon_name;
		getweapon_maxatk = script_weaponstat.weapon_maxatk;
		getweapon_minatk = script_weaponstat.weapon_minatk;
		getweapon_kind = script_weaponstat.weapon_kind;
		getweapon_spd = script_weaponstat.weapon_spd;
		getweapon_special = script_weaponstat.weapon_special;
		getweapon_special_txt = script_weaponstat.weapon_special_txt;
		getitem = true;
		sellcost_get = (int)(((float)getweapon_index + (float)getitem_grade * 0.1f) * 10f);
		script_ui.OpenShop();
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.depth = -5;
		if (stopgui)
		{
			return;
		}
		if (confirm > 0 || nextstart)
		{
			GUI.enabled = false;
		}
		GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 230f, 480f, 84f), bg_black);
		if (getitem)
		{
			GUI.DrawTexture(Crypto.Rect2(posX, posY, 256f, 128f), bg_weapon_l);
			GUI.Box(Crypto.Rect2(posX, posY, icon_size * 8, icon_size * 4), string.Empty, "glow_box");
			if (rewardkind == 1)
			{
				GUI.DrawTexture(Crypto.Rect2(posX + 16f, posY + 33, 64f, 64f), weaponicon[getweapon_meshkind]);
				GUI.Label(Crypto.Rect2(posX + 68f, posY + 10, 128f, 16f), script_name.txt_cos[language, getweapon_name], "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(posX + 212f, posY + 6, 26f, 26f), weapon_kindicon[getweapon_kind]);
				for (int i = 0; i < 3; i++)
				{
					GUI.DrawTexture(Crypto.Rect2(posX + 98f, posY + 33 + i * 20, 140f, 16f), titlebase);
				}
				GUI.Label(Crypto.Rect2(posX + 102f, posY + 33, 128f, 16f), Language.intxt[language, 141] + "   " + getweapon_minatk + " - " + getweapon_maxatk, "txt12_0");
				GUI.Label(Crypto.Rect2(posX + 102f, posY + 53, 128f, 16f), Language.intxt[language, 153] + "   " + (getweapon_spd * 100f).ToString("F1"), "txt12_0");
				GUI.Label(Crypto.Rect2(posX + 102f, posY + 73, 128f, 16f), script_name.txt_cos[language, getweapon_special_txt], "txt12_0");
			}
			else
			{
				GUI.DrawTexture(Crypto.Rect2(posX + 16f, posY + 33, 64f, 64f), armoricon[getequip_index]);
				GUI.Label(Crypto.Rect2(posX + 68f, posY + 10, 128f, 16f), script_name.txt_cos[language, getequip_index + 1], "txt12_w");
				for (int j = 0; j < 2; j++)
				{
					GUI.DrawTexture(Crypto.Rect2(posX + 98f, posY + 33 + j * 20, 140f, 16f), titlebase);
				}
				GUI.Label(Crypto.Rect2(posX + 102f, posY + 33, 128f, 16f), Language.intxt[language, 142] + " + " + getequip_hp, "txt12_0");
				GUI.Label(Crypto.Rect2(posX + 102f, posY + 53, 128f, 16f), script_name.txt_cos[language, getequip_special + 21], "txt12_0");
			}
			GUI.DrawTexture(Crypto.Rect2(posX + 32f, posY + 32, icon_size, 16f), icon_new);
			for (int k = 0; k < getitem_grade; k++)
			{
				GUI.DrawTexture(Crypto.Rect2(posX - (float)(getitem_grade * 7) + 47f + (float)(k * 14), posY + 99, 16f, 16f), star_grade);
			}
			if (GUI.Button(Crypto.Rect2(posX + 84f, posY + 90, 32f, 32f), string.Empty, erase))
			{
				selectslot = -1;
				popupOn = false;
				confirm = 1;
			}
			else if (GUI.Button(Crypto.Rect2(posX + 174f, posY + 90, 64f, 32f), Language.intxt[language, 45], bt_yesno))
			{
				_getslot = -1;
				for (int l = 0; l < slot_item; l++)
				{
					if (item_seed[l] == 0)
					{
						_getslot = l;
						break;
					}
				}
				if (_getslot != -1)
				{
					item_seed[_getslot] = getitem_seed;
					if (rewardkind == 1)
					{
						PlayerPrefsX.SetIntArray("n41", item_seed);
						bottom_icon[_getslot] = script_weaponstat.IconImage_Only(item_seed[_getslot]);
					}
					else
					{
						PlayerPrefsX.SetIntArray("n45", item_seed);
						bottom_icon[_getslot] = item_seed[_getslot] % 100;
					}
					confirm = 7;
					getitem = false;
					popupOn = false;
					nextstart = true;
					bg_posX_r = 480;
				}
				else if (slot_item < 6)
				{
					confirm = 2;
				}
				else
				{
					Delay(1f);
					confirm = 6;
				}
			}
		}
		if (popupOn)
		{
			posX2 = 280;
			posY2 = 90;
			GUI.DrawTexture(Crypto.Rect2(posX2, posY2, 256f, 128f), bg_weapon);
			if (rewardkind == 1)
			{
				if (weapon_plusup[selectslot] > 0)
				{
					GUI.Label(Crypto.Rect2(posX2 + 26, posY2 + 10, 128f, 16f), script_name.txt_cos[language, selweapon_name] + " + " + weapon_plusup[selectslot], "txt12_w");
				}
				else
				{
					GUI.Label(Crypto.Rect2(posX2 + 26, posY2 + 10, 128f, 16f), script_name.txt_cos[language, selweapon_name], "txt12_w");
				}
				GUI.DrawTexture(Crypto.Rect2(posX2 + 137, posY2 + 6, 26f, 26f), weapon_kindicon[selweapon_kind]);
				for (int m = 0; m < 3; m++)
				{
					GUI.DrawTexture(Crypto.Rect2(posX2 + 24, posY2 + 33 + m * 20, 140f, 16f), titlebase);
				}
				for (int n = 0; n < selweapon_grade; n++)
				{
					GUI.DrawTexture(Crypto.Rect2(posX2 - selweapon_grade * 7 + 36 + n * 14, posY2 + 99, 16f, 16f), star_grade);
				}
				GUI.Label(Crypto.Rect2(posX2 + 30, posY2 + 33, 128f, 16f), Language.intxt[language, 141] + "   " + selweapon_minatk + " - " + selweapon_maxatk, "txt12_0");
				GUI.Label(Crypto.Rect2(posX2 + 30, posY2 + 53, 128f, 16f), Language.intxt[language, 153] + "   " + (selweapon_spd * 100f).ToString("F1"), "txt12_0");
				GUI.Label(Crypto.Rect2(posX2 + 30, posY2 + 73, 128f, 16f), script_name.txt_cos[language, selweapon_special_txt], "txt12_0");
			}
			else
			{
				GUI.Label(Crypto.Rect2(posX2 + 26, posY2 + 10, 128f, 16f), script_name.txt_cos[language, selequip_index + 1], "txt12_w");
				for (int num = 0; num < 2; num++)
				{
					GUI.DrawTexture(Crypto.Rect2(posX2 + 24, posY2 + 33 + num * 20, 140f, 16f), titlebase);
				}
				GUI.Label(Crypto.Rect2(posX2 + 30, posY2 + 33, 128f, 16f), Language.intxt[language, 142] + " + " + selequip_hp, "txt12_0");
				GUI.Label(Crypto.Rect2(posX2 + 30, posY2 + 53, 128f, 16f), script_name.txt_cos[language, selequip_special + 21], "txt12_0");
			}
			if (GUI.Button(Crypto.Rect2(posX2 + 62, posY2 + 90, 32f, 32f), string.Empty, erase))
			{
				if (selectslot == cur_item)
				{
					Delay(1.5f);
					confirm = 5;
				}
				else
				{
					confirm = 1;
				}
			}
			else if (GUI.Button(Crypto.Rect2(posX2 + 100, posY2 + 90, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				popupOn = false;
				selectslot = -1;
			}
		}
		for (int num2 = 0; num2 < 6; num2++)
		{
			if (GUI.Button(Crypto.Rect2(num2 * 76 + 18, icon_posY, 64f, 64f), string.Empty, bt_equip))
			{
				if (num2 >= slot_item)
				{
					confirm = 3;
				}
				else if (item_seed[num2] > 0 && selectslot != num2)
				{
					selectslot = num2;
					if (rewardkind == 1)
					{
						SetUpWeapon(item_seed[selectslot]);
					}
					else
					{
						SetUpArmor(item_seed[selectslot]);
					}
					popupOn = true;
				}
			}
			if (item_seed[num2] > 0)
			{
				if (rewardkind == 1)
				{
					GUI.DrawTexture(Crypto.Rect2(num2 * 76 + 18, icon_posY, 64f, 64f), weaponicon[bottom_icon[num2]]);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(num2 * 76 + 18, icon_posY, 64f, 64f), armoricon[bottom_icon[num2]]);
				}
			}
			else if (num2 >= slot_item)
			{
				GUI.DrawTexture(Crypto.Rect2(num2 * 76 + 18, icon_posY, 64f, 64f), icon_lock);
			}
			if (num2 == cur_item && nextdelay > 0f)
			{
				GUI.Label(Crypto.Rect2(num2 * 76 + 18, icon_posY - 16f, 64f, 16f), Language.intxt[language, 89], "txt12_w");
			}
		}
		if (selectslot != -1)
		{
			GUI.DrawTexture(Crypto.Rect2(selectslot * 76 + 18, icon_posY, 64f, 64f), c_equip);
		}
		if (confirm <= 0)
		{
			return;
		}
		GUI.enabled = true;
		if (confirm == 1)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 12], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(200f, 148f, 16f, 16f), icon_coin);
			if (selectslot == -1)
			{
				GUI.Label(Crypto.Rect2(224f, 148f, 48f, 16f), string.Empty + sellcost_get, "txt12_b");
			}
			else
			{
				GUI.Label(Crypto.Rect2(224f, 148f, 48f, 16f), string.Empty + sellcost_sel, "txt12_b");
			}
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				int num3 = Crypto.Load_int_key("n17");
				if (selectslot == -1)
				{
					nextstart = true;
					getitem = false;
					bg_posX_r = 480;
					if (rewardkind == 1)
					{
						Crypto.Property_change((int)(((float)getweapon_index + (float)getitem_grade * 0.1f) * 10f), _jade: false);
						num3 += (int)(((float)getweapon_index + (float)getitem_grade * 0.1f) * 10f);
					}
					else
					{
						Crypto.Property_change(getequip_hp * 5, _jade: false);
						num3 += getequip_hp * 5;
					}
				}
				else
				{
					item_seed[selectslot] = 0;
					if (rewardkind == 1)
					{
						Crypto.Property_change((int)(((float)selweapon_index + (float)selweapon_grade * 0.1f) * 10f), _jade: false);
						num3 += (int)(((float)selweapon_index + (float)selweapon_grade * 0.1f) * 10f);
						weapon_plusup[selectslot] = 0;
						PlayerPrefsX.SetIntArray("n41", item_seed);
						PlayerPrefsX.SetIntArray("n19", weapon_plusup);
					}
					else
					{
						Crypto.Property_change(selequip_hp * 5, _jade: false);
						num3 += selequip_hp * 5;
						PlayerPrefsX.SetIntArray("n45", item_seed);
					}
					popupOn = false;
				}
				confirm = 0;
				script_ui.AmountCoin(num3);
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 2)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 23], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				confirm = 3;
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 3)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.DrawTexture(Crypto.Rect2(160f, 98f, 160f, 18f), titlebase2);
			GUI.Label(Crypto.Rect2(120f, 120f, 230f, 16f), Language.intxt[language, 300], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(186f, 134f, 32f, 32f), icon_unlock);
			GUI.DrawTexture(Crypto.Rect2(244f, 142f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(264f, 142f, 16f, 16f), "3", "txt12_b");
			GUI.Label(Crypto.Rect2(112f, 162f, 256f, 16f), Language.intxt[language, 350], "txt12_b");
			if (rewardkind == 1)
			{
				GUI.Label(Crypto.Rect2(120f, 98f, 230f, 16f), Language.intxt[language, 235] + " +1", "txt12_w");
			}
			else
			{
				GUI.Label(Crypto.Rect2(120f, 98f, 230f, 16f), Language.intxt[language, 198] + " +1", "txt12_w");
			}
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				int num4 = Crypto.Load_int_key("n24");
				confirm = 0;
				if (num4 >= 3)
				{
					if (Crypto.Property_change(-3, _jade: true))
					{
						num4 -= 3;
						slot_item = Mathf.Min(6, slot_item + 1);
						if (rewardkind == 1)
						{
							Crypto.Save_int_key("n03", slot_item);
							PurchaseLog.LogOn(Language.intxt[language, 234] + " (3)\t" + Language.intxt[language, 235]);
						}
						else
						{
							Crypto.Save_int_key("n42", slot_item);
							PurchaseLog.LogOn(Language.intxt[language, 234] + " (3)\t" + Language.intxt[language, 198]);
						}
						script_ui.AmountJade(num4);
					}
				}
				else
				{
					confirm = 4;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 4)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 21], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				script_ui.CashshopOpen(1);
				stopgui = true;
				confirm = 0;
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 5)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 206], "txt12_0");
			if (f_delay == 0f)
			{
				confirm = 0;
			}
		}
		else if (confirm == 6)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 267], "txt12_0");
			if (f_delay == 0f)
			{
				confirm = 0;
			}
		}
		else
		{
			if (confirm != 7)
			{
				return;
			}
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 27], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				cur_item = _getslot;
				if (rewardkind == 1)
				{
					Crypto.Save_int_key("n44", cur_item);
					Crypto.Save_int_key("n34", getweapon_meshkind + getweapon_kind * 100);
					Crypto.Save_int_key("n31", getweapon_maxatk);
					PlayerPrefs.SetInt("updateRank", 1);
					Crypto.Save_int_key("n04", getweapon_minatk);
					Crypto.Save_int_key("n35", (int)(getweapon_spd * 100f));
					int num5 = Mathf.Min(getitem_grade, 3);
					Crypto.Save_int_key("n38", num5 * 10 + getweapon_special);
					GameObject.Find("dummy_weapon").GetComponent<Cha_Weapon>().SetStageWeapon();
				}
				else
				{
					Crypto.Save_int_key("n05", cur_item);
					Crypto.Save_int_key("n43", getequip_index);
					GameObject.FindWithTag("Player").GetComponent<Cha_Costume>().Costume(getequip_index);
				}
				confirm = 0;
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
	}
}
