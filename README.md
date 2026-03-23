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

<img width="1569" height="878" alt="Ekran görüntüsü 2026-03-23 235313" src="https://github.com/user-attachments/assets/e11c57ef-7532-4a5f-a520-d0fd932fde07" />
<img width="1569" height="881" alt="Ekran görüntüsü 2026-03-23 235626" src="https://github.com/user-attachments/assets/25cc737b-dc74-4585-9c9b-3d12a3202fe4" />
<img width="1763" height="836" alt="Ekran görüntüsü 2026-03-23 235423" src="https://github.com/user-attachments/assets/47cbfb63-b157-4406-a78e-2a3607b537bf" />
<img width="1566" height="876" alt="Ekran görüntüsü 2026-03-23 235521" src="https://github.com/user-attachments/assets/d4cca55f-a604-4c84-bf38-6b08bd11d7db" />


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
