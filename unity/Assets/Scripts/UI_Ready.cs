using UnityEngine;

public class UI_Ready : MonoBehaviour
{
	public Texture2D ci;

	public Texture2D review_icon;

	public Texture2D txt_loading;

	public GUISkin basicSkin;

	private int step;

	private int language;

	public Texture2D coco_bg;

	public Texture2D coco_cha;

	public Texture2D duoku_bg;

	public Texture2D uc_bg;

	public Texture2D jinshan_bg;

	public Texture2D menhu_bg;

	public Texture2D anzhi_bg;

	public Texture2D black_bg;

	public Texture2D coco_logo;

	public Texture2D muzhiwan_bg;

	private string channelId = string.Empty;

	private bool usimCheck;

	private void Awake()
	{
		language = PlayerPrefs.GetInt("language");
	}

	private void Start()
	{
		Crypto.Init();
		int num = PlayerPrefs.GetInt("IsContinue");
		InvokeRepeating("NextScreen", 2f, 1.5f);
		ChannelMgr instance = ChannelMgr.GetInstance();
		instance.init();
		string text = ChannelMgr.GetInstance().getChannelId();
		if (text == "000266")
		{
			if (!AndroidScript.i.MusicEnabled())
			{
				PlayerPrefs.SetFloat("vol_bgm", 0f);
				PlayerPrefs.SetFloat("vol_master", 0f);
			}
			else
			{
				PlayerPrefs.SetFloat("vol_bgm", 1f);
				PlayerPrefs.SetFloat("vol_master", 1f);
			}
		}
	}

	private void NextScreen()
	{
		step++;
	}

	public void GetUsim(string index)
	{
		Crypto.SetUsim(index);
		usimCheck = true;
	}

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		if (step == 1)
		{
			Application.LoadLevel("Intro");
			GUI.DrawTexture(Crypto.Rect2(370f, 292f, 100f, 25f), txt_loading);
			step = 2;
		}
		else if (step == 0)
		{
			channelId = ChannelMgr.GetInstance().getChannelId();
			if (channelId == "000215")
			{
				GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), duoku_bg);
			}
			else if (channelId == "000007")
			{
				GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), duoku_bg);
			}
			else if (channelId == "000255")
			{
				GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), uc_bg);
			}
			else if (channelId == "000441")
			{
				GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), jinshan_bg);
			}
			else if (channelId == "000072")
			{
				GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), menhu_bg);
			}
			else if (channelId == "000005")
			{
				GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), anzhi_bg);
			}
			else if (channelId == "000247")
			{
				GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), muzhiwan_bg);
			}
		}
	}
}
