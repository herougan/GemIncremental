namespace TowerDefence.Entity.Skills
{
	public class Kinematics
	{
		public float Speed { get; private set; }
		public float Acceleration { get; private set; }
		public float Deceleration { get; private set; }

		public float Get(KinematicsType type)
		{
			return type switch
			{
				KinematicsType.Speed => Speed,
				KinematicsType.Acceleration => Acceleration,
				KinematicsType.Deceleration => Deceleration,
				_ => 0f,
			};
		}
	}

	public enum KinematicsType
	{
		Speed,
		Acceleration,
		Deceleration,
	}
}