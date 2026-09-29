using UnityEngine;

public class Bullet_Angel3 : MonoBehaviour
{
	private Transform mytransform;

	private Transform cha1;

	private Transform mychild;

	private Vector3 targetPos;

	private Vector3 dir;

	private AI_Asist script_angel;

	private LineRenderer myline;

	private float delay;

	private void Awake()
	{
		mytransform = base.transform;
		myline = GetComponent<LineRenderer>();
		cha1 = GameObject.Find("asist").transform;
		script_angel = cha1.GetComponent<AI_Asist>();
		mychild = mytransform.GetChild(0);
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		targetPos = mytransform.position + mytransform.forward * 0.3f;
		targetPos.y = 0.005f;
		dir = mytransform.forward;
		dir.y = 0f;
		delay = 2.8f;
		mytransform.rotation = Quaternion.identity;
		mychild.forward = dir;
	}

	private void Update()
	{
		dir += mychild.right * Time.deltaTime * 6f;
		dir.y = 0f;
		targetPos += dir * Time.deltaTime * 1f;
		Vector3 position = cha1.position;
		myline.SetPosition(0, position);
		myline.SetPosition(1, targetPos);
		mychild.position = targetPos;
		mychild.forward = dir;
		if (delay < 0f)
		{
			script_angel.AttackFinish();
			base.gameObject.active = false;
		}
		delay -= Time.deltaTime;
	}
}
