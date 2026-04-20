using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 6;
    public float maxSpeed = 15;
    public float acceleration = 0.1f;
    public float horizontalSpeed = 3;
    public float jumpForce = 3;
    public float minX = -2;
    public float maxX = -0.1f;
    
    [SerializeField] bool isRunning;
    [SerializeField] bool isGrounded;
    [SerializeField] bool isDead;
    
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Animator _animator;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponentInChildren<Animator>();
        
        if (_playerInput != null && _playerInput.actions != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
        }
    }

    void Update()
    {
        if (isDead) return;

        if (playerSpeed < maxSpeed)
        {
            playerSpeed += acceleration * Time.deltaTime;
        }

        if(isRunning == false)
        {
            isRunning = true;
            StartCoroutine(addDistance());
        }
        
        transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.World);

        if (_animator != null)
        {
            _animator.SetFloat("Speed", playerSpeed);
        }

        if (_moveAction != null)
        {
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            if (moveInput.x < 0) transform.Translate(Vector3.left * horizontalSpeed * Time.deltaTime);
            else if (moveInput.x > 0) transform.Translate(Vector3.right * horizontalSpeed * Time.deltaTime);
        }

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        transform.position = clampedPosition;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);

        if(_jumpAction != null && _jumpAction.triggered && isGrounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (_animator != null) _animator.SetTrigger("Jump");
        }
    }

    IEnumerator addDistance()
    {
        yield return new WaitForSeconds(0.35f);
        MasterInfo.distanceRun += 1;
        isRunning = false;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Se bater num obstáculo, o movimento para
        if (collision.gameObject.CompareTag("Obstacle") && !isDead)
        {
            isDead = true;
            playerSpeed = 0; 
            isRunning = false;

            // Ativa física de queda (opcional, para realismo)
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(new Vector3(0, 5f, -5f), ForceMode.Impulse);
        }
    }
}