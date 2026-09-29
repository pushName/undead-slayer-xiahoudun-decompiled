using UnityEngine;

public class GiftBox : MonoBehaviour
{
	public Transform ef_risingitem;

	private bool itemrising;

	public Texture spbox;

	public Transform inven_gem;

	public Transform inven_weapon;

	public AudioClip snd_drop;

	public AudioClip snd_open;

	private Vector3 targetpos;

	private Vector3 directionVector;

	private int isclear;

	private float homingdelay;

	private bool drop_impact;

	private void Start()
	{
		targetpos = -Vector3.up * 0.6f + Vector3.forward * 0.2f;
		directionVector = Vector3.up * 0.5f;
		base.GetComponent<Animation>()["invisible"].speed = 0.06f;
		base.GetComponent<Animation>()["open_box"].speed = 0.25f;
		isclear = GameObject.FindWithTag("ui").GetComponent<UI_result>().isclear;
		if (isclear <= 0)
		{
			base.GetComponent<Animation>()["invisible"].wrapMode = WrapMode.ClampForever;
			base.GetComponent<Animation>().Play("invisible");
			return;
		}
		if (isclear > 100)
		{
			base.transform.GetChild(2).GetComponent<Renderer>().material.mainTexture = spbox;
			base.transform.localScale = Vector3.one;
		}
		base.GetComponent<Animation>().Play("invisible");
		AnimationState animationState = base.GetComponent<Animation>().PlayQueued("drop");
		animationState.speed = 0.3f;
		animationState = base.GetComponent<Animation>().PlayQueued("drop_i");
		animationState.speed = 0.3f;
	}

	public void OpenBox()
	{
		base.GetComponent<AudioSource>().PlayOneShot(snd_open);
		base.GetComponent<Animation>().Play("open_box");
		itemrising = true;
		{
			var emission = ef_risingitem.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
	}

	private void Update()
	{
		if (!drop_impact)
		{
			if (base.GetComponent<Animation>().IsPlaying("drop_i"))
			{
				drop_impact = true;
				base.GetComponent<AudioSource>().PlayOneShot(snd_drop);
			}
		}
		else
		{
			if (!itemrising)
			{
				return;
			}
			if (homingdelay < 1.5f)
			{
				homingdelay += Time.deltaTime;
				ef_risingitem.position += directionVector * Time.deltaTime;
				directionVector += targetpos * Time.deltaTime;
				return;
			}
			if (isclear > 100)
			{
				inven_weapon.gameObject.active = true;
				int num = isclear / 100;
				if (num == 1)
				{
					inven_weapon.GetComponent<Inventory_Weapon>().GetWeapon(isclear % 100);
				}
				else
				{
					inven_weapon.GetComponent<Inventory_Weapon>().GetArmor(isclear % 100);
				}
			}
			else
			{
				inven_gem.gameObject.active = true;
				inven_gem.GetComponent<Inventory_Gem>().GetItem();
			}
			ef_risingitem.position = new Vector3(ef_risingitem.position.x, 0.2f, ef_risingitem.position.z);
			{
				var emission = ef_risingitem.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			itemrising = false;
		}
	}
}
