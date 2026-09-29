using UnityEngine;

public class General_dummy : MonoBehaviour
{
	public Transform shadow;

	public Transform r_hand;

	public Transform l_hand;

	private Transform general_weapon;

	private Transform mytransform;

	private Animation myanimation;

	private Cha_Control script_cha;

	private Transform cha1;

	public Transform bip01;

	private bool changeready;

	private bool jumpatk;

	private bool dead;

	private int generalkind;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		myanimation["change_in"].speed = 0.3f;
		myanimation["change_out"].speed = 0.2f;
		myanimation["change_in_atk"].speed = 0.3f;
		myanimation["dead"].speed = 0.3f;
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha = cha1.GetComponent<Cha_Control>();
		Transform transform = (Transform)Object.Instantiate(shadow, mytransform.position, mytransform.rotation);
		transform.GetComponent<Shadow>().Pickparent(bip01, 1f);
	}

	private void Start()
	{
		base.gameObject.active = false;
		mytransform.position = Vector3.one * 6f;
	}

	public void SetCostume(int _generalkind, int _weaponindex)
	{
		generalkind = _generalkind;
		mytransform.GetChild(_generalkind + 1).gameObject.active = true;
		GameObject gameObject = Resources.Load("wp_general" + _weaponindex) as GameObject;
		general_weapon = (Transform)Object.Instantiate(gameObject.transform, r_hand.position, r_hand.rotation);
		general_weapon.parent = r_hand;
		if (_generalkind == 0)
		{
			general_weapon = (Transform)Object.Instantiate(gameObject.transform, l_hand.position, l_hand.rotation);
			general_weapon.parent = l_hand;
		}
	}

	public void ShowIn()
	{
		base.gameObject.active = true;
		myanimation.Play("change_in");
		changeready = true;
		if (generalkind == 4)
		{
			general_weapon.GetComponent<TrailRenderer>().enabled = true;
		}
	}

	public void ShowIn_atk()
	{
		base.gameObject.active = true;
		myanimation.Play("change_in_atk");
		changeready = true;
		jumpatk = true;
		if (generalkind == 4)
		{
			general_weapon.GetComponent<TrailRenderer>().enabled = true;
		}
	}

	public void ShowOut()
	{
		base.gameObject.active = true;
		myanimation.Play("change_out");
		if (generalkind == 4)
		{
			general_weapon.GetComponent<TrailRenderer>().enabled = true;
		}
	}

	public void Dead()
	{
		base.gameObject.active = true;
		GameObject.FindWithTag("Respawn").GetComponent<Spawn>().GeneralDead();
		myanimation.Play("dead");
		dead = true;
		Object.Destroy(base.gameObject, 3f);
	}

	private void Update()
	{
		if (dead)
		{
			return;
		}
		if (!myanimation.isPlaying)
		{
			if (generalkind == 4)
			{
				general_weapon.GetComponent<TrailRenderer>().enabled = false;
			}
			mytransform.position = Vector3.one * 6f;
			base.gameObject.active = false;
			script_cha.ChangeFinish(changeready, jumpatk);
			changeready = false;
			jumpatk = false;
		}
		else
		{
			mytransform.position = cha1.position;
		}
	}
}
