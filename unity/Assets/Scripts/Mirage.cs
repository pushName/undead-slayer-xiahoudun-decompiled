using UnityEngine;

public class Mirage : MonoBehaviour
{
	private Transform mytransform;

	private float delay_finish;

	private Vector3 bouncescale = new Vector3(1f, 1f, 2f);

	private Vector3 startscale = new Vector3(1f, 1f, 0.5f);

	private Vector3 mypos;

	private float dropdown;

	private bool bounce;

	private bool move;

	private Vector3 directionVector;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void OnEnable()
	{
		mytransform.localScale = startscale;
		dropdown = 0.2f;
		move = false;
		bounce = false;
		delay_finish = 0f;
		mypos = mytransform.position;
	}

	private void Update()
	{
		if (move)
		{
			mypos += directionVector * Time.deltaTime * 1.5f;
			if (!bounce)
			{
				dropdown -= Time.deltaTime * 3f;
				directionVector = mytransform.forward + Vector3.up * dropdown;
				mytransform.rotation = Quaternion.LookRotation(directionVector);
			}
			else
			{
				mytransform.localScale = Vector3.Lerp(mytransform.localScale, bouncescale, Time.deltaTime * 3f);
			}
			if (mypos.y < 0f)
			{
				mypos[1] = 0f;
				dropdown = 0.4f;
				bounce = true;
				directionVector[1] = 0f;
				directionVector = (directionVector + Vector3.up * 0.3f) * 2f;
				mytransform.rotation = Quaternion.LookRotation(directionVector);
			}
		}
		else
		{
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, bouncescale, Time.deltaTime * 5f);
		}
		mytransform.position = mypos;
		delay_finish += Time.deltaTime;
		if (delay_finish > 1f)
		{
			base.gameObject.active = false;
			delay_finish = 0f;
		}
		else if (delay_finish > 0.1f)
		{
			move = true;
		}
	}
}
