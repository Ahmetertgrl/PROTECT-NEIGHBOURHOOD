# 🧟 Protect Neighbourhood | Unity FPS Survival

**Protect Neighbourhood** is a performance-optimized, wave-based FPS survival game developed with Unity. This project focuses on implementing core FPS mechanics, dynamic resource management, and efficient memory usage through design patterns.

---

## 🎯 Game Objective
Defend your neighborhood's power transformers from relentless zombie waves!
* **Enemy Waves:** 40 zombies spawn from 2 distinct entry points.
* **Defense Mechanic:** Prevent zombies from reaching the transformers. Each successful breach reduces your **Health Bar**.
* **Survival:** Scavenge the map for randomly spawned Ammo, Health, and Grenade boxes to stay in the fight.

---

## 🛠️ Key Technical Features

### ⚡ Performance Optimization (Object Pooling)
To ensure a smooth gameplay experience and stable FPS, I implemented the **Object Pooling** pattern for:
* `AK47 Bullets`: Reusing bullet objects to avoid frequent `Instantiate` and `Destroy` calls.
* `Shell Casings`: Ejected casings are pooled to minimize Garbage Collector (GC) pressure during intense firefights.

### 🧨 Advanced Mechanics
* **AOE Damage System:** Grenades utilize `Physics.OverlapSphere` to detect all enemies within a specific radius and apply explosive damage simultaneously.
* **Dynamic Spawning:** A randomized loot system that spawns Health and Ammo boxes at unpredictable intervals and locations using `Random.Range` logic.
* **Navigation & AI:** Zombies utilize Unity's Navigation system to intelligently pathfind toward the neighborhood's strategic assets (transformers).

### 🖥️ UI & Inventory
* **Real-time HUD:** Dynamic Health Bar that reacts to enemy proximity and heals via collected Health Kits.
* **Inventory System:** Management of consumable items like Grenades and Medkits with on-screen tracking.

---

## 🖼️ Gameplay Screenshots


| **Combat & UI** | **Environment & Waves** |
| :--- | :--- |
| ![Gameplay 1](https://via.placeholder.com/600x340?text=Combat+Screenshot) | ![Gameplay 2](https://via.placeholder.com/600x340?text=Environment+Screenshot) |

| **Explosions (AOE)** | **Loot System** |
| :--- | :--- |
| ![Gameplay 3](https://via.placeholder.com/600x340?text=Explosion+Effect) | ![Gameplay 4](https://via.placeholder.com/600x340?text=Loot+Spawn) |

---

## 💻 Tech Stack
* **Engine:** Unity 2022.x (C#)
* **Version Control:** Git / GitHub
* **Main Patterns:** Object Pooling, Singleton (for Managers), Observer Pattern (for UI updates).

---

## 🚀 Installation
1.  Clone the repository:
    ```bash
    git clone [https://github.com/Ahmetertgrl/PROTECT-NEIGHBOURHOOD)
    ```
2.  Open the project folder in **Unity Hub**.
3.  Ensure you have the correct Unity Editor version installed.
4.  Open `Assets/Scenes/MainScene.unity` and press **Play**.

---

### 👨‍💻 About the Developer
**Ahmet**
* **Computer Engineering Student** at Süleyman Demirel University (SDÜ) - 2nd Year.
* Aspiring Game Developer specializing in Unity, VR, and Performance Optimization.


---
