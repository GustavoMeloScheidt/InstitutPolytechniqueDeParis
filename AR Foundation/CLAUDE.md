# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 AR Foundation project targeting iOS ARKit. Uses AR Foundation 6.0.6 with Universal Render Pipeline for mobile-optimized augmented reality experiences.

## Build & Development Commands

### Opening the Project
Open with Unity Hub using Unity 6000.0.60f1.

### Building for iOS
1. File → Build Settings → iOS platform
2. Build to generate Xcode project in `iOS/` directory
3. Open `iOS/Unity-iPhone.xcodeproj` in Xcode
4. Build and deploy to ARKit-capable device

### Running Tests
- Unity Editor: Window → General → Test Runner
- Command line:
```bash
Unity -projectPath . -runTests -testPlatform editmode
```

### Editor Testing (XR Simulation)
XR simulation is configured for testing AR features without a device. Use Play mode in editor with simulation environments.

## Architecture

### Scene Structure
Main scene: `Assets/Scenes/SampleScene.unity`
- ARSession manages AR lifecycle
- ARCameraManager handles device camera feed
- ARPlaneManager detects horizontal/vertical surfaces
- ARFaceManager for face tracking features
- Reference image library for marker-based tracking

### Key Directories
- `Assets/Prefabs/` - AR visualization prefabs (AR Default Plane, AR Default Face)
- `Assets/XR/` - XR configuration, loaders, and simulation settings
- `Assets/Settings/` - URP render pipeline assets
- `Assets/Images/` - AR tracking images
- `iOS/` - Generated Xcode project (rebuild, don't edit manually)

### Package Dependencies
Core AR: `com.unity.xr.arfoundation`, `com.unity.xr.arkit`
Rendering: `com.unity.render-pipelines.universal`
Input: `com.unity.inputsystem`

### XR Subsystem Configuration
- ARKit loader: `Assets/XR/Loaders/` (production)
- Simulation loader: Editor testing without device
- Settings in `Assets/XR/Settings/`

## Platform Requirements

- iOS 14+, ARKit-capable device
- Camera permission required (configured in Info.plist)
- ARM64 architecture, Metal graphics API
