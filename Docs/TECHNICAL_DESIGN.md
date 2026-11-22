# Technical Design — Unity 6

## Project intent
A landscape, third-person/isometric-style mobile zombie-survival prototype inspired by the broad survival premise of the user's requested project. This implementation uses original placeholder geometry and does not contain extracted assets from the commercial original.

## Current runtime structure
- `GameBootstrap`: runtime composition root; builds the arena, player, allies, camera, HUD, weapon system and wave manager.
- `PlayerController`: movement, touch joystick input, player health and short damage cooldown.
- `WeaponSystem`: three weapon profiles, ammunition, reload and target damage.
- `ZombieAI`: zombie archetypes, NavMeshAgent pursuit and melee attack.
- `SurvivorAI`: role-based support for Medic, Scout, Guard and Engineer; formation follow, healing and automatic fire.
- `MissionDirector`: staged mission progression from clearing the first wave through rescue, supplies and extraction.
- `WaveManager`: staged enemy spawning and wave progression.
- `GameHUD`: touch controls, status, temporary messages, pause/resume, retry and creator credit.
- `CameraFollow`: tilted follow camera.

## Navigation
The runtime creates a `NavMeshSurface` and requests a bake using physics colliders. Each spawned zombie receives a `NavMeshAgent`; spawn positions are projected onto the baked mesh with `NavMesh.SamplePosition`. A direct-movement fallback remains for cases where the agent is not on a valid NavMesh. This behavior still requires Play Mode verification and tuning.

## Input
The prototype uses Unity's legacy Input API for keyboard axes and touch input, plus Unity UI `Button` components. For first import, set Player > Active Input Handling to `Both` if the project reports that the legacy Input Manager is disabled.

## Performance goals (targets, not measured results)
- Landscape orientation.
- Aim for 30 FPS minimum on the target Android device; 60 FPS is a stretch target.
- Keep initial scene simple and avoid expensive real-time shadows until device profiling.
- Profile actual CPU, GPU, memory and thermal behavior before deciding final enemy counts and visual quality.

## Quality gates
1. **Import gate:** package resolution and C# compilation show no errors.
2. **Play Mode gate:** runtime smoke tests pass; movement, weapons, zombies, allies, rescue and wave flow are manually verified.
3. **Android gate:** debug APK installs and starts on the target device; touch, orientation, pause/resume and performance are checked.
4. **Release gate:** no known critical bugs, stable performance, final art/audio rights cleared, and save/settings flows tested.

## Current limitations
- This source has not yet been opened or compiled in Unity in the current environment.
- Automated tests are authored but have not been executed.
- The runtime-generated scene and UI are still blockout quality.
- NavMesh baking at runtime needs verification against the installed AI Navigation package version.
- No production animation, projectile visuals, audio, save system, pause/settings menu, or complete campaign flow yet.

## Run reset and pause
The prototype now includes pause/resume through `Time.timeScale`, and a retry flow that resets score, rescue count, health, ammunition, beacon position and wave state. This is source-level implementation and still requires Play Mode verification.

## Mission flow
The initial mission is a linear five-step prototype: clear wave 1, rescue at the beacon, survive until wave 3, collect the one-time supply cache, and reach extraction. The HUD displays the active objective. Interactions are gated until their objective becomes active. Completing extraction awards a score bonus. This mission flow is implemented in source and has not yet been run in Unity.

## Survivor roles
- Medic: periodically restores 10 HP when within support range.
- Scout: faster, lighter support fire.
- Guard: slower but stronger support fire.
- Engineer: balanced support fire.
All four retain the follow formation. Role behavior and balance require Play Mode testing.

## Combat feedback and environment dressing
The blockout environment now includes road markings, barricades, wrecked-vehicle shapes and street-light props. The NavMesh bake collects physics colliders, so these props may influence navigation and require scene verification. Weapon hits now require a raycast line of sight to the target; short-lived LineRenderer tracers provide basic visual feedback. Player damage triggers a brief translucent red overlay. These changes are source-level and require Play Mode validation, especially touch UI layering and raycast behavior.

## Title screen and wave pacing
A title overlay pauses simulation on startup and displays the game title, creator credit and Start Game action. Pressing Start Game hides the overlay and resumes the simulation. Cleared waves now have a four-second intermission before the next wave begins. These flows are source-level implementations and require Play Mode testing.

## Procedural character blockout
Version 0.9 adds lightweight child primitives for heads, arms, chest gear and survivor backpacks. Zombie variants receive distinct outfit colors and a jaw detail. Detail colliders are removed to avoid interfering with physics and NavMesh movement. This is a temporary procedural visual pass, not final character art; inspect scale and silhouette in Unity Play Mode.

## Lightweight procedural motion and combat feedback
Version 1.0 adds a ProceduralCharacterMotion component that detects movement from transform displacement and swings child arms/legs, with subtle idle motion. ZombieAI briefly flashes its body renderer on hit and creates a short-lived primitive puff on defeat. This is a blockout animation/effect pass; inspect renderer/material behavior and frame cost in Unity before release.
