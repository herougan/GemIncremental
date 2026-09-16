using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Monster;
using TowerDefence.Entity.Tower;
using TowerDefence.Map;
using UnityEngine;
using Util.Game;

namespace TowerDefence.Manager
{
	/// <summary>
	/// MVP smoke-test spawner, not a real spawn system (EntityWaveManager is that) - drop onto an empty
	/// GameObject in a scene, assign a Tower/Monster prefab (see ExampleContentGenerator's
	/// BasicTower/BasicMonster) and a matching Plan asset (its generated Towers/Monsters), hit Play.
	///
	/// Spawns one of each, TowerPosition/MonsterPosition apart, so the Tower's real targeting/attack
	/// loop (TowerAttackController) has something in range to fire at immediately - this is the whole
	/// pipeline built this session (StatMods, Passives, ProjectileController flight, DamageCalculator,
	/// Nullifier/Shield absorb order, the event bus) actually running, not a mock of it.
	///
	/// Also (optionally) builds a test Map: assign MapContainer (ExampleContentGenerator's generic
	/// MapContainer prefab - it just holds/builds whatever MapPlan it's given, nothing test-specific
	/// about it) and MapPlan (its TestMap) to load+build the map's gamespace and resolve its Source(s) -> Destination
	/// paths on Start, alongside the Tower/Monster above. Once a Map is built, a small IMGUI panel (OnGUI
	/// below - no Canvas/EventSystem needed) lets you interact with it directly:
	///  - "Spawn Monster at Source" - spawns MonsterPlan at the map's first Source tile, and (unlike the
	///    Start-time auto-spawn above) hands it that Source's resolved path (MapGenerator.Paths) via
	///    MonsterController.SetPath, so it actually walks to the Destination - a real end-to-end run of
	///    the whole pipeline, not just a stationary target for the Tower's auto-spawn to shoot at.
	///  - "Place Tower" - toggles click-to-place using the fixed TowerPlan field; clicking a Buildable
	///    tile spawns it there.
	///  - "Randomize Map" - discards the current map and carves a new random one (MapGenerator.GenerateRandomMap).
	///  - "Start Next Wave" - the real EntityWaveManager/GameManager pipeline (needs both in the scene).
	///  - A second panel (needs GameManager, and AvailableTowers assigned) drives Player.Mulligan: reroll
	///    a hand from AvailableTowers, then click a hand item to enter place-mode for THAT TowerPlan
	///    instead of the fixed one above. Hand items that complete a fusion recipe entirely from the hand
	///    itself (see Util.Game.FusionUtil) get an extra "Fuse -> X" button; hand items that only complete
	///    one together with a Tower already on the field are labelled but not auto-fused yet (STUB - see
	///    FusionUtil's own doc comment for why field-consuming fusion isn't wired up).
	/// Monster/TowerPosition (the Start-time auto-spawn above) are still placed by hand and given no path -
	/// that spawn exists purely to give the Tower an immediate, stationary target to smoke-test firing on.
	/// </summary>
	public class MvpDemoSpawner : MonoBehaviour
	{
		[Header("Tower")]
		public GameObject TowerPrefab;
		public TowerPlan TowerPlan;
		public Vector3 TowerPosition = Vector3.zero;

		[Header("Monster")]
		public GameObject MonsterPrefab;
		public MonsterPlan MonsterPlan;
		public Vector3 MonsterPosition = new Vector3(5, 0, 0);

		[Header("Map")]
		public GameObject MapContainer;
		public MapPlan MapPlan;
		public Vector2Int RandomMapSize = new Vector2Int(13, 11);

		[Header("Mulligan")]
		// The reroll pool AND the full recipe list FusionUtil checks against - no runtime TowerPlan
		// registry exists yet (EntityDataWindow's TypeCache scan is Editor-only), so this is manually
		// assigned for now. Include every TowerPlan you want reachable by reroll, fusion result Plans
		// (with .ingredients set) included.
		public List<TowerPlan> AvailableTowers = new();

		[Header("Upgrade Menu")]
		// Clicking a placed Tower (when not already placing another one, and while UpgradeMenu isn't
		// already open) opens this straight to that Tower's Upgrade Centre - see TrySelectTowerAtMouse.
		public UpgradeMenu UpgradeMenu;

		// `TowerDefence.Map.Map`, not bare `Map` - TowerDefence.Map is this session's recurring
		// namespace-vs-class collision pattern (Map the class lives in the TowerDefence.Map namespace, a
		// sibling of this file's TowerDefence.Manager) - see CLAUDE.md's note on this gotcha.
		TowerDefence.Map.Map currentMap;
		MapGenerator generator;
		bool placingTower;
		// What placingTower actually places - defaults to the fixed TowerPlan field (the manual "Place
		// Tower" toggle), overridden per-click by picking a Mulligan hand item.
		TowerPlan pendingPlan;

