using TowerDefence.Entity.Skills.Buffs;

namespace TowerDefence.Stages
{
	/// <summary>
	/// Where the player currently is in overall progression: World > Biome > Stage > Round > Wave (20
	/// Waves per Round, 20 Rounds per Stage, 20 Stages per Biome, every Biome per World). EntityWaveManager
	/// reads Biome/Stage/Round/Wave to index into WaveUtil.ENEMIETRIX; EntityManager reads all five to
	/// scale a freshly spawned Monster to match (see EntityManager.SpawnMonster(plan, world)).
	///
	/// World is the topmost tier - a full loop back through every Biome (like a New Game+ pass), each one
	/// meaningfully harder than the last (see EntityManager.ComputeWaveMultiplier). It's scaling-only for
	/// now: WaveUtil.ENEMIETRIX isn't keyed by World, so which SpawnChains actually get spawned is still
	/// decided purely by Biome/Stage/Round/Wave - EntityWaveManager spawns "irregardless" of World until
	/// there's a reason to author distinct content per loop.
	/// </summary>
	public class WorldProgress
	{
		public int World { get; set; }
		public StageType Biome { get; set; }
		public int Stage { get; set; }
		public int Round { get; set; }
		public int Wave { get; set; }

		public const int WavesPerRound = 20;
		public const int RoundsPerStage = 20;

		// A dummy IEntity representing "the World" as a Caster - see DummyEntityPlan's own doc comment
		// for why. Lazily created, one per WorldProgress (i.e. one per save/session), not per buff.
		TowerDefence.Entity.Entity worldEntity;
		TowerDefence.Entity.Entity WorldEntity => worldEntity ??= new TowerDefence.Entity.Entity(new TowerDefence.Entity.DummyEntityPlan { Name = "World" });

		/// <summary>
		/// Grants target a Buff sourced from "the World" - the same ApplyBuff flow any Entity uses to
		/// grant another one a Buff, just with the World's dummy entity standing in as Caster. E.g. a
		/// biome-wide "monsters here are +20% tankier" effect, applied per-monster onSpawn or to
		/// everything already on the field.
		/// </summary>
		public void ApplyBuff(TowerDefence.Entity.IEntity target, BuffPlan plan)
		{
			Buff buff = new Buff(plan, WorldEntity);
			target.ApplyBuff(buff);
		}

		/// <summary>
		/// STUB: lets "the World" itself learn a Skill (registered on WorldEntity, same as any other
		/// Entity) - "commits their effects onto the world once they are done" per the design discussion.
		/// What actually propagates a World-held Skill's effects onto every entity on the field (rather
		/// than just existing, inertly, on a dummy entity nothing else references) isn't built - that
		/// needs the same kind of spatial/broadcast mechanism Aura propagation is also still waiting on.
		/// </summary>
		public void RegisterSkill(TowerDefence.Entity.Skills.ISkill skill) => WorldEntity.RegisterSkill(skill);

		/// <summary>
		/// Moves to the next Wave, rolling into the next Round/Stage once the current one's Waves/Rounds
		/// are exhausted. Does not roll Stage into the next Biome, nor Biome into the next World - STUB:
		/// both are presumably a deliberate player choice (picking a new Biome; starting a new World loop
		/// once every Biome's cleared), not automatic, but nothing decides either yet.
		/// </summary>
		public void AdvanceWave()
		{
			Wave++;
			if (Wave >= WavesPerRound)
			{
				Wave = 0;
				Round++;
				if (Round >= RoundsPerStage)
				{
					Round = 0;
					Stage++;
				}
			}
		}
	}
}
