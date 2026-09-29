using UnityEngine;

public class Ef_twirl : MonoBehaviour
{
	private int uvAnimationTileX = 4;

	private int uvAnimationTileY = 4;

	private int framesPerSecond = 180;

	private int index;

	private int oldindex;

	private float starttime;

	private int lastframe;

	private bool isloop;

	private Renderer myrenderer;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private void Awake()
	{
		myrenderer = base.GetComponent<Renderer>();
	}

	private void Start()
	{
		starttime = 0f;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		myrenderer.enabled = false;
	}

	public void TwirlOn(int x, int y, int fps, bool loop)
	{
		uvAnimationTileX = x;
		uvAnimationTileY = y;
		framesPerSecond = fps;
		myrenderer.enabled = true;
		starttime = 0f;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		isloop = loop;
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
	}

	public void TwirlOff()
	{
		myrenderer.enabled = false;
	}

	private void Update()
	{
		if (!myrenderer.enabled)
		{
			return;
		}
		starttime += Time.deltaTime;
		index = (int)(starttime * (float)framesPerSecond);
		if (isloop)
		{
			index %= lastframe;
		}
		uIndex = index % uvAnimationTileX;
		vIndex = index / uvAnimationTileX;
		if (index != oldindex)
		{
			if (index >= lastframe)
			{
				myrenderer.enabled = false;
				oldindex = -1;
			}
			offset = new Vector2(uIndex * size.x, 1f - size.y - (float)vIndex * size.y);
			myrenderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
			myrenderer.sharedMaterial.SetTextureScale("_MainTex", size);
			oldindex = index;
		}
	}
}
