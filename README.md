# Endless Runner Room101 — Nippa

Projeto prático da UC **Tecnologias Multimédia** (ano letivo 2025/2026). Jogo 3D do tipo *endless runner* desenvolvido em Unity, com o tema de um porteiro de hotel (Nippa) que percorre infinitamente os corredores do edifício.

---

## 1. Grupo

| Nome | Número |
|---|---|
| Flávio Fernandes | 33215 |
| Micael Martins | 34613 |

---

## 2. Versão do Unity

**Versão usada: `6000.4.2f1`** (a versão pedida no enunciado é a `6000.3.9f1`).

**Justificação:** Iniciámos o projeto numa versão anterior, mas numa fase inicial sofremos uma perda de trabalho no repositório quando gravámos o projeto sem que o Unity tivesse persistido a cena. Tivemos de recomeçar e, na dúvida de que o problema pudesse ser do motor, atualizámos para a versão LTS mais recente disponível no Unity Hub na altura (`6000.4.2f1`). Só mais tarde percebemos que a causa real era o facto de o Unity **não guardar a cena automaticamente** — a partir desse momento passámos a gravar manualmente (`Ctrl+S`) antes de cada commit. A versão usada é retrocompatível com a pedida, pelo que o projeto abre sem alterações numa versão mais recente da série 6000.

---

## 3. Descrição do jogo

O jogador controla o **Nippa**, um porteiro de hotel que corre por corredores gerados proceduralmente. O objetivo é sobreviver o máximo de tempo possível, percorrer a maior distância e recolher colecionáveis enquanto gere os reflexos para desviar de obstáculos.

### Mundo e elementos

- **Corredores procedurais:** o mundo é composto por *segmentos* (prefabs) reutilizados através de um *pool*, que vão sendo reciclados à medida que o jogador avança.
- **Colecionáveis:**
  - **Moedas** (`Coin`) — pontuação.
  - **Chaves** (`Key`) — permitem sobreviver a uma única colisão com uma porta aberta (consumem a chave).
- **Obstáculos:** malas, carrinho de bagagem, cesto de roupa, placa de "chão molhado" — todos com a tag `Obstacle`; colisão é fatal.
- **Portas aleatórias** (`RandomDoor`): as portas dos quartos abrem e fecham aleatoriamente. Colidir com uma porta aberta é fatal a não ser que o jogador tenha uma chave.
- **Dois níveis / estilos:**
  - `HotelCorridor1` — corredor padrão do hotel.
  - `HotelHaunted` — variante assombrada, com estilo visual diferente aplicado dinamicamente pelo `HauntedLevelStyler`.

### Progressão e pontuação

- A **velocidade aumenta ao longo do tempo** (`playerSpeed += acceleration * Time.deltaTime` até ao `maxSpeed`), tornando o jogo progressivamente mais difícil.
- Contadores visíveis no HUD: moedas, chaves, distância, nível.
- Totais persistentes guardados com `PlayerPrefs` (`COINSAVE`, `KEYSAVE`, `DISTANCESAVE`) e exibidos no ecrã de **Achievements**.

---

## 4. Jogabilidade

### Controlos

| Ação | Tecla |
|---|---|
| Mover para a esquerda / direita (troca de *lane*) | `A` / `D` (ou setas ← / →) |
| Saltar | `Espaço` |
| Pausar / retomar | `ESC` ou `P` |
| Navegar nos menus | rato |

### Regras

- 3 *lanes* (esquerda, centro, direita).
- Colisão com qualquer objeto `Obstacle` termina a corrida (Game Over), exceto:
  - Porta aberta com chave disponível → consome a chave e continua com penalização temporária de velocidade.
- Ao morrer: câmara toca animação de colisão (`CollisionCam`), personagem faz `Stumble Backwards`, transição `FadeOut` e volta ao **Stage Select** após 1.5 s.
- O jogo pode ser reiniciado a qualquer momento voltando ao Stage Select e escolhendo de novo o nível.

### Fluxo de cenas

