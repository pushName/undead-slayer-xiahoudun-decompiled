using UnityEngine;

public class Bullet_wheelwind : MonoBehaviour
{
	private int monmovestat;

	private Transform mytransform;

	private Transform myparent;

	private Collider mycollider;

	private AI_Enemy01 script_mon;

	private void Awake()
	{
		mytransform = base.transform;
		myparent = mytransform.root;
	}

	private void Start()
	{
		myparent = mytransform.root;
		script_mon = myparent.GetComponent<AI_Enemy01>();
	}

	private void Update()
	{
		mytransform.Rotate(Vector3.up * Time.deltaTime * -1000f);
		monmovestat = script_mon.monmovestat;
		if (monmovestat <= 0)
		{
			base.gameObject.active = false;
		}
	}
}
