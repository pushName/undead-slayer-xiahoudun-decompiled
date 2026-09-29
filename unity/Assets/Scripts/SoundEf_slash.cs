using UnityEngine;

public class SoundEf_slash : MonoBehaviour
{
	private float delay;

	private AudioSource myaudio;

	public AudioClip thrust;

	public AudioClip smash;

	public AudioClip[] slash = new AudioClip[2];

	public AudioClip block;

	public AudioClip block_break;

	public AudioClip split;

	private int rndsound;

	private void Awake()
	{
		myaudio = base.GetComponent<AudioSource>();
	}

	public void SoundOn(int soundkind)
	{
		switch (soundkind)
		{
		case 4:
			myaudio.PlayOneShot(split);
			break;
		case 3:
			myaudio.clip = thrust;
			myaudio.Play();
			break;
		case 2:
			myaudio.PlayOneShot(smash);
			break;
		case 1:
			if (delay <= 0f)
			{
				rndsound = Random.Range(0, 2);
				myaudio.clip = slash[rndsound];
				myaudio.Play();
			}
			break;
		case 0:
			myaudio.clip = block;
			myaudio.Play();
			break;
		case -1:
			myaudio.PlayOneShot(block_break);
			break;
		}
		delay = 0.05f;
	}

	private void Update()
	{
		if (delay > 0f)
		{
			delay -= Time.deltaTime;
		}
	}
}
