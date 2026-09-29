using UnityEngine;

public class PickParent : MonoBehaviour
{
	private Transform mytransform;

	private Renderer myrenderer;

	private Transform split;

	private Transform parentdummy;

	private void Awake()
	{
		split = base.transform.GetChild(0);
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
		parentdummy = GameObject.FindWithTag("dummy").transform;
		base.transform.parent = parentdummy;
	}

	private void OnEnable()
	{
		mytransform.position = parentdummy.position + Vector3.up * 0.1f;
		mytransform.rotation = Quaternion.Euler(0f, parentdummy.eulerAngles.y, 0f);
		mytransform.localScale = Vector3.zero;
		split.gameObject.active = false;
	}

	private void Update()
	{
		if (myrenderer.enabled && mytransform.localScale.x < 1f)
		{
			mytransform.localScale += Vector3.one * Time.deltaTime * 2f;
			split.gameObject.active = true;
		}
	}
}
