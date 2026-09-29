using UnityEngine;

public class BossWing : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Animation>()["wing_fly"].speed = 0.24f;
		base.GetComponent<Animation>()["wing_attack"].speed = 0.24f;
	}
}
