using UnityEngine;

public class Cam_Move_story2 : MonoBehaviour
{
	public Texture2D cha_cutin;

	public Texture2D txt_box;

	public GUIStyle bt_skip;

	private bool trans;

	private float updelay;

	private float color_alpha;

	private bool showcut;

	public GUISkin basic;

	private int language;

	private string chaname;

	private string speech;

	private void Start()
	{
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Perspective(30f, 1.5f, 0.01f, 10f);
		language = PlayerPrefs.GetInt("language");
		chaname = Language.intxt[language, 71] + " , 26 " + Language.intxt[language, 361];
		int num = Crypto.Load_int_key("n06");
		if (num != -1)
		{
			Object.Destroy(GameObject.Find("back_head"));
			cha_cutin = Resources.Load("cc_cha3") as Texture2D;
			speech = Language.intxt[language, 373];
		}
		else
		{
			speech = Language.intxt[language, 49];
		}
	}

	public void ShowOff()
	{
		color_alpha = 0f;
	}

	private void Update()
	{
		updelay += Time.deltaTime * 0.1f;
		base.transform.position -= new Vector3(0f, 0f, 1f) * Time.deltaTime * updelay;
		if (base.transform.position.z < -4f && !trans)
		{
			GameObject.Find("scene_trans").GetComponent<Story_trans>().ScreenOn(2f);
			trans = true;
			updelay = 0f;
		}
		if (showcut)
		{
			if (color_alpha < 1f)
			{
				color_alpha += Time.deltaTime * 2f;
			}
			else
			{
				color_alpha = 1f;
			}
		}
		else if (color_alpha > 0f)
		{
			color_alpha -= Time.deltaTime * 2f;
		}
		else
		{
			color_alpha = 0f;
		}
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basic;
		GUI.depth = -5;
		GUI.color = new Color(1f, 1f, 1f, color_alpha);
		GUI.DrawTexture(Crypto.Rect2(216f, 0f, 320f, 320f), cha_cutin);
		if (base.transform.position.z < -3.2f)
		{
			showcut = false;
		}
		else if (base.transform.position.z < -1.2f)
		{
			showcut = true;
			GUI.Label(Crypto.Rect2(0f, 280f, 480f, 20f), chaname, "txt12_w");
			GUI.DrawTexture(Crypto.Rect2(80f, 40f, 256f, 64f), txt_box);
			GUI.Label(Crypto.Rect2(88f, 60f, 240f, 20f), speech, "txt12_0");
		}
		if (GUI.Button(Crypto.Rect2(400f, 290f, 64f, 32f), "NEXT", bt_skip))
		{
			GameObject.Find("scene_trans").GetComponent<Story_trans>().ScreenOn(1f);
		}
	}
}
