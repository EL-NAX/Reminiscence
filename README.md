# Reminiscence

![Game Title](https://i.ibb.co.com/cG0Kcyd/Game-Tittle-terbaru.png)

**Reminiscence** is a 2D side-scrolling, story-based RPG developed in **Godot 4** using **C#** (Godot Mono/.NET).

## 📖 Story Synopsis
In a world ruled by the absolute laws of the Creator, every intent of evil is punished by a mysterious curse. Seven years after Amari—Krieger's childhood friend—was forcibly taken by the royal knights, Krieger risks everything for one goal: to bring Amari home. However, the royal fortress has never been enterable by commoners. To break through this forbidden boundary, how far is Krieger willing to challenge fate and endure the Creator's judgment?

## ✨ Key Features
- **2D Side-Scrolling Exploration**
- **Story-Driven RPG & Dialogue System**
- **Combat System** (Defeat monsters & Bosses)
- **Main & Side Quest System**
- **Item Collection** (Boss Souls)
- **Interactive Main Menu** (Start, Load Game, Settings, Diary)

## 🎮 Controls (PC / Keyboard Default)
| Action | Key |
| :--- | :--- |
| Move Left / Right | A / D |
| Jump | Space |
| Dash | Shift |
| Attack | Left Mouse (MB1) |
| Block / Parry | Right Mouse (MB2) |
| Interact | F |
| Abilities | Q / E / R / T / C |
| Inventory | Tab |
| Settings / Menu | Esc |

## 🛠️ Tech Stack & Requirements
- **Engine:** Godot 4.x (Mono/.NET Version)
- **Language:** C#
- **Graphics:** Pixel Art
- **Platform:** PC (Windows)

## 🚀 How to Run
1. Clone this repository: `git clone https://github.com/USERNAME/REMINISCENCE.git`
2. Open **Godot 4 (.NET/Mono)** and import the `project.godot` file.
3. **Important for C#:** Ensure you have installed the .NET SDK. Go to `Project > Tools > C# > Create C# Solution`, then click Build.
4. Press **F6** to run the game!

## 📈 Project Status
- [x] Main Menu (Settings & Diary UI)
- [x] Player Movement (Run, Jump, Dash)
- [x] Parallax Background
- [ ] Combat & Enemy AI
- [ ] Dialogue & NPC Interactions
- [ ] Quest System
- [ ] Boss Fights

## 🤝 Contributing
*(Kamu bisa isi ini kalau tim kamu sudah mulai menambah fitur, atau isi aturan main:)*
- Pull the latest code from `main` before starting your work.
- Do not commit the `.godot` folder or `bin/obj` folders.
- Use **Git LFS** for all images and audio files.

## 📜 License
*(Isi sesuai hak cipta. Contoh:)*
**All rights reserved.** This project is a personal collaboration project and is not yet open for public commercial use.
=======
# Reminiscence — Godot 4 C# Starter Architecture

This package contains a modular C# gameplay foundation for a 2D side-scrolling story RPG.

## Recommended Godot version

Godot 4.x .NET / C#.

## Autoloads

Add these scripts as Autoloads in Project Settings:

- GameManager -> Scripts/GameManager.cs
- SaveManager -> Scripts/SaveManager.cs
- SettingsManager -> Scripts/SettingsManager.cs
- DiaryManager -> Scripts/DiaryManager.cs
- QuestManager -> Scripts/QuestManager.cs
- InventoryManager -> Scripts/InventoryManager.cs
- DialogueManager -> Scripts/DialogueManager.cs

## Input Map

Create these actions:

- move_left
- move_right
- jump
- dash
- attack
- block
- interact
- inventory
- menu
- ability_1
- ability_2
- ability_3
- ability_4
- ability_5

Default controls:

A/D = movement
Space = jump
Shift = dash
Mouse 1 = attack
Mouse 2 = block/parry
F = interact
Q/E/R/T/C = abilities
Tab = inventory
Esc = menu

SettingsManager can rebuild these actions at runtime.

## Suggested player scene

Player (CharacterBody2D)
- CollisionShape2D
- AnimatedSprite2D
- Health (Node + Health.cs)
- PlayerCombat (Node + PlayerCombat.cs)
- PlayerHealth (Node + PlayerHealth.cs)
- AbilitySystem (Node + AbilitySystem.cs)
- InteractionComponent (Node + InteractionComponent.cs)
- Camera2D

Add Player to group:
player

## Enemy scene

Enemy (CharacterBody2D)
- CollisionShape2D
- AnimatedSprite2D
- Health (Node + Health.cs)

Enemy combat collision mask should match the mask used by PlayerCombat.

## Boss scene

Boss (CharacterBody2D)
- CollisionShape2D
- AnimatedSprite2D
- Health (Node + Health.cs)

Boss.cs automatically awards a Boss Soul and unlocks its diary entry when defeated.

## Main menu

MainMenu (Control)
- Background
- Logo
- Menu buttons
- Diary panel
- Settings panel

Attach MainMenu.cs and assign its exported node references.

## Diary

DiaryUI requires:
- VBoxContainer EntryList
- TextureRect EntryImage
- Label EntryTitle
- Label EntryDescription
- Button CloseButton

Unlock an entry anywhere:

DiaryManager.Instance.Unlock("deer");

## Quest

Start:

QuestManager.Instance.StartQuest("main_001");

Complete objective:

QuestManager.Instance.CompleteObjective("main_001", "enter_forest");

Complete quest:

QuestManager.Instance.CompleteQuest("main_001");

## Inventory

Add item:

InventoryManager.Instance.AddItem("potion", "Potion", 1);

Add Boss Soul:

InventoryManager.Instance.AddItem("forest_soul", "Forest Soul", 1, true);

## Save

Save current player:

SaveManager.Instance.SaveGame(playerNode);

Load:

SaveManager.Instance.LoadGame();

Save location:
user://save_01.json

## Important

This is a gameplay foundation, not a finished commercial game. You still need to build:
- actual scenes
- pixel art
- animations
- hitbox/hurtbox tuning
- enemy AI states
- boss attack patterns
- dialogue UI polish
- quest UI
- inventory UI
- audio
- particles
- level design
- checkpoints
- final story
- balancing
- testing

Some advanced mechanics are intentionally exposed as hooks so you can connect them to your own animations and assets.
>>>>>>> 0d0e077 (Initial commit: Reminiscence Project)
