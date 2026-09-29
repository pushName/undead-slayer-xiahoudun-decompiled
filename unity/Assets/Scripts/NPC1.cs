using UnityEngine;

public class NPC1 : MonoBehaviour
{
	private bool startcounter;

	private float neo_delay;

	private Transform ef_split;

	public Transform blood;

	private Monster_efs script_mon;

	public AudioClip snd_scream;

	private void Start()
	{
		base.GetComponent<Animation>()["hurted1"].speed = 0.35f;
		base.GetComponent<Animation>()["hurted2"].speed = 0.3f;
		base.GetComponent<Animation>()["hurted2_i"].speed = 0.4f;
		base.GetComponent<Animation>()["hurted2_i"].layer = 1;
		Camera.main.GetComponent<Cam_Move>().LookTarget(base.transform, 16, 100f);
		ef_split = base.transform.Find("ef_split1");
		script_mon = GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>();
	}

	private void OnTriggerEnter(Collider other)
	{
		base.GetComponent<AudioSource>().Stop();
		base.GetComponent<AudioSource>().PlayOneShot(snd_scream);
		if (base.GetComponent<Animation>().IsPlaying("hurted1"))
		{
			startcounter = true;
			base.GetComponent<Rigidbody>().AddForce(base.transform.forward * 120f);
			Object.Destroy(base.GetComponent<Collider>());
		}
		base.GetComponent<Animation>().Play("hurted2");
	}

	public void Behit()
	{
		base.GetComponent<Animation>().Play("hurted2_i");
		Vector3 pos = base.transform.position + base.transform.forward * 0.02f - Vector3.up * 0.07f;
		script_mon.CreatGrabBlood(pos, Quaternion.Euler(70f, Random.Range(0, 360), 0f));
	}

	private void Update()
	{
		if (base.GetComponent<Animation>().IsPlaying("hurted1"))
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, Vector3.zero, Time.deltaTime * 0.08f);
		}
		if (startcounter)
		{
			neo_delay += Time.deltaTime;
			if (neo_delay >= 2f)
			{
				GameObject.FindWithTag("Player").GetComponent<Cha_Control>().WakToward(base.transform.position, _run: true, _ride: false);
				ef_split.GetComponent<Renderer>().material.SetTextureScale("_MainTex", Vector2.one * 0.5f);
				ef_split.GetComponent<Renderer>().enabled = true;
				startcounter = false;
			}
		}
	}
}
