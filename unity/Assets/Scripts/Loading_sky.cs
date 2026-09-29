using UnityEngine;

public class Loading_sky : MonoBehaviour
{
	private void Awake()
	{
		int num = Crypto.Load_int_key("cur_stage_kind");
		base.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = Resources.Load("sky_ground" + num) as Texture;
		AudioListener.volume = PlayerPrefs.GetFloat("vol_master");
	}
}
