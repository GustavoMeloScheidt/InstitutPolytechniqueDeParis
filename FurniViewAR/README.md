# AR Furniture Placement App

An augmented reality furniture visualization app for iOS built with Unity 6 and AR Foundation. Place, move, rotate, and scale 3D furniture on detected floor surfaces.

<p align="center">
  <a href="./Assets/Images/FurniView.mp4">
    <img src="./Assets/Images/FurniView_thumb.png" width="85%" />
  </a>
</p>


## Features

- **Plane Detection**: Automatically detects horizontal surfaces (floors) using ARKit
- **Placement Reticle**: Visual indicator showing where furniture will be placed
- **Furniture Catalog**: Choose from multiple furniture models (sofas, chairs)
- **Tap to Place**: Tap on the reticle to place selected furniture
- **Selection**: Tap placed furniture to select it
- **Move**: Drag selected furniture along the floor
- **Scale**: Pinch with two fingers to resize (0.5x to 2.0x)
- **Rotate**: Two-finger rotation gesture to spin furniture
- **Delete**: Remove selected furniture with the delete button
- **Anchoring**: Furniture stays anchored to real-world positions

## Requirements

- Unity 6 (6000.0.60f1 or later)
- Xcode 15+
- iOS 13+ device with ARKit support (iPhone 6s or later, iPad 5th gen or later)
- macOS for building to iOS

## Setup

1. **Open the project** in Unity Hub with Unity 6
2. **Open** `Assets/Scenes/SampleScene.unity`
3. **Build Settings**: File → Build Settings → iOS → Switch Platform
4. **Build**: Click Build to generate Xcode project in `iOS/` folder
5. **Xcode**: Open `iOS/Unity-iPhone.xcodeproj`
6. **Sign**: Enable "Automatically manage signing" and select your team
7. **Deploy**: Connect iOS device and click Run

## Scene Hierarchy

```
SampleScene
├── AR Session              # AR lifecycle management
├── XR Origin               # AR camera and tracking
│   ├── Camera Offset
│   │   └── Main Camera
│   ├── AR Anchor Manager
│   ├── AR Raycast Manager
│   └── AR Plane Manager
├── PlacementReticle        # Floor indicator
├── AR Placement System     # Main controller + gesture handling
├── Canvas                  # UI elements
│   ├── InstructionText
│   ├── DeleteButton
│   ├── ScanningIndicator
│   └── FurnitureSelection  # Scrollable furniture picker
└── EventSystem
```

## Usage

1. **Launch the app** and point camera at the floor
2. **Wait for scanning** — move device slowly to detect surfaces
3. **Select furniture** from the bottom panel
4. **Tap the reticle** to place furniture
5. **Tap furniture** to select it (shows blue highlight)
6. **Drag** to move along the floor
7. **Pinch** to scale up/down
8. **Two-finger rotate** to turn
9. **Delete button** to remove selected furniture

## Adding New Furniture

1. Import 3D model into `Assets/`
2. **Convert materials to URP**: Select materials → change Shader to `Universal Render Pipeline/Lit`
3. Create prefab from the model
4. Add prefab to `FurnitureCatalog` on the AR Placement System object:
   - Set Display Name
   - Assign Prefab reference

Note: Colliders are automatically generated at runtime for selection raycasting.

## Technical Details

- **AR Foundation 6.0.6** with ARKit XR Plugin
- **Universal Render Pipeline (URP)** for mobile-optimized rendering
- **Enhanced Touch API** for multi-touch gesture detection
- **AR Anchors** for world-locked furniture positioning

## Troubleshooting

**Pink/magenta materials**: Materials using Built-in shaders. Convert to URP:
- Select materials → Inspector → Shader → Universal Render Pipeline/Lit

**Furniture not appearing**: Check that prefabs are assigned in FurnitureCatalog

**Cannot tap furniture**: Ensure the "Furniture" layer exists and is set in:
- ARPlacementController → Furniture Layer Mask
- ARPlacementController → Furniture Layer (layer number)

**Plane detection slow**: Move device slowly, ensure good lighting, point at textured surfaces

