using UnityEngine;

public class Cha_Weapon : MonoBehaviour
{
	private GameObject blade;

	private Transform mytransform;

	public Transform bowgun;

	public Transform spear;

	public Transform dummy_spine;

	public Transform sub_hand;

	private Transform current_blade;

	private Transform current_blade2;

	private Transform general_blade;

	private Transform general_blade2;

	private Transform current_weapon;

	private float showdelay;

	private bool changestart;

	private Transform c_bowgun;

	private Transform c_spear;

	private float finishtime;

	private short changespd = 1;

	public int dock;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		if (dock < 2)
		{
			SetStageWeapon();
		}
	}

	public void SetStoryBlade()
	{
		blade = Resources.Load("wp_0") as GameObject;
		current_blade = (Transform)Object.Instantiate(blade.transform, dummy_spine.position, dummy_spine.rotation);
	}

	public void SetStageWeapon()
	{
		if (current_blade != null)
		{
			Object.Destroy(current_blade.gameObject);
		}
		if (current_blade2 != null)
		{
			Object.Destroy(current_blade2.gameObject);
		}
		int num = Crypto.Load_int_key("n34");
		int num2 = (int)((float)num * 0.01f);
		blade = Resources.Load("wp_" + num % 100) as GameObject;
		current_blade = (Transform)Object.Instantiate(blade.transform, dummy_spine.position, dummy_spine.rotation);
		current_blade.parent = dummy_spine;
		if (num2 == 1)
		{
			current_blade.position += dummy_spine.forward * 0.01f;
			current_blade2 = (Transform)Object.Instantiate(blade.transform, dummy_spine.position - dummy_spine.up * 0.03f + dummy_spine.forward * 0.04f + dummy_spine.right * 0.01f, dummy_spine.rotation);
			current_blade2.parent = dummy_spine;
			current_blade2.localRotation = Quaternion.Euler(260f, 180f, 0f);
		}
		if (dock == 1)
		{
			current_blade.localScale *= 0.8f;
			if (num2 == 1)
			{
				current_blade2.localScale *= 0.8f;
			}
		}
	}

	public void GeneralWeaponOn(int _generalkind, int _weaponindex)
	{
		current_blade.gameObject.active = false;
		if (current_blade2 != null)
		{
			current_blade2.gameObject.active = false;
		}
		if (general_blade == null)
		{
			blade = Resources.Load("wp_general" + _weaponindex) as GameObject;
			general_blade = (Transform)Object.Instantiate(blade.transform, mytransform.position, mytransform.rotation);
			general_blade.parent = mytransform;
			if (_generalkind == 1)
			{
				general_blade2 = (Transform)Object.Instantiate(blade.transform, sub_hand.position, sub_hand.rotation);
				general_blade2.parent = sub_hand;
			}
		}
		else
		{
			general_blade.gameObject.active = true;
			if (_generalkind == 1)
			{
				general_blade2.gameObject.active = true;
			}
		}
	}

	public void GeneralWeaponOff()
	{
		if (general_blade != null)
		{
			general_blade.gameObject.active = false;
		}
		if (general_blade2 != null)
		{
			general_blade2.gameObject.active = false;
		}
		current_blade.gameObject.active = true;
		if (current_blade2 != null)
		{
			current_blade2.gameObject.active = true;
		}
	}

	public void SetWeapon(int _meshkind, int _weapon_kind)
	{
		if (current_blade != null)
		{
			Object.Destroy(current_blade.gameObject);
		}
		if (current_blade2 != null)
		{
			Object.Destroy(current_blade2.gameObject);
		}
		blade = Resources.Load("wp_" + _meshkind) as GameObject;
		current_blade = (Transform)Object.Instantiate(blade.transform, dummy_spine.position, dummy_spine.rotation);
		current_blade.localScale *= 4.5f;
		current_blade.parent = dummy_spine;
		if (_weapon_kind == 1)
		{
			current_blade.position += dummy_spine.forward * 0.1f;
			current_blade2 = (Transform)Object.Instantiate(blade.transform, dummy_spine.position - dummy_spine.up * 0.18f + dummy_spine.forward * 0.26f + dummy_spine.right * 0.04f, dummy_spine.rotation);
			current_blade2.localScale *= 4.5f;
			current_blade2.parent = dummy_spine;
			current_blade2.localRotation = Quaternion.Euler(260f, 180f, 0f);
		}
	}

	public void SwitchWepon(int kind, short _changespd, float _finishtime, float _showdelay)
	{
		changespd = _changespd;
		finishtime = _finishtime;
		showdelay = _showdelay;
		switch (kind)
		{
		case 0:
			if (c_spear == null)
			{
				c_spear = (Transform)Object.Instantiate(spear, mytransform.position, mytransform.rotation);
			}
			else
			{
				c_spear.position = mytransform.position;
				c_spear.rotation = mytransform.rotation;
				c_spear.GetComponent<WeaponDrop>().DropCancel();
				c_spear.gameObject.active = true;
			}
			current_weapon = c_spear;
			break;
		case 1:
			if (c_bowgun == null)
			{
				c_bowgun = (Transform)Object.Instantiate(bowgun, mytransform.position, mytransform.rotation);
			}
			else
			{
				c_bowgun.position = mytransform.position;
				c_bowgun.rotation = mytransform.rotation;
				c_bowgun.GetComponent<WeaponDrop>().DropCancel();
				c_bowgun.gameObject.active = true;
			}
			current_weapon = c_bowgun;
			break;
		default:
			current_weapon = null;
			break;
		}
		if (current_weapon != null)
		{
			current_weapon.localScale = Vector3.zero;
			current_weapon.parent = mytransform;
			changestart = true;
		}
		current_blade.position = dummy_spine.position;
		current_blade.rotation = dummy_spine.rotation;
		current_blade.parent = dummy_spine;
		if (current_blade2 != null)
		{
			current_blade.position += dummy_spine.forward * 0.01f;
			current_blade2.position = dummy_spine.position - dummy_spine.up * 0.03f + dummy_spine.forward * 0.04f + dummy_spine.right * 0.01f;
			current_blade2.parent = dummy_spine;
			current_blade2.localRotation = Quaternion.Euler(260f, 180f, 0f);
		}
	}

	public void ReturnBlade()
	{
		if (current_weapon != null)
		{
			current_weapon.parent = null;
			current_weapon.GetComponent<WeaponDrop>().Drop(_destroy: false);
		}
		current_blade.position = mytransform.position;
		current_blade.rotation = mytransform.rotation;
		current_blade.parent = mytransform;
		if (current_blade2 != null)
		{
			current_blade2.position = sub_hand.position;
			current_blade2.rotation = sub_hand.rotation;
			current_blade2.parent = sub_hand;
		}
	}

	public void HideBlade()
	{
		current_blade.GetComponent<Renderer>().enabled = false;
		if (current_blade2 != null)
		{
			current_blade2.GetComponent<Renderer>().enabled = false;
		}
		if (general_blade != null)
		{
			general_blade.GetComponent<Renderer>().enabled = false;
		}
		if (general_blade2 != null)
		{
			general_blade2.GetComponent<Renderer>().enabled = false;
		}
	}

	public void ShowBlade()
	{
		current_blade.GetComponent<Renderer>().enabled = true;
		if (current_blade2 != null)
		{
			current_blade2.GetComponent<Renderer>().enabled = true;
		}
		if (general_blade != null)
		{
			general_blade.GetComponent<Renderer>().enabled = true;
		}
		if (general_blade2 != null)
		{
			general_blade2.GetComponent<Renderer>().enabled = true;
		}
	}

	private void Update()
	{
		if (changestart)
		{
			if (current_weapon.localScale.x >= 1f)
			{
				current_weapon.localScale = Vector3.one;
				changestart = false;
				showdelay = 0f;
			}
			else if (showdelay > 1f)
			{
				current_weapon.localScale += Vector3.one * changespd * Time.deltaTime;
			}
			else
			{
				showdelay += Time.deltaTime;
			}
		}
		else if (finishtime > 0f)
		{
			finishtime -= Time.deltaTime;
			if (finishtime < 0f)
			{
				ReturnBlade();
			}
		}
	}
}
