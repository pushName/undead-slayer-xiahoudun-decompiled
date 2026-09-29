using UnityEngine;

public class Bullet_Angel4_m : MonoBehaviour
{
	private float bullet_speed = -1f;

	private float side_speed;

	private Transform mytransform;

	private Vector3 originScale;

	private Vector3 myPos;

	private Vector3 dir;

	private Vector3 sidedir;

	private int side;

	private void Awake()
	{
		mytransform = base.transform;
		originScale = mytransform.localScale;
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.zero;
		bullet_speed = -2f;
		side_speed = 0.2f + Random.Range(-0.1f, 0.1f);
		dir = mytransform.forward;
		sidedir = mytransform.right * side;
	}

	public void Init(int _side)
	{
		side = _side;
	}

	private void Update()
	{
		myPos = mytransform.position;
		bullet_speed += Time.deltaTime * 6f;
		side_speed = Mathf.Lerp(side_speed, 0f, Time.deltaTime * 2f);
		mytransform.localScale = Vector3.Lerp(mytransform.localScale, originScale, Time.deltaTime * 4f);
		myPos += sidedir * Time.deltaTime * side_speed;
		myPos += dir * Time.deltaTime * bullet_speed;
		if (mytransform.position.y < 0f)
		{
			mytransform.root.GetComponent<Bullet_Angel4>().SplashOn(myPos);
			base.gameObject.active = false;
		}
		mytransform.position = myPos;
	}
}
