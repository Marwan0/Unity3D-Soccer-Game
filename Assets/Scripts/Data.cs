using UnityEngine;
using System.Collections;

public class Data : MonoBehaviour {
	public int player;
	public int opp;
	public string playerName;
	public string oppName;
	public int playerScore;
	public int oppScore;
	
	void Awake () {
		DontDestroyOnLoad(this);
	}
}
