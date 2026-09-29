using UnityEngine;

public class Ani_speed : MonoBehaviour
{
	public string aniname;

	public float speed = 0.04f;

	private void Awake()
	{
		base.GetComponent<Animation>()[aniname].speed = speed;
	}
}
