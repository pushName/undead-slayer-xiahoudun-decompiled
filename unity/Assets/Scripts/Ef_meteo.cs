using UnityEngine;

public class Ef_meteo : MonoBehaviour
{
	private bool splash;

	private bool thisIsOrigin;

	public Transform meteosplash;

	private void Start()
	{
		if (base.transform.position.y > 4.6f)
		{
			thisIsOrigin = true;
			base.GetComponent<Renderer>().enabled = false;
		}
		else
		{
			base.GetComponent<Renderer>().enabled = true;
		}
	}

	private void Update()
	{
		if (!thisIsOrigin)
		{
			if (base.transform.position.y > 0f)
			{
				base.transform.eulerAngles -= new Vector3(0f, 400f, 0f) * Time.deltaTime;
				base.transform.position += new Vector3(0f, -2.4f, 0f) * Time.deltaTime;
			}
			else if (!splash)
			{
				base.transform.localScale = new Vector3(0f, 0f, 0f);
				GameObject.Find("MainCamera").GetComponent<Cam_Move>().Hitcam();
				Transform transform = (Transform)Object.Instantiate(meteosplash, new Vector3(base.transform.position.x, 0.05f, base.transform.position.z), Quaternion.identity);
				Object.Destroy(transform.gameObject, 0.3f);
				splash = true;
			}
		}
	}
}
