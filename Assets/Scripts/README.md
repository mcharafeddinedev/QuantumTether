# Quantum Thread - Scripts Folder

This is the Scripts folder from **Quantum Thread**, a 2D infinite side-scrolling grappling hook game built in Unity 6 for a 52-hour game jam. The theme was "Out of Time" and features a Spider-Man style grappling hook system where players swing between grapple points.

**Note**: This is the finalized, cleaned, and updated version that was refined after the initial 52-hour game jam period.

## What This Contains

This Scripts folder contains all the working code for Quantum Thread. The game features:

- **Grappling Hook Mechanics**: Physics-based swinging with mouse controls
- **Dash System**: Quick burst movement with cooldown mechanics  
- **Procedural Generation**: Dynamic anchor spawning with mathematical patterns
- **Roguelike Progression**: Upgrade system with meaningful choices
- **Audio Integration**: Dynamic music and sound effects
- **Performance Optimization**: Object culling and efficient spawning

## Project Details

- **Engine**: Unity 6
- **Language**: C#
- **Initial Development**: 52-hour game jam
- **Post-Jam Refinement**: Additional cleanup, documentation, and optimization
- **Lines of Code**: ~7,600+ (in this Scripts folder)
- **Architecture**: Event-driven, component-based design

## Scripts Structure

All working code is located in the `Simple/` folder, which contains:

### Core Systems (`Simple/Core/`)
- **EnhancedGameManager.cs**: Handles death, restart, and game state with event system
- **EnhancedBootstrap.cs**: Ensures GameManager exists before any scene loads

### Player Systems (`Simple/Player/`)
- **EnhancedPlayerSwing.cs**: Grappling hook swinging mechanics with rope physics
- **EnhancedPlayerDash.cs**: Dash movement with cooldown and visual effects

### Spawning Systems (`Simple/Spawning/`)
- **EnhancedSpawner.cs**: Procedural generation with 15+ mathematical patterns
- **EnhancedCollectible.cs**: Collectible behavior and collection mechanics

### UI Systems (`Simple/UI/`)
- **EnhancedMainMenu.cs**: Main menu controller with navigation
- **EnhancedOptionsMenu.cs**: Settings menu with volume and resolution controls
- **DeathQuoteManager.cs**: Random death quotes for player motivation

### Audio Systems (`Simple/Audio/`)
- **EnhancedMusicManager.cs**: Dynamic music switching and volume management
- **EnhancedAudioBootstrap.cs**: Audio system initialization

### Upgrade Systems (`Simple/Upgrades/`)
- **UpgradeManager.cs**: Roguelike progression system with upgrade selection
- **UpgradeLibrary.cs**: Upgrade definitions and prerequisites

### Tutorials (`Simple/Tutorials/`)
- Comprehensive setup guides for building similar systems in future projects

## Key Features

### Procedural Generation
- 15+ mathematical patterns for anchor placement
- Difficulty scaling over time
- Dynamic hazard generation
- Efficient object culling system

### Player Mechanics
- Physics-based grappling with DistanceJoint2D
- Secondary grapple system (upgrade)
- Dash mechanics with visual effects
- Collision detection and bounce physics

### Audio System
- Scene-based music selection
- Volume controls with persistence
- Dynamic sound effects
- Audio source management

### Upgrade System
- Roguelike progression mechanics
- Upgrade library with prerequisites
- UI selection system
- Persistent upgrade state

## Documentation

This Scripts folder includes extensive documentation:

- **README_System_Architecture.md**: How all systems work together
- **README_Folder_Organization.md**: Folder structure and organization
- **Tutorials/**: Step-by-step guides for building similar systems
- **Project_Analysis_and_Architecture.md**: Technical analysis and architecture
- **Project_Evolution_History.md**: Development process and technical decisions
- **Quantum_Thread_Post_Mortem.md**: Lessons learned and project analysis

## Getting Started

1. Copy this Scripts folder into your Unity project's Assets folder
2. Navigate to the `Simple/` folder for all working scripts
3. Check the `Tutorials/` folder for setup guides
4. Review `README_System_Architecture.md` for system overview
5. Assign the scripts to GameObjects in your Unity scenes

## Development Notes

This Scripts folder demonstrates:
- Clean, modular architecture with proper separation of concerns
- Event-driven system communication for loose coupling
- Performance optimization techniques (object culling, efficient spawning)
- Comprehensive documentation practices for future reference
- Strategic debugging and logging for development workflow

## License

This Scripts folder is available for educational and portfolio purposes. Feel free to use the code and documentation as reference for your own Unity projects.

## Note

This is just the Scripts folder from the full Unity project. To use these scripts, you'll need to:
- Create a new Unity project
- Copy this Scripts folder into the Assets directory
- Set up the required GameObjects and assign the scripts
- Follow the tutorial guides for proper setup

---

*Scripts from a 52-hour game jam project, refined and cleaned up after the jam period to focus on clean architecture, performance optimization, and comprehensive documentation.*
