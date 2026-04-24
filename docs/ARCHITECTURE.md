# Arquitetura — Endless Runner Room101 — Nippa

Documento complementar ao `README.md`. Descreve a arquitetura de cenas, o papel dos scripts principais e as convenções do projeto, para facilitar a avaliação e a leitura do código.

---

## 1. Fluxo das cenas

Todas as cenas estão declaradas em `ProjectSettings/EditorBuildSettings.asset` e são carregadas via `SceneManager.LoadScene` a partir de `RunManager` (constantes `MainMenuSceneName`, `StageSelectSceneName`, etc.).

```
                        ┌────────────────┐
                        │    MainMenu    │◄────────────────────────────┐
                        └───────┬────────┘                             │
                                │                                      │
                                ▼                                      │
        ┌───────────────────────────────────────────┐                  │
        │                StageSelect                │                  │
        │ ┌────────────┐ ┌────────────┐ ┌─────────┐ │                  │
        │ │HotelCorridor1│ │HotelHaunted│ │  ...    │ │                │
        │ └─────┬──────┘ └──────┬─────┘ └─────────┘ │                  │
        └───────┼───────────────┼───────────────────┘                  │
                │               │                                      │
                ▼               ▼                                      │
        ┌───────────────────────────────────────────┐                  │
        │                Corrida ativa              │                  │
        │  PlayerMovement, geração procedural, HUD  │                  │
        └───────────────────┬───────────────────────┘                  │
                            │ (colisão fatal)                          │
                            ▼                                          │
                    ┌───────────────────┐                              │
                    │ GameOverTransition│────► PlayerPrefs (totais)    │
                    └─────────┬─────────┘                              │
                              │                                        │
                              ▼                                        │
                        ┌────────────┐                                 │
                        │StageSelect │─────────────────────────────────┘
                        └────────────┘
```

Cenas auxiliares:

- **`Information`** — ecrã de instruções (acessível a partir de `MainMenu` / `StageSelect`).
- **`Achievements`** — exibe conquistas obtidas, lidas via `AchievementsManager`.
- **`Settings`** — acessível em overlay, controla volume, linguagem, etc.

---

## 2. Scripts por responsabilidade

Todos em `Assets/Scripts/`.

### Jogador
| Script | Responsabilidade |
|---|---|
| `PlayerMovement.cs` | Movimento lateral em 3 *lanes*, salto, aceleração progressiva, detecção de chão por `Raycast`, tratamento de colisões fatais. Usa `Rigidbody` + `FixedUpdate`. |
| `CollisionDetect.cs` | `OnTriggerEnter` alternativo, usado em colisores específicos; dispara animação de colisão, SFX e transição para Game Over. |

### Geração procedural do mundo
| Script | Responsabilidade |
|---|---|
| `SegmentGenerator.cs` | *Spawn* contínuo de segmentos (prefabs) à frente do jogador. |
| `CorridorTile.cs` | Metadados de cada *tile* de corredor (pontos de saída, flags de estilo). |
| `TileManager.cs` | Gestão do buffer de *tiles* ativos, reciclagem. |
| `RuntimeSegmentOptimizer.cs` | Liberta segmentos fora do `Camera.main.WorldToViewportPoint` para reduzir *draw calls*. |
| `RuntimePrefabPool.cs` | *Object pool* genérico — evita `Instantiate`/`Destroy` a cada frame. |
| `RuntimePooledInstance.cs` | *Component marker* para instâncias devolvidas ao pool. |
| `SegmentCollectibleSpawner.cs` | Popula *spawn points* de cada segmento com moedas e chaves. |

### Colecionáveis e props
| Script | Responsabilidade |
|---|---|
| `CollectCoin.cs` | `OnTriggerEnter` → `RunManager.AddCoin()`, toca SFX, destrói o item. |
| `CollectKey.cs` | Igual ao anterior mas para chaves. |
| `CollectableRotate.cs` | Rotação visual contínua dos colecionáveis. |
| `RandomDoor.cs` | Estado aleatório de abertura/fecho das portas. Expõe `IsOpen` e `TryConsumeDoorHit()` para integrar com o `PlayerMovement`. |

