using UnityEngine;

public class Pet_horse_UI : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Animation>()["horse_run"].speed = 0.5f;
		base.GetComponent<Animation>()["horse_stand"].speed = 0.16f;
		base.GetComponent<Animation>()["horse_cry"].speed = 0.32f;
		base.GetComponent<Animation>()["horse_jump"].speed = 0.3f;
	}

	private void Update()
	{
	}
}
