using UnityEngine;

public class Cutin01 : MonoBehaviour
{
	private float starttime;

	private float prevtimescale = 1f;

	private Vector3 originscale = Vector3.zero;

	private Vector3 originpos = Vector3.zero;

	private float startlerp;

	private float endlerp;

	private bool cinematicMode;

	private Cha_Control script_cha;

	private Cam_Move script_cam;

	private Transform mytransform;

	public Transform bg_black;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		script_cha = GameObject.FindWithTag("Player").GetComponent<Cha_Control>();
		script_cam = Camera.main.GetComponent<Cam_Move>();
		base.GetComponent<Renderer>().enabled = false;
		base.gameObject.active = false;
	}

	public void CutinOn(Vector3 startpos, Vector3 finishpos, Vector3 _originscale, float timescale, float a, float b, bool cinema)
	{
		originscale = _originscale;
		base.GetComponent<Renderer>().enabled = true;
		prevtimescale = Time.timeScale;
		starttime = Time.realtimeSinceStartup;
		mytransform.position = startpos;
		mytransform.localScale = originscale * 10f;
		Time.timeScale = timescale;
		originpos = finishpos;
		startlerp = a;
		endlerp = b;
		cinematicMode = cinema;
		bg_black.gameObject.active = true;
	}

	private void Update()
	{
		if (!base.GetComponent<Renderer>().enabled)
		{
			return;
		}
		if (cinematicMode)
		{
			script_cha.StopControl();
			cinematicMode = false;
		}
		if (starttime != 0f)
		{
			if (Time.realtimeSinceStartup - starttime < startlerp)
			{
				mytransform.position = Vector3.Lerp(mytransform.position, originpos, Time.deltaTime * 30f);
				mytransform.localScale = Vector3.Lerp(mytransform.localScale, originscale, Time.deltaTime * 50f);
				return;
			}
			if (Time.realtimeSinceStartup - starttime < endlerp)
			{
				Time.timeScale = prevtimescale;
				mytransform.localScale = Vector3.Lerp(mytransform.localScale, originscale * 10f, Time.deltaTime * 5.6f);
				return;
			}
			Time.timeScale = 1f;
			script_cam.ResetCam();
			script_cha.StartControl();
			base.GetComponent<Renderer>().enabled = false;
			starttime = 0f;
			mytransform.localScale = originscale * 10f;
			mytransform.position = new Vector3(-2f, 1f, 0f);
			base.gameObject.active = false;
			bg_black.gameObject.active = false;
		}
	}
}
