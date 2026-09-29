using UnityEngine;

public class Txt_result : MonoBehaviour
{
	private Mesh thismesh;

	private float destroydelay;

	private Vector3 originpos;

	private Transform mytransform;

	private Vector3 originscale;

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

	private void OnEnable()
	{
		destroydelay = 0f;
		mytransform.position = originpos;
		mytransform.localScale = Vector3.one * 3f;
	}

	private void Update()
	{
		mytransform.localScale = Vector3.Lerp(mytransform.localScale, originscale, Time.deltaTime * 12f);
		if (destroydelay < 2f)
		{
			destroydelay += Time.deltaTime;
			return;
		}
		base.gameObject.active = false;
		mytransform.position = Vector3.one * 5f;
		destroydelay = 0f;
	}

	public void UvMove(Vector2 _startUV)
	{
		thismesh.uv = new Vector2[4]
		{
			_startUV + Vector2.up * 0.25f,
			_startUV + Vector2.right + Vector2.up * 0.25f,
			_startUV,
			_startUV + Vector2.right
		};
	}
}
