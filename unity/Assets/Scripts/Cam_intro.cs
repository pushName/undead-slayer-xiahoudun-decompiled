using UnityEngine;

public class Cam_intro : MonoBehaviour
{
	private float dx = 0.1f;

	private bool startui;

	public Transform UI_intro;

	public Transform gate;

	public Transform cha;

	public Transform mon1;

	public Transform mon2;

	private Color tintcolor;

	private Color tintcolor2;

	private void Awake()
	{
	}

	private void Start()
	{
		base.GetComponent<Camera>().orthographicSize = dx;
		tintcolor = Color.clear;
		tintcolor2 = Color.black;
		cha.GetComponent<Renderer>().material.SetColor("_TintColor", tintcolor);
		mon1.GetComponent<Renderer>().material.SetColor("_TintColor", tintcolor2);
		mon2.GetComponent<Renderer>().material.SetColor("_TintColor", tintcolor2);
	}

	private void StartGUI()
	{
		UI_intro.GetComponent<UI_intro>().StartGUI();
	}

	private void Update()
	{
		dx = Mathf.Lerp(dx, 1f, Time.deltaTime);
		base.GetComponent<Camera>().projectionMatrix = Matrix4x4.Ortho(-1.5f * dx, 1.5f * dx, 0f - dx, dx, 0.3f, 5f);
		gate.localScale = Vector3.Lerp(gate.localScale, new Vector3(6f, 12f, 0f), Time.deltaTime * 1f);
		gate.position = Vector3.Lerp(gate.position, new Vector3(0f, 0f, 2.5f), Time.deltaTime * 1f);
		if (dx > 0.85f)
		{
			if (!startui)
			{
				Invoke("StartGUI", 0.5f);
				startui = true;
			}
			cha.position = Vector3.Lerp(cha.position, new Vector3(0f, -0.22f, 2f), Time.deltaTime * 3f);
			cha.localScale = Vector3.Lerp(cha.localScale, Vector3.one * 5f, Time.deltaTime * 3f);
			tintcolor = Color.Lerp(tintcolor, Color.gray, Time.deltaTime * 2f);
			tintcolor2 = Color.Lerp(tintcolor2, Color.gray, Time.deltaTime * 2f);
			cha.GetComponent<Renderer>().material.SetColor("_TintColor", tintcolor);
		}
		if (dx > 0.4f)
		{
			mon1.GetComponent<Renderer>().material.SetColor("_TintColor", tintcolor2);
			mon2.GetComponent<Renderer>().material.SetColor("_TintColor", tintcolor2);
			mon1.position = Vector3.Lerp(mon1.position, new Vector3(1.2f, -0.32f, 2f), Time.deltaTime * 3f);
			mon1.localScale = Vector3.Lerp(mon1.localScale, new Vector3(2f, 4f, 0f), Time.deltaTime * 3f);
			mon2.position = Vector3.Lerp(mon2.position, new Vector3(-1.2f, -0.32f, 2f), Time.deltaTime * 3f);
			mon2.localScale = Vector3.Lerp(mon2.localScale, new Vector3(2f, 4f, 0f), Time.deltaTime * 3f);
		}
	}
}
