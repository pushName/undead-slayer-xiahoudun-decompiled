using UnityEngine;

public class Loading_cha : MonoBehaviour
{
	public Transform horseSpine;

	public Transform horse;

	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		base.GetComponent<Animation>()["ride2"].speed = 0.7f;
		base.GetComponent<Animation>().Play("ride2");
	}

	private void Update()
	{
		mytransform.position = horseSpine.position + Vector3.up * -0.085f;
		mytransform.rotation = horse.rotation;
	}
}
