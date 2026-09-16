#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TowerDefence.Entity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefence.Editor
{
	/// <summary>
	/// The "easy to access UI DB" from the design discussion - one window listing every IPlan-
	/// implementing ScriptableObject type (found via reflection, via TypeCache - so a new IQuestPlan/
	/// IRecipePlan/IArtefactPlan shows up automatically the moment it exists, no editor code change
	/// needed), every asset of the selected type, and a live SerializedObject-bound detail editor
	/// (InspectorElement - the same machinery a normal Inspector uses) so edits are real edits to the
	/// .asset file directly, not a separate copy that needs syncing back.
	///
	/// STUB-level: three-pane layout (type list / asset list / detail), no search/filter, no custom
	/// per-type layout - InspectorElement's default field-per-line rendering, same as a normal
	/// Inspector would show. Good enough to browse/edit everything in one place; a nicer per-type
	/// layout (e.g. previewing GetTags()/ToDescription() live) is a follow-up polish pass, not structure.
	/// WorldProgress isn't here - it's runtime state, not an IPlan asset, so it doesn't fit this
	/// asset-list pattern; it would need its own small live-Play-Mode panel instead.
	/// </summary>
	public class EntityDataWindow : EditorWindow
	{
		[MenuItem("Tools/TowerDefence/Entity Data")]
		public static void Open()
		{
			EntityDataWindow window = GetWindow<EntityDataWindow>();
			window.titleContent = new GUIContent("Entity Data");
		}

		List<Type> planTypes;
		List<UnityEngine.Object> currentAssets = new();

		ListView typeListView;
		ListView assetListView;
		VisualElement detailPane;

		public void CreateGUI()
		{
			planTypes = TypeCache.GetTypesDerivedFrom<IPlan>()
				.Where(t => !t.IsAbstract && typeof(ScriptableObject).IsAssignableFrom(t))
				.OrderBy(t => t.Name)
				.ToList();

			VisualElement root = rootVisualElement;
			root.style.flexDirection = FlexDirection.Row;

			typeListView = new ListView();
			typeListView.style.width = 160;
			typeListView.style.borderRightWidth = 1;
			typeListView.selectionType = SelectionType.Single;
			typeListView.itemsSource = planTypes;
			typeListView.makeItem = () => new Label();
			typeListView.bindItem = (element, i) => ((Label)element).text = planTypes[i].Name;
			typeListView.selectionChanged += OnTypeSelected;
			root.Add(typeListView);

			assetListView = new ListView();
			assetListView.style.width = 220;
			assetListView.style.borderRightWidth = 1;
			assetListView.selectionType = SelectionType.Single;
			assetListView.itemsSource = currentAssets;
			assetListView.makeItem = () => new Label();
			assetListView.bindItem = (element, i) => ((Label)element).text = currentAssets[i] != null ? currentAssets[i].name : "(missing)";
			assetListView.selectionChanged += OnAssetSelected;
			root.Add(assetListView);

			detailPane = new ScrollView();
			detailPane.style.flexGrow = 1;
			root.Add(detailPane);
		}

		void OnTypeSelected(IEnumerable<object> selection)
		{
			Type selectedType = selection.FirstOrDefault() as Type;
			detailPane.Clear();

			currentAssets.Clear();
			if (selectedType != null)
			{
				currentAssets.AddRange(
					AssetDatabase.FindAssets($"t:{selectedType.Name}")
						.Select(guid => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)))
						.Where(a => a != null));
			}
			assetListView.Rebuild();
		}

		void OnAssetSelected(IEnumerable<object> selection)
		{
			UnityEngine.Object selectedAsset = selection.FirstOrDefault() as UnityEngine.Object;
			detailPane.Clear();
			if (selectedAsset == null) return;

			SerializedObject serializedObject = new SerializedObject(selectedAsset);
			InspectorElement inspector = new InspectorElement(serializedObject);
			detailPane.Add(inspector);
		}
	}
}
#endif
