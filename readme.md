# Action Frisbee with Smart Camera

A Unity prototype for a physics-based frisbee controller and a dynamic cinematic camera. The player aims in third person, charges a throw, curves the frisbee around obstacles, locks onto targets, and chains throws by hitting targets.

For the short technical write-up, see [technical_report.md](technical_report.md) or [frisbee_controller_camera_report.pdf](frisbee_controller_camera_report.pdf).

## Setup

1. Clone or download this repository from GitHub.
2. Open the project in Unity Hub using Unity `2022.3.40f1`.
3. Let Unity import the project and restore packages from `Packages/manifest.json`.
4. Open `Assets/Scenes/SampleScene.unity`.
5. If FMOD shows a setup or migration prompt, click through it or ignore it unless you need to modify audio events. The gameplay scripts can still be inspected and tuned.
6. Press Play.

## Controls

| Action | Input |
| --- | --- |
| Aim camera | Mouse / Cinemachine FreeLook input |
| Charge throw | Hold `Jump` (`Space` by default) |
| Throw | Release `Jump` after charging past minimum throw power |
| Influence throw angle or flight turn | `Horizontal` axis (`A/D` or Left/Right arrows by default) |
| Start another throw while flying | Hold `Jump` when throws are available |
| Restart scene | `P` |

The project uses Unity's old input manager names, mainly `Jump` and `Horizontal`.

## Player Flow

1. The player moves the camera toward the desired target.
2. `TargetSelector` chooses the valid target closest to the screen center.
3. The player holds the action button to charge throw strength.
4. The player can hold left or right to influence the throw angle.
5. Releasing the action button launches the frisbee.
6. While flying, the frisbee can turn and curve toward the stored turning target.
7. Hitting a target refills throws so the player can continue chaining shots.

## Main Scripts

| Script | Role |
| --- | --- |
| `PlayerInputController` | Reads input and manages input buffers. |
| `PlayerController` | Coordinates gameplay decisions, state changes, throw count, collisions, cameras, and UI. |
| `PlayerStateController` | Runs the player state stack. |
| `ThrowingState`, `FlyingState`, `DeadState` | Decide which systems run during each player mode. |
| `FrisbeeMovementController` | Owns Rigidbody movement, throw power, turning, curving, and reset tuning. |
| `TargetSelector` | Chooses aim and turning targets using camera/screen position. |
| `TargetCollisionHandler` | Represents hittable targets and target-side collision behavior. |
| `PlayerCameraController` | Switches between third-person aiming and helicopter flight cameras. |
| `FreeLookVelocityAngleSetter` | Prepares the third-person camera around the frisbee's movement direction. |
| `HeliCameraDirectionSetter` | Generates, evaluates, and applies cinematic helicopter camera angles. |
| `PlayerFeedback`, `TargetFeedback`, `AimVisual`, `PlayerStatsUI` | Handle audio, particles, aim visuals, and throw-count UI. |

## Modularity

The player controller and camera are designed as separate but cooperating systems. The player side owns input, state, physics movement, target selection, collisions, and throw count. The camera side reads the frisbee's position and velocity so it can support aiming and cinematic flight without owning the frisbee rules.

### Using the Player Controller in Another Project

Use `Assets/Prefabs/Player and Cameras.prefab` as the fastest starting point. The core player scripts live in `Assets/Scripts/Player and Camera`, with target/collision helpers in `Assets/Scripts/Behaviour`.

The player object expects:

- A child Rigidbody for the frisbee.
- `PlayerController`, `PlayerInputController`, `PlayerStateController`, `FrisbeeMovementController`, `TargetSelector`, and `PlayerFeedback`.
- Target objects using `TargetCollisionHandler`.
- Collision layers assigned for targets and lethal obstacles.
- Optional `PlayerStatsUI`, `AimVisual`, `TargetFeedback`, and FMOD audio objects.

### Using the Camera in Another Project

The camera system requires Cinemachine. Use `PlayerCameraController` with a `CinemachineFreeLook` for aiming and a `CinemachineVirtualCamera` for helicopter flight.

The helicopter camera behavior is tuned through `HeliCameraDirectionSetter`. It needs a Rigidbody target, shot-blocking layers, and Cinemachine camera references. It can be reused for another fast-moving object as long as that object exposes a Rigidbody and a clear follow/look-at setup.

## Important Tunable Values

| Component | Values to tune |
| --- | --- |
| `PlayerController` | `startingThrows`, `throwsAddedPerTargetHit`, `collisionThreshold`, target and lethal obstacle layers. |
| `PlayerInputController` | `jumpBufferTime`, `switchingBufferTime`. |
| `FrisbeeMovementController` | `angleRate`, `maxAimAngle`, `throwPowerRate`, `minThrowPower`, `maxThrowPower`, `aimAtTargetSpeed`, `turnRate`, `autoTurnThreshold`, `ySpeed`, reset settings. |
| `TargetSelector` | `targetSearchRadius`, `maxScreenDistanceFromCenter`, `targetSwitchCooldown`, `switchThresholdRatio`. |
| `HeliCameraDirectionSetter` | `changeInterval`, `rotationSpeed`, `minimumAngleSeparation`, camera distance multipliers, `shotBoxSecondsAhead`, `shotBoxThickness`, `occupancySamples`, `shotBlockLayers`. |
| `AimVisual` | Arrow usage, arrow scale/color, aim particles, throw-power particles. |
| `PlayerFeedback` and `TargetFeedback` | Charge light, FMOD sounds, target effects, turning target effects. |

## Project Notes

- Main scene: `Assets/Scenes/SampleScene.unity`
- Main prefab: `Assets/Prefabs/Player and Cameras.prefab`
- Unity version: `2022.3.40f1`
- Key packages: Cinemachine, URP, ProBuilder, TextMeshPro, Unity UI, FMOD
