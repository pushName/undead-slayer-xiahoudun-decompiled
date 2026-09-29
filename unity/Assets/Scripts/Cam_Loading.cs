using UnityEngine;

public class Cam_Loading : MonoBehaviour
{
	public Texture2D txt_loading;

	public Texture2D black;

	public GUISkin basicSkin;

	private bool stopCam;

	private Transform horse;

	private int tip_idx;

	private int language;

	private void Start()
	{
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Perspective(50f, 1.5f, 0.01f, 20f);
		horse = GameObject.Find("pet_horse_riding").transform;
		language = PlayerPrefs.GetInt("language");
		tip_idx = Random.Range(311, 331);
	}

	public void DisappearCam()
	{
		stopCam = true;
	}

	private void Update()
	{
		if (!stopCam)
		{
			base.transform.LookAt(horse.position + new Vector3(-0.4f, 0.16f, 0f));
		}
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 64f), black);
		GUI.DrawTexture(Crypto.Rect2(0f, 256f, 480f, 74f), black);
		GUI.DrawTexture(Crypto.Rect2(370f, 292f, 100f, 25f), txt_loading);
		GUI.Label(Crypto.Rect2(0f, 266f, 480f, 16f), Language.intxt[language, tip_idx], "txt12_w");
	}
}
