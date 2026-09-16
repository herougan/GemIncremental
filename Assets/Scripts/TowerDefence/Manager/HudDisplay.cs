using TowerDefence.Entity;
using TowerDefence.Entity.Monster;
using TowerDefence.Entity.Tower;
using TowerDefence.Stages;
using TowerDefence.Stats;
using UnityEngine;
using Util.Game;
using Util.Maths;

namespace TowerDefence.Manager
{
	/// <summary>
	/// Read-only IMGUI overlay (top-right - MvpDemoSpawner's own interactive panel already owns
	/// top-left) - Gold, World/Biome/Stage/Round/Wave, a DPS meter, and current/max HP for every live
	/// Tower/Monster (EntityManager's Position Registry - the same "every registered Entity" list
	/// TickManager itself ticks). No setup beyond dropping it in the scene alongside GameManager - draws
	/// nothing (not even the panel box) if GameManager.Instance is null, same guard MvpDemoSpawner's
	/// Start Next Wave button uses, rather than throwing.
	///
	/// DPS is BattleTracker.GetDPS() - total tracked damage / seconds since the last BattleTracker.Reset(),
	/// an average over the whole window, not an instantaneous per-second rate. Call BattleTracker.Reset()
	/// (e.g. at the start of a wave) to zero the window if a fresh reading matters more than a running one.
	/// </summary>
	public class HudDisplay : MonoBehaviour
	{
		void OnGUI()
		{
			if (GameManager.Instance == null) return;

			Rect panel = new Rect(Screen.width - 310, 10, 300, 360);
			GUILayout.BeginArea(panel, GUI.skin.box);

			WorldProgress world = GameManager.Instance.WorldProgress;
			GUILayout.Label($"Gold: {GameManager.Instance.Player.Gold.PrettyPrint()}");
			GUILayout.Label($"World {world.World}   Biome {world.Biome}");
			GUILayout.Label($"Stage {world.Stage + 1}   Round {world.Round + 1}/{WorldProgress.RoundsPerStage}   Wave {world.Wave + 1}/{WorldProgress.WavesPerRound}");
			GUILayout.Label($"DPS: {BattleTracker.GetDPS().PrettyPrint()}");

			GUILayout.Space(8);
			GUILayout.Label("Towers", BoldLabelStyle());
			foreach (IEntity entity in EntityManager.GetAllEntities())
			{
				if (entity is Tower tower) DrawHealthLine(tower.Plan.Name, entity);
			}

			GUILayout.Space(4);
			GUILayout.Label("Monsters", BoldLabelStyle());
			foreach (IEntity entity in EntityManager.GetAllEntities())
			{
				if (entity is Monster monster) DrawHealthLine(monster.Plan.Name, entity);
			}

			GUILayout.EndArea();
		}

		static void DrawHealthLine(string name, IEntity entity)
		{
			ddouble current = entity.StatBlock.GetCurrent(StatType.Health);
			ddouble max = entity.GetStat(StatType.Health);
			GUILayout.Label($"  {name}: {current.PrettyPrint()} / {max.PrettyPrint()} HP");
		}

		// GUIStyle can only be constructed inside OnGUI (it reads GUI.skin, which isn't set up outside
		// the GUI event loop) - a fresh one per call is wasteful but this is a debug overlay, not a
		// hot path.
		static GUIStyle BoldLabelStyle() => new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
	}
}
