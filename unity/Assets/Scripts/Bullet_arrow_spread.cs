using UnityEngine;

public class Bullet_arrow_spread : MonoBehaviour
{
	private Transform mytransform;

	public float bullet_speed;

	public int spread_count;

	public int spread_angle;

	public Transform sub_arrow;

	private Transform sub1;

	private Transform sub2;

	private Quaternion a;

	private Quaternion b;

	private int damage;

	private void Awake()
	{
		mytransform = base.transform;
		damage = GetComponent<WeaponDamage>().damage;
	}

	private void OnEnable()
	{
		if (spread_count > 0)
		{
			for (int i = 0; i < spread_count; i++)
			{
				a = Quaternion.Euler(mytransform.eulerAngles.x, mytransform.eulerAngles.y + (float)(spread_angle * (i + 1)), 0f);
				sub1 = (Transform)Object.Instantiate(sub_arrow, mytransform.position, a);
				sub1.GetComponent<WeaponDamage>().PressDamage(damage);
				Object.Destroy(sub1.gameObject, 1f);
				b = Quaternion.Euler(mytransform.eulerAngles.x, mytransform.eulerAngles.y - (float)(spread_angle * (i + 1)), 0f);
				sub2 = (Transform)Object.Instantiate(sub_arrow, mytransform.position, b);
				sub2.GetComponent<WeaponDamage>().PressDamage(damage);
				Object.Destroy(sub2.gameObject, 1f);
			}
		}
	}

	private void Update()
	{
		if (mytransform.position.y > 0f)
		{
			mytransform.position += mytransform.forward * Time.deltaTime * bullet_speed;
		}
	}
}
