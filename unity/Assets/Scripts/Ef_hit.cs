using UnityEngine;

public class Ef_hit : MonoBehaviour
{
	private Transform mytransform;

	private float showdelay;

	private Vector3 originscale;

	private Vector3 growVector = new Vector3(-3f, 0f, 10f);

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		originscale = mytransform.localScale;
		base.GetComponent<Renderer>().sharedMaterial.renderQueue = 4051;
	}

	private void Update()
	{
		if (showdelay < 0.25f)
		{
			mytransform.localScale += growVector * Time.deltaTime;
			showdelay += Time.deltaTime;
			return;
		}
		showdelay = 0f;
		mytransform.position = Vector3.one * 3f;
		mytransform.localScale = originscale;
		base.gameObject.active = false;
	}
}
