using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float playerSpeed = 6;
    public float maxSpeed = 15;
    public float acceleration = 0.1f;
    public float horizontalSpeed = 3;
    public float jumpForce = 5; 
    public float minX = -2;
    public float maxX = -0.1f;
    
    [Header("Estado do Jogador")]
    [SerializeField] bool isRunning;
    [SerializeField] bool isGrounded;
    [SerializeField] bool isDead;
    private float timerInicial = 0; // Trava para evitar o salto no início

    [Header("Referências (Arraste aqui no Inspector)")]
    public Animator _animator;        // Arraste o modelo do boneco (Filho)
    public GameObject painelBotoesMenu; // Arraste o painel com Start/Sair

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Rigidbody _rb;

    void Start()
{
    // Força o Animator a esquecer qualquer comando de salto acidental no início
    if (_animator != null)
    {
        _animator.ResetTrigger("Jump");
        _animator.Play("Running", 0, 0f); // Força a animação de corrida a começar do zero
    }
}

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        
        if (_playerInput != null && _playerInput.actions != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
        }
    }

    void Update()
    {
        if (isDead) return;

        // Aumenta o timer nos primeiros frames para evitar bugs de física no início
        if (timerInicial < 0.5f) 
        {
            timerInicial += Time.deltaTime;
        }

        // Aceleração progressiva
        if (playerSpeed < maxSpeed)
        {
            playerSpeed += acceleration * Time.deltaTime;
        }

        // Contador de distância (MasterInfo)
        if(isRunning == false)
        {
            isRunning = true;
            StartCoroutine(addDistance());
        }
        
        // Movimento para a frente
        transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.World);

        // Atualiza velocidade na animação
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

        // Limites do corredor (Clamp)
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        transform.position = clampedPosition;

        // Verificação de chão (Raycast)
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);

        // LÓGICA DO SALTO (Com trava de 0.5s para evitar o salto inicial)
        if(_jumpAction != null && _jumpAction.triggered && isGrounded && timerInicial >= 0.5f)
        {
            // Reset da velocidade vertical para consistência
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            
            // Aplica força de salto
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // Ativa animação
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

            // Ativa Menu de Game Over e liberta o rato
            if(painelBotoesMenu != null) painelBotoesMenu.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Física de impacto
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(new Vector3(0, 5f, -5f), ForceMode.Impulse);
        }
    }
}