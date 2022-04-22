using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f, maxAirAcceleration = 1f;
	[SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
	[SerializeField, Range(0, 5)] private int maxAirJumps = 0;
	[SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;

    private Vector3 velocity, desiredVelocity;
	private Rigidbody body;
	private bool desiredJump;
	private int groundContactCount;
	private bool OnGround => groundContactCount > 0;
	private int jumpPhase;
	private float minGroundDotProduct;
	private Vector3 contactNormal;


	private void Awake()
	{
		body = GetComponent<Rigidbody>();
		OnValidate();
	}

    private void Update()
    {
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

		desiredVelocity =
			new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

		desiredJump = Input.GetButtonDown("Jump");
	}

	private void FixedUpdate()
	{
		UpdateState();
		AdjustVelocity();

		if(desiredJump)
		{
			desiredJump = false;
			Jump();
		}

		body.velocity = velocity;
		ClearState();
	}

	void UpdateState()
	{
		velocity = body.velocity;
		if(OnGround)
		{
			jumpPhase = 0;
			if(groundContactCount > 1)
			{
				contactNormal.Normalize();
			}
		}
		else
		{
			contactNormal = Vector3.up;
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		for(int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			if(normal.y >= minGroundDotProduct)
			{
				groundContactCount += 1;
				contactNormal += normal;
			}

		}
	}

	private void ClearState()
	{
		groundContactCount = 0;
		contactNormal = Vector3.zero;
	}

	private void OnValidate()
	{
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
	}

	private void Jump()
	{
		if(OnGround || jumpPhase < maxAirJumps)
		{
			jumpPhase += 1;
			float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
			float alignedSpeed = Vector3.Dot(velocity, contactNormal);
			if(alignedSpeed > 0f)
			{
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}
			velocity += contactNormal * jumpSpeed;
		}
	}

	private Vector3 ProjectOnContactPlane(Vector3 vector)
	{
		return vector - contactNormal * Vector3.Dot(vector, contactNormal);
	}

	private void AdjustVelocity()
	{
		Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
		Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

		float currentX = Vector3.Dot(velocity, xAxis);
		float currentZ = Vector3.Dot(velocity, zAxis);

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}
}
