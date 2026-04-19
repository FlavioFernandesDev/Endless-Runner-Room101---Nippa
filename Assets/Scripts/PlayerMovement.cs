using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 6;
    public float maxSpeed = 15;        // Limite máximo de velocidade (novo!)
    public float acceleration = 0.1f;  // Quanto aumenta por segundo (novo!)
    public float horizontalSpeed = 3;
    public float jumpForce = 3;        // Baixei para 3 para o teto!
    
    [SerializeField] bool isRunning;
    [SerializeField] bool isGrounded;
    [SerializeField] bool isDead;      // Evita o "problema zombie" (novo!)
    
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
        // Se bateu no obstáculo, o código pára aqui. Ele não corre nem salta mais.
        if (isDead) return;

        // Aceleração gradual contínua
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
            
            if (moveInput.x < 0) 
            {
                transform.Translate(Vector3.left * horizontalSpeed * Time.deltaTime);
            }
            else if (moveInput.x > 0) 
            {
                transform.Translate(Vector3.right * horizontalSpeed * Time.deltaTime);
            }
        }

        // O laser que deteta o chão
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
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isDead = true; // O jogador perdeu! Desliga o movimento.
            playerSpeed = 0; 
            isRunning = false;
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(new Vector3(0, 5f, -10f), ForceMode.Impulse);
        }
    }
}