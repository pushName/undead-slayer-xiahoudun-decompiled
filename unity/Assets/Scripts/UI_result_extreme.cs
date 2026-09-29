using UnityEngine;

public class UI_result_extreme : MonoBehaviour
{
	public int isclear;

	public int max_stage_index;

	private int coin;

	private int jade;

	private int chalv;

	private int language;

	private bool gonext = true;

	public GUISkin basicSkin;

	public Texture2D bg_asset;

	public Texture2D[] img_bts = new Texture2D[5];

	public GUIStyle bt_empty;

	private GameObject cashshop;

	private int changelevel;

	private short icon_pluspos;

	private bool b_openshop;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
	}

	private void Start()
	{
		language = PlayerPrefs.GetInt("language");
		base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
		AudioListener.volume = PlayerPrefs.GetFloat("vol_master");
		chalv = Crypto.Load_int_key("n47");
		if (chalv < 19)
		{
			changelevel = Crypto.Load_int_key("changelevel");
			if (changelevel == 1 && chalv % 2 == 1)
			{
				changelevel = 1;
			}
			else
			{
				changelevel = 0;
			}
		}
		Crypto.Save_int_key("changelevel", 0);
	}

	private void GoNext()
	{
		gonext = true;
	}

	private void Update()
	{
	}

	public void CashshopOpen(int _kind)
	{
		Crypto.Save_int_key("cashshopkind", _kind);
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
	}

	public void OpenShop()
	{
		b_openshop = true;
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		if (b_openshop)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
			GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
			GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
			if (GUI.Button(Crypto.Rect2(144f, 2f, 100f, 24f), string.Empty, bt_empty))
			{
				CashshopOpen(2);
			}
			if (GUI.Button(Crypto.Rect2(272f, 2f, 88f, 24f), string.Empty, bt_empty))
			{
				CashshopOpen(1);
			}
		}
		if (!gonext)
		{
			return;
		}
		GUI.DrawTexture(Crypto.Rect2(400f, 250f, 64f, 64f), img_bts[0]);
		if (GUI.Button(Crypto.Rect2(400f, 250f, 64f, 64f), Language.intxt[language, 202], bt_empty))
		{
			Application.LoadLevel("Extreme");
		}
		if (isclear == 2 && max_stage_index >= 3)
		{
			icon_pluspos = 76;
			GUI.DrawTexture(Crypto.Rect2(324f, 250f, 64f, 64f), img_bts[3]);
			if (GUI.Button(Crypto.Rect2(324f, 250f, 64f, 64f), Language.intxt[language, 398], bt_empty))
			{
				Application.LoadLevel("Forge");
			}
		}
		if (changelevel == 1)
		{
			GUI.DrawTexture(Crypto.Rect2(324 - icon_pluspos, 250f, 64f, 64f), img_bts[2]);
			if (GUI.Button(Crypto.Rect2(324 - icon_pluspos, 250f, 64f, 64f), Language.intxt[language, 395], bt_empty))
			{
				Application.LoadLevel("Skill");
			}
		}
	}
}
