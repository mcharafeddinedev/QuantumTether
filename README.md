# Quantum Tether

An infinite 2D side-scrolling grappling hook game built in Unity 6 for the TX Game Jam. Swing through procedurally generated levels in a race against time.

## About This Project

Quantum Tether is an infinite runner where players swing between grapple points using realistic rope physics. The camera scrolls faster over time to increase difficulty, and players collect time crystals to purchase upgrades during the runs. 

(This project version is a finalized, revised and upgraded update from my game jam submission)

**Game Jam**: TX Game Jam 2024  
**Theme**: "Out of Time"  
**Development Time**: ~48 hours + post-jam refinement

**Platform**: Unity 6 (Windows)

## Gameplay Features

- **Grappling Hook Physics**: Swing between grapple points with realistic rope physics
- **Procedural Generation**: Dynamic anchor spawning with 9+ different patterns
- **Upgrade System**: Collect time crystals to buy upgrades during runs
- **Dash System**: Quick burst movement with cooldown
- **Auto-Scrolling Camera**: Speed increases over time to create challenge
- **Audio Integration**: Music and sound effects with volume controls

## Controls

- **Left Mouse**: Primary grapple hook (main movement)
- **Right Mouse**: Secondary grapple hook (unlocked via upgrades)
- **Space**: Manual rope contraction
- **Left Shift**: Dash (unlocked via upgrades)
- **Escape**: Pause game
- **Objective**: Survive as long as possible while collecting time crystals!

## Technical Details

- **Engine**: Unity 6
- **Language**: C#
- **Architecture**: Modular systems with event-driven communication
- **Performance**: Object cleanup system to prevent memory issues
- **Design Patterns**: Singleton pattern for core systems, component-based design

## Project Structure

All code is organized in the `Assets/Scripts/Simple/` folder:

### Core Systems (`Simple/Core/`)
- **EnhancedGameManager.cs** - Main game state manager
- **EnhancedBootstrap.cs** - Game initialization

### Player Systems (`Simple/Player/`)
- **EnhancedPlayerSwing.cs** - Grappling hook mechanics
- **EnhancedPlayerDash.cs** - Dash movement

### Camera System (`Simple/Camera/`)
- **EnhancedCamera.cs** - Auto-scrolling camera with death detection

### UI Systems (`Simple/UI/`)
- **EnhancedMainMenu.cs** - Main menu
- **EnhancedOptionsMenu.cs** - Settings menu
- **EnhancedCreditsMenu.cs** - Credits screen
- **DeathQuoteManager.cs** - Death screen quotes
- **ResolutionManager.cs** - Resolution handling

### Scoring System (`Simple/Scoring/`)
- **EnhancedScore.cs** - Score management and UI

### Spawning Systems (`Simple/Spawning/`)
- **EnhancedSpawner.cs** - Procedural generation of anchors and collectibles
- **EnhancedCollectible.cs** - Time crystal collection

### Upgrade System (`Simple/Upgrades/`)
- **UpgradeManager.cs** - Upgrade system manager
- **UpgradeLibrary.cs** - Available upgrades
- **RunUpgrade.cs** - Upgrade data
- **UpgradeApplier.cs** - Applies upgrade effects
- **UpgradePanelUI.cs** - Upgrade selection UI
- **UpgradeCardUI.cs** - Individual upgrade cards
- **UpgradeFeedbackUI.cs** - Upgrade feedback

### Audio Systems (`Simple/Audio/`)
- **EnhancedMusicManager.cs** - Background music
- **EnhancedAudioBootstrap.cs** - Audio setup

## Key Features

### Event System
- Systems communicate through events instead of direct references
- Easy to add new features without breaking existing code
- Clean separation between different game systems

### Object Cleanup
- Objects behind the camera are automatically destroyed
- Prevents memory leaks and keeps performance smooth
- Uses efficient object detection

### Modular Design
- Each script has one clear job
- Systems can be developed and tested independently
- Easy to modify or add new features


## Documentation

The project includes documentation in `Assets/Scripts/`:
- **Project_Analysis_and_Architecture.md** - System overview
- **Quantum_Thread_Post_Mortem.md** - Development analysis
- **Simple/README_System_Architecture.md** - How systems work together
- **Simple/Tutorials/** - Setup guides for each system

## What I Learned

This project taught me:
- How to build modular game systems that work together
- Event-driven programming for loose coupling
- Performance optimization through object cleanup
- Procedural generation techniques
- UI/UX design for games
- More about audio integration and management

## Assets

### Audio
- **Music**: Original compositions with scene-aware selection
- **SFX**: Custom sound effects for all game actions

### Art
- **Sprites**: Character, environment, UI elements, and grapple points
- **Materials**: Shaders for rope rendering
- **Icons**: Game icons and cursors

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built for TX Game Jam 2025 (EGaDS)**
