# Bye Bye Brain: App-ocalypse — Unity 6 Prototype

Creator credit in game: **Created by ZIAD ABOLBADA**

## What this is
A fresh Unity 6 source prototype that begins the Godot-to-Unity rebuild. The current scene is generated at runtime from primitive placeholder geometry so the project can be opened without imported art assets.

## Open
Unity AI Navigation is included as a package dependency (`com.unity.ai.navigation`). Allow Unity Package Manager to resolve it on first open.

1. Install a current Unity 6 editor with Android Build Support, Android SDK & NDK Tools, and OpenJDK.
2. In Unity Hub choose **Add > Add project from disk** and select this folder.
3. Open the project and allow package import/compilation to finish.
4. Press Play. The prototype bootstraps itself after the first scene loads.
5. For Android, switch Build Profiles to Android, set landscape orientation, package identifier, and build a debug APK.

## Current prototype systems
- Landscape orientation and 60 FPS target
- Isometric-style follow camera and blockout abandoned-town arena
- Player movement (keyboard in Editor; left-side touch joystick on device), with a short incoming-damage cooldown
- Touch UI: fire, reload, weapon switch, rescue, supplies, pause/resume and retry after game over
- Pistol, rifle, shotgun with separate ammo and fire rates
- Walker, runner, brute zombie variants
- Four role-based survivor allies: Medic (periodic healing), Scout (fast support fire), Guard (higher damage), Engineer (balanced support)
- Environment dressing: road markings, barricades, wrecked-car blockouts and street-light props
- Weapon line-of-sight checks and brief tracer visuals
- Brief red screen flash when the player takes damage
- Wave spawning with a 4-second intermission between cleared waves, score, health and game-over state
- Mission sequence: clear wave 1, rescue a survivor, survive to wave 3, collect supplies, then extract
- Rescue beacon interaction and one-time supplies station (+35 HP, capped at 100)
- Prototype shotgun spread (up to three nearby targets)
- Basic zombie obstacle steering using sphere casts (not full NavMesh)
- Creator credit

## Automated tests
A PlayMode test assembly is included under `Assets/Tests/PlayMode`. After Unity resolves the Test Framework package, open **Window > General > Test Runner**, select **PlayMode**, and run `RuntimeSmokeTests`.

## Important limitations
- This is source code, not an APK.
- It has not been opened, compiled, or run in the Unity editor in this environment. The included automated tests are authored but not executed; the project is not runtime-verified.
- The current world uses Unity primitive shapes as blockout art.
- Zombies now use Unity AI Navigation NavMeshAgents and a runtime NavMesh bake from the blockout colliders, with a direct-movement fallback if an agent is not placed on the baked mesh.
- Combat currently damages the nearest zombie in range; no projectile visuals, line of sight, muzzle effects, hit reactions, or animation.
- Survivor AI is intentionally simple.
- The rescue beacon is a single prototype interaction, not a complete mission system.
- Save/load, audio, final models, textures, animation, settings, localization, and performance profiling remain to be implemented.
- Android export still requires a local Unity 6 installation and Android build modules.

## Controls
Editor: WASD / arrow keys to move. Use on-screen buttons for fire, reload, weapon switching and rescue.
Android: left lower screen touch-drag to move; right-side buttons for actions.

## Next development milestones
1. Compile and fix any Unity-version/API issues in the editor.
2. Replace blockout geometry with modular low-poly environment assets.
3. Validate runtime NavMesh baking and tune agent radius, avoidance, and obstacle traversal.
4. Add real projectile/shotgun spread, hit effects, animations and sound.
5. Improve survivor formation, health and rescue objectives.
6. Add menus, pause/settings, save data, and proper Android safe-area layout.
7. Profile and optimize on the TECNO Spark 20 before release builds.

## Rights note
This prototype contains no extracted assets, code, music, or recordings from the original commercial game. Any original-game assets or soundtrack require appropriate rights/licensing.


## v0.9 visual pass
- Added lightweight procedural head, arm, chest-gear and backpack details to the player and rescued allies.
- Added distinct zombie head, arms and jaw details, with outfit colors differentiated by role/type.
- These are generated primitive meshes, not original game art or imported character models. They are intended as a readable low-cost blockout while dedicated art assets are unavailable.


## v1.0 combat motion pass
- Added low-cost procedural idle and locomotion limb motion for player, survivors and zombies.
- Added a brief hit flash and a short-lived death puff when a zombie is defeated.
- The motion is procedural and intentionally lightweight; it is not a replacement for authored skeletal animation.
