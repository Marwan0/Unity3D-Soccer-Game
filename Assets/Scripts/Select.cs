using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Select : MonoBehaviour {
	public GameObject[] teams;
	int current;
	int i;
	int teamOpp;
	string name;
	Data data;
    public Text label;

	void Start () {
		data = GameObject.Find("Data").GetComponent<Data>();

		// start with argentina
		current=0;
		data.player=0;

		data.playerName = teams[0].name;
        label.text = data.playerName.ToUpper();

		for (i=1; i < teams.Length; i++) {
			teams[i].SetActive(false);
		}
	}

	void Update() {
#if UNITY_ANDROID || UNITY_WP8 || UNITY_WP_8_1
		if (Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene("1 menu");
#endif
	}
	
	public void Next () {
		current++;

		if (current == teams.Length) {
			current = teams.Length-1;
			data.player = current;
			data.playerName = teams[current].name;
			return;
		}
		for (i=0; i < teams.Length; i++) {
			teams[i].SetActive(false);
		}

		teams[current].SetActive(true);
		data.player = current;
		data.playerName = teams[current].name;

        label.text = data.playerName.ToUpper();
	}

	public void Prev () {
		current--;

		if (current == -1) {
			current = 0;
			data.player = current;
			data.playerName = teams[current].name;
			return;
		}
		for (i=0; i < teams.Length; i++) {
			teams[i].SetActive(false);
		}
		
		teams[current].SetActive(true);
		data.player = current;
		data.playerName = teams[current].name;

        label.text = data.playerName.ToUpper();
	}

	public void Play () {
		// Opponent random
		teamOpp = Random.Range(0,teams.Length);
		while (teamOpp == current) {
			teamOpp = Random.Range(0,teams.Length);
		}

		data.opp = teamOpp;
		data.oppName = teams[teamOpp].name;

		SceneManager.LoadScene("3 game");
	}

	public void Quit () {
		SceneManager.LoadScene("1 menu");
	}
}
