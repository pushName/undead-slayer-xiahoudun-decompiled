using UnityEngine;

public class Cam_Move_story : MonoBehaviour
{
	public Texture2D cha_cutin;

	public Texture2D txt_box;

	private Transform mytransform;

	private bool trans;

	private float updelay;

	private float color_alpha;

	private bool showcut;

	private bool story_final;

	public GUISkin basic;

	private string chaname;

	private string speech;

	private int language;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		if (Application.loadedLevelName == "Story_5")
		{
			base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.5f, 1.5f, -1f, 1f, 0.01f, 10f);
			story_final = true;
			language = PlayerPrefs.GetInt("language");
			int num = Crypto.Load_int_key("n06");
			if (num != -1)
			{
				cha_cutin = Resources.Load("cc_yun0") as Texture2D;
				chaname = Language.intxt[language, 372] + " , 36 " + Language.intxt[language, 361];
				speech = Language.intxt[language, 374];
			}
			else
			{
				chaname = Language.intxt[language, 72] + " , 34 " + Language.intxt[language, 361];
				speech = Language.intxt[language, 50];
			}
		}
		else
		{
			mytransform.GetChild(0).GetComponent<Camera>().projectionMatrix = Matrix4x4.Perspective(60f, 1.5f, 0.01f, 5f);
		}
	}

	public void ShowOff()
	{
		color_alpha = 0f;
	}

	private void Update()
	{
		updelay += Time.deltaTime * 0.04f;
		mytransform.position += Vector3.up * Time.deltaTime * updelay;
		if (mytransform.position.y > 0.5f && !trans)
		{
			GameObject.Find("scene_trans").GetComponent<Story_trans>().ScreenOn(2f);
			trans = true;
		}
		if (!story_final)
		{
			return;
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
		if (story_final)
		{
			GUI.depth = -5;
			GUI.color = new Color(1f, 1f, 1f, color_alpha);
			GUI.DrawTexture(Crypto.Rect2(-64f, 0f, 320f, 320f), cha_cutin);
			if (mytransform.position.y > 0.3f)
			{
				showcut = false;
			}
			else if (mytransform.position.y > -0.9f)
			{
				showcut = true;
				GUI.Label(Crypto.Rect2(0f, 280f, 480f, 20f), chaname, "txt12_w");
				GUI.DrawTexture(Crypto.Rect2(150f, 40f, 256f, 64f), txt_box);
				GUI.Label(Crypto.Rect2(158f, 60f, 240f, 20f), speech, "txt12_0");
			}
		}
	}
}
