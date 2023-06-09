using System.Collections;

using UnityEngine;
using UnityEngine.UIElements;

namespace WipeOut
{
	[RequireComponent(typeof(CharacterController))]
	public class CharacterMover : MonoBehaviour
	{
		private CharacterController characterController;
		public float movementSpeed = 10.0f;
		public float jumpVelocity = 10.0f;
		private Vector2 moveInput = new Vector2();
		private bool jumpInput = new bool();
		public Vector3 velocity = new Vector3();
		public bool isGrounded;
		public Transform cam = null;
		public Animator animator;
		public Vector3 hitDirection;
		private Ragdoll ragdoll = null;
		public bool canMove = false;
		public BlackHole blackHole;

		public Vector3 repsawnPoint;

		void OnControllerColliderHit(ControllerColliderHit hit)
		{
			hitDirection = hit.point - transform.position;
		}

		// Start is called before the first frame update
		void Awake()
		{
			characterController = GetComponent<CharacterController>();
			animator = GetComponentInChildren<Animator>();
			cam = Camera.main.transform;
			ragdoll = GetComponent<Ragdoll>();
		}

		void Update()
		{
			moveInput.x = Input.GetAxis("Horizontal");
			moveInput.y = Input.GetAxis("Vertical");

			if(!canMove)
			{
				moveInput = new Vector2(0, 0);
			}

			jumpInput = Input.GetButton("Jump");

			animator.SetFloat("Left", moveInput.x, 0.1f, Time.deltaTime);
			animator.SetFloat("Forwards", moveInput.y, 0.1f, Time.deltaTime);
			animator.SetBool("Jump", !isGrounded);
		}

		// Update is called once per frame
		private void FixedUpdate()
		{
			Vector3 delta;
			// find the horizontal unit vector facing forward from the camera
			Vector3 camForward = cam.forward;
			camForward.y = 0;
			camForward.Normalize();

			// use our camera's right vector, which is always horizontal
			Vector3 camRight = cam.right;
			delta = (moveInput.x * cam.right + moveInput.y * camForward) * movementSpeed;

			// Disables turning the player in ragdoll
			if(!ragdoll.ragdollOn && canMove)
				transform.forward = camForward;

			if(isGrounded || moveInput.x != 0 || moveInput.y != 0)
			{
				velocity.x = delta.x;
				velocity.z = delta.z;
			}

			if(jumpInput && isGrounded)
				velocity.y = jumpVelocity;

			if(isGrounded && velocity.y < 0)
				velocity.y = 0;

			if(blackHole)
			{
				if(!blackHole.hasMoved && !ragdoll.ragdollOn)
					velocity += Physics.gravity * Time.fixedDeltaTime;
			}
			else
			{
				velocity += Physics.gravity * Time.fixedDeltaTime;
			}

			if(!isGrounded)
				hitDirection = Vector3.zero;

			// slide objects off surfaces they're hanging on to
			if(moveInput.x == 0 && moveInput.y == 0)
			{
				Vector3 horizontalHitDirection = hitDirection;
				horizontalHitDirection.y = 0;
				float displacement = horizontalHitDirection.magnitude;
				if(displacement > 0)
					velocity -= 0.2f * horizontalHitDirection / displacement;
			}

			if(characterController.enabled)
				characterController.Move(velocity * Time.fixedDeltaTime);

			isGrounded = characterController.isGrounded;
		}
	}
}