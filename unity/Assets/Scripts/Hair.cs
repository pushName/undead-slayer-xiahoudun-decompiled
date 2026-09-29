using UnityEngine;

public class Hair : MonoBehaviour
{
	private int chamovestat;

	private void Start()
	{
		GetComponent<TrailRenderer>().enabled = true;
	}

	private void Update()
	{
		chamovestat = base.transform.root.GetComponent<Cha_Control>().chamovestat;
		if (chamovestat < 2)
		{
			GetComponent<TrailRenderer>().enabled = false;
		}
		else
		{
			GetComponent<TrailRenderer>().enabled = true;
		}
	}
}
