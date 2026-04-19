using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDoor : MonoBehaviour
{
    [Header("Configurações de Probabilidade")]
    [Range(0, 100)]
    public float chanceToOpen = 40f;
    
    [Header("Configurações de Animação")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float distanceToTrigger = 20f; // Distância para a porta "acordar"

    [Header("Referências (Opcional)")]
    public AudioSource audioSource;

    private bool jaTentouAbrir = false;
    private Transform player;
    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        // Guarda a rotação inicial (fechada)
        closedRot = transform.localRotation;
        
        // Calcula a rotação final (aberta) somando o ângulo no eixo Y
        openRot = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y + openAngle, transform.localEulerAngles.z);

        // Procura o jogador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        Debug.Log("Porta " + gameObject.name + " pronta e à espera do jogador.");
    }

    void Update()
    {
        if (player == null) return;

        // Calcula a distância entre esta porta e o jogador
        float dist = Vector3.Distance(transform.position, player.position);

        // Se o jogador chegar perto e a porta ainda não tiver sido testada
        if (!jaTentouAbrir && dist < distanceToTrigger)
        {
            jaTentouAbrir = true;
            TentarAbrirPorta();
        }
    }

    void TentarAbrirPorta()
    {
        float sorteio = Random.Range(0f, 100f);

        if (sorteio < chanceToOpen)
        {
            StartCoroutine(AnimarPorta());
            
            // Toca o som apenas se ele estiver atribuído (evita o erro vermelho)
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    IEnumerator AnimarPorta()
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Slerp(closedRot, openRot, t);
            yield return null;
        }
    }
}
