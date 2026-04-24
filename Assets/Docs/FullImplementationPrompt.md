# Full Implementation Prompt: Sapu Jagad - Sang Pahlawan Kebersihan

You are an expert Unity Game Developer using the UnityMCP toolset. Your task is to implement a complete, functional prototype for the game "Sapu Jagad - Sang Pahlawan Kebersihan" based on the provided Game Design Document.

## 1. Placeholder Asset Generation
Use `manage_texture` to create the following 2D sprites (64x64) as initial placeholders:
- **Pak Darmo (Player):** Solid Green (`#228B22`)
- **Ibu-ibu (NPC):** Solid Purple (`#800080`)
- **Anak Kecil (NPC):** Solid Yellow (`#FFFF00`)
- **Ormas (NPC):** Solid Red (`#FF0000`)
- **Trash:** Solid Brown (`#8B4513`)
- **Minimap Icons:** Smaller versions (16x16) of the above colors.

## 2. Core Systems to Implement

### A. Player Controller (Pak Darmo)
- **Movement:** WASD movement in a 2D top-down space using the new Input System.
- **Interaction:** Detect nearby trash and NPCs using a `CircleCollider2D` trigger. Press 'E' to pick up trash or start NPC dialogue.
- **States:** Implement a simple state machine for `Normal`, `Stunned` (0 speed), and `Slowed` (reduced speed).

### B. Environment & Spawning
- **GameManager:** Handles the 5-minute countdown and tracks the Cleanliness % and Empathy Score.
- **NPC Spawner:** Manage a maximum of 3 active NPCs. Spawn random types (Ibu-ibu, Anak Kecil, Ormas) at perimeter points every 30-60 seconds.
- **Trash System:** Randomly spawn trash objects. Track the total spawned vs. total picked for the cleanliness metric.

### C. NPC Interaction & Mini-Game
- **Interaction Menu:** On 'E', freeze player movement and show a UXML-based mini-game panel.
- **Sentence Scramble:** 
    - Randomly select a sentence from the GDD based on NPC type.
    - Display scrambled word buttons using UI Toolkit (`manage_ui`).
    - **Success:** Empathy +2, NPC helps pick up 1 trash.
    - **Failure/Timeout (10s):** Empathy -10, trigger NPC penalty (Ibu: 7s Stun, Ormas: 10s Slow, Anak: +5 Trash).

### D. UI & Feedback
- **Main HUD:** Display Timer, Cleanliness %, and Empathy Score.
- **Minimap:** A top-left mini-map showing real-time positions of the player, trash, and NPCs using the colors defined in section 1.
- **Evaluation:** A final screen showing the ending type based on GDD thresholds (Cleanliness >= 70%, Empathy >= 80).

## 3. Technical Requirements
- **Architecture:** Use ScriptableObjects for NPC configuration and dialogue sentences. Use a Singleton for the `GameManager`.
- **UI:** Prefer UXML/USS for the Mini-game and HUD to ensure clean separation of logic and layout.
- **Cleanliness Calculation:** `(TrashPicked / TotalTrashSpawned) * 100`.

Refer to the full `GameDesignDocument.md` for the specific dialogue sentences and detailed penalty behaviors.
