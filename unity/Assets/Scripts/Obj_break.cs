using UnityEngine;

public class Obj_break : MonoBehaviour
{
	public bool isrunning;

	private void Start()
	{
		base.GetComponent<Animation>()["obj_break"].speed = 0.25f;
		if (!isrunning)
		{
			Object.Destroy(base.gameObject, 1.2f);
		}
	}

	private void Update()
	{
		if (isrunning)
		{
			if (base.transform.position.z > -0.6f)
			{
				base.transform.position += Vector3.forward * -1.8f * Time.deltaTime;
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
