using UnityEngine;

public class UI_Ingame_story : MonoBehaviour
{
	private int chahp;

	private int chamaxhp = 100;

	private float chasp;

	private float chamaxsp = 100f;

	private float chaexp;

	private int chalv = 1;

	private int soul;

	private int coin;

	private int guide = -1;

	public Texture2D pop_blank;

	public Transform pack;

	private Transform g_power;

	private MakeUI script_pack;

	private Gauge_UV gauge_hp;

	private Gauge_UV gauge_sp;

	private Gauge_UV gauge_exp;

	private Gauge_UV gauge_power;

	private string[] guide_txt = new string[4];

	public Texture2D[] guide_img = new Texture2D[4];

	public GUIStyle yesno;

	private float exp_height;

	private Vector3 shootdir;

	private bool uistart = true;

	private float f_charge;

	private bool chargeon;

	public GUISkin basicSkin;

	private GameObject cha1;

	private Cha_Control script_cha;

	private Pet_horse script_horse;

	private int language;

	private void Start()
	{
		language = PlayerPrefs.GetInt("language");
		chamaxhp = 100;
		chamaxsp = 100f;
		script_pack = pack.GetComponent<MakeUI>();
		script_pack.CreatCustomPlane(new Vector2(0.25f, 0.25f), 0f, new Vector3(-1.35f, 2.85f, 2f), new Vector2(0.5f, 0.625f), new Vector2(0.625f, 0.75f), "bg_hp", null, 0f, 0);
		Transform transform = script_pack.CreatCustomPlane(new Vector2(0.9f, 0.1f), 0f, new Vector3(-0.757f, 2.92f, 1.8f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5625f), "gauge_hp", "Gauge_UV", 0.2f, 0);
		Transform transform2 = script_pack.CreatCustomPlane(new Vector2(1.06f, 0.05f), 0f, new Vector3(-0.6775f, 2.85f, 1.8f), new Vector2(0f, 0.47f), new Vector2(0.5f, 0.498f), "gauge_sp", "Gauge_UV", 0.12f, 0);
		Transform transform3 = script_pack.CreatCustomPlane(Vector2.one * 0.186f, 0f, new Vector3(-1.352f, 2.85f, 2.1f), new Vector2(0.875f, 0.75f), new Vector2(1f, 0.875f), "gauge_exp", "Gauge_UV", 0f, 0);
		g_power = script_pack.CreatCustomPlane(new Vector2(0.4f, 0.05f), 0f, new Vector3(0f, 2.4f, 2.1f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5625f), "gauge_power", "Gauge_UV", 0f, 0);
		cha1 = GameObject.FindWithTag("Player");
		script_cha = cha1.GetComponent<Cha_Control>();
		gauge_hp = transform.GetComponent<Gauge_UV>();
		gauge_sp = transform2.GetComponent<Gauge_UV>();
		gauge_exp = transform3.GetComponent<Gauge_UV>();
		gauge_power = g_power.GetComponent<Gauge_UV>();
		guide_txt[0] = Language.intxt[language, 33];
		guide_txt[1] = Language.intxt[language, 34];
		guide_txt[2] = Language.intxt[language, 35];
		guide_txt[3] = Language.intxt[language, 360];
		g_power.gameObject.active = false;
	}

	public void ResetPower()
	{
		chargeon = false;
		f_charge = 0f;
		gauge_power.UvMove(Vector2.zero);
		g_power.gameObject.active = false;
	}

	public void PowerCharge()
	{
		chargeon = true;
		g_power.gameObject.active = true;
	}

	public void GainCoin(int a)
	{
		coin += a;
	}

	public void GainSoul(int a)
	{
		soul += a;
		chaexp = script_cha.exp;
		exp_height = (1f - chaexp / (float)(chalv * 100)) * 0.125f;
		gauge_exp.UvMove(Vector2.up * exp_height);
	}

	public void StatUpdate(int _hp, int _maxhp, float _sp)
	{
		chahp = _hp;
		chasp = _sp;
		chamaxhp = _maxhp;
		float x = (1f - (float)chahp / (float)chamaxhp) * 0.5f;
		gauge_hp.UvMove(new Vector2(x, 0f));
		float num = (1f - chasp / chamaxsp) * 0.5f;
		if (chasp >= 50f)
		{
			gauge_sp.UvMove(Vector2.right * num);
		}
		else
		{
			gauge_sp.UvMove(Vector2.right * num - Vector2.up * (1f / 32f));
		}
	}

	public void GuideOn(int a)
	{
		script_cha.StopControl();
		Time.timeScale = 0f;
		guide = a;
	}

	private void Update()
	{
		if (chargeon)
		{
			if (f_charge < 0.5f)
			{
				f_charge += Time.deltaTime * 0.6f;
			}
			else
			{
				f_charge = 0f;
				script_cha.Eximpact();
				chargeon = false;
				g_power.gameObject.active = false;
			}
			gauge_power.UvMove(Vector2.right * (0f - f_charge));
		}
	}

	private void OnGUI()
	{
		if (!uistart)
		{
			return;
		}
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.depth = 0;
		if (guide < 0)
		{
			return;
		}
		GUI.DrawTexture(Crypto.Rect2(112f, 96f, 256f, 128f), pop_blank);
		GUI.Label(Crypto.Rect2(124f, 104f, 232f, 36f), string.Empty + guide_txt[guide], "txt12_0");
		GUI.Label(Crypto.Rect2(118f, 200f, 244f, 14f), string.Empty + (guide + 1) + " / 4", "txt12_0");
		GUI.DrawTexture(Crypto.Rect2(170f, 138f, 128f, 64f), guide_img[guide]);
		if (guide == 3)
		{
			if (GUI.Button(Crypto.Rect2(290f, 184f, 64f, 32f), Language.intxt[language, 4], yesno))
			{
				guide = -1;
				Time.timeScale = 1f;
				script_cha.StartControl();
			}
		}
		else if (GUI.Button(Crypto.Rect2(290f, 184f, 64f, 32f), Language.intxt[language, 47], yesno))
		{
			guide++;
		}
	}
}
