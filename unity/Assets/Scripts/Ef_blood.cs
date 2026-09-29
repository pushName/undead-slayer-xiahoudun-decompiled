using UnityEngine;

public class Ef_blood : MonoBehaviour
{
	private int index;

	private int oldindex = -1;

	private float starttime;

	private Renderer myrenderer;

	private Transform mytransform;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
		size = Vector2.one * 0.25f;
		base.gameObject.active = false;
	}

	private void Update()
	{
		if (!(mytransform.position.y < 1f))
		{
			return;
		}
		starttime += Time.deltaTime;
		index = (int)(starttime * 22f);
		uIndex = index % 4;
		vIndex = index / 4;
		offset = new Vector2(uIndex * size.x, 1f - size.y - (float)vIndex * size.y);
		if (index != oldindex)
		{
			if (index >= 16)
			{
				mytransform.position = Vector3.one * 3f;
				starttime = 0f;
				index = 0;
				base.gameObject.active = false;
			}
			myrenderer.material.mainTextureOffset = offset;
			myrenderer.material.mainTextureScale = size;
			oldindex = index;
		}
	}
}
