using UnityEngine;

public class Cam_UI_Friends : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.5f, 1.5f, -1f, 1f, 0.3f, 5f);
		base.transform.position = PlayerPrefsX.GetVector3("uiCampos2");
	}
}
