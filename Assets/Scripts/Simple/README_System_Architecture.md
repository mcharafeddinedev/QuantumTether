# Quantum Thread - System Architecture Guide

## Overview

This document explains how all the systems in Quantum Thread work together. Each script has a specific job, and they communicate with each other to make the game function properly.

**Note**: This covers the finalized version that was cleaned up and refined after the initial 52-hour game jam period.

## Core Systems

### 1. EnhancedGameManager.cs
**What it does**: Manages overall game state and coordinates all other systems
**Key features**:
- Singleton pattern (only one exists at a time)
- Event system for death/restart notifications
- Handles scene transitions and state resets

**How it works**:
- Other scripts subscribe to `OnPlayerDeath` and `OnGameRestart` events
- When the player dies, it broadcasts the death event
- When restarting, it reloads the scene and resets everything

**Connections**: All other systems connect to this central hub

### 2. EnhancedBootstrap.cs
**What it does**: Makes sure the GameManager exists before anything else tries to use it
**Why it's needed**: Unity's load order can be unpredictable, so this prevents crashes

**How it works**:
- Runs automatically before any scene loads
- Creates the GameManager if it doesn't exist
- Prevents duplicate GameManagers


## Player Systems

### 3. EnhancedPlayerSwing.cs
**What it does**: The grappling hook mechanics - the core of the game
**Key features**:
- Primary and secondary grapple support
- Auto-contraction for that "yoink" effect
- Visual rope with LineRenderer
- Audio feedback for all actions
- Pulse effects for visual feedback

**How it works**:
- Left mouse: Primary grapple
- Right mouse: Secondary grapple (if enabled)
- Space: Manual rope contraction
- Auto-disconnect after time limit
- Collision detection for death

**Input system**:
- Raycast from player to mouse position
- Check if target is in range and valid
- Create DistanceJoint2D for physics
- Update rope visual every frame

### 5. EnhancedPlayerDash.cs
**What it does**: Quick burst movement with cooldown
**Key features**:
- Left Shift to dash
- Cooldown system with visual feedback
- Can dash toward mouse or fixed direction
- Optional dash while grappling (for upgrades)

**How it works**:
- Applies impulse force to Rigidbody2D
- Shows green pulse when cooldown is ready
- Checks if grappling is allowed during dash

## Camera & Scoring

### 6. EnhancedCamera.cs
**What it does**: Auto-scrolling camera with speed ramping and death detection
**Key features**:
- Speed increases over time using AnimationCurve
- Threshold shove system (speed boost when player crosses screen threshold)
- Death detection when player falls behind
- Grace period after restart

**How it works**:
- Moves horizontally at increasing speed
- Tracks player screen position for threshold detection
- Applies shove boost when player crosses from left to right
- Checks if player is too far behind camera

**Threshold shove system**:
- When player crosses the middle of the screen (moving forward)
- Triggers a temporary speed boost
- Has cooldown to prevent spam
- Boost reduces over time as game gets faster

### 7. EnhancedScore.cs
**What it does**: Scoring system with death panel and pause functionality
**Key features**:
- Speed-based scoring (faster = more points)
- Death panel with restart/quit buttons
- Pause screen with Escape key
- Score popups for collectibles

**How it works**:
- Calculates points based on camera speed
- Shows death panel when player dies
- Handles all UI interactions
- Manages pause/resume functionality

## Spawning Systems

### 8. EnhancedSpawner.cs
**What it does**: Procedural generation of anchors, collectibles, and ground tiles
**Key features**:
- Pattern-based anchor spawning
- Hazard anchors (red, deadly)
- Collectible coin spawning
- Ground tile streaming

**How it works**:
- Spawns objects ahead of camera
- Culls objects behind camera
- Uses object culling to destroy objects behind camera
- Applies difficulty scaling over time

### 9. EnhancedCollectible.cs
**What it does**: Gamer Bucks coins with collection mechanics
**Key features**:
- Automatic collection when player gets close
- Visual effects (rotation, bobbing)
- Audio feedback
- Score integration

## Audio Systems

