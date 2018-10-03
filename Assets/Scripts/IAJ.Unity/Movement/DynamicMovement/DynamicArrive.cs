using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicArrive : DynamicVelocityMatch
    {
        public override string Name
        {
            get { return "Arrive"; }
        }

        public float TimeToTarget { get; set; }
        public float StopRadius { get; set; }
        public float SlowRadius { get; set; }
        public KinematicData DestinationTarget { get; set; }

        public DynamicArrive()
        {
            this.TimeToTarget = 2.0f;
            this.SlowRadius = 10.0f;
            this.StopRadius = 1.0f;
            base.Target = new KinematicData();
            this.Output = new MovementOutput();
        }

        public override MovementOutput GetMovement()
        {
            Vector3 direction = this.DestinationTarget.Position - this.Character.Position;
            float desiredSpeed = 0f;

            if (direction.magnitude < this.StopRadius)
            {
                desiredSpeed = 0f;
            }
            else if (direction.magnitude > this.SlowRadius)
            {
                desiredSpeed = MaxAcceleration;
            }
            else
            {
                desiredSpeed = (MaxAcceleration * (direction.magnitude / this.SlowRadius));
            }

            base.Target.velocity = direction.normalized * desiredSpeed;

            return base.GetMovement();
        }
    }
}

