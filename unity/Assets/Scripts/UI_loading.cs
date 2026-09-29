using UnityEngine;

public class UI_loading : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
	}
}
