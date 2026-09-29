using UnityEngine;

public class Txt_star : MonoBehaviour
{
	private Mesh thismesh;

	private float destroydelay;

	private Vector3 originpos;

	private Transform mytransform;

	private Vector3 originscale;

	private bool anistart;

	private void Awake()
	{
		mytransform = base.transform;
		thismesh = GetComponent<MeshFilter>().mesh;
		originpos = mytransform.position;
		originscale = mytransform.localScale;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (anistart)
		{
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, originscale, Time.deltaTime * 8f);
			if (destroydelay < 2f)
			{
				destroydelay += Time.deltaTime;
				return;
			}
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 5f;
			destroydelay = 0f;
			anistart = false;
		}
	}

	public void SetStar(int _count)
	{
		thismesh.uv = new Vector2[4]
		{
			Vector2.up * 0.25f,
			new Vector2(0.3333f * (float)_count, 0.25f),
			Vector2.zero,
			Vector2.right * 0.3333f * _count
		};
		originscale = new Vector3(0.3333f * (float)_count, 1f, 1f);
		mytransform.localScale = new Vector3(2 * _count, 6f, 6f);
		destroydelay = 0f;
		mytransform.position = originpos;
		anistart = true;
	}
}
