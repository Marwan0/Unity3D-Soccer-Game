using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour {
	public AudioClip musicBackground;
	public float musicBackgroundVol = 0.1f;
	//local
	AudioSource musicBrackground;

	void Awake() {
		GameObject[] music = GameObject.FindGameObjectsWithTag("Music");
		if (music.Length != 1 ) {
			for(int i = 1; i < music.Length; i++){
				Destroy(music[i]);
			}
		}

		// add audiosources
		musicBrackground = AddAudioSource(musicBackground, true, false, musicBackgroundVol);
		DontDestroyOnLoad(this);
	}

	// Use this for initialization
	void Start () {
		musicBrackground.Play();

	}

	AudioSource AddAudioSource(AudioClip clip, bool loop, bool awake, float vol) {
		AudioSource newAudio = gameObject.AddComponent<AudioSource>();
		newAudio.clip = clip;
		newAudio.loop = loop;
		newAudio.playOnAwake = awake;
		newAudio.volume = vol;
		return newAudio;
	}
}
