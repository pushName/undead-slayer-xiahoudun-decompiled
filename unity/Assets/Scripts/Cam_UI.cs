using UnityEngine;

public class Cam_UI : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.5f, 1.5f, -1f, 1f, 0.3f, 5f);
	}
}
