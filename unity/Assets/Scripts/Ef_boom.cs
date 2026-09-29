using UnityEngine;

public class Ef_boom : MonoBehaviour
{
	private Transform mytransform;

	private ParticleSystem myparticle;

	private ParticleSystem.EmissionModule myemission;

	private Material mymaterial;

	private Collider mycollider;

	public Texture2D[] pttex = new Texture2D[3];

	private void Awake()
	{
		mytransform = base.transform;
		myparticle = base.GetComponent<ParticleSystem>();
		myemission = myparticle.emission;
		mymaterial = base.GetComponent<Renderer>().sharedMaterial;
		mycollider = base.GetComponent<Collider>();
	}

	public void SetTex(int _index, Vector3 _pos, bool _collider)
	{
		_pos[1] = 0.02f;
		mytransform.position = _pos;
		myemission.enabled = false;
		myparticle.Clear();
		myemission.enabled = true;
		mymaterial.SetTexture("_MainTex", pttex[_index]);
		if (_collider)
		{
			mycollider.enabled = true;
		}
		Invoke("EmitStop", 0.5f);
	}

	private void EmitStop()
	{
		myemission.enabled = false;
		mycollider.enabled = false;
		mytransform.position = Vector3.up * 11f;
	}
}
