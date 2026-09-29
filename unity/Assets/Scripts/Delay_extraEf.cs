using UnityEngine;

public class Delay_extraEf : MonoBehaviour
{
	private float dis_delay;

	private void Start()
	{
	}

	private void OnEnable()
	{
		base.GetComponent<Animation>().Play("uvU_rotY");
		dis_delay = 3f;
	}

	private void Update()
	{
		dis_delay -= Time.deltaTime;
		if (dis_delay <= 0f)
		{
			base.gameObject.active = false;
			base.GetComponent<Animation>().Stop();
		}
	}
}
