using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

public class UI_Friends : MonoBehaviour
{
	public struct facebookfriend
	{
		public string _fbid;

		public long _hspNo;

		public string _name;

		public Texture2D _picture;

		public bool _isfriend;
	}

	public struct hspfriend
	{
		public long _memberNo;

		public string _name;

		public Texture2D _picture;
	}

	public struct recommend
	{
		public long _memberNo;

		public string _name;

		public Texture2D _picture;

		public bool _isfriend;
	}

	private const int MAX_FB_FRIENDS = 5000;

	private const int MAX_HSP_FRIENDS = 100;

	private const int RECOMMEND_FRIENDS = 30;

	private TouchScreenKeyboard mykeyboard;

	private facebookfriend[] ff = new facebookfriend[5000];

	private hspfriend[] hf = new hspfriend[100];

	private recommend[] rf = new recommend[100];

	private int fb_friend_num = 5;

	private int hsp_friend_num;

	private int recommend_num;

	public Texture2D bg_title;

	public Texture2D bg_title_me;

	public Texture2D icon_key;

	public Texture2D toggle_active;

	public Texture2D errorimg;

	public Texture2D[] icon_ranktab = new Texture2D[5];

	public Texture2D please_touch;

	private Texture2D myimage;

	public GUIStyle bt_empty;

	public GUIStyle bt_start;

	public GUIStyle bt_back;

	public GUIStyle bt_plus;

	public GUIStyle bt_remove;

	public GUIStyle bt_yesno;

	public GUISkin basicSkin;

	private float curMousePosY;

	private float startMousePosY;

	private Vector2 scrollPosition;

	private Vector2 prev_scrollPosition;

	private int language;

	private float posX_l = -100f;

	private float posX_r = 500f;

	private float posY = 114f;

	private GameObject ui;

	private bool reload;

	private bool facebooklogin;

	private bool facebook_load;

	private short keyboardactive;

	private string tempnick;

	private short mode;

	private short currenttab;

	private int icon_size;

	private short confirm;

	private short version;

	private string access_token = "abcdef";

	private int detail_count;

	private void Awake()
	{
		language = PlayerPrefs.GetInt("language");
	}

	private void Start()
	{
		GetHSPFriends();
		SetMyImage(_ischange: false);
		InvokeRepeating("Warning_iconsize", 1f, 0.34f);
	}

