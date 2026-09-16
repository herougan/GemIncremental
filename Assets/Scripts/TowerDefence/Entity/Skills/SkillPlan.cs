
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using TowerDefence.Stats;
using Util.Maths;
using Util.Serialisation;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills.Passives;

namespace TowerDefence.Entity.Skills
{
	#region Class and Enums
	[CreateAssetMenu(fileName = "Skill", menuName = "TowerDefence/Skills/Skill")]
	public class SkillPlan : ScriptableObject, IPlan
	{
		[Header("Basic Information")]

		// None of these had [SerializeField] - fine for the old field-per-event style, but anything set
		// here on a ScriptableObject wouldn't have survived a save/domain-reload since Unity's default
		// serializer skips plain auto-properties entirely. Added across the board rather than piecemeal.
		[field: SerializeField] public string Name { get; set; }
		public SerialisableGuid Guid { get; set; }
		// LocalizedString (com.unity.localization), not string - a reference to a String Table entry
		// rather than literal English, so descriptions can actually be translated instead of needing a
		// separate rewrite once localization mattered. Populate via the Inspector's Table/Entry picker;
		// see ToDescription for how {DMG}-style placeholders resolve through Smart Format.
		[field: SerializeField] public LocalizedString Description { get; set; }
		[field: SerializeField] public int Level { get; set; }
		[field: SerializeField] public int MaxLevel { get; set; }
		[field: SerializeField] public float Cooldown { get; set; }
		[field: SerializeField] public float Range { get; set; }
		[field: SerializeField] public ddouble Damage { get; set; }
		[field: SerializeField] public float Radius { get; set; }
		[field: SerializeField] public float ChannelingTime { get; set; }

		[Header("Meta")]
		[field: SerializeField] public bool IsPositive { get; set; }
		[field: SerializeField] public bool ForMonster { get; set; }
		[field: SerializeField] public bool ForTower { get; set; }

		[Header("Gameplay")]
		[field: SerializeField] public double Cost { get; set; }
		[field: SerializeField] public List<DepletableStat> StatCosts { get; set; } = new List<DepletableStat>();
		[field: SerializeField] public List<ResourceStat> ResourceCosts { get; set; } = new List<ResourceStat>();

		// IEffect is an interface (Effect is its only concrete type today, but this is a list of a
		// polymorphic reference either way) - SerializeReference, not SerializeField, or this silently
		// serializes as empty. See Effect.cs.
		[field: SerializeReference]
		public List<IEffect> Effects { get; set; } = new List<IEffect>();
		public List<IPassive> Passives { get; set; } = new List<IPassive>();

		// Ancestry
		public SkillPlan Predecessor { get; set; }
		public SkillPlan Successor { get; set; }

		[Header("Description")]
		// Author-set descriptive tags (AoE/Slow/Ranged/Magic/...), on top of whatever GetTags() can
		// mechanically derive below. See Tag.cs for the shared vocabulary this draws from.
		[field: SerializeField] public List<Tag> Tags { get; set; } = new List<Tag>();

		// public event Action<ISource, IModifier> OnApplied;

		public void Recalculate(ddouble scale)
		{
			foreach (IEffect effect in Effects)
			{
				effect.Recalculate(scale);
			}
		}

		// Category colours, not one flat highlight colour - each placeholder is coloured for what it
		// actually represents, per the design discussion. Hex values are a first pass, not final art
		// direction - easy to retune, the point is the per-category mechanism.
		const string MagicDamageColor = "#4169E1";    // blue
		const string PhysicalDamageColor = "#DC143C"; // red
		const string CooldownColor = "#00CED1";       // cyan
		const string ManaCostColor = "#00008B";       // dark blue
		const string HealthCostColor = "#8B0000";     // dark red

		static readonly Dictionary<ElementType, string> ElementEmoji = new()
		{
			[ElementType.Fire] = "🔥",
			[ElementType.Water] = "💧",
			[ElementType.Ice] = "❄️",
			[ElementType.Earth] = "🪨",
			[ElementType.Wind] = "💨",
			[ElementType.Air] = "🌬️",
			[ElementType.Metal] = "⚙️",
			[ElementType.Gold] = "🪙",
			[ElementType.Poison] = "☠️",
			[ElementType.Nature] = "🌿",
			[ElementType.Light] = "✨",
			[ElementType.Dark] = "🌑",
		};

