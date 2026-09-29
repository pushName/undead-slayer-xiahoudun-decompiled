using UnityEngine;

public class Jin : MonoBehaviour
{
	private float finish_delay;

	private Transform mytransform;

	public Transform cyl;

	private float tune_time;

	private Material cyl_mat;

	private void Awake()
	{
		mytransform = base.transform;
		cyl_mat = cyl.GetComponent<Renderer>().sharedMaterial;
		base.gameObject.active = false;
		cyl.gameObject.active = false;
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.zero;
		cyl.gameObject.active = true;
	}

	private void Update()
	{
		if (Time.timeScale != 0f)
		{
			tune_time = Time.deltaTime / Time.timeScale;
		}
		finish_delay += tune_time;
		cyl_mat.mainTextureOffset += Vector2.up * tune_time * 3f;
		if (finish_delay > 4f)
		{
			base.gameObject.active = false;
			cyl.gameObject.active = false;
			finish_delay = 0f;
		}
		else if (finish_delay > 3f)
		{
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, Vector3.zero, tune_time * 3f);
		}
		else
		{
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, Vector3.one * 1.1f, tune_time * 3f);
		}
	}
}
