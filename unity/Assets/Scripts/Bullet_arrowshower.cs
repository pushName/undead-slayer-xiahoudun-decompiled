using UnityEngine;

public class Bullet_arrowshower : MonoBehaviour
{
	private Transform mytransform;

	private Transform cha1;

	public Transform sub_arrow;

	private Vector3 subarrowPos;

	private int damage;

	private Transform c_arrow;

	private void Awake()
	{
		mytransform = base.transform;
		cha1 = GameObject.FindWithTag("Player").transform;
		damage = GetComponent<WeaponDamage>().damage;
	}

	private void OnEnable()
	{
		mytransform.position = cha1.transform.position + new Vector3(Random.Range(-0.04f, 0.04f), 2f, Random.Range(-0.04f, 0.04f));
		for (int i = 0; i < 4; i++)
		{
			subarrowPos = Random.insideUnitSphere * 0.5f + mytransform.position;
			c_arrow = (Transform)Object.Instantiate(sub_arrow, subarrowPos, mytransform.rotation);
			c_arrow.GetComponent<WeaponDamage>().PressDamage(damage);
		}
	}

	private void Update()
	{
		if (mytransform.position.y > 0.2f)
		{
			mytransform.position -= Vector3.up * Time.deltaTime * 3f;
			return;
		}
		base.gameObject.active = false;
		mytransform.position = Vector3.one * 16f;
	}
}
