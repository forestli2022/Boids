using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class Brain : MonoBehaviour
{
    [Header("Spawn")]
    public int BOIDS_NUMBER;
    public float SPAWN_RANGE;

    [Header("Boid State Constants")]
    public float BOID_MAX_SPEED;
    public float BOID_MAX_ACCELERATION_MAGNITUDE;
    public float TURNING_SPEED;

    [Header("Alignment")]
    public float BOID_ALIGNMENT_SCALE;

    [Header("Cohesion")]
    public float BOID_COHESION_SCALE;

    [Header("Separation")]
    public float BOID_SEPARATION_SCALE;

    [Header("View")]
    public float MAX_VIEW_DISTANCE;
    public float MAX_VIEW_ANGLE;
    
    [Header("Goal")]
    public Transform GOAL;
    public float BOID_GOAL_SCALE;
    public float GOAL_DETECTION_DIST;
    public float GOAL_SPEED;
    public float GOAL_MAX_ACCELERATION_MAGNITUDE;
    private Vector3 goalVelocity;
    private Vector3 goalAcceleration;

    [Header("Obstacles")]
    public int OBSTACLES_NUMBER;
    public float BOID_OBSTACLE_AVOIDANCE_SCALE;
    public float OBSTACLE_SPEED;
    public float OBSTACLE_MAX_ACCELERATION_MAGNITUDE;
    public float OBSTACLE_MIN_SIZE;
    public float OBSTACLE_MAX_SIZE;
    public float OBSTACLE_TURNING_DIST;
    public float ROOM_SIZE;
    [SerializeField] private Transform ROOM;

    
    [Header("Rendering")]
    [SerializeField] private Mesh BOID_MESH;
    [SerializeField] private Material BOID_MATERIAL;
    [SerializeField] private Mesh OBSTACLE_MESH;
    [SerializeField] private Material OBSTACLE_MATERIAL;


    // references in brain.cs
    private EntityManager entityManager;
    private EntityArchetype entityArchetype;
    private Entity goalEntity;

    // boids entity array
    private NativeArray<Entity> entityArrayBoids;
    private Entity entityToFollow;

    [Header("Camera")]
    private int camState = 0;  //0: locked, 1: free, 2: follow boids, 3: control target
    [SerializeField] private Transform cam;
    public float CAMERA_DISTANCE_TO_ORIGIN;
    public float CAMERA_ROTATION_SPEED;
    private float yaw;
    private float pitch;

    private void Start()
    {
        // lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        camState = 0;

        GameObject.Find("DataSetter").GetComponent<DataSetter>().SetData();
        cam.position = new Vector3(cam.position.x, 0, cam.position.z);
        cam.position = Vector3.Normalize(cam.position) * CAMERA_DISTANCE_TO_ORIGIN;
        // create boid entity manager and entity archetype
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityArchetype = entityManager.CreateArchetype(
            typeof(BoidComponent),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(RenderMesh),
            typeof(Translation),
            typeof(Rotation)
        );


        // create boid entity arrays
        entityArrayBoids = new NativeArray<Entity>(BOIDS_NUMBER, Allocator.Persistent);
        entityManager.CreateEntity(entityArchetype, entityArrayBoids);

        for (int i=0; i < entityArrayBoids.Length; i++)
        {
            // create entity
            Entity entity = entityArrayBoids[i];

            // make sure boids are visible by assigning them render meshes
            RenderMeshUtility.AddComponents(entity, entityManager, new RenderMeshDescription(
                mesh: BOID_MESH,
                material: BOID_MATERIAL, 
                shadowCastingMode: ShadowCastingMode.On, 
                receiveShadows: true
            ));

            // init individual boid's state
            entityManager.SetComponentData(entity, new BoidComponent
            {
                //spawn
                boidsNumber = BOIDS_NUMBER,

                // translation
                velocity = Random.insideUnitSphere * BOID_MAX_SPEED,
                acceleration = Random.insideUnitSphere * BOID_MAX_ACCELERATION_MAGNITUDE,
                maxSpeed = BOID_MAX_SPEED,
                maxAccelerationMagnitude = BOID_MAX_ACCELERATION_MAGNITUDE,
                turningSpeed = TURNING_SPEED,


                // scales
                alignmentScale = BOID_ALIGNMENT_SCALE,
                cohesionScale = BOID_COHESION_SCALE,
                seperationScale = BOID_SEPARATION_SCALE,
                goalScale = BOID_GOAL_SCALE,
                obstacleAvoidanceScale = BOID_OBSTACLE_AVOIDANCE_SCALE,

                // view
                maxViewDistance = MAX_VIEW_DISTANCE,
                maxViewAngle = MAX_VIEW_ANGLE,
                goalDetectionDist = GOAL_DETECTION_DIST,

                // wall
                roomSize = ROOM_SIZE,
                wallTurningDist = OBSTACLE_TURNING_DIST,

                // obstacle
                obstacleTurningDist = OBSTACLE_TURNING_DIST
            });

            // set random transform
            entityManager.SetComponentData(entity, new Translation{ Value = Random.insideUnitSphere * SPAWN_RANGE });
        }
        entityToFollow = entityArrayBoids[Random.Range(0, BOIDS_NUMBER - 1)];


        // create goal entity
        entityArchetype = entityManager.CreateArchetype(
            typeof(Goal),
            typeof(Translation)
        );
        goalEntity = entityManager.CreateEntity(entityArchetype);

        // set room size
        ROOM.localScale = new Vector3(ROOM_SIZE, ROOM_SIZE, ROOM_SIZE);

        // create obstacle entities
        entityArchetype = entityManager.CreateArchetype(
            typeof(ObstacleComponent),
            typeof(Translation),
            typeof(Scale)
        );
        NativeArray<Entity> entityArray = new NativeArray<Entity>(OBSTACLES_NUMBER, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        for(int i=0; i<entityArray.Length; i++)
        {
            Entity entity = entityArray[i];

            // make sure obstacles are visible by assigning them render meshes
            RenderMeshUtility.AddComponents(entity, entityManager, new RenderMeshDescription(
                mesh: OBSTACLE_MESH,
                material: OBSTACLE_MATERIAL, 
                shadowCastingMode: ShadowCastingMode.On, 
                receiveShadows: true
            ));

            entityManager.SetComponentData(entity, new ObstacleComponent
            {
                velocity = Random.onUnitSphere * OBSTACLE_SPEED,
                acceleration = Random.insideUnitSphere * OBSTACLE_MAX_ACCELERATION_MAGNITUDE,
                roomSize = ROOM_SIZE,
                speed = OBSTACLE_SPEED,
                maxAccelerationMagnitude = OBSTACLE_MAX_ACCELERATION_MAGNITUDE
            });

            float scale = Random.Range(OBSTACLE_MIN_SIZE, OBSTACLE_MAX_SIZE);
            float halfRoomSize = ROOM_SIZE / 2 - scale / 2;
            entityManager.SetComponentData(entity, new Translation
            {
                Value = new float3(Random.Range(-halfRoomSize, halfRoomSize), Random.Range(-halfRoomSize, halfRoomSize), Random.Range(-halfRoomSize, halfRoomSize))
            });

            entityManager.SetComponentData(entity, new Scale
            {
                Value = scale
            });
        }
        entityArray.Dispose();
    }

    private void LateUpdate()
    {
        // move goal randomly around the room
        goalAcceleration += Random.insideUnitSphere * GOAL_MAX_ACCELERATION_MAGNITUDE;
        float halfRoomSize = ROOM_SIZE / 2 - GOAL.localScale.x / 2;
        float minDistToWall = Mathf.Min(Mathf.Min(halfRoomSize - Mathf.Abs(GOAL.position.x), 
                                                            halfRoomSize - Mathf.Abs(GOAL.position.y)), 
                                                            halfRoomSize - Mathf.Abs(GOAL.position.z));  
        if(minDistToWall < GOAL.localScale.x){
            goalAcceleration -= GOAL.position;
        }
        goalAcceleration = Vector3.ClampMagnitude(goalAcceleration, GOAL_MAX_ACCELERATION_MAGNITUDE);
        goalVelocity += goalAcceleration * Time.deltaTime;
        goalVelocity = Vector3.Normalize(goalVelocity) * GOAL_SPEED;
        GOAL.position += goalVelocity * Time.deltaTime;
        GOAL.position = new Vector3(Mathf.Clamp(GOAL.position.x, -halfRoomSize, halfRoomSize),
                                    Mathf.Clamp(GOAL.position.y, -halfRoomSize, halfRoomSize),
                                    Mathf.Clamp(GOAL.position.z, -halfRoomSize, halfRoomSize));
        // pass data to goal entity
        entityManager.SetComponentData(goalEntity, new Translation{ Value = (float3)GOAL.position });

        // camera movement
        if(Input.GetKeyDown(KeyCode.Alpha0)){
            camState = 0;
            cam.position = new Vector3(cam.position.x, 0, cam.position.z);
            cam.position = Vector3.Normalize(cam.position) * CAMERA_DISTANCE_TO_ORIGIN;
        }else if(Input.GetKeyDown(KeyCode.Alpha1)){
            camState = 1;
        }else if(Input.GetKeyDown(KeyCode.Alpha2)){
            camState = 2;
            entityToFollow = entityArrayBoids[Random.Range(0, BOIDS_NUMBER - 1)];
        }else if(Input.GetKeyDown(KeyCode.Alpha3)){
            camState = 3;
        }else if(Input.GetKeyDown(KeyCode.Escape)){
            // release cursor
            Cursor.lockState = CursorLockMode.None;

            entityManager.DestroyAndResetAllEntities();
            SceneManager.LoadScene("Menu");
        }

        if(camState > 0){
            yaw += Input.GetAxis("Mouse X") * 2f;
            pitch -= Input.GetAxis("Mouse Y") * 2f;
            cam.eulerAngles = new Vector3(pitch, yaw, 0f);
            if(camState == 2){
                cam.position = entityManager.GetComponentData<Translation>(entityToFollow).Value;
            }else if(camState == 1){
                cam.position += (cam.right * Input.GetAxis("Horizontal") + cam.forward * Input.GetAxis("Vertical")) * Time.deltaTime * 50;
                if(Input.GetKey(KeyCode.LeftShift)){
                    cam.position -= new Vector3(0, 50, 0) * Time.deltaTime;
                }
                if(Input.GetKey(KeyCode.Space)){
                    cam.position += new Vector3(0, 50, 0) * Time.deltaTime;
                }
            }else if(camState == 3){
                cam.position = GOAL.position;
            }
        }else{
            cam.LookAt(new Vector3(0, 0, 0));
            cam.RotateAround(new Vector3(0, 0, 0), Vector3.up, CAMERA_ROTATION_SPEED * Time.deltaTime);
        }
        
    }

    private void OnDestroy(){
        entityArrayBoids.Dispose();
    }
}