```
MainMenu ──► StageSelect ──► HotelCorridor1  ─┐
   │            │        └──► HotelHaunted   ─┤
   │            ├──► Information              ├──► (Game Over) ──► StageSelect
   │            └──► Achievements             │
   └──► Settings (overlay)                    │
                                              ▼
                                       (totais guardados
                                         em PlayerPrefs)
```

---

## 5. Como abrir o projeto

1. Instalar o **Unity Hub**.
2. Instalar a versão **`6000.4.2f1`** (ou qualquer versão compatível da série 6000).
3. Clonar este repositório (o projeto Unity está na raiz, não em subpasta):
   ```bash
   git clone https://github.com/FlavioFernandesDev/Endless-Runner-Room101---Nippa.git
   ```
4. No Unity Hub clicar em **Add → Add project from disk** e escolher a pasta clonada.
5. Abrir o projeto e carregar a cena **`Assets/Scenes/MainMenu.unity`**.
6. Clicar em **Play**.

> Cenas registadas em *Build Settings* (por ordem): `MainMenu`, `HotelCorridor1`, `HotelHaunted`, `StageSelect`, `Information`, `Achievements`.

---

## 6. Técnicas Unity aplicadas

A implementação foca-se nos tópicos de avaliação da secção 6 do enunciado:

### Física e colisões
- `Rigidbody` do jogador com `Interpolate` + `Continuous` collision detection (ver `Assets/Scripts/PlayerMovement.cs`).
- Lógica de física em `FixedUpdate` (movimento lateral, salto, leitura de `linearVelocity`).
- **Tags** usadas: `Obstacle`, `Pickup`, `Key`, `Door`, `Player`, `Finish`.
- `Physics.Raycast` para detecção de `isGrounded`.
- `OnCollisionEnter` + `OnTriggerEnter` para os diferentes tipos de interação.

### Input
- Novo **Unity Input System** (package `com.unity.inputsystem`) via `PlayerInput` e *action map* `InputSystem_Actions.inputactions` (ações `Move` e `Jump`).

### Câmara
- Múltiplas câmaras: gameplay (atrás + acima do jogador), menu animado, câmara de colisão.
- Animator Controller `AnimCam` com clips `AnimMenuCam`, `CollisionCam`.
- Script `TrocaCamara.cs` para alternância.

### Geração procedural
- `SegmentGenerator` + `CorridorTile` + `TileManager` — *spawn* contínuo de segmentos.
- `RuntimePrefabPool` + `RuntimePooledInstance` — *object pooling* para reduzir `Instantiate`/`Destroy`.
- `RuntimeSegmentOptimizer` — liberta segmentos fora da vista da câmara.
- `SegmentCollectibleSpawner` — popula cada segmento com moedas/chaves em spawn points.

### Gestão de jogo
- `RunManager` (singleton, inicializado com `RuntimeInitializeOnLoadMethod`) — estado da corrida, pausa, pontuação, Game Over, transições.
- `PauseManager` — lida com `Time.timeScale = 0` e UI de pausa.
- `GameOverTransition` — *fade out* + carregamento de cena via `SceneManager`.
- `SaveLoad` + `PlayerPrefs` — persistência entre sessões.

### UI e UX
- `TextMeshPro` para todos os textos.
- **Localização** própria com `AppLanguage` + `LocalizedText` (múltiplas linguagens — Português / Inglês).
- Menus: Main Menu, Settings, Stage Select, Information, Achievements, Pause, Game Over.

### Achievements
- `AchievementsManager` avalia conquistas após cada corrida (distância, moedas e chaves totais).
- `AchievementsSceneController` apresenta-as numa cena dedicada.

---

## 7. Assets multimédia

### Modelos 3D
- Formato `.fbx` (padrão Unity, mantém *meshes*, *skeleton* e animações).
- Ficheiros grandes geridos por **Git LFS** (ver `.gitattributes`).

### Texturas e ícones
| Uso | Formato | Justificação |
|---|---|---|
| Ícones de UI (moeda, chave, corrida) | `.png` | Suporta transparência alfa e compressão sem perdas. |
| Texturas de ambiente (papel de parede, carpete) | `.jpg` | Compressão com perdas aceitável em superfícies grandes; reduz o tamanho do projeto. |

