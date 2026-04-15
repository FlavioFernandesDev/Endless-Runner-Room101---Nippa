# Project Overview
- **Game Title:** Hotel Bellhop Runner (NiPPa)
- **High-Level Concept:** An endless runner where the player, a hotel bellhop, navigates through infinite hotel corridors, dodging luggage and room service carts while collecting tips.
- **Players:** Single-player.
- **Inspiration:** Subway Surfers, Temple Run.
- **Tone / Art Direction:** Stylized / Low Poly (Bright colors, clean geometry).
- **Target Platform:** PC / Mobile.
- **Render Pipeline:** URP (Universal Render Pipeline).

# Game Mechanics
## Core Gameplay Loop
The player runs forward automatically. They must switch between 3 lanes (Left, Center, Right) to avoid obstacles and collect items. The corridor is generated procedurally ahead of the player and destroyed once it is behind them.

## Controls and Input Methods
- **A/D or Left/Right Arrow:** Switch lanes.
- **Space or W / Up Arrow:** Jump over low obstacles.
- **S / Down Arrow:** Slide under high obstacles (like cleaning carts or hanging signs).

# UI
- **HUD:** Score (distance), Collected Items (tips), Multiplier.
- **Menu:** Start Screen, Game Over with "Restart" and "Main Menu" buttons.

# Key Asset & Context
- **Prefab:** `CorridorTile` (The modular block).
- **Mesh:** `door and frame.fbx` (Instance ID: 69314) - used for the side walls.
- **Lanes:** 3 lanes centered at X: -3, 0, 3.

# Implementation Steps
## 1. Create the CorridorTile Prefab
Define the hierarchy and base geometry for a single 10m tile.
- **File:** `Assets/Prefabs/CorridorTile.prefab`
- **Dependencies:** `door and frame.fbx`

## 2. Setup Hierarchy & Measurements
- **Root (0,0,0):** `CorridorTile` (Empty GameObject).
- **Child: `Geometry`:** Floor (10x9m), Ceiling, Side Walls.
- **Child: `FixedElements`:**
    - `Doors`: Place `door and frame` at Z=2.5 and Z=7.5 on both walls.
    - `Lights`: Ceiling lights at Z=5.
- **Child: `SpawnPoints`:**
    - `Lane_Left` (X:-3, Y:0, Z:5)
    - `Lane_Center` (X:0, Y:0, Z:5)
    - `Lane_Right` (X:3, Y:0, Z:5)
- **Child: `Connectors`:**
    - `EndNode` (X:0, Y:0, Z:10) - Used as a pivot for the next tile.

## 3. Implement TileManager (Script)
Create a script to handle spawning tiles and removing old ones.
- **Logic:** `SpawnTile()` at the `EndNode` of the previous tile.
- **Trigger:** Player passing a collider halfway through the current tile.

## 4. Implement Randomization Logic
A script on the `CorridorTile` prefab that chooses which obstacles to spawn at the `SpawnPoints` on `Start()`.
- **Options:** 0 (Empty), 1 (Jump Obstacle), 2 (Slide Obstacle), 3 (Collectible).

# Verification & Testing
- **Manual Check:** Ensure tiles snap perfectly at (0,0,10), (0,0,20), etc., without gaps.
- **Testing:** Run the game and verify the `TileManager` spawns at least 5 tiles ahead.
- **Obstacle Check:** Verify that at least one lane is always clear to ensure the game is beatable.
