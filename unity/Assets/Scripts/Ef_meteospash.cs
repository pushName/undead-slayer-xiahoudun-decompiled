using UnityEngine;

public class Ef_meteospash : MonoBehaviour
{
	private int uvAnimationTileX = 4;

	private int uvAnimationTileY = 4;

	private int framesPerSecond = 22;

	private float index;

	private float starttime;

	private float currenttime;

	private int lastframe;

	private int var;

	private void Start()
	{
		starttime = Time.time;
		currenttime = Time.time;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		base.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
		base.transform.parent = GameObject.Find("SkillEf").transform;
		base.GetComponent<Collider>().isTrigger = true;
		base.GetComponent<Renderer>().enabled = true;
	}

	private void Update()
	{
		if (base.GetComponent<Renderer>().enabled)
		{
			if (base.transform.localScale.x < 3.5f)
			{
				base.transform.localScale += new Vector3(1f, 1f, 1f) * Time.deltaTime;
			}
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
				base.transform.localScale = new Vector3(0f, 0f, 0f);
				var = 0;
				base.GetComponent<Collider>().isTrigger = false;
			}
			base.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
			base.GetComponent<Renderer>().material.SetTextureScale("_MainTex", scale);
		}
	}
}
