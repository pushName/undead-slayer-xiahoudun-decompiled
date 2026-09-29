using UnityEngine;

public class Ef_rapidstab : MonoBehaviour
{
	private int index;

	private int oldindex = -1;

	private float starttime;

	private Transform mytransform;

	private Renderer myrenderer;

	private Collider mycollider;

	private Vector2 size;

	private Vector2 offset;

	public float showtime = 0.15f;

	public int uvAnimationTileX = 4;

	public int uvAnimationTileY = 4;

	public int framesPerSecond = 18;

	public int damagerate = 2;

	public int loopcount;

	private int count;

	private int lastframe;

	private bool show;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
		mycollider = base.GetComponent<Collider>();
		mycollider.enabled = false;
	}

	private void Start()
	{
		myrenderer.enabled = false;
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
		lastframe = uvAnimationTileX * uvAnimationTileY;
	}

	private void Update()
	{
		if (!show)
		{
			if (starttime < showtime)
			{
				starttime += Time.deltaTime;
				return;
			}
			myrenderer.enabled = true;
			starttime = 0f;
			show = true;
			return;
		}
		mytransform.position += mytransform.forward * Time.deltaTime * 0.1f;
		starttime += Time.deltaTime;
		index = (int)(starttime * (float)framesPerSecond);
		float num = index % uvAnimationTileX;
		int num2 = index / uvAnimationTileX;
		if (index == oldindex)
		{
			return;
		}
		if (index % damagerate == 1)
		{
			mycollider.enabled = true;
		}
		else
		{
			mycollider.enabled = false;
		}
		if (index >= lastframe)
		{
			if (count >= loopcount)
			{
				myrenderer.enabled = false;
				mycollider.enabled = false;
				base.gameObject.active = false;
				mytransform.position = Vector3.one * 10f;
				oldindex = -1;
				starttime = 0f;
				count = 0;
				show = false;
			}
			else
			{
				count++;
			}
			index = 0;
			oldindex = -1;
			starttime = 0f;
		}
		offset = new Vector2(num * size.x, 1f - size.y - (float)num2 * size.y);
		myrenderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
		myrenderer.sharedMaterial.SetTextureScale("_MainTex", size);
		oldindex = index;
	}
}
