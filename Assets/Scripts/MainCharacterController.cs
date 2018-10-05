using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Movement.Arbitration;
using Assets.Scripts.IAJ.Unity.Movement.VO;
using Assets.Scripts.IAJ.Unity.Movement;

public class MainCharacterController : MonoBehaviour {

    public const float X_WORLD_SIZE = 55;
    public const float Z_WORLD_SIZE = 32.5f;
    private const float MAX_ACCELERATION = 25.0f;
    private const float MAX_SPEED = 15.0f;
    private const float DRAG = 0.1f;
    private const float TIME_TO_TARGET = 2.0f;
    private const float SLOW_RADIUS = 10.0f;
    private const float STOP_RADIUS = 1.0f;
    private const float MAX_LOOK_AHEAD = 6.0f;
    private const float AVOID_MARGIN = 10.0f;
    private const float COLLISION_RADIUS = 3.0f;
    private const float CHARACTER_SIZE = 3.0f;
    private const float IGNORE_DISTANCE = 15f;
    private const int NUMBER_OF_SAMPLES = 15;
    private const int RVO_WEIGHT = 10;

    public KeyCode stopKey = KeyCode.S;
    public KeyCode priorityKey = KeyCode.P;
    public KeyCode blendedKey = KeyCode.B;

    public GameObject movementText;
    public DynamicCharacter character;

    public PriorityMovement priorityMovement;
    public BlendedMovement blendedMovement;
    public RVOMovement rvoMovement;

    private DynamicPatrol patrolMovement;


    //early initialization
    void Awake()
    {
        this.character = new DynamicCharacter(this.gameObject);
    

        this.priorityMovement = new PriorityMovement
        {
            Character = this.character.KinematicData
        };

        this.blendedMovement = new BlendedMovement
        {
            Character = this.character.KinematicData
        };
    }

    // Use this for initialization
    void Start ()
    {
    }

    public void InitializeMovement(GameObject[] obstacles, List<DynamicCharacter> characters)
    {
        foreach (var obstacle in obstacles)
        {
            var avoidObstacleMovement = new DynamicAvoidObstacle(obstacle)
            {
                MaxAcceleration = MAX_ACCELERATION,
                AvoidDistance = AVOID_MARGIN,
                LookAhead = MAX_LOOK_AHEAD,
                Character = this.character.KinematicData,
                DebugColor = Color.magenta
            };
            this.blendedMovement.Movements.Add(new MovementWithWeight(avoidObstacleMovement, 4.0f));
            this.priorityMovement.Movements.Add(avoidObstacleMovement);
        }

        foreach (var otherCharacter in characters)
        {
            if (otherCharacter != this.character)
            {
                var avoidCharacter = new DynamicAvoidCharacter(otherCharacter.KinematicData)
                {
                    Character = this.character.KinematicData,
                    MaxAcceleration = MAX_ACCELERATION,
                    AvoidMargin = AVOID_MARGIN,
                    CollisionRadius = COLLISION_RADIUS,
                    DebugColor = Color.cyan
                };

                this.blendedMovement.Movements.Add(new MovementWithWeight(avoidCharacter, 2.0f));
                this.priorityMovement.Movements.Add(avoidCharacter);
            }
        }

        //var targetPosition = this.character.KinematicData.Position + (Vector3.zero - this.character.KinematicData.Position) * 2;
        var targetPosition = -this.character.KinematicData.Position;

        this.patrolMovement = new DynamicPatrol(this.character.KinematicData.Position, targetPosition)
        {
            Character = this.character.KinematicData,
            TimeToTarget = TIME_TO_TARGET,
            SlowRadius = SLOW_RADIUS,
            StopRadius = STOP_RADIUS,
            MaxAcceleration = MAX_ACCELERATION,
            DebugColor = Color.yellow
        };

        List<KinematicData> chars = characters.Select(c => c.KinematicData).ToList();
        List<GameObject> obs = obstacles.ToList();
        List<KinematicData> obst = new List<KinematicData>();
        foreach (GameObject ob in obs) {
            obst.Add(new KinematicData(new StaticData(ob.transform)));
        }
        this.rvoMovement = new RVOMovement(this.patrolMovement, chars, obst, this.character.KinematicData)
        {
            //Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            CharacterSize = CHARACTER_SIZE,
            IgnoreDistance = IGNORE_DISTANCE,
            NumberOfSamples = NUMBER_OF_SAMPLES,
            Weight = RVO_WEIGHT
        };

        this.priorityMovement.Movements.Add(patrolMovement);
        this.blendedMovement.Movements.Add(new MovementWithWeight(patrolMovement, 1f));
        this.character.Movement = this.priorityMovement;
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            this.patrolMovement.ChangeTarget();
        }
        if (Input.GetKeyDown(this.stopKey))
        {
            this.character.Movement = null;
        }
        else if (Input.GetKeyDown(this.blendedKey))
        {
            this.character.Movement = this.blendedMovement;
        }
        else if (Input.GetKeyDown(this.priorityKey))
        {
            this.character.Movement = this.priorityMovement;
        }
        else if(Input.GetKeyDown(KeyCode.R))
        {
            this.character.Movement = this.rvoMovement;
        }

        this.UpdateMovingGameObject();
    }

    void OnDrawGizmos()
    {
    }

    private void UpdateMovingGameObject()
    {
        if (this.character.Movement != null)
        {
            this.character.Update();
            this.character.KinematicData.ApplyWorldLimit(X_WORLD_SIZE, Z_WORLD_SIZE);
        }
    }
}
