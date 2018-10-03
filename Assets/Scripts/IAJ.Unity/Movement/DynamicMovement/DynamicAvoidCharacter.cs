using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidCharacter : DynamicSeek
    {
        public override string Name
        {
            get { return "Avoid Obstacle"; }
        }

        public float AvoidMargin { get; set; }
        public float CollisionRadius { get; set; }
        public KinematicData TargetCharacter { get; set; }

        public DynamicAvoidCharacter(KinematicData NewTarget)
        {
            AvoidMargin = 1f;
            CollisionRadius = 3f;
            TargetCharacter = NewTarget;
        }

        public override MovementOutput GetMovement()
        {
            Vector3 deltaPos = TargetCharacter.Position - Character.Position;
            Vector3 deltaVel = TargetCharacter.velocity - Character.velocity;
            float deltaSpeed = deltaVel.magnitude;

            if (deltaSpeed == 0f) return new MovementOutput();

            float timeToClosest = -Vector3.Dot(deltaPos, deltaVel) / (deltaSpeed * deltaSpeed);

            if (timeToClosest > AvoidMargin) return new MovementOutput();

            Vector3 futureDeltaPos = deltaPos + deltaVel * timeToClosest;
            float futureDistance = futureDeltaPos.magnitude;

            if (futureDistance > 2 * CollisionRadius) return new MovementOutput();

            if (futureDistance <= 0 || deltaPos.magnitude < 2 * CollisionRadius) this.Output.linear = Character.Position - TargetCharacter.Position;
            else this.Output.linear = futureDeltaPos * -1;

            this.Output.linear = this.Output.linear.normalized * MaxAcceleration;

            return this.Output;
        }
    }
}