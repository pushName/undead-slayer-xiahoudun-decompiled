using UnityEngine;

public class Ef_forge_hammer : MonoBehaviour
{
	private int framesPerSecond = 20;

	private int uvAnimationTileX = 4;

	private int uvAnimationTileY = 4;

	private int index;

	private int oldindex = -1;

	private float starttime;

	private int lastframe;

	private Transform mytransform;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private Material mymaterial;

	private bool anistart;

	private bool impact;

	private UI_forge script_ui;

	private void Awake()
	{
		mytransform = base.transform;
		mymaterial = base.GetComponent<Renderer>().material;
		script_ui = GameObject.FindWithTag("ui").GetComponent<UI_forge>();
	}

	private void Start()
	{
		starttime = 0f;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.zero;
	}

	public void HammerHit()
	{
		index = 0;
		starttime = 0f;
		oldindex = -1;
		impact = false;
		anistart = true;
	}

	private void Update()
	{
		mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, Vector3.one * 2f, Time.deltaTime * 15f);
		if (!anistart)
		{
			return;
		}
		starttime += Time.deltaTime;
		index = (int)(starttime * (float)framesPerSecond);
		uIndex = index % uvAnimationTileX;
		vIndex = index / uvAnimationTileY;
		if (index != oldindex)
		{
			if (index >= 2 && !impact)
			{
				script_ui.Impact();
				impact = true;
			}
			if (index >= lastframe)
			{
				anistart = false;
				return;
			}
			offset = Vector2.right * uIndex * size.x + Vector2.up * (1f - size.y - (float)vIndex * size.y);
			mymaterial.SetTextureOffset("_MainTex", offset);
			mymaterial.SetTextureScale("_MainTex", size);
			oldindex = index;
		}
	}
}
