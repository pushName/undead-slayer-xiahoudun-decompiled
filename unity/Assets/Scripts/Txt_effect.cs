using UnityEngine;

public class Txt_effect : MonoBehaviour
{
	private Material mymaterial;

	private Transform mytransform;

	private Vector3 startscale;

	private Vector3 targetscale = new Vector3(4f, 1f, 0f);

	private float finish_delay;

	private bool isdelay;

	private void Awake()
	{
		mymaterial = base.GetComponent<Renderer>().sharedMaterial;
		mytransform = base.transform;
		startscale = mytransform.localScale;
	}

	public void TxtEfOn(short _index)
	{
		base.gameObject.active = true;
		mytransform.localScale = startscale;
		finish_delay = 0f;
		if (_index == 2)
		{
			mytransform.localScale = Vector3.zero;
			isdelay = true;
		}
		mymaterial.mainTextureOffset = Vector2.up * 0.25f * _index;
	}

	private void Update()
	{
		finish_delay += Time.deltaTime;
		if (finish_delay > 0.7f)
		{
			base.gameObject.active = false;
		}
		else if (isdelay)
		{
			if (finish_delay > 0.2f)
			{
				mytransform.localScale = startscale;
				isdelay = false;
			}
		}
		else
		{
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, targetscale, Time.deltaTime * 15f);
		}
	}
}