### Áudio
| Uso | Formato | Justificação |
|---|---|---|
| Música e SFX em jogo (corrida, moedas, colisão) | `.wav` | Áudio sem compressão, baixa latência e sem artefactos ao entrar em loop — importante num *endless runner* onde a faixa é repetida continuamente. |
| Música do menu | `.mp3` | Ficheiro muito mais pequeno; no menu o CPU tem folga para descomprimir sem impacto na experiência. |

---

## 8. Pontos extra implementados

Implementamos os seguintes pontos de forma a cumprir com os pontos extra do documento fornecido pelo professor:

- Múltiplos níveis / cenas — `HotelCorridor1` e `HotelHaunted` com estilos distintos.
- Dificuldade incremental — aumento progressivo da velocidade.
- Elementos procedurais — segmentos e colecionáveis gerados em runtime.
- Múltiplas câmaras — menu animado, gameplay, colisão.
- Som — música de fundo + SFX (moedas, chaves, colisão).
- UI completa — pontuação, instruções, pausa, game over, settings, achievements.
- Múltiplas línguas — sistema de localização próprio (PT / EN).
- Criatividade — tema original (porteiro de hotel), variante "haunted" do nível, mecanismo de portas aleatórias com chaves.

---

## 9. Dificuldades enfrentadas durante o desenvolvimento

- **Perda de cena e de progresso** logo numa fase incial, por desconhecimento de que o Unity **não guarda a cena automaticamente**. Obrigou a recomeçar o projeto do zero e a adotar a regra de guardar manualmente antes de cada commit. Aproveitámos para atualizar o motor, pensando (erradamente) que o problema era de versão.
- **Configuração de componentes Unity** — colisões e triggers a não dispararem por razões subtis: tag `Obstacle` em falta, `Rigidbody` a faltar num dos objetos da colisão, `Collider` sem *Is Trigger* marcado quando devia, ou o oposto. Grande parte do tempo de *debug* foi em inspecionar o estado dos componentes no Inspector.
- **Performance num endless runner** — manter FPS estável com geração contínua de corredores. Resolvido com *object pooling* (`RuntimePrefabPool`) e optimização de segmentos fora de vista (`RuntimeSegmentOptimizer`). A afinação final desta parte, assim como a implementação do sistema de **achievements**, foram feitas com apoio de ferramentas de IA (GitHub Copilot / Claude) já sobre a estrutura definida por nós.
- **Aprendizagem inicial do Unity** — baseámos a estrutura de partida no tutorial <https://www.youtube.com/watch?v=ufDO-IGv8L8> e adaptámos a ideia para o tema "porteiro de hotel" com a nossa própria arte, mecânicas (chaves, portas aleatórias, variante haunted) e arquitetura de cenas.

---

## 10. Estrutura do repositório

```
.
├── Assets/              # Projeto Unity (scripts, cenas, prefabs, assets)
│   ├── Scripts/         # ~30 scripts C#
│   ├── Scenes/          # MainMenu, StageSelect, HotelCorridor1, HotelHaunted, ...
│   ├── Prefabs/         # Segments, Collectibles, Obstacles, Decorations, ...
│   ├── Characters/      # Modelo Nippa + animações
│   ├── Audio/           # BGM + FX
│   ├── Textures/        # Ícones UI e materiais
│   └── ...
├── Packages/            # Dependências Unity
├── ProjectSettings/     # Build Settings, Input Actions, Tags, Layers
├── docs/
│   └── ARCHITECTURE.md  # Diagrama das cenas e responsabilidades dos scripts
├── README.md            # Este ficheiro
├── .gitignore
└── .gitattributes       # Git LFS para .fbx
```

Para um mapa detalhado dos scripts por responsabilidade, ver [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

---

## 11. Entrega

- **Tag Git da versão entregue:** `1.0`
- **Data de entrega:** 24 de abril de 2026
- **UC:** Tecnologias Multimédia 2025/2026 — TP1
