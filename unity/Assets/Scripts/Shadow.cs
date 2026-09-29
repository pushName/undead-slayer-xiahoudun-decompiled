using UnityEngine;

public class Shadow : MonoBehaviour
{
	private Transform pickparent;

	private Transform mytransform;

	private Vector3 shadowpos;

	private void Awake()
	{
		mytransform = base.transform;
		base.GetComponent<Renderer>().sharedMaterial.renderQueue = 2001;
	}

	public void Pickparent(Transform pick, float shadowscale)
	{
		pickparent = pick;
		if (shadowscale > 1f)
		{
			mytransform.localScale = Vector3.one * shadowscale;
		}
	}

	public void Finish()
	{
		mytransform.position = Vector3.one * 3f;
		base.gameObject.active = false;
		pickparent = null;
	}

	private void Update()
	{
		if (pickparent != null)
		{
			shadowpos = pickparent.position;
			shadowpos[1] = 0f;
			mytransform.position = shadowpos;
		}
		else
		{
			Finish();
		}
	}
}