### Gestão de jogo
| Script | Responsabilidade |
|---|---|
| `RunManager.cs` | Singleton com estado da corrida (coins, keys, distance, speed, game over), persistência em `PlayerPrefs`. Inicializado via `RuntimeInitializeOnLoadMethod`. |
| `GameOverTransition.cs` | Coroutine de *fade out* + `SceneManager.LoadScene` para voltar ao StageSelect. |
| `PauseManager.cs` | `Time.timeScale = 0`, UI de pausa, retoma. |
| `MasterInfo.cs` | Partilha de dados não-persistentes entre cenas. |
| `StageControls.cs` | Botões de seleção de nível na cena `StageSelect`. |
| `LoadToStage.cs` | Carrega a cena de gameplay escolhida. |

### UI e câmaras
| Script | Responsabilidade |
|---|---|
| `MainMenuControl.cs` | Botões Play / Settings / Exit do menu principal. |
| `SettingsMenuController.cs` | Interações dos controlos de settings. |
| `SettingsManager.cs` | Estado persistente das settings (volume, linguagem). |
| `SettingsSceneBootstrap.cs` | Garante que as settings são aplicadas ao entrar na cena. |
| `AchievementsSceneController.cs` | Render da cena de achievements. |
| `TrocaCamara.cs` | Alterna entre câmaras (menu animado ↔ gameplay). |

### Achievements e persistência
| Script | Responsabilidade |
|---|---|
| `AchievementsManager.cs` | Regras das conquistas. Avaliadas em `RunManager.CommitRunTotals()`. |
| `SaveLoad.cs` | Wrapper sobre `PlayerPrefs` para leitura/escrita centralizada. |

### Localização (PT / EN)
| Script | Responsabilidade |
|---|---|
| `AppLanguage.cs` | Estado global da linguagem; expõe `Current`. |
| `LocalizedText.cs` | Componente de `TextMeshPro` que se subscreve ao `AppLanguage` e atualiza a string. |

### Estilo de nível
| Script | Responsabilidade |
|---|---|
| `HauntedLevelStyler.cs` | Aplica variações visuais (iluminação, materiais) na cena `HotelHaunted`. |

---

## 3. Tags utilizadas

| Tag | Onde aparece | Efeito |
|---|---|---|
| `Player` | No GameObject do jogador | Filtro em `OnTriggerEnter` dos colecionáveis e da `CollisionDetect`. |
| `Obstacle` | Malas, carrinho, cesto, placa "chão molhado", portas | Colisão fatal em `PlayerMovement.OnCollisionEnter`. |
| `Pickup` | Moedas | Identifica colecionáveis de pontuação. |
| `Key` | Chaves | Identifica chaves (consumíveis para porta aberta). |
| `Door` | Portas com `RandomDoor` | Usado para lógica de consumo de chave. |
| `Finish` | Marcador de fim de segmento | Usado pela geração procedural. |

---

## 4. Convenções de assets

- `Assets/Prefabs/Collectibles/` — moedas, chaves.
- `Assets/Prefabs/Obstacles/` — obstáculos físicos.
- `Assets/Prefabs/Decorations/` — elementos decorativos dos corredores.
- `Assets/Prefabs/PropsInteractive/` — portas, interruptores.
- `Assets/Prefabs/Tiles/` — segmentos base do corredor.
- `Assets/Prefabs/Segment.prefab`, `Segment (1).prefab`, `Segment (2).prefab`, `StartSegment.prefab` — segmentos compostos usados pelo `SegmentGenerator`.
- `Assets/Audio/BGM/` — música de fundo.
- `Assets/Audio/Fx/` — efeitos sonoros.
- `Assets/Characters/Nippa/` — modelo do jogador + animações (`Running`, `Stumble Backwards`, `Jump`).

---

## 5. Build Settings

Cenas incluídas (ordem relevante para `SceneManager`):

1. `MainMenu`
2. `HotelCorridor1`
3. `HotelHaunted`
4. `StageSelect`
5. `Information`
6. `Achievements`

---

## 6. Persistência

- Chaves `PlayerPrefs`:
  - `COINSAVE` — total de moedas acumulado.
  - `KEYSAVE` — total de chaves acumulado.
  - `DISTANCESAVE` — distância total acumulada (em metros).
- Gravadas apenas no fim de cada corrida em `RunManager.CommitRunTotals()`.
- Lidas no arranque em `RunManager.LoadTotals()` (bootstrap antes da primeira cena).
