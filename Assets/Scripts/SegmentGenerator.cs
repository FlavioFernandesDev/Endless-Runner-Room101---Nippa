using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SegmentGenerator : MonoBehaviour
{
    public GameObject[] segment;
    public Transform player; // Precisamos de saber onde está o jogador!

    [SerializeField] float zPos = 30f;
    [SerializeField] int segmentNum;
    
    // A nossa "Fila" que vai guardar a memória dos corredores criados
    private List<GameObject> activeSegments = new List<GameObject>();

    void Start()
    {
        // Se te esqueceres de colocar o Player no Inspector, o código procura-o sozinho
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // Se o boneco estiver a chegar perto do limite (a menos de 60 metros da ponta), cria mais chão!
        // Assim ele pode correr à velocidade da luz que nunca mais cai no vazio.
        if (player != null && player.position.z + 60 > zPos)
        {
            SpawnSegment();
        }
    }

    void SpawnSegment()
    {
        // Escolhe um corredor à sorte
        segmentNum = Random.Range(0, segment.Length);
        
        // Cria o corredor e guarda-o na variável 'newSegment'
        GameObject newSegment = Instantiate(segment[segmentNum], new Vector3(0, 0, zPos), Quaternion.identity);
        
        // Adiciona este corredor novo à nossa "Fila"
        activeSegments.Add(newSegment);
        
        // Prepara a distância para o próximo
        zPos += 30; 
        
       
        // Se a nossa fila tiver mais de 4 corredores, destruímos o mais antigo (o número 0)
        if (activeSegments.Count > 4)
        {
            Destroy(activeSegments[0]);      // Apaga o corredor da cena
            activeSegments.RemoveAt(0);      // Tira o nome dele da nossa lista
        }
    }
}