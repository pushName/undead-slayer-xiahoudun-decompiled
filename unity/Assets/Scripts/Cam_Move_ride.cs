using UnityEngine;

public class Cam_Move_ride : MonoBehaviour
{
	public Transform cha1;

	private Transform mytransform;

	private Vector3 camPos = new Vector3(0f, 1.3f, -0.8f);

	private int dx = 1;

	private Vector3 hit_shake1 = new Vector3(0.1f, 0f, 0.06f);

	private Camera mycamera;

	private void Awake()
	{
		mytransform = base.transform;
		mycamera = base.GetComponent<Camera>();
	}

	public void Hitcam()
	{
		dx = -dx;
		mytransform.position += hit_shake1 * dx;
		if (mycamera.fieldOfView > 23f)
		{
			mycamera.fieldOfView -= 1f;
		}
	}

	private void Update()
	{
		mytransform.LookAt(cha1);
		mytransform.position = camPos + Vector3.right * Mathf.Sin(Time.time * 0.5f) * 0.8f;
		if (mycamera.fieldOfView < 38f)
		{
			mycamera.fieldOfView = Mathf.Lerp(mycamera.fieldOfView, 38f, Time.deltaTime * 3f);
		}
		else
		{
			mycamera.fieldOfView = 38f;
		}
	}
}
