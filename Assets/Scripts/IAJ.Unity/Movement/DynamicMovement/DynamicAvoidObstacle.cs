using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidObstacle : DynamicSeek
    {
        public override string Name
        {
            get { return "Avoid Obstacle"; }
        }

        public float LookAhead { get; set; }
        public float AvoidDistance { get; set; }
        private GameObject Obstacle { get; set; }

        public DynamicAvoidObstacle(GameObject obstacle)
        {
            LookAhead = 10f;
            AvoidDistance = 4f;
            Obstacle = obstacle;
            base.Target = new KinematicData();
        }

        public override MovementOutput GetMovement()
        {
            Vector3 dir = Character.velocity.normalized;
            if (dir.magnitude <= 0.01f) return new MovementOutput();
            RaycastHit Hit;
            Ray ray1 = new Ray(Character.Position, dir);
            if (Obstacle.GetComponent<Collider>().Raycast(ray1, out Hit, LookAhead))
            {
                base.Target.Position = Hit.point + Hit.normal * AvoidDistance;
                return base.GetMovement();
            }

            else
            {
                return new MovementOutput();
            }
        }
    }
}