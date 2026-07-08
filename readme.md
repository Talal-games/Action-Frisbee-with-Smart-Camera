# Physics-Based Frisbee Controller with Dynamic Cinematic Camera

## Overview

This skill assessment is a small Unity gameplay system extracted from a larger work-in-progress PC frisbee game. The system focuses on a minimal, arcade-style frisbee controller paired with a dynamic camera system that supports both precise aiming and cinematic high-speed movement.

The goal of the system is to capture the feeling of throwing and controlling a frisbee: the player can choose strength and direction, but once the frisbee is released, they must watch and respond as its path curves through space. The mechanic is designed around fast movement, tactile feedback, readable targets, and camera behavior that makes the action feel both skill-based and cinematic.

The system is built around two core parts:

1. A physics-based frisbee controller with lock-on targeting, charged throws, directional influence, and curved auto-guidance.
2. A dynamic camera system with an aiming mode and a cinematic “helicopter” mode for high-speed action moments.

---

## Frisbee Controller

The frisbee controller controls a minimal, action-focused frisbee object. The player locks on to the nearest valid target, charges the strength of the throw, adjusts the throw direction, and releases the frisbee toward the target.

The player flow is:

1. The system finds or receives a valid target.
2. The player holds the action button to charge throw strength.
3. The player can hold left or right to influence the throw direction/curve.
4. On release, the frisbee is launched.
5. If the throw is not directly aligned with the target, the frisbee curves and auto-guides back toward it.

The mechanic is inspired by the real feeling of throwing a frisbee. In real life, the thrower controls the strength and initial direction, but the frisbee’s path still curves through the air. I wanted to capture that feeling in a more arcade-like form: the player makes a skillful decision at the moment of release, then watches the path bend and respond.

Important tunable values include:

* throw strength range
* curve strength
* target lock-on range
* guidance amount
* speed limits
* input sensitivity
* timing values for charge and release

The system is designed so these values can be adjusted in the Unity Inspector without rewriting the core code. This makes the mechanic easier to tune for different target layouts, level shapes, and difficulty levels.

---

## Dynamic Camera System

The camera system was developed to solve a central problem in the frisbee game: the game needs both precision and spectacle.

One camera mode prioritizes aiming and gives the player a more traditional third-person view. This allows the player to look around, read the space, aim toward targets, and make precise decisions before throwing.

The second mode is a cinematic “helicopter view.” This mode uses Cinemachine and custom camera logic to follow the frisbee during high-speed movement. It is designed for gameplay moments where the player has less direct control and the game needs to highlight the motion: big throws, jumps, fast movement, ramps, impacts, or other action events.

The camera system has two main modes:

### Aiming Mode

Aiming mode prioritizes control and readability. It gives the player a stable view for choosing direction, reading targets, and preparing the throw. This mode is closer to a traditional third-person camera because the player needs precision and spatial understanding.

### Cinematic / Helicopter Mode

The cinematic mode is designed to follow high-speed action from a more dramatic angle. It uses Cinemachine obstacle avoidance together with custom scripts that decide which side or angle the camera should approach from.

The system exposes important settings in the editor, including:

* camera target
* camera distance/size
* transition timing
* flow/smoothness values
* obstacle avoidance behavior
* angle-switch timing
* randomness
* stickiness

The custom direction setter chooses possible camera angles and scores them based on obstruction. It can randomly choose a candidate angle, check how obstructed it is, and switch to a better view when appropriate.

“Stickiness” controls how willing the camera is to change angles. Higher stickiness means the camera will stay with its current angle longer and avoid constant switching. Lower stickiness allows the camera to change angles more often when it finds a clearer or more cinematic option.

This camera behavior is important because the frisbee mechanic is not fully readable or playable without the right camera. The system is not just visual presentation; it is part of the gameplay feel.

---
