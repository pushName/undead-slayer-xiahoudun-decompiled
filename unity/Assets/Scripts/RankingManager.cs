using System;
using System.Collections;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
	private const int MAXRANKING = 50;

	private short scopemode;

	public Texture2D[] userimg = new Texture2D[50];

	public int[] userpoint = new int[50];

	public string[] username = new string[50];

	public int mypoint;

	public int mygrade;

	public string resettime;

	public int maxfriend;

	private int factor = 2;

	private short range = 1;

	public bool loadfinish;

	public bool imagefinish;

	private int gameNo;

	private void Awake()
	{
	}

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void S_LoadRanking(int _factor, short _all, short _range)
	{
		DateTime dateTime = DateTime.Now.ToUniversalTime();
		dateTime = dateTime.AddHours(-dateTime.Hour + 21);
		dateTime = dateTime.AddMinutes(-dateTime.Minute);
		dateTime = dateTime.AddSeconds(-dateTime.Second);
		factor = _factor;
		DayOfWeek dayOfWeek = dateTime.DayOfWeek;
		int num = 0 - dayOfWeek;
		resettime = ((num >= 0) ? dateTime.AddDays(num) : dateTime.AddDays(num + 7)).ToLocalTime().ToString("MM/dd hh:mm tt");
		loadfinish = false;
		imagefinish = false;
		scopemode = _all;
		range = _range;
	}

	private IEnumerator ImageLoading(int _imagecount, string _url)
	{
		WWW www = new WWW(_url);
		yield return www;
		if (www.error == null)
		{
			userimg[_imagecount] = www.texture;
		}
		if (maxfriend - 1 == _imagecount)
		{
			imagefinish = true;
		}
	}
}
