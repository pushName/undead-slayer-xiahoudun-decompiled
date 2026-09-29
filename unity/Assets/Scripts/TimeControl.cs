using System;
using UnityEngine;

public static class TimeControl
{
	private const int MAXWORK = 2;

	public static DateTime[] work_starttime = new DateTime[2];

	public static string[] string_starttime = new string[2];

	public static TimeSpan difference;

	public static DateTime servertime;

	public static void InitStart()
	{
		string_starttime = PlayerPrefsX.GetStringArray("n60");
		long[] array = new long[2];
		for (int i = 0; i < 2; i++)
		{
			try
			{
				array[i] = Convert.ToInt64(string_starttime[i]);
				ref DateTime reference = ref work_starttime[i];
				reference = DateTime.FromBinary(array[i]);
			}
			catch (IndexOutOfRangeException)
			{
				ref DateTime reference2 = ref work_starttime[i];
				reference2 = DateTime.Now;
			}
		}
	}

	public static void SetDelay(int _index)
	{
		if (!(servertime == default(DateTime)))
		{
			ref DateTime reference = ref work_starttime[_index];
			reference = servertime;
			string_starttime[_index] = work_starttime[_index].ToBinary().ToString();
			PlayerPrefsX.SetStringArray("n60", string_starttime);
		}
	}

	public static void SetRemain(int _index)
	{
		PlayerPrefsX.SetIntArray("n51", _index);
	}

	public static void EraseAll()
	{
		string_starttime = new string[2];
		work_starttime = new DateTime[2];
		for (int i = 0; i < 2; i++)
		{
			if (servertime == default(DateTime))
			{
				ref DateTime reference = ref work_starttime[i];
				reference = DateTime.UtcNow;
			}
			else
			{
				ref DateTime reference2 = ref work_starttime[i];
				reference2 = servertime;
			}
			string_starttime[i] = work_starttime[i].ToBinary().ToString();
		}
		PlayerPrefsX.SetStringArray("n60", string_starttime);
	}

	public static void RequestServerTime()
	{
		servertime = DateTime.Now;
	}

	public static int SubtractDelay(int _index)
	{
		if (servertime == default(DateTime))
		{
			return 0;
		}
		difference = servertime.Subtract(work_starttime[_index]);
		int num = 0;
		num = (int)difference.TotalMinutes;
		return Mathf.Clamp(num, 0, 10000);
	}

	private static void GameQuit()
	{
		for (int i = 0; i < 2; i++)
		{
			string_starttime[i] = work_starttime[i].ToBinary().ToString();
		}
		PlayerPrefsX.SetStringArray("n60", string_starttime);
	}
}
