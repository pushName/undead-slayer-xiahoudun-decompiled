using UnityEngine;

public class LoopArchive : MonoBehaviour
{
	private const int MAXSKILL = 20;

	private const int MAXARCHIVE = 76;

	private const int MAXGENERALPOOL = 30;

	private DB_Archive script_archive;

	private bool gift_archive;

	private int[] temp_staff = new int[30];

	private int[] temp_treasure = new int[24];

	private int[] complete_archive = new int[76];

	private int[] unlock_costume = new int[20];

	private int[] bosskill = new int[12];

	private int[] skill_use = new int[20];

	private int[] pet_skill_use = new int[2];

	private int[] cur_skill_grade = new int[20];

	private int enemykill;

	private int grappling;

	private int exattack;

	private int death;

	private int resurrection;

	private int cashing;

	private int max_stage_index = -1;

	private int remain_arch;

	private int generalsearch;

	private int caveplay;

	private int perfectplay;

	private void Awake()
	{
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
		script_archive = GetComponent<DB_Archive>();
		cur_skill_grade = PlayerPrefsX.GetIntArray("n22");
		caveplay = Crypto.Load_int_key("caveplay");
		perfectplay = Crypto.Load_int_key("perfectplay");
	}

	public bool FindArchive()
	{
		temp_staff = PlayerPrefsX.GetIntArray("staff");
		generalsearch = Crypto.Load_int_key("generalsearch");
		cashing = Crypto.Load_int_key("cashing");
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
					gift_archive = true;
				}
				break;
			case 2:
				if (grappling >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 3:
				if (death >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 4:
				if (resurrection >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 5:
				if (exattack >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 6:
			{
				for (int l = 0; l < 30; l++)
				{
					if (temp_staff[l] == 5)
					{
						gift_archive = true;
						break;
					}
				}
				break;
			}
			case 7:
				if (max_stage_index >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 8:
				if (bosskill[script_archive.aci[i]._require_kind_sub] >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 9:
				if (pet_skill_use[script_archive.aci[i]._require_kind_sub] >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 10:
				if (skill_use[script_archive.aci[i]._require_kind_sub] >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 11:
				if (generalsearch >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 12:
			{
				int num2 = 0;
				for (int k = 0; k < 30; k++)
				{
					if (temp_staff[k] >= 4)
					{
						num2++;
					}
				}
				if (num2 >= 5)
				{
					gift_archive = true;
				}
				break;
			}
			case 13:
			{
				int num4 = 0;
				for (int n = 0; n < 20; n++)
				{
					if (cur_skill_grade[n] >= 0)
					{
						num4++;
					}
				}
				if (num4 >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			}
			case 14:
			{
				int num3 = 0;
				for (int m = 0; m < 20; m++)
				{
					if (cur_skill_grade[m] >= 4)
					{
						num3++;
					}
				}
				if (num3 >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			}
			case 15:
				if (caveplay >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 16:
				if (perfectplay >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
				}
				break;
			case 18:
				if (cashing >= script_archive.aci[i]._require_amount)
				{
					gift_archive = true;
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
					gift_archive = true;
				}
				break;
			}
			case 20:
				if (remain_arch <= 0)
				{
					gift_archive = true;
				}
				break;
			}
		}
		return gift_archive;
	}

	public bool FindTreasure()
	{
		temp_treasure = PlayerPrefsX.GetIntArray("treasure");
		bool result = false;
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
				result = true;
				break;
			}
		}
		return result;
	}
}
