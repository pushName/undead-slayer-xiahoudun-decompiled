using UnityEngine;

public class Ef_ani_walk : MonoBehaviour
{
	private int framesPerSecond = 16;

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

	private int loopcount;

	private int targetloopcount;

	private Material mymaterial;

	private bool anistart;

	private Texture2D ani_img;

	private UI_map script_ui;

	private int ani_index;

	private void Awake()
	{
		mytransform = base.transform;
		mymaterial = base.GetComponent<Renderer>().material;
		script_ui = GameObject.FindWithTag("ui").GetComponent<UI_map>();
	}

	private void Start()
	{
		starttime = 0f;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
		base.gameObject.active = false;
	}

	public void AniStart(int _index)
	{
		base.gameObject.active = true;
		ani_index = _index;
		starttime = 0f;
		index = 0;
		oldindex = -1;
		offset = Vector2.up * (1f - size.y);
		mymaterial.SetTextureOffset("_MainTex", offset);
		switch (_index)
		{
		case 0:
			ani_img = Resources.Load("ani_explore_cha") as Texture2D;
			base.GetComponent<Renderer>().material.mainTexture = ani_img;
			mytransform.localScale = Vector3.zero;
			targetloopcount = 3;
			break;
		case 2:
			ani_img = Resources.Load("ani_explore_cha_suc") as Texture2D;
			base.GetComponent<Renderer>().material.mainTexture = ani_img;
			mytransform.localScale = Vector3.one * 2f;
			targetloopcount = 0;
			break;
		case 3:
			ani_img = Resources.Load("ani_explore_cha_fail") as Texture2D;
			base.GetComponent<Renderer>().material.mainTexture = ani_img;
			mytransform.localScale = Vector3.one * 2f;
			targetloopcount = 0;
			break;
		case 1:
			ani_img = Resources.Load("ani_explore_treasure") as Texture2D;
			base.GetComponent<Renderer>().material.mainTexture = ani_img;
			mytransform.localScale = Vector3.one * 2f;
			targetloopcount = 1;
			break;
		case 4:
			ani_img = Resources.Load("ef_perfect") as Texture2D;
			base.GetComponent<Renderer>().material.mainTexture = ani_img;
			mytransform.localScale = Vector3.one * 2f;
			targetloopcount = 100;
			break;
		}
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
		if (index == oldindex)
		{
			return;
		}
		if (index >= lastframe)
		{
			if (loopcount >= targetloopcount)
			{
				anistart = false;
				loopcount = 0;
				if (ani_index == 0)
				{
					if (script_ui.GetGeneral())
					{
						AniStart(2);
					}
					else
					{
						AniStart(3);
					}
				}
				else if (ani_index == 1)
				{
					script_ui.GetTreasure();
				}
				else if (ani_index == 2)
				{
					script_ui.AniFinish(_success: true);
				}
				else if (ani_index == 3)
				{
					script_ui.AniFinish(_success: false);
				}
			}
			else
			{
				loopcount++;
				index = 0;
				starttime = 0f;
			}
		}
		else
		{
			offset = Vector2.right * uIndex * size.x + Vector2.up * (1f - size.y - (float)vIndex * size.y);
			mymaterial.SetTextureOffset("_MainTex", offset);
			mymaterial.SetTextureScale("_MainTex", size);
			oldindex = index;
		}
	}
}
