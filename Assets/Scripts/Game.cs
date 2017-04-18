using UnityEngine;
using System.Collections;
using Heyzap;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
	public Player player;
	public ForwardAI opponent;
	public GoalKeeperAI gkPlayer;
	public GoalKeeperAI gkOpponent;
	public Ball ball;
	public AudioClip[] clipBackground;
	public float clipBackgroundVol = 0.7f;
	public AudioClip[] clipGoal;
	public AudioClip clipGoalOpp;
	public float clipGoalVol = 0.7f;
	public AudioClip clipWhistle;
	public AudioClip clipWhistle2;
	public string[] teams;
	public Text playerName;
	public Text oppName;
	public Text playerScore;
	public Text oppScore;
	public Text timer;
	public float timeLeft = 90;
    public GameObject rematchBtn;
    public GameObject menuBtn;
	public Transform camera;
	//local
	bool goal = false;
	bool gameEnded = false;
    AudioSource audioBrackground;
    AudioSource audioGoal;
	AudioSource audioWhistle;
	AudioSource audioWhistle2;
	string pName;
	Transform trans;
	int teamOpp;
	int pScore = 0;
	int oScore = 0;
	bool startTimer = false;
	int x = 0;
	GameObject data;

	void Awake() {
		GameObject music = GameObject.Find("Music");
		Destroy(music);
		data = GameObject.Find("Data");

        menuBtn.SetActive(false);
        rematchBtn.SetActive(false);

		pName = data.GetComponent<Data>().playerName;
        playerName.text = pName.ToUpper();
		playerName.text = playerName.text.Substring(0, 3);

		// Player 
		// body
		trans = player.transform.Find("ForwardModel/v_body");
		trans.GetComponent<Renderer>().material = Resources.Load("soccer_" + pName) as Material;
		// head
		trans = player.transform.Find("ForwardModel/ChamferBox002");
		trans.GetComponent<Renderer>().material = Resources.Load("soccer_" + pName) as Material;

		// Goalkeeper player
		if (pName == "germany" || pName == "england") {
			// head
			trans = gkPlayer.transform.Find("goalkeeperPlayerModel/ChamferBox002");
			trans.GetComponent<Renderer>().material = Resources.Load("soccer_germany") as Material;
		}

		// Opponent
		teamOpp = data.GetComponent<Data>().opp;
        oppName.text = teams[teamOpp].ToString().ToUpper();
		oppName.text = oppName.text.Substring(0, 3);

		// body
		trans = opponent.transform.Find("AIForwardModel/v_body");
		trans.GetComponent<Renderer>().material = Resources.Load("soccer_" + teams[teamOpp].ToString()) as Material;

		// head
		trans = opponent.transform.Find("AIForwardModel/ChamferBox002");
		trans.GetComponent<Renderer>().material = Resources.Load("soccer_" + teams[teamOpp].ToString()) as Material;

		// Goalkeeper opponent
		if (teams[teamOpp].ToString() == "germany" || teams[teamOpp].ToString() == "england") {
			// head
			trans = gkOpponent.transform.Find("goalkeeper_blond/ChamferBox4");
			trans.GetComponent<Renderer>().material = Resources.Load("soccer_germany") as Material;
		}

		// add audiosources
		audioBrackground = AddAudioSource(clipBackground[Random.Range(0, clipBackground.Length)], true, false, clipBackgroundVol);
		audioGoal = AddAudioSource(null, false, false, clipGoalVol);
		audioWhistle = AddAudioSource(clipWhistle, false, false, clipGoalVol);
		audioWhistle2 = AddAudioSource(clipWhistle2, false, false, clipGoalVol);
	}
	
	void Start() {
		timer.text = timeLeft.ToString();
		oppScore.text = oScore.ToString();
		playerScore.text = pScore.ToString();
		audioBrackground.Play(); // stadium sound

		PlayerPrefs.SetInt("PlayedOnce", 1);
		StartCoroutine(PlayWhistle(2));

		cameraStart();
	}

	// Update is called once per frame
	void Update () {
#if UNITY_ANDROID || UNITY_WP8 || UNITY_WP_8_1
		if (Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene("1 menu");

#endif

		if (startTimer) {
			timeLeft -= Time.deltaTime * 1.2f;
			
			if(timeLeft <= 0.9f) {
				// end
				startTimer = false;
				if (gameEnded) return;
				StartCoroutine(End(3));
			}
			else {
				x = (int)timeLeft;
				timer.text = x.ToString();
			}
		}
	}

	public void Goal(string side) {
		if (goal || gameEnded) return;

		goal = true;
		ball.wasKicked = false;
		ball.GetComponent<Rigidbody>().isKinematic = true;

		// Opponent goal
		if (side == "GoalLeft") {
			// sound
			audioGoal.clip = clipGoalOpp;
			audioGoal.Play();
			// anim
			opponent.Commemorate();
			player.Reset();
			oScore++;
			oppScore.text = oScore.ToString();

		// Player goal
		} else { // "GoalRight"
			// sound
			audioGoal.clip = clipGoal[Random.Range(0, clipGoal.Length)];
			audioGoal.Play();
			// anim
			player.Commemorate();
			opponent.Reset();
			pScore++;
			playerScore.text = pScore.ToString();
		}

		StartCoroutine(Reset(2));
	}

	IEnumerator Reset(float waitTime) {
		yield return new WaitForSeconds(waitTime);
		if (!gameEnded) {
			ball.Reset();
			goal = false;
			StartCoroutine(PlayWhistle(1));
		}
	}

	IEnumerator PlayWhistle(float waitTime) {
		yield return new WaitForSeconds(waitTime);
		if (!gameEnded) {
			audioWhistle.Play();
			startTimer = true;
		}
	}

	IEnumerator AdCache(float waitTime) {
		yield return new WaitForSeconds(waitTime);
        HZInterstitialAd.Fetch();
        if (Random.Range(0, 10) >= 2)
            HZVideoAd.Fetch();
	}

	IEnumerator End(float waitTime) {
		gameEnded = true;
		audioWhistle2.Play();

		player.gameEnded = true;
		opponent.gameEnded = true;

		data.GetComponent<Data>().playerScore = pScore;
		data.GetComponent<Data>().oppScore = oScore;

		yield return new WaitForSeconds(waitTime);

		//
		// Show AD
		// 
		if (Random.Range(0, 10) >= 2) {
			if (HZVideoAd.IsAvailable())
				HZVideoAd.Show();
			else
				HZInterstitialAd.Show();
		} else {
			HZInterstitialAd.Show();
		}

		// Show Menu
        menuBtn.SetActive(true);
        rematchBtn.SetActive(true);

		//
		// AD cache
		//
		StartCoroutine(AdCache(0.1F));
	}

	public void Menu () {
		SceneManager.LoadScene("1 menu");
	}
	
	public void Rematch () {
		data.GetComponent<Data>().playerScore = 0;
		data.GetComponent<Data>().oppScore = 0;
		SceneManager.LoadScene("3 game");
	}


	void cameraStart () {
		Vector3 pos = camera.position;

#if UNITY_IPHONE
		switch (UnityEngine.iOS.Device.generation) {
		case UnityEngine.iOS.DeviceGeneration.iPad2Gen:
		case UnityEngine.iOS.DeviceGeneration.iPad3Gen:
		case UnityEngine.iOS.DeviceGeneration.iPad4Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadAir1:
		case UnityEngine.iOS.DeviceGeneration.iPadAir2:
		case UnityEngine.iOS.DeviceGeneration.iPadMini1Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadMini2Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadMini3Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadMini4Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadPro1Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadPro10Inch1Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadUnknown:
			pos.y = 15f;
			pos.z = -13f;
			break;
			// 3.5 inch
		case UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen:
		case UnityEngine.iOS.DeviceGeneration.iPhone4:
		case UnityEngine.iOS.DeviceGeneration.iPhone4S:
			pos.y = 13.5f;
			pos.z = -12.5f;
			break;
			// 4+ inch
		case UnityEngine.iOS.DeviceGeneration.iPodTouch5Gen:
		case UnityEngine.iOS.DeviceGeneration.iPhone5:
		case UnityEngine.iOS.DeviceGeneration.iPhone5C:
		case UnityEngine.iOS.DeviceGeneration.iPhone5S:
		case UnityEngine.iOS.DeviceGeneration.iPhone6:
		case UnityEngine.iOS.DeviceGeneration.iPhone6Plus:
		case UnityEngine.iOS.DeviceGeneration.iPhone6S:
		case UnityEngine.iOS.DeviceGeneration.iPhone6SPlus:
	    case UnityEngine.iOS.DeviceGeneration.iPhoneSE1Gen:
		case UnityEngine.iOS.DeviceGeneration.iPhoneUnknown:
			pos.y = 12.5f;
			pos.z = -11f;
			break;
		default:
			pos.y = 13.5f;
			pos.z = -12f;	
			break;
		}
	
#endif		
#if UNITY_ANDROID || UNITY_EDITOR
		pos.y = 13f;
		pos.z = -12f;	
#endif

        camera.position = new Vector3 (pos.x, pos.y, pos.z);
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
