using UnityEngine;

public struct scenario
{
	public string _chaimg;

	public bool _mirror;

	public short _pos;

	public short _showef;

	public short _txtidx;

	public short _extraimg;
}
public class Scenario : MonoBehaviour
{
	private DB_Scenario script_db;

	private Language_Scenario script_scenetxt;

	public Texture2D black;

	public Texture2D txtbox1;

	public GUISkin basicSkin;

	private Texture2D[] portrait = new Texture2D[2];

	private Texture2D temp;

	private Texture2D extraimg;

	private UI_map script_uimap;

	private int cur_difficulty;

	private float[] startposX = new float[2] { -320f, 480f };

	private float[] targetposX = new float[2] { 20f, 180f };

	private float[] remainposX = new float[2] { -80f, 240f };

	private float[] currentposX = new float[2] { -320f, 480f };

	private float[] movetowardX = new float[2];

	private float temptowardX;

	private float input_delay = 0.2f;

	private float txtboxscale;

	private short pos_idx;

	private short scenecount;

	private float f_delay;

	private bool b_delay;

	private bool shake;

	private short finish;

	private bool disappear;

	private int dx = 3;

	private int language;

	private int showef = 1;

	private short extraimg_num;

	private void Awake()
	{
		script_uimap = GameObject.FindWithTag("ui").GetComponent<UI_map>();
		script_db = GetComponent<DB_Scenario>();
		script_scenetxt = GetComponent<Language_Scenario>();
		language = PlayerPrefs.GetInt("language");
	}

	public void SceneStart(int _difficulty)
	{
		base.gameObject.active = true;
		cur_difficulty = _difficulty;
		NextScene(0);
		scenecount++;
		showef = 1;
		finish = 0;
		disappear = false;
	}

	public void NextScene(short _scenenum)
	{
		targetposX[0] = 20f;
		targetposX[1] = 180f;
		pos_idx = script_db.sn[cur_difficulty, _scenenum]._pos;
		showef = script_db.sn[cur_difficulty, _scenenum]._showef;
		extraimg_num = script_db.sn[cur_difficulty, _scenenum]._extraimg;
		if (extraimg_num > 1000)
		{
			extraimg = Resources.Load("extraimg" + extraimg_num) as Texture2D;
		}
		if (portrait[(pos_idx + 1) % 2] == null)
		{
			targetposX[pos_idx] = 80f;
		}
		if (showef >= 4)
		{
			finish = 1;
			movetowardX[pos_idx] = targetposX[pos_idx];
			movetowardX[(pos_idx + 1) % 2] = remainposX[(pos_idx + 1) % 2];
			if (showef == 5)
			{
				Delay(0.3f);
				movetowardX[pos_idx] = startposX[pos_idx];
				temp = Resources.Load(script_db.sn[cur_difficulty, _scenenum]._chaimg) as Texture2D;
				movetowardX[(pos_idx + 1) % 2] = remainposX[(pos_idx + 1) % 2];
				return;
			}
		}
		else if (showef == 3)
		{
			targetposX[pos_idx] = 80f;
			disappear = true;
			movetowardX[(pos_idx + 1) % 2] = startposX[(pos_idx + 1) % 2];
			Delay(0.5f);
		}
		else if (showef == 2)
		{
			if (movetowardX[(pos_idx + 1) % 2] == 80f)
			{
				movetowardX[(pos_idx + 1) % 2] = targetposX[(pos_idx + 1) % 2];
			}
			movetowardX[pos_idx] = targetposX[pos_idx];
			temptowardX = movetowardX[pos_idx];
			shake = true;
			Delay(0.5f);
			InvokeRepeating("ShakePos", 0.05f, 0.05f);
			movetowardX[(pos_idx + 1) % 2] = remainposX[(pos_idx + 1) % 2];
		}
		else if (showef == 1)
		{
			if (movetowardX[(pos_idx + 1) % 2] == 80f)
			{
				movetowardX[(pos_idx + 1) % 2] = targetposX[(pos_idx + 1) % 2];
			}
			if (portrait[pos_idx] != null)
			{
				Delay(0.3f);
				movetowardX[pos_idx] = startposX[pos_idx];
				temp = Resources.Load(script_db.sn[cur_difficulty, _scenenum]._chaimg) as Texture2D;
				movetowardX[(pos_idx + 1) % 2] = remainposX[(pos_idx + 1) % 2];
				return;
			}
			currentposX[pos_idx] = startposX[pos_idx];
			movetowardX[pos_idx] = targetposX[pos_idx];
			movetowardX[(pos_idx + 1) % 2] = remainposX[(pos_idx + 1) % 2];
		}
		else if (showef == 0)
		{
			movetowardX[pos_idx] = targetposX[pos_idx];
			movetowardX[(pos_idx + 1) % 2] = remainposX[(pos_idx + 1) % 2];
		}
		portrait[pos_idx] = Resources.Load(script_db.sn[cur_difficulty, _scenenum]._chaimg) as Texture2D;
	}

