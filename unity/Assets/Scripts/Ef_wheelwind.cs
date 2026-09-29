using UnityEngine;

public class Ef_wheelwind : MonoBehaviour
{
	private int index;

	private int oldindex = -1;

	private float starttime;

	private float finishtime = 1.6f;

	private Transform mytransform;

	private Renderer myrenderer;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
	}

	private void Start()
	{
		mytransform.localScale = Vector3.one * 0.3f;
		base.GetComponent<Collider>().isTrigger = true;
		base.GetComponent<Renderer>().enabled = false;
		size = Vector2.one * 0.5f;
	}

	private void Update()
	{
		if (mytransform.localScale.x > 1f)
		{
			base.GetComponent<Renderer>().enabled = true;
		}
		if (mytransform.localScale.x < 1.8f)
		{
			mytransform.localScale += Vector3.one * 4f * Time.deltaTime;
		}
		else
		{
			mytransform.localScale = Vector3.one * 1.8f;
		}
		if (starttime > finishtime)
		{
			base.GetComponent<Renderer>().enabled = false;
			base.gameObject.active = false;
			mytransform.localScale = Vector3.one * 0.3f;
			starttime = 0f;
		}
		starttime += Time.deltaTime;
		index = (int)(starttime * 16f);
		index %= 4;
		uIndex = index % 2;
		vIndex = index / 2;
		if (index != oldindex)
		{
			if (index == 0 || index == 3)
			{
				base.GetComponent<Collider>().enabled = true;
			}
			else
			{
				base.GetComponent<Collider>().enabled = false;
			}
			offset = new Vector2(uIndex * size.x, 1f - size.y - (float)vIndex * size.y);
			myrenderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
			myrenderer.sharedMaterial.SetTextureScale("_MainTex", size);
			oldindex = index;
		}
	}
}
