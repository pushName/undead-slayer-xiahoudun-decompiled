using UnityEngine;

public class RankingUI : MonoBehaviour
{
	private short MAXRANKING = 50;

	public Texture2D bg_title;

	public Texture2D bg_title_me;

	public Texture2D icon_key;

	public Texture2D toggle_active;

	public Texture2D errorimg;

	public Texture2D[] icon_ranktab = new Texture2D[3];

	public Texture2D[] icon_toprank = new Texture2D[3];

	private Texture2D myimage;

	public GUIStyle bt_empty;

	public GUIStyle bt_start;

	public GUIStyle bt_back;

	public GUISkin basicSkin;

	private float curMousePosY;

	private float startMousePosY;

	private Vector2 scrollPosition;

	private Vector2 prev_scrollPosition;

	private int language;

	private float posX = 112f;

	private float posY = 114f;

	private GameObject ui;

	private short scopemode;

	private RankingManager script_ranking;

	private void Awake()
	{
		language = PlayerPrefs.GetInt("language");
		script_ranking = GameObject.FindWithTag("ranking").GetComponent<RankingManager>();
	}

	private void Start()
	{
		script_ranking.S_LoadRanking(1, 0, 30);
	}

	public void RankingOn()
	{
	}

	private void Update()
	{
		posX = Mathf.MoveTowards(posX, 183f, Time.deltaTime * 1000f);
	}

