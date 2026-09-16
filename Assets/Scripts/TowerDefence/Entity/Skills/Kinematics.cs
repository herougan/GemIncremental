namespace TowerDefence.Entity.Skills
{
	public interface IKinematics
	{
		public float Speed { get; }
		public float Acceleration { get; }
		public float Deceleration { get; }

		public float Get(KinematicsType type);

	}
	public class Kinematics : IKinematics
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