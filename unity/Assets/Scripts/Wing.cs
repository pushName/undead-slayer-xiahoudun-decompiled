using UnityEngine;

public class Wing : MonoBehaviour
{
	private Transform mytransform;

	private Transform wingmesh;

	private float delay;

	private Animation myanimation;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		wingmesh = mytransform.GetChild(2);
	}

	private void OnEnable()
	{
		wingmesh.gameObject.active = true;
		myanimation.Play("wing_start");
		delay = 0f;
	}

	private void Start()
	{
		myanimation["wing_fly"].speed = 0.5f;
		myanimation["wing_attack"].speed = 0.25f;
		myanimation["wing_start"].speed = 2.2f;
	}

	private void Update()
	{
		delay += Time.deltaTime;
		if (delay > 4f)
		{
			base.gameObject.active = false;
			wingmesh.gameObject.active = false;
			myanimation.Stop();
		}
		else if (delay > 3f)
		{
			myanimation.CrossFade("wing_attack");
		}
		else if (delay > 0.6f)
		{
			myanimation.Play("wing_fly");
		}
	}
}
