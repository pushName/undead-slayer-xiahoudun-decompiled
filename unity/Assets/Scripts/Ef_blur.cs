using UnityEngine;

public class Ef_blur : MonoBehaviour
{
	private Material mymat;

	private void Awake()
	{
		mymat = base.GetComponent<Renderer>().sharedMaterial;
		mymat.mainTextureOffset = Vector2.zero;
		base.gameObject.active = false;
	}

	private void Update()
	{
		if (mymat.mainTextureOffset.y > -1f)
		{
			mymat.mainTextureOffset -= Vector2.up * Time.deltaTime * 2f;
			return;
		}
		mymat.mainTextureOffset = Vector2.zero;
		base.gameObject.active = false;
	}
}
