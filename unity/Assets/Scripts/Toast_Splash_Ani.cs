using UnityEngine;

public class Toast_Splash_Ani : MonoBehaviour
{
	private int uvAnimationTileX = 4;

	private int uvAnimationTileY = 5;

	private int framesPerSecond = 20;

	private int index;

	private int oldindex = -1;

	private float starttime;

	private int lastframe;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private float ani_time;

	private bool start_snd;

	private bool finish_ani;

	private Material mymaterial;

	public Transform ci_hidea;

	private void Awake()
	{
	}

	private void Start()
	{
		starttime = 0f;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
		mymaterial = base.GetComponent<Renderer>().material;
	}

	private void Update()
	{
		ani_time += Time.deltaTime;
		if (finish_ani)
		{
			if (ani_time > 4f)
			{
				ci_hidea.localScale = Vector3.MoveTowards(ci_hidea.localScale, Vector3.zero, Time.deltaTime * 800f);
			}
			else if (ani_time > 2.6f)
			{
				base.transform.localScale = Vector3.zero;
				ci_hidea.localScale = Vector3.MoveTowards(ci_hidea.localScale, new Vector3(60f, 30f, 0f), Time.deltaTime * 600f);
			}
			return;
		}
		if (ani_time > 0.5f)
		{
			starttime += Time.deltaTime;
			index = (int)(starttime * (float)framesPerSecond);
			uIndex = index % uvAnimationTileX;
			vIndex = index / uvAnimationTileX;
			if (!start_snd)
			{
				base.GetComponent<AudioSource>().Play();
				start_snd = true;
			}
		}
		if (index != oldindex)
		{
			if (index >= lastframe - 1)
			{
				index = lastframe - 1;
				oldindex = index;
				finish_ani = true;
			}
			offset = Vector2.right * uIndex * size.x + Vector2.up * (1f - size.y - (float)vIndex * size.y);
			mymaterial.mainTextureOffset = offset;
			mymaterial.mainTextureScale = size;
			oldindex = index;
		}
	}
}
