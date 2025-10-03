# Quantum Thread - Simple Folder Organization

This document describes the organized structure of the Simple folder, which contains all the working scripts for Quantum Thread. These are the versions that actually work well together and make the game function properly.

## Folder Structure

### **Core/**
Core game systems and managers:
- `EnhancedGameManager.cs` - Main game state manager (death, restart, events)
- `EnhancedBootstrap.cs` - Game initialization and setup

### **Player/**
Player movement and abilities:
- `EnhancedPlayerDash.cs` - Player dash mechanics and upgrades
- `EnhancedPlayerSwing.cs` - Player grappling mechanics and upgrades

### **Camera/**
Camera movement and death detection:
- `EnhancedCamera.cs` - Camera movement, death detection, and upgrades

### **UI/**
User interface systems and menus:
- `EnhancedMainMenu.cs` - Main menu controller
- `EnhancedOptionsMenu.cs` - Options menu controller
- `EnhancedCreditsMenu.cs` - Credits menu controller
- `DeathQuoteManager.cs` - Death screen quotes and feedback

### **Scoring/**
Score and progression systems - keeps track of how well you're doing
- `EnhancedScore.cs` - Score management, death panel, and live HUD

### **Spawning/**
Object spawning and environment systems - creates the world around you
- `EnhancedSpawner.cs` - Anchor, collectible, and hazard spawning
- `EnhancedCollectible.cs` - Collectible behavior and scoring

### **Upgrades/**
Upgrade system - the roguelike progression stuff
- `RunUpgrade.cs` - Upgrade data structure
- `RunUpgradeState.cs` - Upgrade state tracking
- `UpgradeLibrary.cs` - Upgrade definitions and pool
- `UpgradeManager.cs` - Upgrade system manager and timing (runs the whole show)
- `UpgradeApplier.cs` - Upgrade effect application
- `UpgradePanelUI.cs` - Upgrade panel UI controller
- `UpgradeCardUI.cs` - Individual upgrade card UI
- `UpgradeFeedbackUI.cs` - Upgrade feedback messages

### **Audio/**
Audio systems and music management - makes everything sound awesome
- `EnhancedMusicManager.cs` - Background music manager
- `EnhancedSFXManager.cs` - Sound effects manager
- `EnhancedAudioBootstrap.cs` - Audio system initialization
- `README_Audio_Setup.md` - Audio setup guide

### **Documentation/**
Documentation and setup guides - all the helpful reading material
- `README_MainMenu_Setup.md` - Main menu setup guide
- `README_Simple_Setup.md` - Simple systems setup guide
- `README_Folder_Organization.md` - This file

## Why This Organization Works

1. **Clear Separation** - Each folder contains related functionality (obvious but super important)
2. **Easy to Find Stuff** - You can quickly locate the script you need without hunting around
3. **Team-Friendly** - Multiple people can work on different systems without conflicts
4. **Maintainable** - Related code is grouped together (no more hunting through random folders)
5. **Scalable** - Easy to add new scripts to the right folder as the project grows

## Usage

When adding new scripts to the Simple folder:
1. Identify which system category the script belongs to (should be obvious)
2. Place it in the appropriate folder
3. Update this README if adding a new category (so other people know what's going on)

## Important Notes

- All `.meta` files are automatically managed by Unity (don't worry about these)
- Scripts maintain their original functionality and references
- No code changes were required for this reorganization (just moved stuff around)
- Unity will automatically update all script references (super convenient)
- Check out `README_System_Architecture.md` for a deep dive into how everything works together
- These are the working scripts - clean, organized, and ready for packaging
