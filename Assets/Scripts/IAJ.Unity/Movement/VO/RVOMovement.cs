//class adapted from the HRVO library http://gamma.cs.unc.edu/HRVO/
//adapted to IAJ classes by João Dias

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.VO
{
    public class RVOMovement : DynamicMovement.DynamicVelocityMatch
    {
        public override string Name
        {
            get { return "RVO"; }
        }

        protected List<KinematicData> Characters { get; set; }
        protected List<StaticData> Obstacles { get; set; }
        public float CharacterSize { get; set; }
        public float IgnoreDistance { get; set; }
        public float MaxSpeed { get; set; }
        protected List<Vector3> samples { get; set; }
        //create additional properties if necessary

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<StaticData> obstacles)
        {
            this.DesiredMovement = goalMovement;
            this.Characters = movingCharacters;
            this.Obstacles = obstacles;
            base.Target = new KinematicData();
            this.Output = new MovementOutput();
            //initialize other properties if you think is relevant
        }

        private Vector3 getBestSample(Vector3 desiredVelocity, List<Vector3> samples)
        {
            //Best sample should be zero if every sample sucks
            Vector3 bestSample = Vector3.zero;
            //minimumPenalty deve ser calculado como o timetoclosest feito no DynamicAvoidCharacter;

            foreach(Vector3 sample in samples)
            {
                float distancePenalty = (desiredVelocity - sample).magnitude;
                float maximumTimePenalty = 0;
                float timePenalty;

                foreach(KinematicData b in Characters)
                {
                    Vector3 deltaPos = b.Position - Character.Position;

                    if (deltaPos.magnitude > IgnoreDistance)
                        continue;

                    Ray ray1 = new Ray(Character.Position, 2 * sample - Character.velocity - b.velocity);
                    RaycastHit hit;
                    float TimeToCollision = -1;
                    if (Physics.Raycast(Character.Position, ray1.direction, out hit, IgnoreDistance))
                    {
                        TimeToCollision = hit.distance / Character.velocity.magnitude;
                    }

                    if (TimeToCollision > 0)
                        timePenalty = 2 / TimeToCollision;
                    else if (TimeToCollision == 0)
                        timePenalty = minimumPenalty;
                    else
                        timePenalty = 0;

                    if (timePenalty > maximumTimePenalty)
                        timePenalty = maximumTimePenalty;

                    float penalty = distancePenalty + timePenalty;

                    if(penalty < minimumPenalty)
                    {
                        minimumPenalty = penalty;
                        bestSample = sample;
                    }
                }
            }

            return new Vector3();
        }

        public override MovementOutput GetMovement()
        {
            //Shoudl start by converting the deridedOutput to velocity if it's acceleration
            MovementOutput desiredOutput = DesiredMovement.GetMovement();
            Vector3 desiredVelocity = Character.velocity + desiredOutput.linear;

            //Trim the velocity if the desired one is bigger than the established max velocity
            if(desiredVelocity.magnitude > MaxSpeed)
            {
                desiredVelocity = desiredVelocity.normalized;
                desiredVelocity *= MaxSpeed;
            }

            //---------------------------Generate Samples---------------------------------

            samples.Add(desiredVelocity);
            for(int i = 0; i<samples.Count; i++)
            {
                samples.Add(MathHelper.ConvertOrientationToVector(Random.Range(0, MathConstants.MATH_2PI)) * Random.Range(0, MaxSpeed));
            }

            base.Target.velocity = getBestSample(desiredVelocity, samples);

            return base.GetMovement();
        }
    }
}