		// Every Tower this spawner has placed (Start's auto-spawn included) - the "field" half of
		// FusionUtil.FindCombinables. Not a general-purpose Tower registry (EntityManager.GetAllEntities
		// is that); just enough bookkeeping for this demo's own fusion-highlighting.
		readonly List<Tower> placedTowers = new();

		static readonly Rect PanelRect = new Rect(10, 10, 240, 130);
		static readonly Rect MulliganPanelRect = new Rect(10, 150, 260, 220);

		void Start()
		{
			if (MapContainer != null && MapPlan != null)
			{
				generator = Instantiate(MapContainer).GetComponent<MapGenerator>();
				BuildMap(MapGenerator.LoadMap(MapPlan));
			}

			if (TowerPrefab != null && TowerPlan != null) PlaceTower(TowerPlan, TowerPosition);

			if (MonsterPrefab != null && MonsterPlan != null)
			{
				GameObject monsterObject = Instantiate(MonsterPrefab, MonsterPosition, Quaternion.identity);
				Monster monster = EntityManager.SpawnMonster(MonsterPlan);
				monsterObject.GetComponent<MonsterController>().Init(monster);
			}
		}

		void BuildMap(TowerDefence.Map.Map map)
		{
			currentMap = map;
			generator.BuildGamespace(currentMap);
			var paths = generator.BuildPaths(currentMap);
			int reachable = 0;
			foreach (var path in paths.Values)
			{
				if (path.Count > 0) reachable++;
			}
			Debug.Log($"MvpDemoSpawner: map built, {reachable}/{paths.Count} Source(s) have a resolved path to a Destination.");
		}

		/// <summary>The one place a Tower actually gets instantiated+spawned+registered - both Start's auto-spawn and every click-to-place path (manual or Mulligan-driven) go through this, so placedTowers (FusionUtil's "field") never misses one.</summary>
		Tower PlaceTower(TowerPlan plan, Vector3 position)
		{
			GameObject towerObject = Instantiate(TowerPrefab, position, Quaternion.identity);
			Tower tower = new Tower(plan);
			tower.Spawn();
			towerObject.GetComponent<TowerController>().Init(tower);
			placedTowers.Add(tower);
			return tower;
		}

		#region Interactive controls (IMGUI - no Canvas/EventSystem needed for an MVP smoke test)

		void OnGUI()
		{
			if (generator != null)
			{
				GUILayout.BeginArea(PanelRect, GUI.skin.box);
				if (GUILayout.Button("Spawn Monster at Source")) SpawnMonsterAtSource();
				bool wantsManualPlace = GUILayout.Toggle(placingTower && pendingPlan == TowerPlan, placingTower && pendingPlan == TowerPlan ? "Placing... click a Buildable tile" : "Place Tower");
				if (wantsManualPlace && !(placingTower && pendingPlan == TowerPlan)) BeginPlacing(TowerPlan);
				else if (!wantsManualPlace && placingTower && pendingPlan == TowerPlan) placingTower = false;
				if (GUILayout.Button("Randomize Map")) RandomizeMap();
				if (GUILayout.Button("Start Next Wave")) StartNextWave();
				GUILayout.EndArea();
			}

			if (GameManager.Instance != null) DrawMulliganPanel();
		}

		void DrawMulliganPanel()
		{
			var mulligan = GameManager.Instance.Player.Mulligan;

			GUILayout.BeginArea(MulliganPanelRect, GUI.skin.box);
			GUILayout.Label("Mulligan");
			if (GUILayout.Button("Reroll")) mulligan.Reroll(AvailableTowers);

			if (mulligan.Choices.Count == 0)
			{
				GUILayout.EndArea();
				return;
			}

			List<FusionMatch> matches = FusionUtil.FindCombinables(mulligan.Choices, placedTowers, AvailableTowers);

			foreach (TowerPlan choice in mulligan.Choices)
			{
				bool combinesWithField = matches.Exists(m => !m.IsHandOnly && m.FromHand.Contains(choice));
				string label = choice.Name + (combinesWithField ? " (combines w/ field)" : "");
				if (GUILayout.Button(label)) Pick(choice);
			}

			// Hand-only matches get an extra pick option, per the design discussion - picking it takes
			// the fused Recipe instead of any individual ingredient, and (like any pick) ends this hand.
			foreach (FusionMatch match in matches)
			{
				if (!match.IsHandOnly) continue;
				if (GUILayout.Button($"⚡ Fuse -> {match.Recipe.Name}")) Pick(match.Recipe);
			}

			GUILayout.EndArea();
		}

		/// <summary>Commits a mulligan pick (individual or fused) - enters place-mode for it and ends this hand (picking is a one-shot per roll, same as any mulligan/shop).</summary>
		void Pick(TowerPlan plan)
		{
			GameManager.Instance.Player.Mulligan.Choices.Clear();
			BeginPlacing(plan);
		}