	public void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
	}

	public void SetMyImage(bool _ischange)
	{
	}

	private IEnumerator MyImageLoading(string _url)
	{
		WWW www = new WWW(_url);
		yield return www;
		if (www.texture.height > 8)
		{
			myimage = www.texture;
		}
	}

	public void FaceLoginFinish()
	{
		facebooklogin = true;
		StartCoroutine(GetFaceBookFriends());
	}

	private IEnumerator GetFaceBookMe()
	{
		string url = "https://graph.facebook.com/me?fields=id,name,picture";
		WWW www = new WWW(url);
		yield return www;
		string myprofile = www.text;
		JSONNode aa = JSON.Parse(myprofile);
		StartCoroutine(GetMyFacebookImg(aa["picture"]["data"]["url"]));
	}

	private IEnumerator GetMyFacebookImg(string _url)
	{
		yield return new WWW(_url);
	}

	private IEnumerator GetFaceBookFriends()
	{
		string url = "https://graph.facebook.com/me/friends?access_token=" + access_token + "&fields=id,name,picture";
		WWW www = new WWW(url);
		yield return www;
		string friendslist = www.text;
		JSONNode aa = JSON.Parse(friendslist);
		fb_friend_num = aa["data"].Count;
		List<string> facebookid = new List<string>();
		for (int i = 0; i < fb_friend_num; i++)
		{
			ff[i]._fbid = aa["data"][i]["id"];
			ff[i]._name = aa["data"][i]["name"];
			StartCoroutine(GetFacebookImg(aa["data"][i]["picture"]["data"]["url"], i));
			facebookid.Add(ff[i]._fbid);
		}
	}

	private IEnumerator GetFacebookImg(string _url, int _index)
	{
		WWW www = new WWW(_url);
		yield return www;
		if (www.texture.height > 8)
		{
			ff[_index]._picture = www.texture;
		}
	}

	public void ChangeNickName()
	{
	}

	public void GetHSPFriends()
	{
		hf = new hspfriend[100];
		reload = true;
	}

	public void RemoveHSPFriends(long _memberNo)
	{
		List<long> list = new List<long>(1);
		list.Add(_memberNo);
	}

	public void SetMemberNos(List<long> _memberNos)
	{
		rf = new recommend[100];
		recommend_num = 0;
		int num = 0;
		foreach (long _memberNo in _memberNos)
		{
			if (currenttab == 0)
			{
				hf[num]._memberNo = _memberNo;
				num = (hsp_friend_num = num + 1);
			}
			else
			{
				rf[num]._memberNo = _memberNo;
				num = (recommend_num = num + 1);
			}
		}
	}

	public void JudgeFriend(bool _isfriend)
	{
		rf[detail_count]._isfriend = _isfriend;
		detail_count++;
	}

	public void HSPFriendsImg(List<string> _urls)
	{
		int num = 0;
		foreach (string _url in _urls)
		{
			StartCoroutine(GetProfileImg(_url, num));
			num++;
		}
	}

	private IEnumerator GetProfileImg(string _url, int _index)
	{
		WWW www = new WWW(_url);
		yield return www;
		if (www.texture.height > 8)
		{
			if (currenttab == 0)
			{
				hf[_index]._picture = www.texture;
			}
			else
			{
				rf[_index]._picture = www.texture;
			}
		}
	}

	private void Delay()
	{
		confirm = 0;
	}

	private void Update()
	{
		posX_l = Mathf.MoveTowards(posX_l, 100f, Time.deltaTime * 1000f);
		posX_r = Mathf.MoveTowards(posX_r, 284f, Time.deltaTime * 1000f);
		if (keyboardactive <= 0 || !mykeyboard.done)
		{
			return;
		}
		string text = mykeyboard.text;
		int num = Encoding.Default.GetBytes(text).Length;
		if (num < 4)
		{
			if (keyboardactive == 2)
			{
				recommend_num = 0;
			}
			confirm = 1;
			Invoke("Delay", 1f);
		}
		else if (text != string.Empty)
		{
			switch (keyboardactive)
			{
			case 1:
				tempnick = text;
				break;
			}
		}
		else if (keyboardactive == 2)
		{
			recommend_num = 0;
		}
		keyboardactive = 0;
	}

	private void OnGUI()
	{
		GUI.depth = -8;
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		if (GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
		{
			Application.LoadLevel("Extreme");
		}
		if (Input.anyKeyDown)
		{
			if (Crypto.Rect2(posX_r - 74f, posY - 26f, 258f, 218f).Contains(Event.current.mousePosition))
			{
				startMousePosY = Input.mousePosition.y;
				prev_scrollPosition = scrollPosition;
			}
		}
		else if (Input.anyKey && Crypto.Rect2(posX_r - 74f, posY - 26f, 258f, 218f).Contains(Event.current.mousePosition))
		{
			curMousePosY = Input.mousePosition.y;
			scrollPosition = prev_scrollPosition + Vector2.up * (curMousePosY - startMousePosY) * (640f / (float)Screen.height);
		}
		GUI.Box(Crypto.Rect2(posX_r - 74f, posY - 42f, 262f, 240f), string.Empty, "blank_box");
		GUI.Box(Crypto.Rect2(posX_r - 69f, posY - 74f, 96f, 32f), string.Empty, "blank_tab");
		if (GUI.Button(Crypto.Rect2(posX_r - 69f, posY - 74f, 96f, 32f), string.Empty, bt_empty))
		{
			currenttab = 0;
			if (!reload)
			{
				GetHSPFriends();
			}
		}
		if (currenttab == 0)
		{
			scrollPosition = GUI.BeginScrollView(Crypto.Rect2(posX_r - 74f, posY - 26f, 258f, 218f), scrollPosition, Crypto.Rect2(0f, 0f, 242f, hsp_friend_num * 44));
			for (short num = 0; num < hsp_friend_num; num++)
			{
				if (!(scrollPosition.y - (float)(num * 88) > 80f) && !(scrollPosition.y - (float)(num * 88) < -438f))
				{
					GUI.Box(Crypto.Rect2(12f, num * 44, 234f, 40f), string.Empty, "blank_white");
					if (hf[num]._picture != null)
					{
						GUI.DrawTexture(Crypto.Rect2(16f, num * 44 + 2, 36f, 36f), hf[num]._picture);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(16f, num * 44 + 2, 36f, 36f), errorimg);
					}
					if (hf[num]._name != null)
					{
						GUI.Label(Crypto.Rect2(60f, num * 44, 128f, 40f), hf[num]._name, "txt12_0");
					}
				}
			}
			GUI.EndScrollView();
			GUI.DrawTexture(Crypto.Rect2(posX_r - 69f, posY - 74f, 96f, 32f), toggle_active);
		}
		else if (currenttab == 1)
		{
			scrollPosition = GUI.BeginScrollView(Crypto.Rect2(posX_r - 74f, posY - 26f, 258f, 218f), scrollPosition, Crypto.Rect2(0f, 0f, 242f, fb_friend_num * 44));
			for (short num2 = 0; num2 < fb_friend_num; num2++)
			{
				if (!(scrollPosition.y - (float)(num2 * 88) > 80f) && !(scrollPosition.y - (float)(num2 * 88) < -438f))
				{
					if (ff[num2]._isfriend)
					{
						GUI.Box(Crypto.Rect2(12f, num2 * 44, 234f, 40f), string.Empty, "blank_red");
					}
					else
					{
						GUI.Box(Crypto.Rect2(12f, num2 * 44, 234f, 40f), string.Empty, "blank_white");
					}
					if (ff[num2]._picture != null)
					{
						GUI.DrawTexture(Crypto.Rect2(16f, num2 * 44 + 2, 36f, 36f), ff[num2]._picture);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(16f, num2 * 44 + 2, 36f, 36f), errorimg);
					}
					if (ff[num2]._name != null)
					{
						GUI.Label(Crypto.Rect2(60f, num2 * 44, 128f, 40f), ff[num2]._name, "txt12_0");
					}
					if (!facebook_load)
					{
						GUI.Box(Crypto.Rect2(178f, num2 * 44 + 4, 64f, 32f), "...", "blank_red");
					}
					else if (!ff[num2]._isfriend)
					{
						if (ff[num2]._hspNo != 0L)
						{
							if (GUI.Button(Crypto.Rect2(204f, num2 * 44 - 4, 48f, 48f), string.Empty, bt_plus))
							{
								List<long> list = new List<long>(1);
								list.Add(ff[num2]._hspNo);
								reload = false;
								ff[num2]._isfriend = true;
							}
						}
						else if (GUI.Button(Crypto.Rect2(178f, num2 * 44 + 4, 64f, 32f), Language.intxt[language, 410], bt_yesno))
						{
							List<string> list2 = new List<string>();
							list2.Add(ff[num2]._fbid);
						}
					}
				}
			}
			GUI.EndScrollView();
		}
		else if (currenttab > 1)
		{
			scrollPosition = GUI.BeginScrollView(Crypto.Rect2(posX_r - 74f, posY - 26f, 258f, 218f), scrollPosition, Crypto.Rect2(0f, 0f, 242f, recommend_num * 44));
			for (short num3 = 0; num3 < recommend_num; num3++)
			{
				if (!(scrollPosition.y - (float)(num3 * 88) > 80f) && !(scrollPosition.y - (float)(num3 * 88) < -438f))
				{
					if (rf[num3]._isfriend)
					{
						GUI.Box(Crypto.Rect2(12f, num3 * 44, 234f, 40f), string.Empty, "blank_red");
					}
					else
					{
						GUI.Box(Crypto.Rect2(12f, num3 * 44, 234f, 40f), string.Empty, "blank_white");
					}
					if (rf[num3]._picture != null)
					{
						GUI.DrawTexture(Crypto.Rect2(16f, num3 * 44 + 2, 36f, 36f), rf[num3]._picture);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(16f, num3 * 44 + 2, 36f, 36f), errorimg);
					}
					if (rf[num3]._name != null)
					{
						GUI.Label(Crypto.Rect2(60f, num3 * 44, 128f, 40f), rf[num3]._name, "txt12_0");
					}
					if (!rf[num3]._isfriend && rf[num3]._memberNo != 0L && GUI.Button(Crypto.Rect2(204f, num3 * 44 - 4, 48f, 48f), string.Empty, bt_plus))
					{
						rf[num3]._isfriend = true;
						List<long> list3 = new List<long>(1);
						list3.Add(rf[num3]._memberNo);
						reload = false;
					}
				}
			}
			GUI.EndScrollView();
		}
		if (mode > 0)
		{
			GUI.Box(Crypto.Rect2(posX_r + 33f, posY - 74f, 96f, 32f), string.Empty, "blank_tab");
			if (currenttab > 0)
			{
				GUI.DrawTexture(Crypto.Rect2(posX_r + 33f, posY - 74f, 96f, 32f), toggle_active);
			}
			if (GUI.Button(Crypto.Rect2(posX_r + 33f, posY - 74f, 96f, 32f), string.Empty, bt_empty))
			{
				currenttab = mode;
			}
			GUI.DrawTexture(Crypto.Rect2(posX_r + 37f, posY - 68f, 20f, 20f), icon_ranktab[mode]);
			GUI.Label(Crypto.Rect2(posX_r + 60f, posY - 74f, 64f, 32f), Language.intxt[language, 405 + mode], "txt12_w");
		}
		GUI.DrawTexture(Crypto.Rect2(posX_r - 64f, posY - 68f, 20f, 20f), icon_ranktab[0]);
		GUI.Box(Crypto.Rect2(posX_l - 63f, posY - 74f, 140f, 104f), string.Empty, "blank_box");
		if (myimage != null)
		{
			GUI.DrawTexture(Crypto.Rect2(posX_l - 25f, posY - 66f, 64f, 64f), myimage);
		}
		else
		{
			GUI.DrawTexture(Crypto.Rect2(posX_l - 25f, posY - 66f, 64f, 64f), errorimg);
		}
		if (GUI.Button(Crypto.Rect2(posX_l - 25f, posY - 64f, 64f, 64f), string.Empty, bt_empty))
		{
		}
		if (GUI.Button(Crypto.Rect2(posX_l - 57f, posY + 4f, 128f, 20f), string.Empty, bt_empty))
		{
			mykeyboard = TouchScreenKeyboard.Open(Language.intxt[language, 424]);
			keyboardactive = 1;
		}
		GUI.Label(Crypto.Rect2(posX_r - 69f, posY - 74f, 114f, 32f), Language.intxt[language, 405], "txt12_w");
		for (short num4 = version; num4 < 4; num4++)
		{
			GUI.Box(Crypto.Rect2(posX_l - 73f, posY + 44f + (float)(num4 * 38), 160f, 32f), "     " + Language.intxt[language, 406 + num4], "blank_red");
			GUI.DrawTexture(Crypto.Rect2(posX_l - 70f, posY + 50f + (float)(num4 * 38), 20f, 20f), icon_ranktab[num4 + 1]);
			if (GUI.Button(Crypto.Rect2(posX_l - 73f, posY + 44f + (float)(num4 * 38), 160f, 32f), string.Empty, bt_empty))
			{
				mode = (short)(num4 + 1);
				currenttab = mode;
				switch (currenttab)
				{
				case 4:
					mykeyboard = TouchScreenKeyboard.Open(Language.intxt[language, 425]);
					keyboardactive = 2;
					break;
				}
			}
		}
		if (confirm > 0)
		{
			switch (confirm)
			{
			case 1:
				GUI.enabled = true;
				GUI.Box(Crypto.Rect2(112f, 140f, 256f, 48f), Language.intxt[language, 426], "blank_white");
				break;
			}
		}
	}
}
