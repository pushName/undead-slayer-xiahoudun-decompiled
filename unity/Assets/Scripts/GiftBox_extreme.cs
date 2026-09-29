using UnityEngine;

public class GiftBox_extreme : MonoBehaviour
{
	public Transform ef_risingitem;

	public AudioClip snd_drop;

	public AudioClip snd_open;

	private bool drop_impact;

	private bool itemrising;

	private UI_Ingame script_ingameUI;

	private int itemrate = 70;

	private void Awake()
	{
		base.GetComponent<Animation>()["invisible"].speed = 0.06f;
		base.GetComponent<Animation>()["open_box"].speed = 0.25f;
		base.GetComponent<Animation>()["drop"].speed = 0.3f;
		base.GetComponent<Animation>().Play("invisible");
		base.gameObject.active = false;
		script_ingameUI = GameObject.Find("UI_Ingame").GetComponent<UI_Ingame>();
	}

	private void OnEnable()
	{
		drop_impact = false;
		base.GetComponent<Animation>().Play("drop");
		AnimationState animationState = base.GetComponent<Animation>().PlayQueued("drop_i");
		animationState.speed = 0.3f;
		ef_risingitem.position = base.transform.position;
	}

	private void OnTriggerEnter(Collider other)
	{
		OpenBox();
		base.GetComponent<Collider>().enabled = false;
	}

	private void OpenBox()
	{
		base.GetComponent<AudioSource>().PlayOneShot(snd_open);
		base.GetComponent<Animation>().Play("open_box");
		itemrising = true;
		{
			var emission = ef_risingitem.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		if (script_ingameUI.angelOn)
		{
			script_ingameUI.GetAngel();
		}
		else if (Random.Range(0, 100) > itemrate)
		{
			itemrate = 70;
			script_ingameUI.GetGeneral();
		}
		else
		{
			itemrate -= 10;
			script_ingameUI.GetCoin();
		}
		script_ingameUI.IntermissionOff();
	}

	private void Update()
	{
		if (!drop_impact)
		{
			if (base.GetComponent<Animation>().IsPlaying("drop_i"))
			{
				drop_impact = true;
				base.GetComponent<AudioSource>().PlayOneShot(snd_drop);
				base.GetComponent<Collider>().enabled = true;
			}
		}
		else if (itemrising)
		{
			if (ef_risingitem.position.y < 1f)
			{
				ef_risingitem.position += Vector3.up * Time.deltaTime;
				return;
			}
			itemrising = false;
			{
				var emission = ef_risingitem.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
		}
	}
}
