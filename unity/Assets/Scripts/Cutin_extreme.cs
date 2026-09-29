using UnityEngine;

public class Cutin_extreme : MonoBehaviour
{
	private Transform mytransform;

	private Vector3 originPos;

	private Vector3 originScale;

	private float duration;

	public float delay = 2f;

	private void Awake()
	{
		mytransform = base.transform;
		originPos = mytransform.position;
		originScale = mytransform.localScale;
	}

	private void OnEnable()
	{
		int num = 3;
		if (originPos.x < 0f)
		{
			num *= -1;
		}
		mytransform.position += Vector3.right * num;
		mytransform.localScale = Vector3.zero;
		duration = delay;
	}

	private void Update()
	{
		float num = 0f;
		if (Time.timeScale != 0f)
		{
			num = Time.deltaTime / Time.timeScale;
		}
		if (duration < 0f)
		{
			base.gameObject.active = false;
		}
		else if (duration < 1f)
		{
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, Vector3.zero, num * 25f);
		}
		else if (duration < 2f)
		{
			mytransform.position = Vector3.MoveTowards(mytransform.position, originPos, num * 15f);
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, originScale, num * 15f);
		}
		duration -= num;
	}
}
