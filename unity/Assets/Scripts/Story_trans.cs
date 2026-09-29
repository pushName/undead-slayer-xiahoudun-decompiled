using UnityEngine;

public class Story_trans : MonoBehaviour
{
	public Texture2D black2;

	private bool b_delay = true;

	private bool cinema_window = true;

	private float f_delay = 2f;

	private float color_alpha = 1f;

	private int story_index;

	private GameObject cha1;

	private GameObject horse;

	private Cha_Control script_cha;

	public GUISkin basicSkin;

	public GUIStyle bt_skip;

	private string[] storytxt = new string[8];

	private int language;

	private int max_stage_index = -1;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.transform.gameObject);
	}

	private void Start()
	{
		language = PlayerPrefs.GetInt("language");
		cha1 = GameObject.FindWithTag("Player");
		horse = GameObject.Find("pet_horse");
		script_cha = cha1.GetComponent<Cha_Control>();
		max_stage_index = Crypto.Load_int_key("n06");
		if (max_stage_index == -1)
		{
			storytxt[0] = Language.intxt[language, 37];
			storytxt[1] = " ";
			storytxt[2] = Language.intxt[language, 38];
			storytxt[3] = Language.intxt[language, 39];
			storytxt[4] = Language.intxt[language, 40];
			storytxt[5] = Language.intxt[language, 41];
			storytxt[6] = Language.intxt[language, 42];
			storytxt[7] = Language.intxt[language, 43];
		}
		else
		{
			storytxt[0] = Language.intxt[language, 362];
			storytxt[1] = " ";
			storytxt[2] = Language.intxt[language, 363];
			storytxt[3] = Language.intxt[language, 364];
			storytxt[4] = Language.intxt[language, 365];
			storytxt[5] = Language.intxt[language, 366];
			storytxt[6] = Language.intxt[language, 367];
			storytxt[7] = Language.intxt[language, 368];
		}
	}

	public void ScreenOn(float t)
	{
		b_delay = true;
		f_delay = t;
	}

	public void Story_Contents()
	{
		switch (story_index)
		{
		case 0:
			cinema_window = true;
			break;
		case 1:
			cinema_window = false;
			base.GetComponent<AudioSource>().Play();
			break;
		case 2:
		{
			cinema_window = true;
			cha1.transform.position = Vector3.zero;
			GameObject gameObject = GameObject.Find("npc");
			gameObject.transform.position = Vector3.right * -0.1f;
			Camera.main.GetComponent<Cam_Move>().LookTarget(gameObject.transform, 25, 10f);
			script_cha.WakToward(horse.transform.position, _run: false, _ride: true);
			break;
		}
		case 3:
			Application.LoadLevel("Story_2");
			break;
		case 4:
			GameObject.Find("cha1_riding").GetComponent<Cha_Control_ride_cha_story1>().CrynStop();
			GameObject.Find("pet_horse_riding").GetComponent<Cha_Control_ride_horse_story>().Scene_third();
			Application.LoadLevel("Story_3");
			break;
		case 5:
		{
			Object.Destroy(GameObject.Find("pet_horse_riding"));
			GameObject gameObject2 = GameObject.Find("cha1_riding");
			Object.Destroy(gameObject2.GetComponent<Cha_Control_ride_cha_story1>());
			gameObject2.AddComponent<Cha_Control_ride_cha_story2>();
			Application.LoadLevel("Story_4");
			break;
		}
		case 6:
			GameObject.Find("Main_Camera").GetComponent<Cam_Move_story2>().ShowOff();
			Object.Destroy(GameObject.Find("cha1_riding"));
			Application.LoadLevel("Story_5");
			break;
		case 7:
		{
			GameObject.Find("Main_Camera").GetComponent<Cam_Move_story>().ShowOff();
			if (max_stage_index == -1)
			{
				int[] intArray = new int[2] { 1, 1 };
				PlayerPrefsX.SetIntArray("petgrade", intArray);
			}
			int num = Crypto.Load_int_key("gamemode");
			PlayerPrefs.SetInt("IsContinue", 6);
			if (num == 0)
			{
				Application.LoadLevel("Map");
			}
			else
			{
				Application.LoadLevel("Extreme");
			}
			cinema_window = false;
			break;
		}
		}
	}

	private void Update()
	{
		if (b_delay)
		{
			color_alpha += Time.deltaTime * 2f;
			color_alpha = Mathf.Clamp(color_alpha, 0f, 1f);
			f_delay -= Time.deltaTime;
			if (f_delay <= 0f)
			{
				f_delay = 0f;
				b_delay = false;
				Story_Contents();
				story_index++;
			}
		}
		else
		{
			color_alpha -= Time.deltaTime * 2f;
			color_alpha = Mathf.Clamp(color_alpha, 0f, 1f);
		}
	}

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.depth = -1;
		if (cinema_window)
		{
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 64f), black2);
			GUI.DrawTexture(Crypto.Rect2(0f, 256f, 480f, 74f), black2);
		}
		if (color_alpha == 0f)
		{
			if (story_index >= 8)
			{
				Object.Destroy(base.gameObject);
			}
			else if (story_index > 3 && GUI.Button(Crypto.Rect2(400f, 290f, 64f, 32f), "NEXT", bt_skip))
			{
				ScreenOn(1f);
			}
		}
		else if (b_delay)
		{
			GUI.depth = -8;
			GUI.color = new Color(0f, 0f, 0f, color_alpha);
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 490f, 330f), black2);
			GUI.color = Color.white;
			GUI.Label(Crypto.Rect2(0f, 0f, 480f, 320f), string.Empty + storytxt[story_index], "txt12_w");
			if (story_index >= 7)
			{
				base.GetComponent<AudioSource>().volume -= Time.deltaTime;
			}
		}
		else if (!b_delay)
		{
			GUI.color = new Color(0f, 0f, 0f, color_alpha);
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 490f, 330f), black2);
		}
	}
}
