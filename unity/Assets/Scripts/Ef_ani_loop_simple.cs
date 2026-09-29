using UnityEngine;

public class Ef_ani_loop_simple : MonoBehaviour
{
	public int uvAnimationTileX = 4;

	public int uvAnimationTileY = 4;

	public int framesPerSecond = 20;

	public bool loop;

	public bool instance_material;

	public bool timetune;

	private int index;

	private int oldindex = -1;

	private float starttime;

	private int lastframe;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private Renderer myrenderer;

	private Material mymaterial;

	private void Awake()
	{
		myrenderer = base.GetComponent<Renderer>();
	}

	private void Start()
	{
		starttime = 0f;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
		if (!instance_material)
		{
			mymaterial = myrenderer.sharedMaterial;
		}
		else
		{
			mymaterial = myrenderer.material;
		}
	}

	private void Update()
	{
		if (!myrenderer.enabled)
		{
			return;
		}
		if (!timetune)
		{
			starttime += Time.deltaTime;
		}
		else if (Time.timeScale != 0f)
		{
			starttime += Time.deltaTime / Time.timeScale;
		}
		index = (int)(starttime * (float)framesPerSecond);
		if (loop)
		{
			index %= lastframe;
		}
		uIndex = index % uvAnimationTileX;
		vIndex = index / uvAnimationTileX;
		if (index != oldindex)
		{
			if (index >= lastframe)
			{
				starttime = 0f;
				oldindex = -1;
				base.gameObject.active = false;
			}
			offset = Vector2.right * uIndex * size.x + Vector2.up * (1f - size.y - (float)vIndex * size.y);
			mymaterial.mainTextureOffset = offset;
			mymaterial.mainTextureScale = size;
			oldindex = index;
		}
	}
}