### 10. EnhancedMusicManager.cs
**What it does**: Background music and SFX management
**Key features**:
- Volume controls for music and SFX
- Fade transitions
- Persistent settings

### 11. EnhancedAudioBootstrap.cs
**What it does**: Sets up the audio system at startup
**Why it's needed**: Ensures audio settings are loaded before any sounds play

## UI Systems

### 12. EnhancedMainMenu.cs
**What it does**: Main menu controller
**Features**:
- Play button
- Options button
- Credits button
- Quit button

### 13. EnhancedOptionsMenu.cs
**What it does**: Settings menu
**Features**:
- Volume sliders
- Resolution settings
- Graphics options

### 14. EnhancedCreditsMenu.cs
**What it does**: Credits screen
**Features**:
- Team credits
- Back to main menu

### 15. DeathQuoteManager.cs
**What it does**: Shows random death quotes when you die
**Features**:
- Multiple quote categories (encouraging, philosophical, etc.)
- Random selection
- Timed display

## Upgrade Systems

### 16. UpgradeManager.cs
**What it does**: Manages the roguelike upgrade system
**Features**:
- Upgrade selection after death
- Upgrade application to player systems
- Upgrade state tracking

### 17. UpgradeLibrary.cs
**What it does**: Defines all available upgrades
**Features**:
- Upgrade definitions
- Upgrade pools
- Upgrade categories

### 18. UpgradeApplier.cs
**What it does**: Applies upgrade effects to player systems
**Features**:
- Modifies player stats
- Updates UI elements
- Handles upgrade combinations

## How Everything Connects

### Event Flow
1. **Game Start**: EnhancedBootstrap creates EnhancedGameManager
2. **Player Input**: EnhancedPlayerSwing/Dash handle input
3. **Movement**: Physics systems move the player
4. **Camera**: EnhancedCamera follows and detects death
5. **Scoring**: EnhancedScore tracks points and shows UI
6. **Spawning**: EnhancedSpawner creates the world
7. **Death**: EnhancedGameManager broadcasts death event
8. **UI**: EnhancedScore shows death panel
9. **Restart**: EnhancedGameManager reloads scene

### Data Flow
- **EnhancedBalance** → All systems (configuration)
- **EnhancedGameManager** → All systems (events)
- **EnhancedCamera** → EnhancedScore (speed data)
- **EnhancedSpawner** → EnhancedCollectible (spawning)
- **EnhancedCollectible** → EnhancedScore (points)

### Key Design Patterns
- **Singleton**: EnhancedGameManager, EnhancedScore
- **Event-driven**: Death/restart notifications
- **Component-based**: Each script has one clear job
- **Data-driven**: Balance settings in ScriptableObjects
- **Object culling**: Destroys objects behind camera to prevent memory leaks

## Performance Considerations

### Object Culling
- Objects behind camera are destroyed to prevent memory leaks
- Uses FindObjectsByType to find spawned objects
- Culls by object name patterns (AnchorPoint, Collectible, etc.)

### Event System
- Loose coupling between systems
- Easy to add new death-triggered behaviors
- Clean separation of concerns

### Update Optimization
- Only necessary systems run every frame
- Camera speed calculations are cached
- UI updates are batched

## Debugging Tips

### Common Issues
1. **GameManager not found**: Check EnhancedBootstrap is in scene
2. **Audio not playing**: Check EnhancedAudioBootstrap setup
3. **UI not showing**: Check Canvas setup and button references
4. **Performance issues**: Check object culling is working properly

### Useful Debug Info
- Camera speed progression
- Player grapple state
- Score calculation
- Spawner activity

## Future Expansion

### Easy to Add
- New upgrade types (just add to UpgradeLibrary)
- New collectible types (extend EnhancedCollectible)
- New audio effects (add to EnhancedMusicManager)
- New UI screens (follow existing pattern)

### System Integration
- All systems use the event system for communication
- Balance settings are centralized
- Audio system is independent
- UI system is modular

This architecture makes it easy to understand, modify, and extend the game. Each system has a clear purpose and well-defined interfaces with other systems.
