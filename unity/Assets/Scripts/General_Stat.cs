using UnityEngine;

public class General_Stat : MonoBehaviour
{
	private const int GENERALKIND = 5;

	public short general_index;

	public short general_kind;

	public short g_maxhp;

	public short g_maxatk;

	public short g_def;

	public float g_atkspd;

	public short g_unique = -1;

	public short g_level;

	public short g_grade;

	private int[] rndkey = new int[8];

	private int[] rndkey_og = new int[8];

	public void SetGeneral(int _seed)
	{
		general_index = (short)(_seed % 10000000 / 100000);
		general_kind = (short)(general_index % 5);
		g_level = (short)((float)_seed / 10000000f);
		if (general_index < 10)
		{
			g_unique = general_index;
		}
		else
		{
			g_unique = -1;
		}
		for (int i = 0; i < 8; i++)
		{
			rndkey_og[i] = _seed % 10;
			_seed /= 10;
			rndkey[i] = rndkey_og[i];
		}
		g_grade = (short)rndkey_og[4];
		for (int j = 0; j < g_level - 1; j++)
		{
			rndkey[j % 4]++;
			if (j != 0 && j % 4 == 0)
			{
				rndkey[(rndkey_og[j % 4] + general_index) % 4]++;
			}
		}
		g_maxatk = (short)((float)rndkey_og[0] * 0.5f + (float)(rndkey[0] - rndkey_og[0]) + (float)g_level + 3f + (float)(g_grade * 2));
		g_def = (short)((float)rndkey_og[1] * 0.5f + 1f + (float)(rndkey[1] - rndkey_og[1]) + (float)(g_grade * 2));
		g_maxhp = (short)(rndkey[2] * 10 + 60 + g_grade * 10);
		g_atkspd = 0.01f + (float)rndkey[3] * 0.002f + (float)g_grade * 0.004f;
	}
}
