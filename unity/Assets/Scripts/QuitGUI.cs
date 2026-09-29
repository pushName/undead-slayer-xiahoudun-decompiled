using UnityEngine;

public class QuitGUI : MonoBehaviour
{
	public Texture2D pop_blank;

	public Texture2D bg_black;

	public GUIStyle bt_yesno;

	public GUISkin basicSkin;

	private int language = 1;

	private float timescale = 1f;

	private GameObject ui;

	private GameObject ui2;

	private void Start()
	{
		ui = GameObject.Find("scenario");
		if (ui == null)
		{
			ui = GameObject.FindWithTag("ui");
		}
		if (ui != null)
		{
			ui.active = false;
		}
		ui2 = GameObject.FindWithTag("ranking");
		if (ui2 != null)
		{
			ui2.active = false;
		}
		timescale = Time.timeScale;
		Time.timeScale = 0f;
		language = PlayerPrefs.GetInt("language");
	}

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.depth = -100;
		GUI.matrix = Matrix4x4.Scale(new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), bg_black);
		GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
		GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 260], "txt12_0");
		if (GUI.Button(Crypto.Rect2(170f, 138f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
		{
			Time.timeScale = 0f;
			Application.Quit();
		}
		else if (GUI.Button(Crypto.Rect2(240f, 138f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
		{
			Time.timeScale = timescale;
			if (ui != null)
			{
				ui.active = true;
			}
			if (ui2 != null)
			{
				ui2.active = true;
			}
			Object.Destroy(base.gameObject);
		}
	}
}
