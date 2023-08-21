using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedCharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public float maxHealth = 100f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public GameObject interactPrompt;
    public Animator animator;
    
    private Rigidbody rb;
    private bool isGrounded;
    private bool canDoubleJump = true;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private float groundCheckRadius = 0.2f;
    private float currentHealth;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Sprinting
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprinting = true;
            moveSpeed = sprintSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
            moveSpeed = crouchSpeed;
        }

        // Crouching
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            animator.SetBool("IsCrouching", isCrouching);
            if (isCrouching)
            {
                moveSpeed = crouchSpeed;
                transform.localScale = new Vector3(1f, 0.5f, 1f);
            }
            else
            {
                moveSpeed = isSprinting ? sprintSpeed : 5f;
                transform.localScale = Vector3.one;
            }
        }

        // Movement
        movement *= moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);

        if (movement.magnitude > 0.1f)
        {
            Quaternion newRotation = Quaternion.LookRotation(movement, Vector3.up);
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, newRotation, 10f * Time.deltaTime));
        }

        if (isGrounded)
        {
            canDoubleJump = true;
        }

        // Jumping
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator.SetTrigger("Jump");
            }
            else if (canDoubleJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
                animator.SetTrigger("Jump");
                canDoubleJump = false;
            }
        }

        // Interacting
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        // Update Animator
        animator.SetFloat("Speed", movement.magnitude);
    }

    private void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetTrigger("Die");
        // Add death logic here
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 200, 20), "Health: " + currentHealth.ToString("F0") + " / " + maxHealth);
    }
}