	public void Delay(float _time)
	{
		b_delay = true;
		f_delay = _time;
	}

	private void ShakePos()
	{
		dx = -dx;
		movetowardX[pos_idx] = temptowardX + (float)dx;
	}

	private void Update()
	{
		if (input_delay < 0f)
		{
			if (Input.GetMouseButtonDown(0) && finish != 2)
			{
				if (finish == 1)
				{
					Delay(0.5f);
					finish = 2;
				}
				else
				{
					NextScene(scenecount);
					scenecount++;
					input_delay = 0.2f;
					txtboxscale = 0f;
				}
			}
		}
		else
		{
			input_delay -= Time.deltaTime;
		}
		if (b_delay)
		{
			f_delay -= Time.deltaTime;
			if (f_delay <= 0f)
			{
				b_delay = false;
				f_delay = 0f;
				movetowardX[pos_idx] = targetposX[pos_idx];
				if (finish == 2)
				{
					scenecount = 0;
					base.gameObject.active = false;
					for (int i = 0; i < 2; i++)
					{
						portrait[i] = null;
					}
					script_uimap.ScenarioFinish(extraimg_num);
				}
				else if (disappear)
				{
					disappear = false;
					portrait[(pos_idx + 1) % 2] = null;
				}
				else if (shake)
				{
					shake = false;
					CancelInvoke("ShakePos");
				}
				else
				{
					portrait[pos_idx] = temp;
				}
			}
		}
		if (finish == 2)
		{
			for (short num = 0; num < 2; num++)
			{
				if (portrait[num] != null)
				{
					currentposX[num] = Mathf.MoveTowards(currentposX[num], startposX[num], Time.deltaTime * 1500f);
				}
			}
		}
		else
		{
			for (int j = 0; j < 2; j++)
			{
				currentposX[j] = Mathf.MoveTowards(currentposX[j], movetowardX[j], Time.deltaTime * 1500f);
			}
		}
		txtboxscale = Mathf.MoveTowards(txtboxscale, 64f, Time.deltaTime * 500f);
	}

	private void OnGUI()
	{
		GUI.depth = -1;
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.Scale(new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 64f), black);
		GUI.DrawTexture(Crypto.Rect2(0f, 256f, 480f, 74f), black);
		for (short num = 0; num < 2; num++)
		{
			if (portrait[num] != null)
			{
				if (num != pos_idx)
				{
					GUI.color = Color.gray;
				}
				GUI.DrawTexture(Crypto.Rect2(currentposX[num], 0f, 300f, 300f), portrait[num]);
			}
			GUI.color = Color.white;
		}
		if (extraimg_num > 1000)
		{
			if (targetposX[pos_idx] == 80f)
			{
				GUI.DrawTexture(Crypto.Rect2(280f, 30f, 80f, 80f), extraimg);
			}
			else
			{
				GUI.DrawTexture(Crypto.Rect2(200f, 30f, 80f, 80f), extraimg);
			}
		}
		if (finish != 2)
		{
			GUI.DrawTexture(Crypto.Rect2(40f, 282f - txtboxscale * 0.5f, 400f, txtboxscale), txtbox1);
			GUI.Label(Crypto.Rect2(60f, 250f, 360f, 64f), script_scenetxt.txt_scene[language, script_db.sn[cur_difficulty, scenecount - 1]._txtidx], "txt12_0");
		}
	}
}
