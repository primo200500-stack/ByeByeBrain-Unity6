using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace ByeByeBrain
{

    public static class CharacterVisuals
    {
        private static GameObject Part(Transform parent, string name, PrimitiveType shape,
            Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(shape);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = localScale;
            Collider collider = part.GetComponent<Collider>();
            if (collider) UnityEngine.Object.Destroy(collider);
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer) renderer.material = GameBootstrap.Mat(color);
            return part;
        }

        public static void AddSurvivorDetails(Transform root, Color outfit, bool zombie)
        {
            Color skin = zombie ? new Color(.42f, .53f, .39f) : new Color(.78f, .62f, .48f);
            Color dark = zombie ? new Color(.19f, .25f, .18f) : new Color(.12f, .16f, .2f);

            Part(root, "Head", PrimitiveType.Sphere, new Vector3(0, .79f, 0),
                new Vector3(.48f, .48f, .46f), skin);
            Part(root, "Left_Arm", PrimitiveType.Capsule, new Vector3(-.39f, .12f, 0),
                new Vector3(.22f, .52f, .22f), outfit);
            Part(root, "Right_Arm", PrimitiveType.Capsule, new Vector3(.39f, .12f, 0),
                new Vector3(.22f, .52f, .22f), outfit);
            Part(root, "Left_Leg", PrimitiveType.Capsule, new Vector3(-.16f, -.48f, 0),
                new Vector3(.24f, .62f, .25f), dark);
            Part(root, "Right_Leg", PrimitiveType.Capsule, new Vector3(.16f, -.48f, 0),
                new Vector3(.24f, .62f, .25f), dark);
            Part(root, "Chest_Gear", PrimitiveType.Cube, new Vector3(0, .12f, -.205f),
                new Vector3(.42f, .38f, .055f), dark);
            if (!zombie)
            {
                Part(root, "Backpack", PrimitiveType.Cube, new Vector3(0, .1f, .28f),
                    new Vector3(.38f, .42f, .2f), new Color(.24f, .27f, .2f));
            }
            else
            {
                Part(root, "Jaw", PrimitiveType.Cube, new Vector3(0, .62f, -.12f),
                    new Vector3(.25f, .09f, .2f), new Color(.25f, .12f, .12f));
            }
            if (!root.GetComponent<ProceduralCharacterMotion>())
                root.gameObject.AddComponent<ProceduralCharacterMotion>();
        }
    }

    public class ProceduralCharacterMotion : MonoBehaviour
    {
        private Transform leftArm, rightArm, leftLeg, rightLeg, head;
        private Vector3 lastPosition;
        private float phase;

        void Awake()
        {
            leftArm = transform.Find("Left_Arm");
            rightArm = transform.Find("Right_Arm");
            leftLeg = transform.Find("Left_Leg");
            rightLeg = transform.Find("Right_Leg");
            head = transform.Find("Head");
            lastPosition = transform.position;
        }

        void Update()
        {
            float speed = (transform.position - lastPosition).magnitude / Mathf.Max(Time.deltaTime, .0001f);
            lastPosition = transform.position;
            bool moving = speed > .12f;
            phase += Time.deltaTime * (moving ? 9f : 2.2f);
            float swing = moving ? Mathf.Sin(phase) * .48f : Mathf.Sin(phase) * .035f;

            if (leftArm) leftArm.localRotation = Quaternion.Euler(swing * 55f, 0, -8f);
            if (rightArm) rightArm.localRotation = Quaternion.Euler(-swing * 55f, 0, 8f);
            if (leftLeg) leftLeg.localRotation = Quaternion.Euler(-swing * 65f, 0, 0);
            if (rightLeg) rightLeg.localRotation = Quaternion.Euler(swing * 65f, 0, 0);
            if (head)
            {
                Vector3 p = head.localPosition;
                p.y = .79f + (moving ? Mathf.Abs(Mathf.Sin(phase * 2f)) * .025f : 0f);
                head.localPosition = p;
            }
        }
    }

    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance;
        public Transform Player { get; private set; }
        public GameHUD HUD { get; private set; }
        public int Score { get; private set; }
        public int Rescued { get; private set; }
        public int Wave { get; private set; } = 1;
        public bool IsGameOver { get; private set; }
        public bool IsPaused { get; private set; }

        private WaveManager waves;
        private WeaponSystem weapons;
        private MissionDirector missions;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart()
        {
            if (FindObjectOfType<GameBootstrap>() == null)
                new GameObject("ByeByeBrain_Runtime").AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 0f;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Application.targetFrameRate = 60;
            BuildWorld();
            BuildPlayer();
            BuildAllies();
            BuildCamera();
            weapons = Player.gameObject.AddComponent<WeaponSystem>();
            BuildUI();
            waves = gameObject.AddComponent<WaveManager>();
            waves.Begin();
            missions = gameObject.AddComponent<MissionDirector>();
            missions.Begin();
        }

        private void BuildWorld()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "AbandonedTown_Ground";
            ground.transform.localScale = new Vector3(5, 1, 5);
            ground.GetComponent<Renderer>().material = Mat(new Color(0.23f, 0.25f, 0.23f));
            for (int i = 0; i < 22; i++)
            {
                Vector3 p = new Vector3(Random.Range(-22, 22), 0.7f, Random.Range(-22, 22));
                if (p.magnitude < 7) continue;
                var prop = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cube : PrimitiveType.Cylinder);
                prop.name = "RuinedBuilding_Blockout";
                prop.transform.position = p;
                prop.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                prop.transform.localScale = new Vector3(Random.Range(1.5f, 4f), Random.Range(1.2f, 3.8f), Random.Range(1.5f, 4f));
                prop.GetComponent<Renderer>().material = Mat(new Color(0.29f, 0.28f, 0.25f));
            }
            var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "Rescue_Beacon";
            beacon.transform.position = new Vector3(0, 0.2f, 12);
            beacon.transform.localScale = new Vector3(2.2f, 0.2f, 2.2f);
            beacon.GetComponent<Renderer>().material = Mat(new Color(0.1f, 0.75f, 0.9f));

            var supplies = GameObject.CreatePrimitive(PrimitiveType.Cube);
            supplies.name = "Supplies_Station";
            supplies.transform.position = new Vector3(-11, 0.8f, 3);
            supplies.transform.localScale = new Vector3(2.2f, 1.6f, 1.8f);
            supplies.GetComponent<Renderer>().material = Mat(new Color(0.72f, 0.55f, 0.22f));

            var extraction = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            extraction.name = "Extraction_Zone";
            extraction.transform.position = new Vector3(14, 0.18f, -14);
            extraction.transform.localScale = new Vector3(3.2f, 0.18f, 3.2f);
            extraction.GetComponent<Renderer>().material = Mat(new Color(0.95f, 0.68f, 0.12f));

            // Modular blockout dressing: road strips, barricades, wrecks and lamps.
            for (int i = -2; i <= 2; i++)
            {
                var roadMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roadMark.name = "Road_Marking";
                roadMark.transform.position = new Vector3(i * 2.2f, 0.025f, -1.5f);
                roadMark.transform.localScale = new Vector3(0.12f, 0.035f, 4.2f);
                roadMark.GetComponent<Renderer>().material = Mat(new Color(0.68f, 0.65f, 0.48f));
            }
            for (int i = 0; i < 5; i++)
            {
                var barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
                barrier.name = "Road_Barricade";
                barrier.transform.position = new Vector3(-15 + i * 7.5f, 0.55f, 9 + (i % 2) * 2);
                barrier.transform.rotation = Quaternion.Euler(0, (i % 2) * 12f, 0);
                barrier.transform.localScale = new Vector3(2.8f, 1.1f, 0.45f);
                barrier.GetComponent<Renderer>().material = Mat(new Color(0.42f, 0.31f, 0.22f));
            }
            for (int i = 0; i < 3; i++)
            {
                var wreck = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wreck.name = "Abandoned_Vehicle_Blockout";
                wreck.transform.position = new Vector3(-18 + i * 17f, 0.55f, -8 - (i % 2) * 5);
                wreck.transform.rotation = Quaternion.Euler(0, 18 + i * 27f, 0);
                wreck.transform.localScale = new Vector3(2.2f, 1.1f, 4.4f);
                wreck.GetComponent<Renderer>().material = Mat(new Color(0.24f, 0.29f, 0.31f));
                var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roof.name = "Vehicle_Roof";
                roof.transform.SetParent(wreck.transform, false);
                roof.transform.localPosition = new Vector3(0, 0.72f, 0);
                roof.transform.localScale = new Vector3(0.82f, 0.55f, 0.48f);
                roof.GetComponent<Renderer>().material = Mat(new Color(0.16f, 0.19f, 0.2f));
            }
            for (int i = 0; i < 4; i++)
            {
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Street_Light_Pole";
                pole.transform.position = new Vector3(-20 + i * 13f, 3.2f, 16);
                pole.transform.localScale = new Vector3(0.12f, 3.2f, 0.12f);
                pole.GetComponent<Renderer>().material = Mat(new Color(0.18f, 0.19f, 0.18f));
                var lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lamp.name = "Street_Light_Head";
                lamp.transform.position = pole.transform.position + new Vector3(0, 3.15f, 0.4f);
                lamp.transform.localScale = new Vector3(0.75f, 0.16f, 0.3f);
                lamp.GetComponent<Renderer>().material = Mat(new Color(0.75f, 0.68f, 0.42f));
            }

            // Bake navigation from the static blockout colliders for the zombie agents.
            var navObject = new GameObject("Runtime_NavMesh");
            var surface = navObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.agentTypeID = 0;
            surface.BuildNavMesh();
        }

        private void BuildPlayer()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Player";
            go.transform.position = Vector3.zero;
            go.GetComponent<Renderer>().material = Mat(new Color(0.18f, 0.55f, 0.95f));
            CharacterVisuals.AddSurvivorDetails(go.transform, new Color(0.12f, 0.32f, 0.62f), false);
            // Avoid stacking the primitive capsule collider with CharacterController.
            Destroy(go.GetComponent<CapsuleCollider>());
            var cc = go.AddComponent<CharacterController>();
            cc.height = 2f; cc.radius = 0.42f; cc.center = new Vector3(0, 1, 0);
            go.AddComponent<PlayerController>();
            Player = go.transform;
        }

        private void BuildAllies()
        {
            for (int i = 0; i < 4; i++)
            {
                var ally = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                ally.name = "Survivor_" + (i + 1);
                ally.transform.position = new Vector3((i - 1.5f) * 1.5f, 1, -2.5f);
                var survivor = ally.AddComponent<SurvivorAI>();
                survivor.Configure((SurvivorAI.Role)i, i);

            }
        }

        private void BuildCamera()
        {
            var camObj = new GameObject("Isometric_Camera");
            var cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.fieldOfView = 48;
            camObj.transform.position = new Vector3(0, 22, -19);
            camObj.transform.rotation = Quaternion.Euler(48, 0, 0);
            camObj.AddComponent<AudioListener>();
            var lightObj = new GameObject("Sun");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            CameraFollow follow = camObj.AddComponent<CameraFollow>();
            follow.target = Player;
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("Game_UI");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);
            canvasObj.AddComponent<GraphicRaycaster>();
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
            HUD = canvasObj.AddComponent<GameHUD>();
            HUD.Initialize(canvas.transform);
        }

        public void AddScore(int value) { Score += value; if (HUD) HUD.Refresh(); }
        public void SetWave(int value) { Wave = value; if (HUD) HUD.Refresh(); }
        public void RescueOne() { Rescued++; AddScore(100); if (HUD) HUD.ShowMessage("Survivor rescued!"); }
        public void DamagePlayer()
        {
            if (IsGameOver) return;
            PlayerController pc = Player.GetComponent<PlayerController>();
            pc.TakeDamage(10);
        }
        public void EndGame()
        {
            IsGameOver = true;
            IsPaused = false;
            Time.timeScale = 1f;
            if (HUD) HUD.ShowGameOver();
        }
        public void BeginGame()
        {
            if (IsGameOver) return;
            IsPaused = false;
            Time.timeScale = 1f;
            if (HUD) HUD.HideStartScreen();
        }
        public void TogglePause()
        {
            if (IsGameOver) return;
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            if (HUD) HUD.SetPauseState(IsPaused);
        }
        public void RestartRun()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            IsGameOver = false;
            Score = 0;
            Rescued = 0;
            Wave = 1;
            if (Player)
            {
                Player.position = Vector3.zero;
                Player.rotation = Quaternion.identity;
                PlayerController pc = Player.GetComponent<PlayerController>();
                pc.health = 100;
                pc.ResetDamageCooldown();
                WeaponSystem ws = Player.GetComponent<WeaponSystem>();
                if (ws) ws.ResetWeapons();
            }
            foreach (ZombieAI zombie in FindObjectsOfType<ZombieAI>())
                if (zombie) Destroy(zombie.gameObject);
            var beacon = GameObject.Find("Rescue_Beacon");
            if (beacon) beacon.transform.position = new Vector3(0, 0.2f, 12);
            if (waves) waves.Restart();
            if (missions) missions.Restart();
            var extraction = GameObject.Find("Extraction_Zone");
            if (extraction) extraction.transform.position = new Vector3(14, 0.18f, -14);
            var supplies = GameObject.Find("Supplies_Station");
            if (supplies) supplies.transform.position = new Vector3(-11, 0.8f, 3);
            if (HUD) HUD.ResetHUD();
        }
        public void Fire() { if (!IsGameOver && weapons) weapons.Fire(); }
        public void Reload() { if (weapons) weapons.Reload(); }
        public void NextWeapon() { if (weapons) weapons.NextWeapon(); }
        public void UseRescue()
        {
            if (missions == null || missions.Stage != 1)
            {
                if (HUD) HUD.ShowMessage("Rescue objective is not active yet");
                return;
            }
            if (Player && Vector3.Distance(Player.position, new Vector3(0, 0, 12)) < 4.2f)
            {
                RescueOne();
                var beacon = GameObject.Find("Rescue_Beacon");
                if (beacon) beacon.transform.position = new Vector3(0, -20, 12);
                missions.NotifyRescued();
            }
            else if (HUD) HUD.ShowMessage("Move closer to the rescue beacon");
        }

        public void UseSupplies()
        {
            if (missions == null || missions.Stage != 3)
            {
                if (HUD) HUD.ShowMessage("Supplies objective is not active yet");
                return;
            }
            if (Player && Vector3.Distance(Player.position, new Vector3(-11, 0, 3)) < 4.5f)
            {
                PlayerController pc = Player.GetComponent<PlayerController>();
                pc.health = Mathf.Min(100, pc.health + 35);
                var supplies = GameObject.Find("Supplies_Station");
                if (supplies) supplies.transform.position = new Vector3(-11, -20, 3);
                HUD.Refresh();
                HUD.ShowMessage("Supplies collected: +35 HP");
                missions.NotifySuppliesCollected();
            }
            else if (HUD) HUD.ShowMessage("Move closer to the supplies station");
        }

        public void UseExtraction()
        {
            if (missions == null || missions.Stage != 4)
            {
                if (HUD) HUD.ShowMessage("Extraction is not ready");
                return;
            }
            if (Player && Vector3.Distance(Player.position, new Vector3(14, 0, -14)) < 5f)
                missions.CompleteMission();
            else if (HUD) HUD.ShowMessage("Reach the extraction zone");
        }

        public static Material Mat(Color c)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = c;
            return m;
        }
    }

    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        void LateUpdate()
        {
            if (!target) return;
            Vector3 desired = target.position + new Vector3(0, 22, -19);
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Euler(48, 0, 0);
        }
    }

    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 6f;
        public int health = 100;
        private float nextDamageAllowed;
        private CharacterController controller;
        private Vector2 touchMove;
        private Vector2 keyboardMove;
        private int joystickFinger = -1;
        private Vector2 joystickOrigin;
        private bool useTouch;

        void Awake() { controller = GetComponent<CharacterController>(); }
        void Update()
        {
            keyboardMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 move = useTouch ? touchMove : keyboardMove;
            Vector3 direction = new Vector3(move.x, 0, move.y);
            if (direction.sqrMagnitude > 1) direction.Normalize();
            controller.SimpleMove(direction * moveSpeed);
            if (direction.sqrMagnitude > 0.05f) transform.rotation = Quaternion.LookRotation(direction);
        }
        public void BeginJoystick(int finger, Vector2 pos) { joystickFinger = finger; joystickOrigin = pos; useTouch = true; }
        public void MoveJoystick(Vector2 pos)
        {
            Vector2 delta = (pos - joystickOrigin) / 85f;
            touchMove = Vector2.ClampMagnitude(delta, 1f);
        }
        public void EndJoystick() { joystickFinger = -1; touchMove = Vector2.zero; useTouch = false; }
        public void TakeDamage(int amount)
        {
            if (Time.time < nextDamageAllowed || GameBootstrap.Instance.IsGameOver) return;
            nextDamageAllowed = Time.time + 0.65f;
            health = Mathf.Max(0, health - amount);
            if (GameBootstrap.Instance.HUD)
            {
                GameBootstrap.Instance.HUD.Refresh();
                GameBootstrap.Instance.HUD.FlashDamage();
            }
            if (health == 0) GameBootstrap.Instance.EndGame();
        }
        public void ResetDamageCooldown() { nextDamageAllowed = 0f; }
    }

    public class WeaponSystem : MonoBehaviour
    {
        private readonly string[] names = { "PISTOL", "RIFLE", "SHOTGUN" };
        private readonly int[] damage = { 25, 16, 48 };
        private readonly float[] range = { 12f, 19f, 7f };
        private readonly int[] ammo = { 48, 120, 24 };
        private readonly int[] magazine = { 12, 30, 6 };
        private readonly int[] loaded = { 12, 30, 6 };
        private readonly float[] cooldown = { .35f, .12f, .8f };
        private int selected;
        private float nextFire;
        public string WeaponLabel => names[selected] + "  " + loaded[selected] + "/" + ammo[selected];

        public void Fire()
        {
            if (Time.time < nextFire || GameBootstrap.Instance.IsPaused || GameBootstrap.Instance.IsGameOver) return;
            nextFire = Time.time + cooldown[selected];
            if (loaded[selected] <= 0) { GameBootstrap.Instance.HUD.ShowMessage("Reload!"); return; }
            loaded[selected]--;

            Vector3 origin = transform.position + Vector3.up * 1.0f;
            ZombieAI[] targets = FindObjectsOfType<ZombieAI>();
            System.Array.Sort(targets, (a, b) =>
                Vector3.Distance(origin, a.transform.position)
                .CompareTo(Vector3.Distance(origin, b.transform.position)));

            int hitCount = 0;
            int maxTargets = selected == 2 ? 3 : 1;
            foreach (ZombieAI target in targets)
            {
                if (!target) continue;
                Vector3 aimPoint = target.transform.position + Vector3.up * 0.75f;
                Vector3 direction = aimPoint - origin;
                float distance = direction.magnitude;
                if (distance > range[selected] || distance < 0.01f) continue;
                direction /= distance;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, distance + 0.25f))
                {
                    ZombieAI hitZombie = hit.collider.GetComponent<ZombieAI>();
                    if (hitZombie != target) continue;
                }
                else continue;

                target.TakeDamage(damage[selected]);
                CreateTracer(origin, aimPoint, selected == 2);
                hitCount++;
                if (hitCount >= maxTargets) break;
            }

            GameBootstrap.Instance.HUD.Refresh();
        }

        private void CreateTracer(Vector3 from, Vector3 to, bool shotgun)
        {
            var tracerObject = new GameObject("Weapon_Tracer");
            var line = tracerObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startWidth = shotgun ? 0.075f : 0.045f;
            line.endWidth = 0.012f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = shotgun ? new Color(1f, 0.72f, 0.32f, 0.95f) : new Color(1f, 0.9f, 0.55f, 0.9f);
            line.endColor = new Color(1f, 0.55f, 0.2f, 0.1f);
            line.useWorldSpace = true;
            Destroy(tracerObject, 0.075f);
        }
        public void Reload()
        {
            int needed = magazine[selected] - loaded[selected];
            int amount = Mathf.Min(needed, ammo[selected]);
            loaded[selected] += amount; ammo[selected] -= amount;
            GameBootstrap.Instance.HUD.Refresh();
        }
        public void NextWeapon() { selected = (selected + 1) % names.Length; GameBootstrap.Instance.HUD.Refresh(); }
        public void ResetWeapons()
        {
            selected = 0;
            ammo[0] = 48; ammo[1] = 120; ammo[2] = 24;
            loaded[0] = 12; loaded[1] = 30; loaded[2] = 6;
            nextFire = 0f;
        }
    }

    public static class ZombieSearch
    {
        public static ZombieAI FindClosest(this ZombieAI[] list, Vector3 pos, float maxRange)
        {
            ZombieAI best = null; float bestD = maxRange;
            foreach (var z in list)
            {
                if (!z) continue;
                float d = Vector3.Distance(pos, z.transform.position);
                if (d < bestD) { bestD = d; best = z; }
            }
            return best;
        }
    }

    public class ZombieAI : MonoBehaviour
    {
        public enum Kind { Walker, Runner, Brute }
        public Kind type;
        public int health = 60;
        public float speed = 1.8f;
        public NavMeshAgent Agent { get; set; }
        private float attackTimer;
        private Renderer bodyRenderer;
        private Color baseColor;
        private float hitFlashUntil;
        public void Configure(Kind kind)
        {
            type = kind;
            bodyRenderer = GetComponent<Renderer>();
            if (kind == Kind.Runner) { health = 35; speed = 3.7f; GetComponent<Renderer>().material.color = new Color(.8f,.65f,.15f); }
            else if (kind == Kind.Brute) { health = 150; speed = 1.05f; transform.localScale = new Vector3(1.35f,1.4f,1.35f); GetComponent<Renderer>().material.color = new Color(.55f,.15f,.15f); }
            if (Agent) Agent.speed = speed;
            if (bodyRenderer) baseColor = bodyRenderer.material.color;
            Color outfit = kind == Kind.Runner ? new Color(.48f,.48f,.18f) :
                kind == Kind.Brute ? new Color(.32f,.12f,.12f) : new Color(.25f,.34f,.24f);
            CharacterVisuals.AddSurvivorDetails(transform, outfit, true);
        }
        void Update()
        {
            if (GameBootstrap.Instance == null || GameBootstrap.Instance.IsGameOver ||
                GameBootstrap.Instance.IsPaused) return;
            if (bodyRenderer && Time.time < hitFlashUntil)
                bodyRenderer.material.color = Color.white;
            else if (bodyRenderer)
                bodyRenderer.material.color = baseColor;
            Transform player = GameBootstrap.Instance.Player;
            Vector3 delta = player.position - transform.position; delta.y = 0;
            float dist = delta.magnitude;
            if (dist > 1.5f)
            {
                if (Agent && Agent.enabled && Agent.isOnNavMesh)
                {
                    Agent.isStopped = false;
                    Agent.SetDestination(player.position);
                }
                else
                {
                    // Fallback if runtime NavMesh baking fails or the agent spawns off-mesh.
                    Vector3 desired = delta.normalized;
                    transform.position += desired * speed * Time.deltaTime;
                    if (desired.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(desired);
                }
            }
            else
            {
                if (Agent && Agent.enabled && Agent.isOnNavMesh) Agent.isStopped = true;
                if (Time.time > attackTimer) { attackTimer = Time.time + 1.2f; GameBootstrap.Instance.DamagePlayer(); }
            }
        }
        public void TakeDamage(int amount)
        {
            if (health <= 0) return;
            health -= amount;
            hitFlashUntil = Time.time + .09f;
            if (health <= 0)
            {
                GameBootstrap.Instance.AddScore(type == Kind.Brute ? 50 : 20);
                CreateDeathPuff();
                Destroy(gameObject);
            }
        }

        private void CreateDeathPuff()
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = "Zombie_Down_Effect";
            puff.transform.position = transform.position + Vector3.up * .8f;
            puff.transform.localScale = Vector3.one * (type == Kind.Brute ? 1.5f : .85f);
            Renderer r = puff.GetComponent<Renderer>();
            if (r) r.material = GameBootstrap.Mat(new Color(.45f, .12f, .12f, .7f));
            Collider c = puff.GetComponent<Collider>();
            if (c) Destroy(c);
            Destroy(puff, .18f);
        }
    }

    public class SurvivorAI : MonoBehaviour
    {
        public enum Role { Medic, Scout, Guard, Engineer }
        public Role role;
        private int formationSlot;
        private float fireTimer;
        private float healTimer;

        public void Configure(Role survivorRole, int slot)
        {
            role = survivorRole;
            formationSlot = Mathf.Clamp(slot, 0, 3);
            Renderer renderer = GetComponent<Renderer>();
            if (renderer)
            {
                Color[] roleColors =
                {
                    new Color(0.25f, 0.85f, 0.55f), // Medic
                    new Color(0.25f, 0.65f, 0.95f), // Scout
                    new Color(0.9f, 0.55f, 0.2f),   // Guard
                    new Color(0.72f, 0.65f, 0.9f)   // Engineer
                };
                renderer.material = GameBootstrap.Mat(roleColors[formationSlot]);
                CharacterVisuals.AddSurvivorDetails(transform, roleColors[formationSlot], false);
            }
            gameObject.name = role.ToString() + "_Survivor";
        }

        void Update()
        {
            if (GameBootstrap.Instance == null || GameBootstrap.Instance.IsGameOver ||
                GameBootstrap.Instance.IsPaused) return;

            Transform player = GameBootstrap.Instance.Player;
            if (!player) return;

            PlayerController playerController = player.GetComponent<PlayerController>();
            float playerDistance = Vector3.Distance(transform.position, player.position);

            // Medic periodically restores a small amount of health when close to the player.
            if (role == Role.Medic && playerDistance < 5f &&
                playerController.health < 100 && Time.time >= healTimer)
            {
                healTimer = Time.time + 8f;
                playerController.health = Mathf.Min(100, playerController.health + 10);
                if (GameBootstrap.Instance.HUD) GameBootstrap.Instance.HUD.Refresh();
                GameBootstrap.Instance.HUD.ShowMessage("Medic: +10 HP");
            }

            ZombieAI target = FindObjectsOfType<ZombieAI>().FindClosest(transform.position, 10f);
            if (target && Time.time > fireTimer)
            {
                float interval = role == Role.Scout ? 0.75f : role == Role.Guard ? 1.15f : 1.35f;
                int damage = role == Role.Guard ? 16 : role == Role.Engineer ? 11 : role == Role.Scout ? 8 : 5;
                fireTimer = Time.time + interval;
                target.TakeDamage(damage);
                Vector3 aim = target.transform.position - transform.position;
                aim.y = 0;
                if (aim.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(aim);
            }
            else
            {
                Vector3[] offsets =
                {
                    new Vector3(-2.0f, 0, -2.4f), new Vector3(2.0f, 0, -2.4f),
                    new Vector3(-3.1f, 0, -4.0f), new Vector3(3.1f, 0, -4.0f)
                };
                Vector3 follow = player.position + offsets[formationSlot];
                transform.position = Vector3.MoveTowards(transform.position, follow, 2.6f * Time.deltaTime);
            }
        }
    }

    public class WaveManager : MonoBehaviour
    {
        private int remaining;
        private float nextSpawn;
        private bool intermission;
        private float intermissionEnds;
        public void Begin() { StartWave(1); }
        public void Restart()
        {
            intermission = false;
            foreach (ZombieAI zombie in FindObjectsOfType<ZombieAI>())
                if (zombie) Destroy(zombie.gameObject);
            StartWave(1);
        }
        private void StartWave(int wave)
        {
            intermission = false;
            GameBootstrap.Instance.SetWave(wave);
            remaining = 4 + wave * 2;
            nextSpawn = Time.time + 1f;
            if (GameBootstrap.Instance.HUD) GameBootstrap.Instance.HUD.ShowMessage("WAVE " + wave);
        }
        void Update()
        {
            if (GameBootstrap.Instance.IsGameOver || GameBootstrap.Instance.IsPaused) return;
            if (intermission)
            {
                if (Time.time >= intermissionEnds)
                    StartWave(GameBootstrap.Instance.Wave + 1);
                return;
            }
            if (remaining > 0 && Time.time >= nextSpawn)
            {
                SpawnZombie();
                remaining--;
                nextSpawn = Time.time + Mathf.Max(.45f, 1.5f - GameBootstrap.Instance.Wave * .05f);
            }
            if (remaining == 0 && FindObjectsOfType<ZombieAI>().Length == 0)
            {
                intermission = true;
                intermissionEnds = Time.time + 4f;
                if (GameBootstrap.Instance.HUD) GameBootstrap.Instance.HUD.ShowMessage("AREA CLEAR - NEXT WAVE IN 4 SECONDS");
            }
        }
        private void SpawnZombie()
        {
            Vector2 edge = Random.insideUnitCircle.normalized * Random.Range(15f, 22f);
            var z = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            z.name = "Zombie";
            z.transform.position = new Vector3(edge.x, 1, edge.y);
            z.GetComponent<Renderer>().material = GameBootstrap.Mat(new Color(.35f,.48f,.3f));
            var agent = z.AddComponent<NavMeshAgent>();
            agent.radius = 0.38f;
            agent.height = 2f;
            agent.acceleration = 12f;
            agent.angularSpeed = 540f;
            agent.stoppingDistance = 1.45f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            var ai = z.AddComponent<ZombieAI>();
            ai.Agent = agent;
            if (NavMesh.SamplePosition(z.transform.position, out NavMeshHit spawnHit, 5f, NavMesh.AllAreas))
            {
                z.transform.position = spawnHit.position;
                agent.Warp(spawnHit.position);
            }
            float r = Random.value;
            ai.Configure(r < .18f ? ZombieAI.Kind.Brute : r < .48f ? ZombieAI.Kind.Runner : ZombieAI.Kind.Walker);
        }
    }

    public class MissionDirector : MonoBehaviour
    {
        // 0 clear wave 1, 1 rescue, 2 survive until wave 3,
        // 3 collect supplies, 4 extract, 5 complete.
        public int Stage { get; private set; }
        private string[] objectives =
        {
            "Clear the first zombie wave",
            "Reach the rescue beacon and rescue the survivor",
            "Survive until wave 3",
            "Collect the medical supplies",
            "Reach the extraction zone",
            "MISSION COMPLETE"
        };

        public void Begin()
        {
            Stage = 0;
            UpdateObjective();
        }

        public void Restart()
        {
            Stage = 0;
            UpdateObjective();
        }

        void Update()
        {
            if (GameBootstrap.Instance == null || GameBootstrap.Instance.IsGameOver ||
                GameBootstrap.Instance.IsPaused) return;

            if (Stage == 0 && GameBootstrap.Instance.Wave >= 2)
                SetStage(1);
            else if (Stage == 2 && GameBootstrap.Instance.Wave >= 3)
                SetStage(3);
        }

        public void NotifyRescued()
        {
            if (Stage == 1) SetStage(2);
        }

        public void NotifySuppliesCollected()
        {
            if (Stage == 3) SetStage(4);
        }

        public void CompleteMission()
        {
            if (Stage != 4) return;
            SetStage(5);
            GameBootstrap.Instance.AddScore(500);
            if (GameBootstrap.Instance.HUD) GameBootstrap.Instance.HUD.ShowMessage("MISSION COMPLETE!");
        }

        private void SetStage(int stage)
        {
            Stage = Mathf.Clamp(stage, 0, objectives.Length - 1);
            UpdateObjective();
            if (GameBootstrap.Instance.HUD)
                GameBootstrap.Instance.HUD.ShowMessage(objectives[Stage]);
        }

        private void UpdateObjective()
        {
            if (GameBootstrap.Instance && GameBootstrap.Instance.HUD)
                GameBootstrap.Instance.HUD.SetObjective(objectives[Stage]);
        }
    }

    public class GameHUD : MonoBehaviour
    {
        private Text status, message, credit, objective;
        private Image damageFlash;
        private GameObject gameOverPanel;
        private Button restartButton;
        private Button pauseButton;
        private GameObject startPanel;
        private PlayerController player;
        private RectTransform joystickArea;
        private int activeFinger = -1;
        private float messageUntil;

        public void Initialize(Transform parent)
        {
            player = GameBootstrap.Instance.Player.GetComponent<PlayerController>();
            status = MakeText(parent, "STATUS", new Vector2(.02f,.95f), new Vector2(.4f,.99f), 25, TextAnchor.UpperLeft);
            message = MakeText(parent, "MESSAGE", new Vector2(.3f,.84f), new Vector2(.7f,.94f), 27, TextAnchor.MiddleCenter);
            objective = MakeText(parent, "OBJECTIVE", new Vector2(.32f,.88f), new Vector2(.72f,.95f), 18, TextAnchor.UpperCenter);
            credit = MakeText(parent, "CREDIT", new Vector2(.72f,.02f), new Vector2(.98f,.07f), 18, TextAnchor.LowerRight);
            credit.text = "Created by ZIAD ABOLBADA";
            var flashObject = new GameObject("Damage_Flash");
            flashObject.transform.SetParent(parent, false);
            var flashRect = flashObject.AddComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;
            damageFlash = flashObject.AddComponent<Image>();
            damageFlash.color = new Color(0.9f, 0.02f, 0.02f, 0f);
            damageFlash.raycastTarget = false;
            var joy = MakeButton(parent, "MOVE", new Vector2(.03f,.08f), new Vector2(.2f,.34f), () => { });
            joystickArea = joy.GetComponent<RectTransform>();
            MakeButton(parent, "FIRE", new Vector2(.84f,.12f), new Vector2(.97f,.34f), () => GameBootstrap.Instance.Fire());
            MakeButton(parent, "RELOAD", new Vector2(.72f,.08f), new Vector2(.83f,.18f), () => GameBootstrap.Instance.Reload());
            MakeButton(parent, "WEAPON", new Vector2(.72f,.2f), new Vector2(.83f,.3f), () => GameBootstrap.Instance.NextWeapon());
            MakeButton(parent, "RESCUE", new Vector2(.42f,.06f), new Vector2(.58f,.16f), () => GameBootstrap.Instance.UseRescue());
            MakeButton(parent, "SUPPLIES", new Vector2(.42f,.18f), new Vector2(.58f,.28f), () => GameBootstrap.Instance.UseSupplies());
            MakeButton(parent, "EXTRACT", new Vector2(.42f,.30f), new Vector2(.58f,.40f), () => GameBootstrap.Instance.UseExtraction());
            pauseButton = MakeButton(parent, "PAUSE", new Vector2(.88f,.88f), new Vector2(.98f,.97f), () => GameBootstrap.Instance.TogglePause());
            restartButton = MakeButton(parent, "RETRY", new Vector2(.42f,.38f), new Vector2(.58f,.49f), () => GameBootstrap.Instance.RestartRun());
            restartButton.gameObject.SetActive(false);
            BuildStartScreen(parent);
            Refresh();
        }

        private void BuildStartScreen(Transform parent)
        {
            startPanel = new GameObject("Title_Screen");
            startPanel.transform.SetParent(parent, false);
            var panelRect = startPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var background = startPanel.AddComponent<Image>();
            background.color = new Color(0.025f, 0.045f, 0.055f, 0.94f);

            Text title = MakeText(startPanel.transform, "Game_Title",
                new Vector2(.12f,.58f), new Vector2(.88f,.82f), 54, TextAnchor.MiddleCenter);
            title.text = "BYE BYE BRAIN";
            title.fontStyle = FontStyle.Bold;

            Text subtitle = MakeText(startPanel.transform, "Game_Subtitle",
                new Vector2(.16f,.46f), new Vector2(.84f,.59f), 23, TextAnchor.MiddleCenter);
            subtitle.text = "APP-OCALYPSE  |  SURVIVE THE OUTBREAK";

            Text creator = MakeText(startPanel.transform, "Creator_Credit",
                new Vector2(.2f,.35f), new Vector2(.8f,.43f), 20, TextAnchor.MiddleCenter);
            creator.text = "Created by ZIAD ABOLBADA";

            MakeButton(startPanel.transform, "START GAME",
                new Vector2(.38f,.16f), new Vector2(.62f,.29f),
                () => GameBootstrap.Instance.BeginGame());
        }

        public void HideStartScreen()
        {
            if (startPanel) startPanel.SetActive(false);
        }

        private Text MakeText(Transform parent, string name, Vector2 min, Vector2 max, int size, TextAnchor anchor)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>(); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            var t = go.AddComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize=size; t.color=Color.white; t.alignment=anchor; t.horizontalOverflow=HorizontalWrapMode.Overflow;
            return t;
        }
        private Button MakeButton(Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label+"_Button"); go.transform.SetParent(parent,false);
            var rt=go.AddComponent<RectTransform>(); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            var img=go.AddComponent<Image>(); img.color=new Color(.06f,.1f,.13f,.78f);
            var b=go.AddComponent<Button>(); b.onClick.AddListener(action);
            var txt=MakeText(go.transform,"Label",Vector2.zero,Vector2.one,20,TextAnchor.MiddleCenter); txt.text=label;
            return b;
        }
        public void Refresh()
        {
            if (!status) return;
            var pc = GameBootstrap.Instance.Player.GetComponent<PlayerController>();
            var ws = GameBootstrap.Instance.Player.GetComponent<WeaponSystem>();
            status.text = "HP " + pc.health + "   WAVE " + GameBootstrap.Instance.Wave + "   SCORE " + GameBootstrap.Instance.Score + "\n" + ws.WeaponLabel;
        }
        public void FlashDamage()
        {
            if (damageFlash) StartCoroutine(DamageFlashRoutine());
        }
        private System.Collections.IEnumerator DamageFlashRoutine()
        {
            damageFlash.color = new Color(0.9f, 0.02f, 0.02f, 0.28f);
            float elapsed = 0f;
            const float duration = 0.22f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0.28f, 0f, elapsed / duration);
                damageFlash.color = new Color(0.9f, 0.02f, 0.02f, alpha);
                yield return null;
            }
            damageFlash.color = new Color(0.9f, 0.02f, 0.02f, 0f);
        }
        public void SetObjective(string text) { if (objective) objective.text = "MISSION: " + text; }
        public void ShowMessage(string text) { message.text=text; messageUntil=Time.time+2.5f; }
        public void ShowGameOver()
        {
            message.text="YOU WERE OVERRUN";
            messageUntil=float.MaxValue;
            if (restartButton) restartButton.gameObject.SetActive(true);
        }
        public void SetPauseState(bool paused)
        {
            if (pauseButton)
            {
                Text label = pauseButton.GetComponentInChildren<Text>();
                if (label) label.text = paused ? "RESUME" : "PAUSE";
            }
            if (paused) ShowMessage("PAUSED");
            else { message.text = ""; messageUntil = 0f; }
        }
        public void ResetHUD()
        {
            if (restartButton) restartButton.gameObject.SetActive(false);
            if (pauseButton)
            {
                Text label = pauseButton.GetComponentInChildren<Text>();
                if (label) label.text = "PAUSE";
            }
            message.text = "";
            messageUntil = 0f;
            Refresh();
        }
        void Update()
        {
            if (message && Time.time > messageUntil) message.text="";
            if (joystickArea == null || player == null) return;
            Vector2 screen = Vector2.zero;
            bool found = false;
            for (int i=0;i<Input.touchCount;i++)
            {
                Touch t=Input.GetTouch(i);
                if (activeFinger == -1 && t.phase==TouchPhase.Began && t.position.x < Screen.width*.35f && t.position.y < Screen.height*.55f)
                { activeFinger=t.fingerId; player.BeginJoystick(t.fingerId,t.position); }
                if (t.fingerId==activeFinger)
                {
                    if (t.phase==TouchPhase.Ended || t.phase==TouchPhase.Canceled) { player.EndJoystick(); activeFinger=-1; }
                    else { screen=t.position; found=true; }
                }
            }
            if (found) player.MoveJoystick(screen);
        }
    }
}
