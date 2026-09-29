using UnityEngine;

public class WeaponStat : MonoBehaviour
{
	public int weapon_index;

	public int weapon_meshkind;

	public int weapon_kind;

	public int weapon_name;

	public int weapon_maxatk;

	public int weapon_minatk;

	public float weapon_spd;

	public int weapon_special;

	public int weapon_grade;

	public int weapon_upgrade;

	public int weapon_special_txt;

	public int weapon_cost;

	private int bottom_weaponicon;

	public weapon[] wi = new weapon[25];

	private int[] extractseed = new int[6];

	private void Awake()
	{
		wi = GetComponent<DB_Weapon>().wi;
	}

	public int IconImage_Only(int _seed)
	{
		int num = _seed / 1000000;
		bottom_weaponicon = wi[num]._mesh;
		return bottom_weaponicon;
	}

	public int Cost_only(int _seed)
	{
		int num = _seed / 1000000;
		return wi[num]._jadecost;
	}

	public void SetStat(int _swordindex)
	{
		for (int i = 0; i < 6; i++)
		{
			extractseed[i] = _swordindex % 10;
			_swordindex /= 10;
		}
		weapon_index = _swordindex;
		weapon_meshkind = wi[weapon_index]._mesh;
		weapon_kind = wi[weapon_index]._kind;
		weapon_grade = extractseed[4];
		weapon_upgrade = extractseed[5];
		weapon_name = wi[weapon_index]._name;
		if (weapon_grade != 4)
		{
			weapon_maxatk = (int)((float)(extractseed[0] * weapon_index) * 0.05f + (float)weapon_grade * (2f + (float)weapon_index * 0.05f) + (float)wi[weapon_index]._power);
		}
		else
		{
			weapon_maxatk = extractseed[0] + weapon_grade * 3 + wi[weapon_index]._power;
		}
		weapon_minatk = (int)((0.7f + (float)extractseed[1] * 0.03f) * (float)weapon_maxatk);
		weapon_spd = (float)extractseed[2] * 0.002f + wi[weapon_index]._speed * 0.01f;
		weapon_cost = wi[weapon_index]._jadecost;
		if (weapon_grade > 1)
		{
			weapon_special = extractseed[3] % 7;
		}
		else
		{
			weapon_special = 0;
		}
		if (weapon_grade == 1)
		{
			weapon_special_txt = 0;
		}
		else if (weapon_grade == 2)
		{
			weapon_special_txt = weapon_special % 10 + 91;
		}
		else if (weapon_grade == 3)
		{
			weapon_special_txt = weapon_special % 10 + 101;
		}
		else if (weapon_grade == 4)
		{
			if (weapon_index < 16)
			{
				weapon_special_txt = weapon_special % 10 + 101;
			}
			else
			{
				weapon_special_txt = weapon_special % 10 + 111;
			}
		}
	}
}
