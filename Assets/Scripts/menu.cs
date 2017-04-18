using UnityEngine;
using System.Collections;
using Heyzap;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour {

	void Awake () {
		GameObject data = GameObject.Find("Data");
		Destroy(data);
	}

	void Start() {
		Application.targetFrameRate = 60;

		Time.timeScale = 1;
		AudioListener.volume = 1;

        HeyzapAds.Start("e75a46b975f6cb4a120ac6c5fe89e2c6", HeyzapAds.FLAG_DISABLE_AUTOMATIC_FETCHING);
        HZInterstitialAd.Fetch();

        if (Random.Range(0, 10) >= 2)
            HZVideoAd.Fetch();
    }

	void Update() {
#if UNITY_ANDROID || UNITY_WP8 || UNITY_WP_8_1
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit(); 
#endif
	}

	public void loadSelect() {
		SceneManager.LoadScene("2 select");
	}
}