		void BeginPlacing(TowerPlan plan)
		{
			pendingPlan = plan;
			placingTower = true;
		}

		void Update()
		{
			if (!Input.GetMouseButtonDown(0)) return;
			// Screen Y is bottom-up, GUI/Rect space is top-down - flip before comparing against either panel.
			Vector2 screenPoint = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			if (PanelRect.Contains(screenPoint) || MulliganPanelRect.Contains(screenPoint)) return; // Clicked a panel, not the map.

			if (placingTower) TryPlaceTowerAtMouse();
			else TrySelectTowerAtMouse();
		}

		/// <summary>Nearest placedTowers entry to the click, in screen space - Towers have no colliders (see MapGenerator.BuildGamespace's own reasoning for tiles), so this is a distance check against each one's live Transform (EntityManager's Position Registry) rather than a real raycast. No-ops while UpgradeMenu is already open (treat it as modal) or with nothing assigned.</summary>
		void TrySelectTowerAtMouse()
		{
			if (UpgradeMenu == null || UpgradeMenu.IsOpen || Camera.main == null) return;

			Tower closest = null;
			float closestDistance = 40f; // pixels
			foreach (Tower tower in placedTowers)
			{
				Transform towerTransform = EntityManager.GetTransform(tower);
				if (towerTransform == null) continue;

				Vector3 screenPos = Camera.main.WorldToScreenPoint(towerTransform.position);
				if (screenPos.z < 0) continue; // Behind the camera.

				float distance = Vector2.Distance(screenPos, Input.mousePosition); // Both bottom-up screen space - no flip needed here.
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closest = tower;
				}
			}

			if (closest != null) UpgradeMenu.Open(closest);
		}

		void SpawnMonsterAtSource()
		{
			if (currentMap == null || MonsterPrefab == null || MonsterPlan == null) return;
			if (!currentMap.TryFindTileOfType(TileType.Source, out Vector2Int sourcePosition))
			{
				Debug.LogWarning("MvpDemoSpawner: map has no Source tile to spawn at.");
				return;
			}

			GameObject monsterObject = Instantiate(MonsterPrefab, generator.TileToWorld(sourcePosition), Quaternion.identity);
			Monster monster = EntityManager.SpawnMonster(MonsterPlan);
			MonsterController controller = monsterObject.GetComponent<MonsterController>();
			controller.Init(monster);

			// Hand over the whole path at spawn time (not looked up per-frame) - see MapGenerator.Paths
			// and MonsterController.SetPath. Missing/empty if this Source has no resolved route.
			if (generator.Paths.TryGetValue(sourcePosition, out var path) && path.Count > 0) controller.SetPath(path);
			else Debug.LogWarning("MvpDemoSpawner: this Source has no resolved path - monster will spawn but won't move.");
		}

		void TryPlaceTowerAtMouse()
		{
			TowerPlan plan = pendingPlan;
			if (currentMap == null || TowerPrefab == null || plan == null) return;
			Camera camera = Camera.main;
			if (camera == null) return;

			// No physics/collider system on tiles (see MapGenerator.BuildGamespace) - resolve the click by
			// intersecting the camera ray with the map's own ground plane (y = 0, same as TileToWorld)
			// instead, then rounding back to a grid coordinate.
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);
			if (!new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance)) return;

			Vector3 worldPoint = ray.GetPoint(distance);
			Vector2Int grid = new Vector2Int(Mathf.RoundToInt(worldPoint.x / generator.TileSize), Mathf.RoundToInt(worldPoint.z / generator.TileSize));
			if (!currentMap.InBounds(grid) || currentMap.GetTile(grid).Type != TileType.Buildable) return;

			PlaceTower(plan, generator.TileToWorld(grid));
			placingTower = false;
			pendingPlan = null;
		}

		void RandomizeMap()
		{
			if (generator == null) return;
			generator.Clear();
			BuildMap(MapGenerator.GenerateRandomMap(RandomMapSize.x, RandomMapSize.y));
		}

		/// <summary>Real wave pipeline, not this class's own spawn buttons - needs a GameManager (+ EntityWaveManager, TickManager) also present in the scene. See GameManager.StartNextWave/WaveUtil.ENEMIETRIX for what actually gets spawned (WorldProgress.Biome defaults to Forest, where ExampleContentGenerator writes its first-Stage content).</summary>
		void StartNextWave()
		{
			if (GameManager.Instance == null)
			{
				Debug.LogWarning("MvpDemoSpawner: no GameManager in the scene - add one (and an EntityWaveManager) to use Start Next Wave.");
				return;
			}
			GameManager.Instance.StartNextWave();
		}

		#endregion Interactive controls
	}
}
