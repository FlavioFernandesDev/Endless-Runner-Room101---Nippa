using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 6;
    public float maxSpeed = 15;
    public float acceleration = 0.1f;
    public float horizontalSpeed = 3;
    public float jumpForce = 5; // Recomendo 5 ou 6 para começar
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

        // Aceleração progressiva
        if (playerSpeed < maxSpeed)
        {
            playerSpeed += acceleration * Time.deltaTime;
        }

        // Lógica de distância (MasterInfo)
        if(isRunning == false)
        {
            isRunning = true;
            StartCoroutine(addDistance());
        }
        
        // Movimento para a frente
        transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.World);

        if (_animator != null)
        {
            _animator.SetFloat("Speed", playerSpeed);
        }

        // Movimento lateral
        if (_moveAction != null)
        {
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            if (moveInput.x < 0) transform.Translate(Vector3.left * horizontalSpeed * Time.deltaTime);
            else if (moveInput.x > 0) transform.Translate(Vector3.right * horizontalSpeed * Time.deltaTime);
        }

        // Limites laterais (Clamp)
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        transform.position = clampedPosition;

        // Verificação de chão
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);

        // --- AQUI ESTÁ A PARTE DO SALTO CORRIGIDA ---
        if(_jumpAction != null && _jumpAction.triggered && isGrounded)
        {
            // Resetamos a velocidade vertical para o salto ser sempre igual,
            // não importa a velocidade ou se o boneco estava a descer um degrau.
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

            // Aplica a força de salto
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // Ativa a animação no Animator
            if (_animator != null) 
            {
                _animator.SetTrigger("Jump");
            }
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
        if (collision.gameObject.CompareTag("Obstacle") && !isDead)
        {
            isDead = true;
            playerSpeed = 0; 
            isRunning = false;

            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(new Vector3(0, 5f, -5f), ForceMode.Impulse);
        }
    }
}