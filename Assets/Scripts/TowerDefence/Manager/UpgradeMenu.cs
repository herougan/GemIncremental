using TowerDefence.Entity.Tower;
using UnityEngine;
using Util.Game;

namespace TowerDefence.Manager
{
	/// <summary>
	/// The "Upgrade Menu" - outside/on top of the TowerDefence-run UI (MvpDemoSpawner/HudDisplay's own
	/// panels), a tabbed home for every progression-mechanic page (per the design discussion: Upgrade
	/// Centre first, Foundry/Compendium/Achievements/... expected to join it as their own tabs later,
	/// same list Player.cs's own doc comment already names). Opens either from a future top-level "Menu"
	/// button (not built - nothing outside this class calls Open() unprompted yet) or by clicking a
	/// placed Tower on the field (see MvpDemoSpawner.TrySelectTowerAtMouse), which both opens this AND
	/// jumps straight to the Upgrade Centre tab pointed at that specific Tower.
	///
	/// Only Upgrade Centre is actually implemented - every other tab is a labelled placeholder, same
	/// "structure now, content later" STUB pattern as everywhere else this session.
	/// </summary>
	public class UpgradeMenu : MonoBehaviour
	{
		public enum Tab
		{
			UpgradeCentre,
			Foundry,
			Compendium,
			Achievements,
		}

		public bool IsOpen { get; private set; }
		Tower selectedTower;
		Tab activeTab = Tab.UpgradeCentre;

		static readonly Rect PanelRect = new Rect(0, 0, 420, 360); // Recomputed centred in OnGUI - Screen.width/height aren't valid outside it.

		/// <summary>Opens straight to the Upgrade Centre tab for `tower` - what a field click does. Pass null to open the menu without any Tower selected (e.g. a future top-level Menu button, before Compendium/Foundry/etc. exist to land on instead).</summary>
		public void Open(Tower tower)
		{
			selectedTower = tower;
			activeTab = Tab.UpgradeCentre;
			IsOpen = true;
		}

		public void Close()
		{
			IsOpen = false;
			selectedTower = null;
		}

		void OnGUI()
		{
			if (!IsOpen) return;

			Rect panel = new Rect((Screen.width - PanelRect.width) / 2, (Screen.height - PanelRect.height) / 2, PanelRect.width, PanelRect.height);
			GUILayout.BeginArea(panel, GUI.skin.box);

			GUILayout.BeginHorizontal();
			foreach (Tab tab in System.Enum.GetValues(typeof(Tab)))
			{
				if (GUILayout.Toggle(activeTab == tab, tab.ToString(), "Button")) activeTab = tab;
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			if (activeTab == Tab.UpgradeCentre) DrawUpgradeCentre();
			else GUILayout.Label($"{activeTab} - not built yet.");

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Close")) Close();

			GUILayout.EndArea();
		}

		void DrawUpgradeCentre()
		{
			if (selectedTower == null)
			{
				GUILayout.Label("No Tower selected - click one on the field.");
				return;
			}
			if (GameManager.Instance == null)
			{
				GUILayout.Label("No GameManager in the scene - can't spend Gold.");
				return;
			}

			var player = GameManager.Instance.Player;
			GUILayout.Label($"{selectedTower.Plan.Name}   (Gold: {player.Gold.PrettyPrint()})");

			GUILayout.Space(4);
			foreach (var stat in TowerUpgradeUtil.UpgradableStats.Keys)
			{
				int purchases = selectedTower.UpgradePurchases.TryGetValue(stat, out int n) ? n : 0;
				var cost = TowerUpgradeUtil.GetStatCost(selectedTower, stat);
				if (GUILayout.Button($"+{stat} (bought {purchases}) - {cost.PrettyPrint()} Gold"))
					TowerUpgradeUtil.PurchaseStat(selectedTower, stat, player);
			}

			var eligible = TowerUpgradeUtil.GetEligibleSkillUnlocks(selectedTower);
			if (eligible.Count > 0)
			{
				GUILayout.Space(8);
				GUILayout.Label("Skill Unlocks");
				foreach (var unlock in eligible)
				{
					if (GUILayout.Button($"Unlock {unlock.Skill.Name} - {unlock.Cost.PrettyPrint()} Gold"))
						TowerUpgradeUtil.PurchaseSkill(selectedTower, unlock, player);
				}
			}
		}
	}
}
