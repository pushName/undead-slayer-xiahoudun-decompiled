using UnityEngine;

public class CamScroll : MonoBehaviour
{
	private bool stopmove;

	private bool scrollOn;

	private bool isevent;

	public bool drag;

	private float movedelay;

	private float tempx;

	private float tempy;

	private float prevMposX;

	private float prevMposY;

	private Vector3 currentCampos;

	private Vector3 targetpos;

	private Transform mytransform;

	private Transform target;

	private float current_camsize = 1f;

	private float camsize = 1f;

	private bool camsizechange;

	private int movecamspeed = 7;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.8f, 1.8f, -1.2f, 1.2f, 0.3f, 5f);
		mytransform.position = PlayerPrefsX.GetVector3("uiCampos");
	}

	public void DisableMove()
	{
		stopmove = true;
	}

	public void EnableMove()
	{
		stopmove = false;
	}

	public void Zoom(bool _zoomout, Vector3 _pos)
	{
		drag = false;
		camsizechange = true;
		isevent = false;
		if (_zoomout)
		{
			stopmove = true;
			camsize = 2.3f;
			targetpos = new Vector3(0f, -0.05f, 1f);
			movedelay = 0.6f;
			movecamspeed = 7;
		}
		else
		{
			stopmove = false;
			camsize = 1.2f;
			targetpos = _pos;
			targetpos[0] = Mathf.Clamp(targetpos[0], -1.7f, 1.7f);
			targetpos[1] = Mathf.Clamp(targetpos[1], -2.3f, 2.3f);
			targetpos[2] = 1f;
			movedelay = 0.8f;
			movecamspeed = 3;
		}
	}

	public void MoveTarget(Transform _target, bool _isevent)
	{
		target = _target;
		targetpos = _target.position;
		targetpos[0] = Mathf.Clamp(targetpos[0], -1.7f, 1.7f);
		targetpos[1] = Mathf.Clamp(targetpos[1], -2.3f, 2.3f);
		targetpos[2] = 1f;
		movedelay = 0.6f;
		tempx = 0f;
		tempy = 0f;
		PlayerPrefsX.SetVector3("uiCampos", targetpos);
		isevent = _isevent;
		movecamspeed = 7;
	}

	public void MoveTargetCancel()
	{
		movedelay = 0f;
		isevent = false;
	}

	private void Update()
	{
		if (movedelay > 0f)
		{
			movedelay -= Time.deltaTime;
			mytransform.position = Vector3.Lerp(mytransform.position, targetpos, Time.deltaTime * (float)movecamspeed);
			currentCampos = mytransform.position;
			scrollOn = false;
			if (isevent && movedelay <= 0f)
			{
				target.GetComponent<Icon_Stage>().IconDown();
			}
		}
		else
		{
			if (stopmove)
			{
				return;
			}
			if (Input.GetMouseButtonDown(0))
			{
				drag = false;
				scrollOn = true;
				prevMposX = Input.mousePosition.x;
				prevMposY = Input.mousePosition.y;
				currentCampos = mytransform.position;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				scrollOn = false;
			}
		}
		if (scrollOn)
		{
			tempx = 0.006f * (Input.mousePosition.x - prevMposX) * (480f / (float)Screen.width);
			tempy = -0.006f * (Input.mousePosition.y - prevMposY) * (320f / (float)Screen.height);
			if (Mathf.Abs(tempx) > 0.06f || Mathf.Abs(tempy) > 0.06f)
			{
				drag = true;
			}
			tempx += currentCampos.x;
			tempy += currentCampos.y;
			tempx = Mathf.Clamp(tempx, -1.7f, 1.7f);
			tempy = Mathf.Clamp(tempy, -2.3f, 2.3f);
			mytransform.position = new Vector3(tempx, tempy, 1f);
		}
		if (camsizechange)
		{
			if (current_camsize == camsize)
			{
				camsizechange = false;
			}
			current_camsize = Mathf.MoveTowards(current_camsize, camsize, Time.deltaTime * 3f);
			base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.5f * current_camsize, 1.5f * current_camsize, 0f - current_camsize, current_camsize, 0.1f, 5f);
		}
	}
}
