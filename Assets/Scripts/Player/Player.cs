using Incremental.Currency;
using TowerDefence.Entity.Attack.Damage;
using Util.Maths;

namespace Player
{
	/// <summary>
	/// The player's persistent, cross-run meta state - everything that exists independent of any one TD
	/// match. Plain C# class, not a MonoBehaviour/singleton - matches Incremental.Currency's convention
	/// rather than GameManager/EntityManager's, since there's exactly one of these per save, not per
	/// scene. Owned by GameManager (GameManager.Player); constructed there, not self-instantiating.
	///
	/// Each subsystem below is its own small class rather than a pile of fields directly on Player, so
	/// each one can grow its own data/behaviour independently:
	///  - PlayerSkills  - skills the player has personally learned and can bring into a run.
	///  - ArtefactCollection - artefacts the player has found/owns.
	///  - Compendium    - bestiary + elemental/type matchup data (also an IDamageModifier - see below).
	///  - Forge         - equipment crafting. Not designed yet - placeholder shape only.
	///  - Foundry       - permanent stat growth, applied onto Tower/Monster StatBlocks via the
	///                    Entity-level IStatMod registry (see Foundry.cs) - NOT part of the
	///                    per-hit damage-modifier tree below, which is for per-interaction multipliers.
	///  - Achievements  - unlocked achievements.
	///
	/// IDamageModifier: Player is one of the top-level nodes DamageCalculator walks for every hit
	/// (Player, Map, Game, Boss per the intended design - Map/Boss don't exist as systems yet, so
	/// they're not registered as roots until they do; see DamageCalculator.GetRoots). Player's own
	/// GetMultiplier is the player-global flat multiplier (e.g. "player has 2x dmg"); Compendium is
	/// its child so type-matchup bonuses stack underneath it automatically.
	/// </summary>
	public class Player : IDamageModifier
	{
		public PlayerSkills PlayerSkills { get; }
		public ArtefactCollection ArtefactCollection { get; }
		public Compendium Compendium { get; }
		public Forge Forge { get; }
		public Foundry Foundry { get; }
		public Achievements Achievements { get; }

		/// <summary>The base currency (see CLAUDE.md's Currency section - copper/silver/gold tiers eventually; just "Gold" for now). Awarded on Monster death, scaled by WorldProgress - see GameManager.HandleEntityEvent/StatType.Reward.</summary>
		public Currency Gold { get; }

		/// <summary>This run's current tower-choice reroll - see Mulligan.cs.</summary>
		public Mulligan Mulligan { get; }

		public Player()
		{
			PlayerSkills = new PlayerSkills();
			ArtefactCollection = new ArtefactCollection();
			Compendium = new Compendium();
			Forge = new Forge();
			Foundry = new Foundry();
			Achievements = new Achievements();
			Gold = new Currency("Gold", "G");
			Mulligan = new Mulligan();
		}

		// A dummy IEntity representing "the Player" as a Caster - same reasoning as WorldProgress's own
		// WorldEntity (see DummyEntityPlan). Lazily created, one per Player (i.e. one per save).
		TowerDefence.Entity.Entity playerEntity;
		TowerDefence.Entity.Entity PlayerEntity => playerEntity ??= new TowerDefence.Entity.Entity(new TowerDefence.Entity.DummyEntityPlan { Name = "Player" });

		/// <summary>
		/// Grants target a Buff sourced from "the Player" - same ApplyBuff flow any Entity uses to grant
		/// another one a Buff, just with the Player's dummy entity standing in as Caster. E.g. a Foundry
		/// upgrade modelled as a Buff instead of a direct StatBlock mutation, or a global "+5% ALL_FIRE_DMG"
		/// effect granted to every Tower onSpawn.
		/// </summary>
		public void ApplyBuff(TowerDefence.Entity.IEntity target, TowerDefence.Entity.Skills.Buffs.BuffPlan plan)
		{
			TowerDefence.Entity.Skills.Buffs.Buff buff = new TowerDefence.Entity.Skills.Buffs.Buff(plan, PlayerEntity);
			target.ApplyBuff(buff);
		}

		/// <summary>
		/// STUB: lets "the Player" itself learn a Skill (registered on PlayerEntity, same as any other
		/// Entity) - "commits their effects onto the world once they are done" per the design discussion.
		/// Same caveat as WorldProgress.RegisterSkill - nothing yet propagates a Player-held Skill's
		/// effects onto every Tower/Monster on the field, this just lets it exist.
		/// </summary>
		public void RegisterSkill(TowerDefence.Entity.Skills.ISkill skill) => PlayerEntity.RegisterSkill(skill);

		#region IDamageModifier

		/// <summary>
		/// Player-global flat multiplier - the "player has 2x dmg" example from the design discussion.
		/// Plain settable field for now, not backed by a StatBlock/StatType entry: no StatType for
		/// "global damage multiplier" exists yet, and adding one is a data-design decision, not an
		/// architecture one - wire this to a real stat once that's decided.
		/// </summary>
		public ddouble GlobalDamageMultiplier = 1;

		public ddouble GetMultiplier(Damage damage) => GlobalDamageMultiplier;

		public System.Collections.Generic.IEnumerable<IDamageModifier> Children
		{
			get { yield return Compendium; }
		}

		#endregion IDamageModifier
	}
}
