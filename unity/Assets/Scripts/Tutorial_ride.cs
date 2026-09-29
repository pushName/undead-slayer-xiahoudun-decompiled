using UnityEngine;

public class Tutorial_ride : MonoBehaviour
{
	private Texture guide_img;

	public Texture pop_blank;

	private int guide_curent_subindex;

	private int language;

	public GUISkin basicSkin;

	public GUIStyle bt_yesno;

	private void Start()
	{
		guide_img = Resources.Load("guide9" + guide_curent_subindex) as Texture2D;
		language = PlayerPrefs.GetInt("language");
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.DrawTexture(Crypto.Rect2(112f, 96f, 256f, 128f), pop_blank);
		Debug.Log(guide_curent_subindex);
		GUI.Label(Crypto.Rect2(124f, 102f, 232f, 36f), string.Empty + Language.intxt[language, 389 + guide_curent_subindex], "txt12_0");
		GUI.Label(Crypto.Rect2(124f, 200f, 232f, 14f), string.Empty + (guide_curent_subindex + 1) + " / 2", "txt12_0");
		GUI.DrawTexture(Crypto.Rect2(170f, 134f, 128f, 64f), guide_img);
		if (guide_curent_subindex >= 1)
		{
			Debug.Log(Language.intxt[language, 4]);
			if (GUI.Button(Crypto.Rect2(290f, 184f, 64f, 32f), Language.intxt[language, 4], bt_yesno))
			{
				Time.timeScale = 1f;
				base.gameObject.active = false;
				GameObject.FindWithTag("general").GetComponent<AudioSource>().Play();
			}
		}
		else
		{
			Debug.Log(Language.intxt[language, 47]);
			if (GUI.Button(Crypto.Rect2(290f, 184f, 64f, 32f), Language.intxt[language, 47], bt_yesno))
			{
				guide_curent_subindex++;
				guide_img = Resources.Load("guide9" + guide_curent_subindex) as Texture2D;
			}
		}
	}
}
