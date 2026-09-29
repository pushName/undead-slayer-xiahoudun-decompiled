using UnityEngine;

public class BG_Extreme_Gift : MonoBehaviour
{
	private Transform mytransform;

	private bool movefinish;

	private bool ptemit;

	private float delay;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.zero;
		movefinish = false;
		delay = 3f;
		ptemit = false;
	}

	private void Update()
	{
		if (movefinish)
		{
			if (mytransform.localScale.x < 0f)
			{
				movefinish = false;
				mytransform.localScale = Vector3.zero;
				base.gameObject.active = false;
				mytransform.rotation = Quaternion.identity;
			}
			else
			{
				mytransform.localScale -= Vector3.one * Time.deltaTime * 8f;
				mytransform.Rotate(0f, 0f, 500f * Time.deltaTime);
			}
		}
		else if (delay > 0f)
		{
			if (mytransform.position.y > 2.2f)
			{
				mytransform.position -= Vector3.up * Time.deltaTime * 3f;
			}
			else if (!ptemit)
			{
				mytransform.position = new Vector3(0f, 2.2f, 1.5f);
				mytransform.GetChild(0).GetComponent<ParticleSystem>().Emit(1);
				ptemit = true;
			}
			delay -= Time.deltaTime;
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, Vector3.one * 1.5f, Time.deltaTime * 5f);
		}
		else
		{
			movefinish = true;
		}
	}
}
