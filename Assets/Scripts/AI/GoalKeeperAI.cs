using UnityEngine;
using System.Collections;

public class GoalKeeperAI : MonoBehaviour {
	public Animator animator;	
	public Transform ball;
    public float runningSpeed = 6;
    public float runningSnappyness = 3;        
    public float rotationSpeed = .9f;
    public float lookSpeed = 3;
    public float maxPosDown = -1.6f;
    // component caching
    public Transform mTransform;
    public Rigidbody mRigidbody;
    // local
    Vector3 lookingAt = Vector3.zero;
    float posX;
    float maxPosUp;

	void Awake() {
		animator = gameObject.GetComponentInChildren<Animator>();
        mTransform = transform;
        mRigidbody = GetComponent<Rigidbody>();
        posX = mTransform.position.x;  
        maxPosUp = Mathf.Abs(maxPosDown);     
    }
	
	void Update() {
        // animation
        if (mRigidbody.velocity == Vector3.zero) {
			animator.SetFloat("velocity", 0);
		} else {
			animator.SetFloat("velocity", mRigidbody.velocity.magnitude);
		}	
	}

	void OnAnimatorIK() {
		// look ball
		lookingAt = Vector3.Lerp(lookingAt, ball.position, Time.deltaTime * lookSpeed);
		animator.SetLookAtPosition(lookingAt);
		animator.SetLookAtWeight(0.5f);	}

	void FixedUpdate() {
        // rotation
        mTransform.rotation = Quaternion.Slerp(mTransform.rotation, Quaternion.LookRotation(ball.transform.position - mTransform.position), rotationSpeed * Time.deltaTime);

        // fix X position
        if (mTransform.position.x != posX)
            mTransform.position = new Vector3(posX, 0, mTransform.position.z);

        // movement
        float d = (Mathf.Clamp(ball.transform.position.z, maxPosDown, maxPosUp) - mTransform.position.z);
        Vector3 targetVelocity = new Vector3(0, 0, d * runningSpeed);
        Vector3 deltaVelocity = targetVelocity - mRigidbody.velocity;
        mRigidbody.AddForce(deltaVelocity * runningSnappyness, ForceMode.Acceleration);
	}	
}
