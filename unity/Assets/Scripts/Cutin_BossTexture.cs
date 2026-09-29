using UnityEngine;

public class Cutin_BossTexture : MonoBehaviour
{
	private float starttime;

	private float namepos;

	private Texture2D nameimg;

	private bool nameon;

	private int language = 1;

	private int cutindex = 1;

	public GUISkin basicSkin;

	private void Awake()
	{
		language = PlayerPrefs.GetInt("language");
	}

	public void SetCutinTexture(int _index)
	{
		cutindex = _index;
		string text = cutindex.ToString();
		base.GetComponent<Renderer>().material.mainTexture = Resources.Load("portrait_boss" + text) as Texture;
		nameimg = Resources.Load("name_boss" + text) as Texture2D;
		starttime = Time.realtimeSinceStartup;
		nameon = true;
	}

	private void Update()
	{
		namepos = Mathf.MoveTowards(-100f, 10f, (Time.realtimeSinceStartup - starttime) * 1000f);
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.depth = -8;
		if (nameon)
		{
			GUI.DrawTexture(Crypto.Rect2(namepos, 200f, 200f, 100f), nameimg);
			GUI.Label(Crypto.Rect2(namepos + 40f, 294f, 128f, 24f), Language.intxt[language, 74 + cutindex], "txt14_w");
		}
	}
}