	private void OnGUI()
	{
		GUI.depth = -8;
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		if (Input.anyKeyDown)
		{
			if (Crypto.Rect2(posX, posY + 40f, 256f, 142f).Contains(Event.current.mousePosition))
			{
				startMousePosY = Input.mousePosition.y;
				prev_scrollPosition = scrollPosition;
			}
		}
		else if (Input.anyKey && Crypto.Rect2(posX, posY + 40f, 256f, 142f).Contains(Event.current.mousePosition))
		{
			curMousePosY = Input.mousePosition.y;
			scrollPosition = prev_scrollPosition + Vector2.up * (curMousePosY - startMousePosY) * (640f / (float)Screen.height);
		}
		GUI.Box(Crypto.Rect2(posX - 74f, posY - 42f, 262f, 240f), string.Empty, "blank_box");
		if (script_ranking.loadfinish)
		{
			int maxfriend = script_ranking.maxfriend;
			scrollPosition = GUI.BeginScrollView(Crypto.Rect2(posX - 74f, posY + 18f, 258f, 174f), scrollPosition, Crypto.Rect2(0f, 0f, 242f, maxfriend * 44));
			for (short num = 0; num < maxfriend; num++)
			{
				if (!(scrollPosition.y - (float)(num * 88) > 80f) && !(scrollPosition.y - (float)(num * 88) < -350f))
				{
					if (script_ranking.userimg[num] != null)
					{
						GUI.DrawTexture(Crypto.Rect2(49f, num * 44 + 2, 36f, 36f), script_ranking.userimg[num]);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(49f, num * 44 + 2, 36f, 36f), errorimg);
					}
					GUI.DrawTexture(Crypto.Rect2(12f, num * 44, 234f, 40f), bg_title);
					if (script_ranking.userpoint[num] != 0)
					{
						GUI.Label(Crypto.Rect2(150f, num * 44, 128f, 40f), string.Empty + script_ranking.userpoint[num], "txt12_0");
					}
					if (script_ranking.username[num] != null)
					{
						GUI.Label(Crypto.Rect2(92f, num * 44, 90f, 40f), script_ranking.username[num], "txt12_0");
					}
					if (num < 3)
					{
						GUI.DrawTexture(Crypto.Rect2(12f, num * 44 + 2, 36f, 36f), icon_toprank[num]);
					}
					GUI.Label(Crypto.Rect2(-3f, num * 44, 64f, 40f), string.Empty + (num + 1), "imagenum");
				}
			}
			GUI.EndScrollView();
			if (myimage != null)
			{
				GUI.DrawTexture(Crypto.Rect2(posX - 25f, posY - 26f, 36f, 36f), myimage);
			}
			else
			{
				GUI.DrawTexture(Crypto.Rect2(posX - 25f, posY - 24f, 36f, 36f), errorimg);
			}
			GUI.DrawTexture(Crypto.Rect2(posX - 62f, posY - 26f, 234f, 40f), bg_title_me);
			if (script_ranking.mygrade < 1000)
			{
				GUI.Label(Crypto.Rect2(posX - 76f, posY - 26f, 64f, 40f), string.Empty + script_ranking.mygrade, "imagenum");
			}
			else if (script_ranking.mygrade < 1000000)
			{
				GUI.Label(Crypto.Rect2(posX - 76f, posY - 26f, 64f, 40f), string.Empty + script_ranking.mygrade, "txt12_w");
			}
			else
			{
				GUI.Label(Crypto.Rect2(posX - 76f, posY - 26f, 64f, 40f), "...", "txt12_w");
			}
			GUI.Label(Crypto.Rect2(posX + 76f, posY - 26f, 128f, 40f), string.Empty + script_ranking.mypoint, "txt12_w");
		}
		else
		{
			GUI.Label(Crypto.Rect2(posX - 82f, posY - 36f, 284f, 210f), Language.intxt[language, 303], "txt12_w");
		}
		GUI.Box(Crypto.Rect2(posX - 69f, posY - 74f, 80f, 32f), string.Empty, "blank_tab");
		GUI.Box(Crypto.Rect2(posX + 17f, posY - 74f, 80f, 32f), string.Empty, "blank_tab");
		GUI.Box(Crypto.Rect2(posX + 103f, posY - 74f, 80f, 32f), string.Empty, "blank_tab");
		if (scopemode == 0)
		{
			GUI.DrawTexture(Crypto.Rect2(posX - 69f, posY - 74f, 80f, 32f), toggle_active);
			GUI.Label(Crypto.Rect2(posX - 84f, posY - 44f, 284f, 18f), Language.intxt[language, 384] + "    " + script_ranking.resettime, "txt12_w");
		}
		else
		{
			GUI.DrawTexture(Crypto.Rect2(posX + 17f, posY - 74f, 80f, 32f), toggle_active);
		}
		GUI.DrawTexture(Crypto.Rect2(posX - 64f, posY - 68f, 20f, 20f), icon_ranktab[0]);
		GUI.DrawTexture(Crypto.Rect2(posX + 21f, posY - 68f, 20f, 20f), icon_ranktab[1]);
		GUI.DrawTexture(Crypto.Rect2(posX + 107f, posY - 68f, 20f, 20f), icon_ranktab[2]);
		GUI.Label(Crypto.Rect2(posX - 48f, posY - 74f, 56f, 32f), Language.intxt[language, 194], "txt12_w");
		GUI.Label(Crypto.Rect2(posX + 38f, posY - 74f, 56f, 32f), Language.intxt[language, 193], "txt12_w");
		GUI.Label(Crypto.Rect2(posX + 124f, posY - 74f, 56f, 32f), Language.intxt[language, 410], "txt12_w");
		if (GUI.Button(Crypto.Rect2(posX - 69f, posY - 74f, 80f, 32f), string.Empty, bt_empty))
		{
			scopemode = 0;
			script_ranking.S_LoadRanking(1, scopemode, MAXRANKING);
		}
		if (GUI.Button(Crypto.Rect2(posX + 17f, posY - 74f, 80f, 32f), string.Empty, bt_empty))
		{
			scopemode = 1;
			script_ranking.S_LoadRanking(1, scopemode, MAXRANKING);
		}
		if (GUI.Button(Crypto.Rect2(posX + 103f, posY - 74f, 80f, 32f), string.Empty, bt_empty))
		{
			Application.LoadLevel("Friends");
		}
		GUI.enabled = true;
	}
}
