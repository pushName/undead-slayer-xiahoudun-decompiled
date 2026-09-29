using UnityEngine;

public class CostumeAniSpeed : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Animation>()["costume"].speed = 0.24f;
	}
}
