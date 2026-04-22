# Endless Runner Room101 - Nippa

Elementos do grupo:

Flávio Fernandes 33215
Micael Martins 34613

Versão do Unity: 6000.4.2f1

Descrição: 

Este é um jogo to tipo Endless Runner em 3D onde o jogador controla o porteiro de um Hotel que percorre infinitamente os corredores do edifício. O objetivo é percorrer a maior distância possível enquanto gere o tempo e a precisão dos movimentos para evitar colisões fatais. 
Durante a corrida, deve recolher colecionáveis como moedas e chaves espalhadas pelo corredor para aumentar a pontuação. Deve evitar obstáculos como malas, carrinho de malas, cestos de roupa e placa do "Chão molhado".
Para aumentar a dificuldade as portas dos quartos abrem e fecham aleatoriamente, exigindo reflexos rápidos para nao bater contra elas.

Jogabilidade: 

São usadas as teclas A e D para mover, respectivament para o lado esquerdo e lado direito. Para saltar utiliza-se a tecla de espaço (SPACEBAR). Durante o jogo, se for preciso fazer Pause tem as teclas ESC ou P.
Objetivo: Sobreviver o máximo de tempo possível. O jogo termina quando o jogador bate contra algum objeto ou uma porta aberta.
Dificuldade Incremental: À medida que o tempo passa, a velocidade de corrida do personagem aumenta, tornando o desvio dos obstáculos mais desafiante.


Como abrir o projeto:

1- Faz o Download ou Clone deste repositório para o teu computador.
2- Abre o Unity Hub.
3- Clica no botão "Add" e seleciona a pasta raiz onde descarregaste o projeto.
4- Certifica-te de que a versão do editor é a 6000.4.2f1.
5 - Após o projeto abrir, vai à pasta Assets/Scenes e abre a cena principal HotelCorridor1.
6 - Pressiona o botão Play no topo do editor para iniciar o jogo.

Assets Multimédia:

Modelos 3D, ícones e texturas:

Modelos 3D: Os objetos do jogo (personagem, portas, malas e moedas) utilizam o formato .fbx. Texturas e Ícones: As texturas do cenário e os ícones da interface (UI) estão em formato .png.
O .fbx foi escolhido por ser o padrão da indústria e do Unity, permitindo importar corretamente as malhas (meshes), as animações do personagem e a hierarquia de objetos de forma eficiente. O formato .png oferece o melhor equilíbrio entre qualidade visual e compressão, suportando transparências necessárias para os ícones das moedas e chaves na interface, sem perder detalhes importantes das texturas do hotel. 

Física: 

Uso de Rigidbody para o movimento e Colliders (como a tag "Obstacle") para a deteção de colisões e fim de jogo. (falta acabar)

Áudio: 

Inclui música de fundo contínua e efeitos sonoros para a recolha de itens e colisões.
Utilizamos o formato .wav para a música principal (durante a corrida) e efeitos sonoros (moedas e colisões), e o formato .mp3 para a música de seleção de de jogo.
O formato .wav é áudio "puro" (sem compressão). Usamos na música da corrida e nos sons das moedas/chaves/colisões porque o Unity processa estes ficheiros mais rapidamente, enquanto o jogador está a correr. Além disso, evita pequenos estalidos ou silêncios quando a música recomeça em loop. O formato .mp3 é comprimido, o que torna o ficheiro muito mais pequeno. Como no menu o jogo não precisa de processar tanta coisa ao mesmo tempo (como a corrida ou as portas a abrir), o computador tem "tempo" para ler este formato sem prejudicar a performance, ajudando a manter o tamanho total do projeto reduzido.



Interface (UI): Ecrã



