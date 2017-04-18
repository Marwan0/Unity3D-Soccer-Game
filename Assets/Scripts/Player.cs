using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public Animator animator;
	public EasyJoystick joystick;
	public Transform ball;
	public bool hasBall;
	public bool gameEnded = false;
	public float walkingSpeed = 2;
	public float runningSpeed = 6;
	public float rotationSpeed = 5;
	public float kickIntensity = 30;
    public float snappiness = 5;       
	public AudioClip clipKick;
	public AudioClip clipSlide;
		// component caching
	public BoxCollider mCollider;
	public Transform mTransform;
	// local	
	Vector3 movementDirection;
	Vector3 joystickDisplacement;
	Ball ballComponent;
	AnimatorStateInfo animState;
	float nextKickTime = 0;
	float nextTackleTime;
	float speed;
	float slidingSpeed;
	float posX;
	bool startedSliding;
	bool idle = true;
	AudioSource audioKick;
	AudioSource audioSlide;
	
	static int HashLocomotion = Animator.StringToHash("Base.Locomotion");
	static int HashKick = Animator.StringToHash("Base.Kick");
	static int HashSlidingTackle = Animator.StringToHash("Base.SlidingTackle");
	static int HashCommemorate = Animator.StringToHash("Base.Commemoration");
	static int HashIdle = Animator.StringToHash("Base.Idle");

	// Subscribe to events
	void OnEnable() {
		EasyButton.On_ButtonPress += On_ButtonPress;
	}
	// Unsubscribe 
	void OnDisable(){
		EasyButton.On_ButtonPress -= On_ButtonPress;
	}
	void OnDestroy(){
		EasyButton.On_ButtonPress -= On_ButtonPress;
	}

	void Awake() {
        /* rigidbody.interpolation = RigidbodyInterpolation.Interpolate; */
		animator = gameObject.GetComponentInChildren<Animator>();
		mCollider = gameObject.GetComponent<BoxCollider>();
		ballComponent = ball.GetComponent<Ball>();
		mTransform = transform;
		posX = mTransform.position.x;
		audioKick = AddAudioSource(clipKick, false, false, .8f);
		audioSlide = AddAudioSource(clipSlide, false, false, .8f);
	}

	void Start() {
		StartCoroutine(ResetNow(1));
	}

	void Update() {
		speed = GetComponent<Rigidbody>().velocity.magnitude;
		HandleBall();
		Animate();
	}
	
	void FixedUpdate() {
		if (idle) return;		
		Movement();
	}
	
	public void Reset() {
		if (gameEnded) return;
		StartCoroutine(ResetNow(2.1f));
	}

	IEnumerator ResetNow(float waitTime) {
		idle = true;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		nextKickTime = 0;

		yield return new WaitForSeconds(waitTime);    	

		if (!gameEnded) {
			// move to start position
			mTransform.position = new Vector3(posX, 0, 0);
			// look forward (ball)
			var newRotation = Quaternion.LookRotation(ball.transform.position - mTransform.position).eulerAngles;
			newRotation.x = newRotation.z = 0;        
			mTransform.rotation = Quaternion.Euler(newRotation);
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;	

			yield return new WaitForSeconds(1);
			mTransform.position = new Vector3(posX, 0, 0);
			idle = false;
		}
	}	

	#region Controls and Physics

	public void On_ButtonPress (string buttonName) {
		if (hasBall) KickPressed();
		else SlidingTacklePressed();

	}
	
	void KickPressed() {
		if (Time.time < nextKickTime) return;

		animator.SetBool("kick", true);
		nextKickTime = Time.time + 0.5f;
	}
	
	void SlidingTacklePressed() {
		if (Time.time < nextTackleTime || speed < runningSpeed - 1) return;
		
		animator.SetBool("sliding_tackle", true);
		nextTackleTime = Time.time + 2;
		slidingSpeed = speed * 0.9f;
		startedSliding = true;
		audioSlide.Play();
	}	
	
	void HandleBall() {
		if (!hasBall) return;

		ball.position = transform.TransformPoint(new Vector3(0.25f, ballComponent.baseHeight, 0.9f));
		if (animState.fullPathHash == HashLocomotion || animState.fullPathHash == HashKick) {
			var axis = transform.TransformDirection(Vector3.right);
			var vel = speed * 90 * Time.deltaTime;
			ball.Rotate(axis, vel, Space.World);
		}
	}

	void Kick() {
		if (!hasBall) return;

		Vector3 direction = movementDirection == Vector3.zero ? mTransform.forward : movementDirection;
		ballComponent.Kicked(direction * kickIntensity);
		audioKick.Play();
		direction.y = 0;
		hasBall = false;
		animator.SetBool("kick", false);
	}

	void Movement() {
		joystickDisplacement = new Vector3(joystick.JoystickValue.x, 0, joystick.JoystickValue.y);
		
		movementDirection = joystickDisplacement.normalized;
		float newSpeed = 0;
		
		if (startedSliding || animState.fullPathHash == HashSlidingTackle) {
			movementDirection = transform.forward;
			newSpeed = slidingSpeed;
		} else if (animState.fullPathHash == HashLocomotion || animState.fullPathHash == HashIdle) {
			newSpeed = joystickDisplacement.sqrMagnitude < 0.5f ? walkingSpeed : runningSpeed;
		}
		
		var targetVelocity = movementDirection * newSpeed;
		var deltaVelocity = targetVelocity - GetComponent<Rigidbody>().velocity;
		if (GetComponent<Rigidbody>().useGravity)
			deltaVelocity.y = 0;
		GetComponent<Rigidbody>().AddForce(deltaVelocity * snappiness, ForceMode.Acceleration);
		
		//if (joystick.position != Vector2.zero) {
		if (joystick.JoystickValue != Vector2.zero) {
			
			var targetRotation = Quaternion.LookRotation(movementDirection);
			var deltaRot = Quaternion.Lerp(mTransform.rotation, targetRotation, rotationSpeed);
			GetComponent<Rigidbody>().MoveRotation(deltaRot);
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
	}
	
	#endregion
	#region Animation

	public void Commemorate() {
		idle = true;

		// animation
		animator.SetBool("commemorate", true);
		hasBall = false;

		// look camera		
		var newRotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position).eulerAngles;
		newRotation.x = newRotation.z = 0;
		transform.rotation = Quaternion.Euler(newRotation);

		StartCoroutine(ResetNow(2.2f));
	}

	void Animate() {
		animState = animator.GetCurrentAnimatorStateInfo(0);

		if (GetComponent<Rigidbody>().velocity == Vector3.zero) {
			animator.SetFloat("velocity", 0);
		} else {
			animator.SetFloat("velocity", GetComponent<Rigidbody>().velocity.magnitude);
		}
		
		if (animState.fullPathHash == HashLocomotion) {
			//
		} else if (animState.fullPathHash == HashKick) {
			if (animState.normalizedTime > 0.5f) {
				Kick(); 
				animator.SetBool("kick", false);
			}
		} else if (animState.fullPathHash == HashSlidingTackle) {
			// adjust player's position according to animation curve.
			animator.SetBool("sliding_tackle", false);
			startedSliding = false;
			var p = mTransform.position;
			var height = animator.GetFloat("tacke_height");
			p.y = height;
			mTransform.position = p;
			
			p = mCollider.center;
			p.y = 1 - height; // height is negative.
			mCollider.center = p;
		} else if (animState.fullPathHash == HashCommemorate) {
			animator.SetBool("commemorate", false);
		}		 
	}
	
	#endregion

	AudioSource AddAudioSource(AudioClip clip, bool loop, bool awake, float vol) {
		AudioSource newAudio = gameObject.AddComponent<AudioSource>();
		newAudio.clip = clip;
		newAudio.loop = loop;
		newAudio.playOnAwake = awake;
		newAudio.volume = vol;
		return newAudio;
	}
}
