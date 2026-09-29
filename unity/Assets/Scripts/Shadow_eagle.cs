using UnityEngine;

public class Shadow_eagle : MonoBehaviour
{
	private Transform mytransform;

	private Vector3 shadowpos;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
	}

	private void Update()
	{
		shadowpos = mytransform.position;
		shadowpos[1] = 0.002f;
		mytransform.position = shadowpos;
	}
}
