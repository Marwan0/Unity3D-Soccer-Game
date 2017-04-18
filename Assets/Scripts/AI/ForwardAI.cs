using UnityEngine;
using System.Collections;

public class ForwardAI : MonoBehaviour {
	public Animator animator;
	public Transform target;
	public Transform playerForward;
	public Transform ball;
	public Transform defendingGoal;
	public Transform goal1;
	public Transform goal2;
	public Transform kickArea;
	public bool hasBall;
	public bool gameEnded = false;
	public float runningSpeed = 4;
    public float snappiness = 3;
	public float rotationSpeed  = 3;
	public float kickIntensity = 30;
    public float lookSpeed = 4;
	public AudioClip clipKick;
	public AudioClip clipSlide;
	// component caching
	public BoxCollider mCollider;
	public Transform mTransform;
	public Rigidbody mRigidbody;
	// local
	Vector3 lookingAt = Vector3.zero;
	Vector3 movementDirection = Vector3.zero;
	Vector3 direction = Vector3.zero;
	ForwardState state;
	AnimatorStateInfo animState;
	Ball ballComponent;
	float rigidbodySpeed;
	float nextKickTime = 0;
	//bool startedSliding;
	float posX;
	bool goToKickArea = true;
	RaycastHit hit;
	AudioSource audioKick;
	//AudioSource audioSlide;

	static int HashLocomotion = Animator.StringToHash("Base.Locomotion");
	static int HashKick = Animator.StringToHash("Base.Kick");
	static int HashSlidingTackle = Animator.StringToHash("Base.SlidingTackle");
	static int HashCommemorate = Animator.StringToHash("Base.Commemoration");

	void Awake() {
		animator = gameObject.GetComponentInChildren<Animator>();
		mCollider = gameObject.GetComponent<BoxCollider>();
		ballComponent = ball.GetComponent<Ball>();
		mRigidbody = GetComponent<Rigidbody>();
		mTransform = transform;
		posX = mTransform.position.x;
		audioKick = AddAudioSource(clipKick, false, false, .8f);
	}

	void Start() {
		target = mTransform;
		StartCoroutine(ResetNow(1));
	}

	void Update() {
		if (gameEnded) {
			mRigidbody.velocity = Vector3.zero;
			state = ForwardState.IDLE;
		}
		rigidbodySpeed = mRigidbody.velocity.magnitude;
		direction = target.position - mTransform.position;
		direction.y = 0;
		direction.Normalize();

		Animate();

		if (state == ForwardState.LOOK) {
			// rotation
			movementDirection = Vector3.Lerp(mTransform.forward, direction, Time.deltaTime * rotationSpeed);
			var targetRotation = Quaternion.LookRotation(movementDirection);
			var deltaRot = Quaternion.Lerp(mTransform.rotation, targetRotation, rotationSpeed);
			mRigidbody.MoveRotation(deltaRot);
			mRigidbody.angularVelocity = Vector3.zero;
			mRigidbody.velocity = Vector3.zero;
			// look
			lookingAt = Vector3.Lerp (lookingAt, ball.position, Time.deltaTime * rotationSpeed);
			animator.SetLookAtPosition(lookingAt);
			animator.SetLookAtWeight(0.5f);
		} else if (state == ForwardState.IDLE) {
			mRigidbody.velocity = Vector3.zero;
			mRigidbody.angularVelocity = Vector3.zero;
		}
		else {
			if (hasBall) {

				if (goToKickArea) {
					target = kickArea; // goal;
				}
				else {
					mRigidbody.velocity = Vector3.zero; // stop ?
					int range = Random.Range(1, 3);
					if (range == 1)
						target = goal1;
					else
						target = goal2;

					if (Physics.Raycast(transform.position, transform.forward, out hit)) {
						if (hit.transform.name == "GoalColliderLeft1" ||
						    hit.transform.name == "GoalColliderLeft2" ||
						    hit.transform.name == "GoalKeeperPlayer"  ) {

							TryKick();
						}
					}

				}

				HandleBall();
			}
			else {
				target = ball;
			}
		}
	}

	void FixedUpdate() {
		if (state == ForwardState.LOOK) {
			//
		} else if (state == ForwardState.IDLE) {
			//
		}
		else {
			Movement();
		}
	}

	public void Reset() {
		if (gameEnded) return;
		StartCoroutine(ResetNow(2.1f));
	}

	IEnumerator ResetNow(float waitTime) {
		state = ForwardState.IDLE;
		nextKickTime = 0;

		yield return new WaitForSeconds(waitTime);

		if (!gameEnded) {
			// move to start position
			mTransform.position = new Vector3(posX, 0, 0);
			// look forward (ball)
			var newRotation = Quaternion.LookRotation(ball.transform.position - mTransform.position).eulerAngles;
			newRotation.x = newRotation.z = 0;
			mTransform.rotation = Quaternion.Euler(newRotation);
			mRigidbody.velocity = Vector3.zero;
			mRigidbody.angularVelocity = Vector3.zero;

			yield return new WaitForSeconds(1);
			// move to start position - bug
			mTransform.position = new Vector3(posX, 0, 0);
			state = ForwardState.DEFEND;
			goToKickArea = true;
		}
    }

	#region Controls and Physicvs

	void HandleBall() {
		ball.position = mTransform.TransformPoint(new Vector3(0.25f, ballComponent.baseHeight, 0.9f));

		if (animState.fullPathHash == HashLocomotion || animState.fullPathHash == HashKick) {
			var axis = mTransform.TransformDirection(Vector3.right);
			var vel = rigidbodySpeed * 60;
			ball.Rotate(axis, vel * Time.deltaTime, Space.World);
		}
	}

	void TryKick() {
		if (Time.time < nextKickTime) return;

		animator.SetBool("kick", true);
		nextKickTime = Time.time + .8f;
	}

	void Kick() {
		if (!hasBall) return;
		audioKick.Play();

		var d = target.position - mTransform.position;

		d.Normalize();
		ballComponent.Kicked(d * kickIntensity);
		hasBall = false;
		goToKickArea = true;
	}

	void Movement() {
		var targetVelocity = direction * runningSpeed;
        var deltaVelocity = targetVelocity - mRigidbody.velocity;
        deltaVelocity.y = 0;
        mRigidbody.AddForce (deltaVelocity * snappiness, ForceMode.Acceleration);

		// roration
		if (direction != Vector3.zero) {
			var targetRotation = Quaternion.LookRotation(direction);
			var deltaRot = Quaternion.Lerp(mTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
			mRigidbody.MoveRotation(deltaRot);
			mRigidbody.angularVelocity = Vector3.zero;
		}
	}

	#endregion
	#region Animation

	public void Commemorate() {
		state = ForwardState.IDLE;

		// animation
		animator.SetBool("kick", false);
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

		if (mRigidbody.velocity == Vector3.zero) {
			animator.SetFloat("velocity", 0);
		} else {
			animator.SetFloat("velocity", mRigidbody.velocity.magnitude);
		}

		if (animState.fullPathHash == HashKick && animState.normalizedTime > 0.5f) {
			Kick();
			animator.SetBool("kick", false);
		} else if (animState.fullPathHash == HashSlidingTackle) {
			// adjust player's position according to animation curve.
			animator.SetBool("sliding_tackle", false);
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

	void OnTriggerEnter(Collider other) {
		if (other.tag == "KickArea" && hasBall) {
			goToKickArea = false;
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

enum ForwardState {
	LOOK,
	DEFEND,
	COMMEMORATE,
	IDLE,
	ATTACK,
	AIM,
	KICK,
}
