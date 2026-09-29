using UnityEngine;

public class Mon_Destroy : MonoBehaviour
{
	public Transform headbone;

	private Transform mytransform;

	private float delay_finish;

	private float downaccel = 1f;

	private void Awake()
	{
		base.GetComponent<Animation>()["dt1"].speed = 0.3f;
		mytransform = base.transform;
		base.gameObject.active = false;
	}

	public void FinishNow()
	{
		delay_finish = 5f;
	}

	public void TextureChange(Texture a, Vector3 _scale, int _kind)
	{
		base.GetComponent<Animation>().Stop();
		base.GetComponent<Animation>().Play("dt1");
		delay_finish = 0f;
		downaccel = 0f;
		if (_kind == 2)
		{
			mytransform.Find("p1").GetComponent<Renderer>().material.SetTexture("_MainTex", a);
			mytransform.Find("p2").GetComponent<Renderer>().material.SetTexture("_MainTex", a);
			if (_scale.x > 1f)
			{
				mytransform.localScale = _scale * 1.2f;
				headbone.localScale = Vector3.one * 0.7f;
			}
			else
			{
				mytransform.localScale = Vector3.one * 1.2f;
				headbone.localScale = Vector3.one;
			}
		}
		else
		{
			base.GetComponent<Animation>()["dt1"].speed = 0.55f;
		}
	}

	private void Update()
	{
		if (delay_finish > 3f)
		{
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 6f;
			delay_finish = 0f;
		}
		else
		{
			delay_finish += Time.deltaTime;
		}
		if (mytransform.position.y > 0f)
		{
			downaccel += Time.deltaTime * 5f;
			mytransform.position -= Vector3.up * Time.deltaTime * downaccel;
		}
		else
		{
			mytransform.position = new Vector3(mytransform.position.x, 0f, mytransform.position.z);
		}
	}
}
