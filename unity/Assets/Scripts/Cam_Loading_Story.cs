using UnityEngine;

public class Cam_Loading_Story : MonoBehaviour
{
	private bool stopCam;

	private Transform horse;

	private void Start()
	{
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Perspective(50f, 1.5f, 0.01f, 20f);
		horse = GameObject.Find("pet_horse_riding").transform;
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
}
