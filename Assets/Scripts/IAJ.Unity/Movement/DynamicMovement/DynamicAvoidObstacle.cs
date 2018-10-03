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
            Vector3 avoidVector = Vector3.zero;
            if (dir.magnitude <= 0.01f) return new MovementOutput();
            RaycastHit Hit;
            //Debug.DrawLine(Character.Position, Character.Position + dir * LookAhead, Color.red);
            Ray ray1 = new Ray(Character.Position, dir);
            //Debug.DrawLine(Character.Position, Character.Position + Quaternion.Euler(0, -45, 0) * dir * LookAhead, Color.red);
            //Ray ray2 = new Ray(Character.Position, Quaternion.Euler(0, -45, 0) * dir);
            //Debug.DrawLine(Character.Position, Character.Position + Quaternion.Euler(0, 45, 0) * dir * LookAhead, Color.red);
            //Ray ray3 = new Ray(Character.Position, Quaternion.Euler(0, 45, 0) * dir);
            if (Obstacle.GetComponent<Collider>().Raycast(ray1, out Hit, LookAhead))
            {
                //avoidVector += Hit.normal;
                base.Target.Position = Hit.point + Hit.normal * AvoidDistance;
                return base.GetMovement();
            }
            //else if (Obstacle.GetComponent<Collider>().Raycast(ray2, out Hit, LookAhead))
            //{
            //    avoidVector += Hit.normal;
            //}
            //else if(Obstacle.GetComponent<Collider>().Raycast(ray3, out Hit, LookAhead))
            //{
            //    avoidVector += Hit.normal;
            //}
            else
            {
                return new MovementOutput();
            }
            //base.Target.Position = avoidVector.normalized * AvoidDistance;
            //return base.GetMovement();
        }
    }
}