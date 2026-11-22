# Unity 6 Prototype — Static QA Notes

Status: v1.0 source prototype; static updates and PlayMode smoke tests authored; not compiled or executed.

## Static review findings
- One runtime bootstrap creates the arena, player, camera, allies, UI and wave manager.
- Scripts are intentionally consolidated into one C# file to simplify first import.
- UI uses UnityEngine.UI and StandaloneInputModule for broad compatibility.
- Landscape orientation is requested at runtime.
- Touch joystick uses legacy `Input.touchCount`; verify Active Input Handling is set to **Both** or **Input Manager (Old)** in Player settings.
- Built-in Standard shader is used for broad initial compatibility. URP migration should happen after the first clean compile.
- The rescue beacon is moved out of view after one rescue; replace this with a multi-survivor mission flow later.

## Automated PlayMode smoke tests authored
- Bootstrap creates player, HUD and first wave
- Player damage reduces health
- Score increments
- Pause/resume toggles time scale
- Retry resets score, health, ammunition, rescue beacon and wave
- Mission stages progress from wave clear to rescue, supplies and extraction
- Medic can heal nearby player; survivor roles have distinct support damage/cadence
- Rescue beacon interaction succeeds at close range

These tests are source files only; they have not been executed in Unity.

## Required editor checks
1. Open with Unity 6 and inspect Console for compile errors.
1a. Open Window > General > Test Runner, select PlayMode, and run RuntimeSmokeTests.
2. Press Play; confirm runtime bootstrap creates the scene.
3. Test keyboard movement and all HUD buttons.
4. Test Android touch joystick and buttons in landscape.
5. Test zombie spawning, weapon damage, reload, wave progression, player death and restart flow.
6. Check UI on 16:9, 19.5:9 and 20:9 landscape screens.
7. Profile CPU/GPU/memory and thermal behavior on target phone.
8. Configure Android Build Profile and create a debug APK only after successful tests.

## New v0.8 review items
- Verify title screen blocks gameplay until Start Game is pressed
- Verify start button restores time scale and wave spawning begins
- Verify the 4-second intermission occurs after all enemies are cleared
- Verify pause/resume and retry still work after title-screen changes

## New v0.7 review items
- Verify new road props, barriers, wrecks and street lamps bake correctly into navigation
- Verify weapon raycasts hit zombies and are blocked by intervening props
- Verify tracer objects are cleaned up and do not accumulate
- Verify damage flash remains behind HUD controls and does not block touch input

## New v0.6 review items
- Verify mission stage transitions occur in the intended order
- Verify rescue and supplies buttons are gated until their objectives are active
- Verify extraction completes mission only at the extraction zone
- Verify survivor role colors, follow formation, healing and attack differences
- Verify weapon line-of-sight, tracer rendering and damage feedback

## New v0.5 review items
- Verify pause/resume behavior and retry after game over in Play Mode
- Verify retry does not leave duplicate zombies or stale UI state
- Verify damage cooldown feels fair when multiple zombies are nearby

## Known blockers
- No compile or Play Mode validation performed.
- No Android APK/export performed.
- No device validation.
- Runtime NavMesh bake and NavMeshAgent behavior must be verified in Unity; package resolution and agent placement are not tested
- No projectile physics/visuals, animation, audio, persistence, or production art.

## New v0.9 review items
- Check character detail scale and attachment positions in Play Mode
- Confirm child detail colliders are removed and do not interfere with shooting or navigation
- Check visual clarity on a 16:9 phone display and performance with a full wave active

## New v1.0 review items
- Check limb motion for player, survivors and all zombie variants
- Confirm idle motion is subtle and does not cause visual jitter
- Confirm hit flash returns to each zombie's original body color
- Confirm death puff cleans itself up and does not leave colliders
- Profile frame time with the maximum intended wave on a mid-range Android device
