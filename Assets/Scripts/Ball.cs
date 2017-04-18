using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
	public Player player;
	public ForwardAI opponent;
	public bool wasKicked = false;
	public float baseHeight = 0.34f;
	public AudioClip clipPost;
	float nextBallTime = 0;
	float switchBall = .1f;
	AudioSource audioPost;
	
	void Awake() {
		GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		audioPost = AddAudioSource(clipPost, false, false, .8f);
	}

	public void Kicked(Vector3 force) {
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
		wasKicked = true;
	}

	public void Reset() {
		transform.position = new Vector3(0, baseHeight, 0);
		GetComponent<Rigidbody>().isKinematic = true;
		wasKicked = false;
		player.hasBall = false;
		opponent.hasBall = false;
		nextBallTime = 0;
		transform.position = new Vector3(0, baseHeight, 0);
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
			if (Time.time < nextBallTime) return;
			nextBallTime = Time.time + switchBall;

			GetComponent<Rigidbody>().isKinematic = true;
			transform.rotation = Quaternion.identity;
			player.hasBall = true;
			opponent.hasBall = false;
			wasKicked = false;
		} else if (other.tag == "OppForward") {
			GetComponent<Rigidbody>().isKinematic = true;
			opponent.hasBall = true;
			player.hasBall = false;
			wasKicked = false;
		} else if (other.tag == "OppGoalkeeper"){
			print("OppGoalkeeper");
		} else if (other.tag == "PlayerGoalkeeper") {
			print("PlayerGoalkeeper");
		} else if (other.tag == "Post") {
			audioPost.Play();
		} else if (other.tag == "Lateral") {
			// Should not happen
			Reset();
			player.Reset();
			opponent.Reset();
		}

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
