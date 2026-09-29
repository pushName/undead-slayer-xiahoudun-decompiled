using UnityEngine;

public class Bullet_magicmissile : MonoBehaviour
{
	public float closetime = 3f;

	private float delay;

	public float homingrate;

	public float homingspeed;

	private Transform cha1;

	private Transform mytransform;

	private Vector3 directionVector;

	private Quaternion rotate;

	private Vector3 originscale;

	private Vector3 reduceVector;

	private void Awake()
	{
		mytransform = base.transform;
		originscale = mytransform.localScale;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		InvokeRepeating("SetDir", 0f, 0.1f);
		reduceVector = new Vector3(0.5f, 1f, 1f);
	}

	private void OnEnable()
	{
		delay = 0f;
		mytransform.localScale = originscale;
	}

	public void SetDir()
	{
		directionVector = cha1.position - mytransform.position;
		directionVector[1] = 0f;
		if (directionVector != Vector3.zero)
		{
			rotate = Quaternion.LookRotation(directionVector);
		}
	}

	private void Update()
	{
		mytransform.rotation = Quaternion.Lerp(mytransform.rotation, rotate, Time.deltaTime * homingrate);
		mytransform.position += mytransform.forward * Time.deltaTime * homingspeed;
		delay += Time.deltaTime;
		if (delay > closetime)
		{
			mytransform.localScale -= reduceVector * Time.deltaTime * 1.5f;
			if (mytransform.localScale.z < 0.02f)
			{
				base.gameObject.active = false;
				mytransform.position = Vector3.one * 4f;
			}
		}
	}
}
