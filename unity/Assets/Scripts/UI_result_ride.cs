using UnityEngine;

public class UI_result_ride : MonoBehaviour
{
	private float show_delay;

	private float posX = 1000f;

	private bool show_ui;

	private bool getpoint;

	private bool movefinish;

	private int coin;

	private int jade;

	private int getcoin;

	private int getcoin_og;

	private int count_coin;

	private int count_monster;

	private int count_behit;

	private int count_loss;

	private float getcoin_f;

	private int language;

	private bool gonext;

	public GUISkin basicSkin;

	public Texture2D bg_asset;

	public Texture2D bg_list;

	public Texture2D img_map;

	public GUIStyle bt_empty;

	private int[] pet_passiveskill = new int[2];

	private Cha_Control_ride_cha script_cha;

	private void Awake()
	{
		Time.timeScale = 1f;
	}

	private void Start()
	{
		pet_passiveskill = PlayerPrefsX.GetIntArray("n27");
		language = PlayerPrefs.GetInt("language");
		base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
		AudioListener.volume = PlayerPrefs.GetFloat("vol_master");
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
		script_cha = GameObject.FindWithTag("Player").GetComponent<Cha_Control_ride_cha>();
		getcoin = script_cha.amount_soulstone;
		getcoin_og = getcoin;
		count_monster = script_cha.count_monster;
		count_coin = script_cha.count_coin;
		count_behit = script_cha.count_behit;
		count_loss = count_monster * 3 + count_coin - getcoin;
		count_loss = Mathf.Max(0, count_loss);
		getcoin += (int)((float)(getcoin * pet_passiveskill[0]) * 0.1f);
	}

	private void Update()
	{
		if (getpoint)
		{
			getcoin_f += Time.deltaTime * 20f;
			if (getcoin_f >= (float)getcoin)
			{
				getcoin_f = getcoin;
				getpoint = false;
				coin += getcoin;
				Crypto.Property_change(getcoin, _jade: false);
				gonext = true;
			}
		}
		else if (!show_ui)
		{
			show_delay += Time.deltaTime;
			if (show_delay > 1f)
			{
				getpoint = true;
				show_ui = true;
			}
		}
		if (!movefinish)
		{
			posX = Mathf.MoveTowards(posX, 270f, Time.deltaTime * 1200f);
			if (posX == 270f)
			{
				movefinish = true;
			}
		}
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
		GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
		GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
		if (show_ui)
		{
			GUI.DrawTexture(Crypto.Rect2(posX, 64f, 180f, 135f), bg_list);
			GUI.Label(Crypto.Rect2(300f, 96f, 124f, 14f), string.Empty + count_coin, "txt12_0");
			GUI.Label(Crypto.Rect2(348f, 96f, 124f, 14f), string.Empty + count_coin, "txt12_0");
			GUI.Label(Crypto.Rect2(300f, 121f, 124f, 14f), string.Empty + count_monster, "txt12_0");
			GUI.Label(Crypto.Rect2(348f, 121f, 124f, 14f), string.Empty + count_monster * 3, "txt12_0");
			GUI.Label(Crypto.Rect2(300f, 146f, 124f, 14f), string.Empty + count_behit, "txt12_0");
			GUI.Label(Crypto.Rect2(348f, 146f, 124f, 14f), string.Empty + count_loss, "txt12_0");
			if (getcoin == getcoin_og)
			{
				GUI.Label(Crypto.Rect2(348f, 171f, 124f, 14f), string.Empty + getcoin, "txt12_0");
			}
			else
			{
				GUI.Label(Crypto.Rect2(320f, 171f, 124f, 14f), string.Empty + getcoin_og, "txt12_0");
				GUI.Label(Crypto.Rect2(348f, 171f, 124f, 14f), "(+" + (getcoin - getcoin_og) + ")", "txt12_b");
			}
			GUI.Label(Crypto.Rect2(posX, 70f, 180f, 14f), Language.intxt[language, 305], "txt14_w");
		}
		if (gonext)
		{
			GUI.DrawTexture(Crypto.Rect2(400f, 250f, 64f, 64f), img_map);
			if (GUI.Button(Crypto.Rect2(400f, 250f, 64f, 64f), Language.intxt[language, 202], bt_empty))
			{
				Application.LoadLevel("Map");
				Object.Destroy(GameObject.FindWithTag("Player"));
				Object.Destroy(GameObject.FindWithTag("general"));
			}
		}
	}
}
