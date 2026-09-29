using System.Collections;
using UnityEngine;

public class Loading_horse : MonoBehaviour
{
	private Transform mytransform;

	private bool camstop;

	public Transform pt_stepfog;

	public Texture[] tex_stepfog = new Texture[5];

	private int playkind;

	private AsyncOperation async;

	private void Awake()
	{
		mytransform = base.transform;
		base.GetComponent<Animation>()["horse_run"].speed = 0.7f;
		base.GetComponent<Animation>().Play("horse_run");
		int num = Crypto.Load_int_key("cur_stage_kind");
		playkind = Crypto.Load_int_key("play_kind");
		pt_stepfog.GetComponent<Renderer>().sharedMaterial.mainTexture = tex_stepfog[num - 1];
	}

	private IEnumerator Start()
	{
		if (playkind == 4)
		{
			async = Application.LoadLevelAdditiveAsync("Stage_ride");
		}
		else
		{
			async = Application.LoadLevelAdditiveAsync("Stage");
		}
		yield return async;
	}

	private void Update()
	{
		mytransform.position += Vector3.forward * Time.deltaTime;
		if (mytransform.position.z > 5.1f)
		{
			if (async.isDone)
			{
				Object.Destroy(mytransform.root.gameObject);
				Camera.main.transform.GetChild(0).gameObject.active = true;
				GameObject.FindWithTag("ui").SendMessage("LoadingFinish");
			}
		}
		else if (mytransform.position.z > 4.55f && !camstop)
		{
			camstop = true;
			GameObject.Find("Cam_loading").GetComponent<Cam_Loading>().DisappearCam();
		}
	}
}