		/// <summary>
		/// The localized Description with {DMG}/{RANGE}/{COOLDOWN}/{MANA}/{HEALTHCOST} resolved from
		/// this Plan's own fields (MANA/HEALTHCOST only appear if StatCosts actually has a Mana/Health
		/// entry), and anything in `overrides` resolved on top/instead - via the Localization package's
		/// Smart Format, so a translator can move a placeholder anywhere in their language's sentence
		/// and it still resolves. Numbers go through ddouble.PrettyPrint(), not ToString() directly -
		/// ToString is mantissa/exponent-formatted for incremental-scale currency numbers ("1.000e1"),
		/// which reads badly for small skill-balance numbers.
		///
		/// `colour` wraps DMG/COOLDOWN/MANA/HEALTHCOST in Unity/TextMeshPro rich-text &lt;color&gt; tags
		/// (works directly in UGUI Text or TMP, which both already understand that markup) - Magic
		/// damage blue, Physical red (i.e. untagged - see Tag.cs), Cooldown cyan, Mana cost dark blue,
		/// Health cost dark red; an elemental action (see Action.Element) also appends its emoji next to
		/// the damage number. Custom `overrides` keys aren't auto-coloured - add them to this method if
		/// they need a category colour too.
		///
		/// NOTE: written against com.unity.localization's LocalizedString/Smart Format API from memory -
		/// the package was added to Packages/manifest.json this same pass, so this one file couldn't be
		/// dotnet-build-verified like everything else this session (the package isn't fetched/resolved
		/// until Unity's Editor next opens and regenerates the csproj with the new assembly reference).
		/// Flagging rather than presenting it as verified - open the Editor once, confirm this compiles
		/// and Arguments/dictionary-lookup behaves as expected, and ping me to fix anything the real API
		/// surface disagrees with.
		/// </summary>
		public string ToDescription(Dictionary<string, string> overrides = null, bool colour = true)
		{
			if (Description == null) return "";

			Dictionary<string, string> values = new()
			{
				["DMG"] = Damage.PrettyPrint(),
				["RANGE"] = Range.ToString("0.#"),
				["COOLDOWN"] = Cooldown.ToString("0.#"),
			};

			DepletableStat manaCost = StatCosts.FirstOrDefault(s => s.StatType == StatType.Mana);
			if (manaCost != null) values["MANA"] = manaCost.Value.PrettyPrint();

			DepletableStat healthCost = StatCosts.FirstOrDefault(s => s.StatType == StatType.Health);
			if (healthCost != null) values["HEALTHCOST"] = healthCost.Value.PrettyPrint();

			if (overrides != null)
			{
				foreach (var kv in overrides) values[kv.Key] = kv.Value;
			}

			if (colour)
			{
				if (values.ContainsKey("DMG"))
				{
					string dmgColor = IsMagic() ? MagicDamageColor : PhysicalDamageColor;
					ElementType element = GetPrimaryElement();
					string emoji = element != ElementType.None && ElementEmoji.TryGetValue(element, out string e) ? " " + e : "";
					values["DMG"] = $"<color={dmgColor}>{values["DMG"]}{emoji}</color>";
				}
				if (values.ContainsKey("COOLDOWN")) values["COOLDOWN"] = $"<color={CooldownColor}>{values["COOLDOWN"]}</color>";
				if (values.ContainsKey("MANA")) values["MANA"] = $"<color={ManaCostColor}>{values["MANA"]}</color>";
				if (values.ContainsKey("HEALTHCOST")) values["HEALTHCOST"] = $"<color={HealthCostColor}>{values["HEALTHCOST"]}</color>";
			}

			// Smart Format resolves {DMG} etc. against this dictionary's keys directly.
			Description.Arguments = new object[] { values };
			return Description.GetLocalizedString();
		}

		bool IsMagic() => GetTags().Contains(Tag.Magic.ToString());

		ElementType GetPrimaryElement()
		{
			foreach (IEffect effect in Effects)
			{
				foreach (IAction action in effect.Actions)
				{
					if (action is Action concreteAction && concreteAction.Element != ElementType.None) return concreteAction.Element;
				}
			}
			return ElementType.None;
		}

		/// <summary>
		/// Author-set Plan-level Tags, plus each Action's own author-set Tags (see Action.Tags - e.g. a
		/// Damage action hand-tagged Fire/Magic), plus whatever's mechanically derivable - currently just
		/// Ranged, from having a ProjectileAction.
		/// </summary>
		public List<string> GetTags()
		{
			List<string> tags = Tags.Select(t => t.ToString()).ToList();
			foreach (IEffect effect in Effects)
			{
				foreach (IAction action in effect.Actions)
				{
					if (action.ActionType == ActionType.Projectile) tags.Add(Tag.Ranged.ToString());
					if (action is Action concreteAction) tags.AddRange(concreteAction.Tags.Select(t => t.ToString()));
				}
			}
			return tags.Distinct().ToList();
		}
	}
	#endregion Class and Enums

	#region Editor


#if UNITY_EDITOR
	[CustomEditor(typeof(SkillPlan))]
	public class SkillDataEditor : Editor
	{
		public enum SkillType
		{

		}
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			// MonsterPlan script = (MonsterPlan)target;
			if (GUILayout.Button("Generate new GUID"))
			{
				// script.Guid = new SerialisableGuid(System.Guid.NewGuid());
			}
		}
	}

#endif // UNITY_EDITOR
	#endregion Editor
}