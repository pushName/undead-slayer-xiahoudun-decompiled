using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
	public float updateInterval = 0.5f;

	private float accum;

	private int frames;

	private float timeleft;

	// 原脚本用的是 GUIText，该组件在 Unity 5.0 起被整体移除、无同名替代品；
	// 这里换成官方指定的后继者 UI.Text。注意 UI.Text 需要挂在 Canvas 下才会渲染，
	// 场景重建时得给它补一个 Canvas，否则下面的 Start 守卫会打日志并自我禁用。
	private Text uitext;

	private void Start()
	{
		uitext = base.GetComponent<Text>();
		if (!uitext)
		{
			Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
			base.enabled = false;
		}
		else
		{
			timeleft = updateInterval;
		}
	}

	private void Update()
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		frames++;
		if ((double)timeleft <= 0.0)
		{
			float num = accum / (float)frames;
			string text = $"{num:F2} FPS";
			uitext.text = text;
			if (num < 30f)
			{
				uitext.color = Color.yellow;
			}
			else if (num < 10f)
			{
				uitext.color = Color.red;
			}
			else
			{
				uitext.color = Color.green;
			}
			timeleft = updateInterval;
			accum = 0f;
			frames = 0;
		}
	}
}
