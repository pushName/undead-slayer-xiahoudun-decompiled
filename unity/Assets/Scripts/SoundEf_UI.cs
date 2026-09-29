using UnityEngine;

public class SoundEf_UI : MonoBehaviour
{
	public AudioClip[] s_click = new AudioClip[2];

	private void Start()
	{
		Object.DontDestroyOnLoad(base.transform.gameObject);
		base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
	}

	public void SoundOn(short s)
	{
		base.transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(s_click[s]);
	}

	public void SetBGM(float _vol)
	{
		base.GetComponent<AudioSource>().volume = _vol;
	}

	private void Update()
	{
	}
}
