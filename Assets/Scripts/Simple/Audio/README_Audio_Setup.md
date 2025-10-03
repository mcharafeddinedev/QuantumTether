# Audio System Setup Guide

## What This Document Contains

This guide explains how to set up the enhanced audio system for Quantum Thread. It covers music management, volume controls, scene-based music selection, and integration with the options menu. This document provides step-by-step setup instructions and explains how the audio system integrates with other game systems.

## System Overview

The audio system consists of:
- **EnhancedMusicManager**: Handles background music with volume control and fade effects
- **EnhancedAudioBootstrap**: Ensures music manager exists in the scene
- **Individual AudioClip fields**: In action scripts for SFX (controlled by SFX slider)

## Quick Setup

### Step 1: Create Audio Managers GameObject

1. In your scene, create an empty GameObject
2. Name it "AudioManagers"
3. Add the `EnhancedAudioBootstrap` script to it

### Step 2: Set Up Music Manager

1. The `EnhancedMusicManager` will be automatically created
2. In the Inspector, assign your music tracks:
   - **Main Menu Music**: Background music for menu scenes
   - **Gameplay Music**: Background music for gameplay scenes
   - **Fade In Duration**: How long music takes to fade in (default: 2 seconds)
   - **Fade Out Duration**: How long music takes to fade out (default: 1 second)

### Step 3: Connect to Options Menu

The music manager automatically connects to the options menu:
- **Music Volume Slider** → Controls background music volume with smooth fade effects
- **SFX Volume Slider** → Controls all individual sound effects in action scripts

## Volume Control

### Music Volume
- Controlled by the Music Volume slider in options
- Affects all background music tracks
- Includes smooth fade-in/fade-out effects
- Saved to PlayerPrefs automatically

### SFX Volume  
- Controlled by the SFX Volume slider in options
- Affects all sound effects in individual scripts
- Each script reads `PlayerPrefs.GetFloat("SFXVolume", 1f)` for volume
- Saved to PlayerPrefs automatically

## Music System

### Automatic Scene Detection
The music manager automatically plays appropriate music based on scene name:
- Scenes containing "menu" → Main Menu Music
- Scenes named "Game" → Gameplay Music
- Other scenes → Gameplay Music (default)

### Fade Effects
- Music fades in smoothly when starting (no abrupt volume jumps)
- Music fades out when stopping
- Customizable fade durations in the Inspector

### Manual Control
You can manually control music:
```csharp
// Play specific music
EnhancedMusicManager.Instance.PlayMainMenuMusic();
EnhancedMusicManager.Instance.PlayGameplayMusic();

// Stop music (with fade-out)
EnhancedMusicManager.Instance.StopMusic();

// Set volume
EnhancedMusicManager.Instance.SetMusicVolume(0.5f);
```

## Sound Effects System

### Individual Script Integration
Each action script has its own AudioClip fields and uses SFX volume:
```csharp
// In any script with AudioSource
float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
audioSource.PlayOneShot(soundClip, sfxVolume);
```

### Pre-configured Scripts
These scripts already have audio support:
- **EnhancedPlayerSwing**: `grappleSound`, `releaseSound`
- **EnhancedPlayerDash**: `dashSound`
- **EnhancedCollectible**: `collectSound`
- **EnhancedScore**: `deathSound`
- **EnhancedMainMenu**: `buttonClickSound`, `buttonHoverSound`

## Adding Audio to New Scripts

### Step 1: Add Audio Fields
```csharp
[Header("Audio")]
[SerializeField, Tooltip("Sound effect")]
private AudioClip soundEffect;

[SerializeField, Tooltip("Audio source")]
private AudioSource audioSource;
```

### Step 2: Play Sound with Volume Control
```csharp
void PlaySound()
{
    if (audioSource && soundEffect)
    {
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        audioSource.PlayOneShot(soundEffect, sfxVolume);
    }
}
```

## Customization

### Adding New Music Tracks
1. Import your music files as AudioClips
2. Assign them to the Music Manager fields
3. Music will automatically play based on scene with fade effects

### Adding New SFX
1. Import your sound files as AudioClips
2. Add AudioClip fields to your scripts
3. Use the SFX volume system for consistent control

### Custom Fade Durations
- **Fade In Duration**: How long music takes to fade in (default: 2 seconds)
- **Fade Out Duration**: How long music takes to fade out (default: 1 second)

## 🐛 Troubleshooting

### Music Not Playing
- Check that AudioClips are assigned to Music Manager
- Verify the scene name matches expected patterns
- Check that Music Volume slider is not at 0 (pretty obvious but worth checking)
- Ensure AudioSource component exists on Music Manager

### SFX Not Playing
- Check that AudioClips are assigned to scripts
- Verify AudioSource components exist on scripts
- Check that SFX Volume slider is not at 0 (another obvious one)
- Ensure scripts are reading SFX volume from PlayerPrefs

### Volume Not Saving
- Ensure `saveToPlayerPrefs` is enabled in options menu
- Check that PlayerPrefs are being saved properly

## Notes

- Music manager persists across scene loads (DontDestroyOnLoad - super convenient)
- Volume settings are automatically saved to PlayerPrefs
- Music includes smooth fade-in/fade-out effects (no jarring volume jumps)
- SFX volume is controlled per-script using PlayerPrefs
- All audio components are created automatically - no manual setup required (honestly pretty cool)