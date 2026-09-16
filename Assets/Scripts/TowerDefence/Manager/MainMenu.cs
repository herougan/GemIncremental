using UnityEngine;

namespace TowerDefence.Manager
{
	/// <summary>
	/// The top-level "above the game" nav bar - a persistent IMGUI strip across the top of the screen
	/// (distinct from MvpDemoSpawner's top-left panel, UpgradeMenu's centred one, and HudDisplay's
	/// top-right one - same "no Canvas/EventSystem yet, all straight OnGUI" pattern as everything else),
	/// not a separate scene you navigate away from the game to reach.
	///
	/// The first button is a "return to the game" control, labelled with WHERE the game currently is
	/// rather than a fixed name - "Field N-M" per the design discussion: Field = Biome, N = Stage,
	/// M = Round (1-indexed for display, same "human-facing numbers" convention WorldProgress's own docs
	/// use), read live off GameManager.WorldProgress every frame. Clicking it just closes UpgradeMenu (the
	/// only other "page" that currently exists) so the game view underneath is what's visible - the base
	/// game panels (MvpDemoSpawner/HudDisplay) are always drawn regardless, there's nothing to switch back
	/// TO, only something to close.
	///
	/// "Upgrades" opens UpgradeMenu with no Tower pre-selected (UpgradeMenu.Open(null) already handles
	/// that - see its own Upgrade Centre tab, "No Tower selected - click one on the field"). The remaining
	/// 6 buttons are disabled placeholders for future top-level pages (Foundry/Compendium/Achievements/a
	/// shop/settings/... - not decided), reserving the layout rather than one-off adding buttons as each
	/// gets designed.
	/// </summary>
	public class MainMenu : MonoBehaviour
	{
		public UpgradeMenu UpgradeMenu;
		public bool IsOpen = true; // Persistent nav bar, not something you toggle open/closed like UpgradeMenu.

		const int WipButtonCount = 6;
		const float BarHeight = 36f;

		void OnGUI()
		{
			if (!IsOpen) return;

			GUILayout.BeginArea(new Rect(0, 0, Screen.width, BarHeight), GUI.skin.box);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(GetFieldLabel(), GUILayout.Width(120))) ReturnToGame();

			GUILayout.Space(12);
			if (GUILayout.Button("Upgrades", GUILayout.Width(90))) OpenUpgrades();

			GUILayout.Space(12);
			GUI.enabled = false;
			for (int i = 0; i < WipButtonCount; i++)
			{
				GUILayout.Button($"WIP {i + 1}", GUILayout.Width(70));
			}
			GUI.enabled = true;

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		/// <summary>"Field N-M" - Field = Biome, N = Stage, M = Round, 1-indexed for display (WorldProgress's own fields are 0-indexed). Falls back to a placeholder if there's no GameManager/WorldProgress yet.</summary>
		string GetFieldLabel()
		{
			var world = GameManager.Instance?.WorldProgress;
			if (world == null) return "Field -";
			return $"{world.Biome} {world.Stage + 1}-{world.Round + 1}";
		}

		void ReturnToGame()
		{
			if (UpgradeMenu != null) UpgradeMenu.Close();
		}

		void OpenUpgrades()
		{
			if (UpgradeMenu == null)
			{
				Debug.LogWarning("MainMenu: no UpgradeMenu assigned.");
				return;
			}
			UpgradeMenu.Open(null);
		}
	}
}
