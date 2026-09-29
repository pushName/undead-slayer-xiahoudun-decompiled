using UnityEngine;

public class Beam_boss11 : MonoBehaviour
{
	public float uvspeed = 1f;

	public float finishdelay = 3f;

	private float delay;

	private Material mymaterial;

	private Vector2 offset;

	private Transform mytransform;

	private float rotatefactor = 1f;

	private void Awake()
	{
		mytransform = base.transform;
		mymaterial = base.GetComponent<Renderer>().sharedMaterial;
	}

	private void OnEnable()
	{
		offset = Vector2.zero;
		mymaterial.mainTextureOffset = Vector2.zero;
		mytransform.localScale = Vector3.zero;
		delay = 0f;
		rotatefactor = 1f;
	}

	private void DanceHit()
	{
	}

	private void Update()
	{
		offset += Vector2.up * Time.deltaTime * uvspeed;
		if (delay > finishdelay)
		{
			mytransform.position = Vector3.one * 38f;
			base.gameObject.active = false;
			delay = 0f;
		}
		else
		{
			rotatefactor += Time.deltaTime * 100f;
			delay += Time.deltaTime;
			mymaterial.mainTextureOffset = offset;
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, new Vector3(1.4f, 1.4f, 2f), Time.deltaTime * 3f);
			mytransform.root.Rotate(Vector3.up * Time.deltaTime * rotatefactor);
		}
	}
}
