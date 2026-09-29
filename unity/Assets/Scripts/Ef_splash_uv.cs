using UnityEngine;

public class Ef_splash_uv : MonoBehaviour
{
	private int uvAnimationTileX = 4;

	private int uvAnimationTileY = 4;

	private int framesPerSecond = 30;

	private float index;

	private float starttime;

	private float currenttime;

	private int lastframe;

	private int var;

	private GameObject cha1;

	private GameObject ascraps;

	private void Start()
	{
		cha1 = base.transform.root.gameObject;
		ascraps = GameObject.Find("Attack_scraps");
		Physics.IgnoreCollision(cha1.GetComponent<Collider>(), base.GetComponent<Collider>());
		currenttime = 0f;
		starttime = 0f;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		base.GetComponent<Renderer>().enabled = false;
		base.transform.localScale = Vector3.zero;
		base.GetComponent<Collider>().isTrigger = true;
		base.GetComponent<Collider>().enabled = false;
	}

	public void SplashOn()
	{
		base.transform.parent = ascraps.transform;
		base.transform.localScale = new Vector3(2f, 2f, 2f);
		starttime = Time.time;
		base.GetComponent<Renderer>().enabled = true;
		base.GetComponent<Collider>().enabled = true;
		base.transform.rotation = cha1.transform.rotation;
		base.transform.position = cha1.transform.position + new Vector3(0f, 0.02f, 0f) + cha1.transform.forward * 0.1f;
		base.transform.localRotation = Quaternion.identity;
	}

	private void Update()
	{
		if (base.GetComponent<Renderer>().enabled)
		{
			currenttime = Time.time - starttime;
			index = currenttime * (float)framesPerSecond;
			index = (int)(index % (float)lastframe);
			var -= (int)index;
			Vector2 scale = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
			float num = index % (float)uvAnimationTileX;
			int num2 = (int)index / uvAnimationTileX;
			Vector2 offset = new Vector2(num * scale.x, 1f - scale.y - (float)num2 * scale.y);
			if (index == 0f && var != 0)
			{
				base.GetComponent<Renderer>().enabled = false;
				base.GetComponent<Collider>().enabled = false;
				base.transform.localScale = Vector3.zero;
				var = 0;
				base.transform.parent = cha1.transform;
				base.transform.rotation = cha1.transform.rotation;
				base.transform.position = cha1.transform.position + new Vector3(0f, 0.02f, 0f) + cha1.transform.forward * 0.3f;
			}
			base.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
			base.GetComponent<Renderer>().material.SetTextureScale("_MainTex", scale);
		}
	}
}
