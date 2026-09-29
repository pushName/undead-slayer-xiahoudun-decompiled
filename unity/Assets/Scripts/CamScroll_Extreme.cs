using UnityEngine;

public class CamScroll_Extreme : MonoBehaviour
{
	private Transform cover;

	private Transform below;

	private bool stopmove;

	private bool scrollOn;

	private bool isevent;

	public bool drag;

	private float movedelay;

	private float tempx;

	private float prevMposX;

	private Vector3 currentCampos;

	private Transform mytransform;

	private float current_camsize = 1f;

	private float camsize = 1f;

	private bool camsizechange;

	private float movecamspeed = 7f;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.5f, 1.5f, -1f, 1f, 0.3f, 5f);
	}

	public void SetCover(Transform _cover, Transform _below, float _camposX, bool _isscroll, float _speed)
	{
		cover = _cover;
		below = _below;
		tempx = _camposX;
		tempx = Mathf.Clamp(tempx, -10.4f, 10.4f);
		if (_isscroll)
		{
			mytransform.position = new Vector3(10.4f, 0f, 1f);
			cover.transform.position = new Vector3(-10.4f, -0.15f, -0.5f);
			below.transform.position = new Vector3(6.344f, -0.15f, -1.5f);
			movedelay = 1f;
			movecamspeed = _speed;
		}
		else
		{
			mytransform.position = new Vector3(tempx, 0f, 1f);
			cover.transform.position = new Vector3(0f - tempx, -0.15f, -0.5f);
			below.transform.position = new Vector3(tempx * 0.61f, -0.15f, -1.5f);
		}
	}

	public void DisableMove()
	{
		stopmove = true;
	}

	public void EnableMove()
	{
		stopmove = false;
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
			float x = mytransform.position.x;
			x = Mathf.Lerp(x, tempx, Time.deltaTime * movecamspeed);
			mytransform.position = new Vector3(x, 0f, 1f);
			cover.transform.position = new Vector3(0f - x, -0.15f, -0.5f);
			below.transform.position = new Vector3(x * 0.61f, -0.15f, -1.5f);
			currentCampos = mytransform.position;
			scrollOn = false;
			if (isevent && !(movedelay <= 0f))
			{
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
			if (Mathf.Abs(tempx) > 0.06f)
			{
				drag = true;
			}
			tempx += currentCampos.x;
			tempx = Mathf.Clamp(tempx, -10.4f, 10.4f);
			mytransform.position = new Vector3(tempx, 0f, 1f);
			cover.transform.position = new Vector3(0f - tempx, -0.15f, -0.5f);
			below.transform.position = new Vector3(tempx * 0.61f, -0.15f, -1.5f);
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
