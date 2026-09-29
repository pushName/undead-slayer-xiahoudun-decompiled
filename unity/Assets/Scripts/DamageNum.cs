using UnityEngine;

public class DamageNum : MonoBehaviour
{
	private Transform mytransform;

	private float finishdelay = 1f;

	private TextMesh text_mesh;

	private Vector3 dir;

	private void Awake()
	{
		mytransform = base.transform;
		text_mesh = GetComponent<TextMesh>();
		base.gameObject.active = false;
	}

	public void TextOn(Vector3 _pos, short _damage, Vector3 _dir)
	{
		text_mesh.text = _damage.ToString();
		mytransform.position = _pos + Vector3.up * 0.2f;
		dir = _dir;
		base.gameObject.active = true;
	}

	private void Update()
	{
		finishdelay -= Time.deltaTime * 2f;
		if (finishdelay < 0f)
		{
			finishdelay = 1f;
			base.gameObject.active = false;
		}
		mytransform.position += (Vector3.up + dir) * finishdelay * Time.deltaTime * 0.3f;
	}
}
