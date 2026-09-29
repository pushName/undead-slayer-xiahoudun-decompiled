using UnityEngine;

public class UICampos : MonoBehaviour
{
	private void Start()
	{
		if (Crypto.Load_int_key("gamemode") == 0)
		{
			if (GameObject.Find("bg_worldmap") == null)
			{
				GameObject gameObject = Resources.Load("bg_worldmap") as GameObject;
				Transform transform = (Transform)Object.Instantiate(gameObject.transform, Vector3.zero, Quaternion.identity);
				transform.name = "bg_worldmap";
			}
			base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.8f, 1.8f, -1.2f, 1.2f, 0.3f, 5f);
			base.transform.position = PlayerPrefsX.GetVector3("uiCampos");
			return;
		}
		if (GameObject.Find("bg_dun") == null)
		{
			GameObject gameObject2 = Resources.Load("bg_dun") as GameObject;
			GameObject gameObject3 = Resources.Load("bg_dun_cover") as GameObject;
			GameObject gameObject4 = Resources.Load("bg_dun_below") as GameObject;
			Transform transform2 = (Transform)Object.Instantiate(gameObject2.transform, new Vector3(0f, -0.15f, -1f), Quaternion.identity);
			Transform transform3 = (Transform)Object.Instantiate(gameObject3.transform, new Vector3(0f, -0.15f, -0.5f), Quaternion.identity);
			Transform transform4 = (Transform)Object.Instantiate(gameObject4.transform, new Vector3(0f, -0.15f, -1f), Quaternion.identity);
			transform2.name = "bg_dun";
			transform3.name = "bg_dun_cover";
			transform4.name = "bg_dun_below";
		}
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.5f, 1.5f, -1f, 1f, 0.3f, 5f);
		base.transform.position = PlayerPrefsX.GetVector3("uiCampos2");
	}
}
