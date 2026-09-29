using System.Collections;
using UnityEngine;

public class Cam_island : MonoBehaviour
{
	private Transform mytransform;

	public Transform bg;

	public Texture2D txt_loading;

	private AsyncOperation async;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private IEnumerator Start()
	{
		async = Application.LoadLevelAdditiveAsync("Stage");
		yield return async;
	}

	private void Update()
	{
		mytransform.position -= Vector3.forward * Time.deltaTime * 0.12f;
		if (mytransform.position.z < -0.6f && async.isDone)
		{
			Object.Destroy(mytransform.root.gameObject);
			Camera.main.transform.GetChild(0).gameObject.active = true;
			GameObject.FindWithTag("ui").SendMessage("LoadingFinish");
		}
		bg.position = Vector3.Lerp(bg.position, Vector3.up * 0.5f, Time.deltaTime * 0.5f);
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.Scale(new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.DrawTexture(Crypto.Rect2(370f, 286f, 100f, 25f), txt_loading);
	}
}
