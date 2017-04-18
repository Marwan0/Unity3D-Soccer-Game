using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour
{
	Game game;

	void Awake() {
		game = GameObject.Find("Game").GetComponent<Game>();
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Ball") {
			game.Goal(this.tag);
		}
	}
}
