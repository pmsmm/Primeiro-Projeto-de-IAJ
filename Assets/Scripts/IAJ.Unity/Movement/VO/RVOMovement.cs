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
        protected List<KinematicData> Obstacles { get; set; }
        public float CharacterSize { get; set; }
        public float IgnoreDistance { get; set; }
        public float MaxSpeed { get; set; }
        public int NumberOfSamples { get; set; }
        public int Weight { get; set; }
        private bool isStopped { get; set; }
        private int ObsStart { get; set; }
        protected List<Vector3> samples { get; set; }
        private const float TOLERANCE = 0.001f;
        //create additional properties if necessary

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<KinematicData> obstacles, KinematicData character)
        {
            this.DesiredMovement = goalMovement;
            this.CharacterSize = 3f;
            this.NumberOfSamples = 5;
            this.Weight = 2;
            this.IgnoreDistance = 5f;
            this.Character = character;
            this.Characters = movingCharacters;
            this.Characters.Remove(this.Character);
            this.ObsStart = this.Characters.Count;
            this.Characters.AddRange(obstacles);
            base.Target = new KinematicData();
            this.Output = new MovementOutput();
            this.samples = new List<Vector3>();
            //initialize other properties if you think is relevant
        }

        private Vector3 getBestSample(Vector3 desiredVelocity, List<Vector3> samples)
        {
            //Best sample should be zero if every sample sucks
            Vector3 bestSample = Vector3.zero;
            float minimumPenalty = Mathf.Infinity;
            Vector3 charPos = Character.Position;
            Vector3 charVel = Character.velocity;
            float maximumTimePenalty = 0f;
            float timePenalty = 0f;
            int CharacterCount = Characters.Count;

            foreach (Vector3 sample in samples)
            {
                float distancePenalty = (desiredVelocity - sample).magnitude;
                maximumTimePenalty = 0f;
                Vector3 sample_charVel = 2 * sample - charVel;

                for (int i = 0; i < CharacterCount; i++)
                {
                    KinematicData b = Characters[i];
                    Vector3 bPos = b.Position;
                    bool isObstacle = i >= ObsStart;

                    if ((bPos - charPos).magnitude > IgnoreDistance)
                        continue;

                    float TimeToCollision = MathHelper.TimeToCollisionBetweenRayAndCircle(charPos, sample_charVel - b.velocity, bPos, CharacterSize * (isObstacle ? 1.5f : 1f));

                    if (TimeToCollision > TOLERANCE)
                        timePenalty = (Weight * (isObstacle ? 3f : 1f)) / TimeToCollision;
                    else if (TimeToCollision >= 0f)
                        timePenalty = Mathf.Infinity;
                    else
                        timePenalty = 0f;

                    maximumTimePenalty = timePenalty > maximumTimePenalty ? timePenalty : maximumTimePenalty;

                    float penalty = distancePenalty + maximumTimePenalty;

                    if (penalty <= 0.1f) return sample;

                    if (penalty < minimumPenalty)
                    {
                        minimumPenalty = penalty;
                        bestSample = sample;
                    }
                }
            }

            return bestSample;
        }

        public override MovementOutput GetMovement()
        {
            //Shoudl start by converting the deridedOutput to velocity if it's acceleration
            MovementOutput desiredOutput = DesiredMovement.GetMovement();
            Vector3 desiredVelocity = Character.velocity + desiredOutput.linear;

            if (desiredVelocity.magnitude <= TOLERANCE)
            {
                if (!isStopped)
                {
                    isStopped = true;
                    base.Target.velocity = Vector3.zero;
                }
                return base.GetMovement();
            }
            else if (isStopped)
            {
                isStopped = false;
            }

            //Trim the velocity if the desired one is bigger than the established max velocity
            if(desiredVelocity.magnitude > MaxSpeed)
            {
                desiredVelocity = desiredVelocity.normalized;
                desiredVelocity *= MaxSpeed;
            }

            //---------------------------Generate Samples---------------------------------

            samples.Add(desiredVelocity);
            for(int i = 0; i < this.NumberOfSamples - 1; i++)
            {
                samples.Add(MathHelper.ConvertOrientationToVector(Random.Range(0, MathConstants.MATH_2PI)) * Random.Range(0, MaxSpeed));
            }

            Vector3 bestSample = getBestSample(desiredVelocity, samples);
            base.Target.velocity = bestSample != Vector3.zero ? bestSample : desiredOutput.linear;
            Debug.DrawLine(Character.Position, Character.Position + base.Target.velocity, Color.red);

            return base.GetMovement();
        }
    }
}
