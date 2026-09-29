using UnityEngine;

public class Ef_Coin : MonoBehaviour
{
	private Transform mytransform;

	private float d_finish;

	private Vector3 targetpos;

	private void Awake()
	{
		mytransform = base.transform;
		base.gameObject.active = false;
	}

	public void GetCoin(Vector3 _pos)
	{
		mytransform.position = _pos;
		targetpos = _pos;
		targetpos[1] = 0.15f;
		d_finish = 0f;
		base.gameObject.active = true;
	}

	private void Update()
	{
		d_finish += Time.deltaTime;
		if (d_finish > 0.5f)
		{
			base.gameObject.active = false;
			return;
		}
		mytransform.position = Vector3.Lerp(mytransform.position, targetpos, Time.deltaTime * 15f);
		mytransform.Rotate(Vector3.up * 1200f * Time.deltaTime);
	}
}
