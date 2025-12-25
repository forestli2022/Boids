using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DataSetter : MonoBehaviour
{
    // spawn
    private int numBoids;
    private float spawnRange;
    private float roomSize;

    // boid movement
    private float maximumSpeed;
    private float maximumAcceleration;
    private float turningSpeed;
    
    // boid scales
    private float alignmentScale;
    private float cohesionScale;
    private float SeparationScale;
    private float targetScale;
    private float obstacleAvoidanceScale;

    // boid view
    private float maximumViewDistance;
    private float maximumViewAngle;

    // target
    private float targetDetectDistance;
    private float targetSpeed;
    private float targetMaxAcceleration;

    // obstacle
    private int numObstacles;
    private float obstacleSpeed;
    private float obstacleMaxAcceleration;
    private float obstacleDetectDistance;
    private float obstacleMinSize;
    private float obstacleMaxSize;

    // camera
    private float cameraDistanceToOrigin;
    private float cameraRotationSpeed;

    [Header("Default")]
    // spawn
    [SerializeField] private int numBoidsDefault;
    [SerializeField] private float spawnRangeDefault;
    [SerializeField] private float roomSizeDefault;

    // boid movement
    [SerializeField] private float maximumSpeedDefault;
    [SerializeField] private float maximumAccelerationDefault;
    [SerializeField] private float turningSpeedDefault;
    
    // boid scales
    [SerializeField] private float alignmentScaleDefault;
    [SerializeField] private float cohesionScaleDefault;
    [SerializeField] private float SeparationScaleDefault;
    [SerializeField] private float targetScaleDefault;
    [SerializeField] private float obstacleAvoidanceScaleDefault;

    // boid view
    [SerializeField] private float maximumViewDistanceDefault;
    [SerializeField] private float maximumViewAngleDefault;

    // target
    [SerializeField] private float targetDetectDistanceDefault;
    [SerializeField] private float targetSpeedDefault;
    [SerializeField] private float targetMaxAccelerationDefault;

    // obstacle
    [SerializeField] private int numObstaclesDefault;
    [SerializeField] private float obstacleSpeedDefault;
    [SerializeField] private float obstacleMaxAccelerationDefault;
    [SerializeField] private float obstacleDetectDistanceDefault;
    [SerializeField] private float obstacleMinSizeDefault;
    [SerializeField] private float obstacleMaxSizeDefault;
    
    // camera
    [SerializeField] private float cameraDistanceToOriginDefault;
    [SerializeField] private float cameraRotationSpeedDefault;

    [Header("References")]
    // spawn
    [SerializeField] private TMP_InputField numBoidsText;
    [SerializeField] private TMP_InputField spawnRangeText;
    [SerializeField] private TMP_InputField roomSizeText;

    // boid movement
    [SerializeField] private TMP_InputField maximumSpeedText;
    [SerializeField] private TMP_InputField maximumAccelerationText;
    [SerializeField] private TMP_InputField turningSpeedText;
    
    // boid scales
    [SerializeField] private TMP_InputField alignmentScaleText;
    [SerializeField] private TMP_InputField cohesionScaleText;
    [SerializeField] private TMP_InputField SeparationScaleText;
    [SerializeField] private TMP_InputField targetScaleText;
    [SerializeField] private TMP_InputField obstacleAvoidanceScaleText;

    // boid view
    [SerializeField] private TMP_InputField maximumViewDistanceText;
    [SerializeField] private TMP_InputField maximumViewAngleText;

    // target
    [SerializeField] private TMP_InputField targetDetectDistanceText;
    [SerializeField] private TMP_InputField targetSpeedText;
    [SerializeField] private TMP_InputField targetMaxAccelerationText;

    // obstacle
    [SerializeField] private TMP_InputField numObstaclesText;
    [SerializeField] private TMP_InputField obstacleSpeedText;
    [SerializeField] private TMP_InputField obstacleMaxAccelerationText;
    [SerializeField] private TMP_InputField obstacleDetectDistanceText;
    [SerializeField] private TMP_InputField obstacleMinSizeText;
    [SerializeField] private TMP_InputField obstacleMaxSizeText;

    // camera
    [SerializeField] private TMP_InputField cameraDistanceToOriginText;
    [SerializeField] private TMP_InputField cameraRotationSpeedText;

    public static DataSetter instance;

    private void Start(){
        if(instance == null){
            instance = this;
        }else{
            GameObject.Find("Canvas").SetActive(true);
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Reset();
    }

    public void Reset(){
        numBoids = numBoidsDefault;
        spawnRange = spawnRangeDefault;
        roomSize = roomSizeDefault;
        maximumSpeed = maximumSpeedDefault;
        maximumAcceleration = maximumAccelerationDefault;
        turningSpeed = turningSpeedDefault;
        alignmentScale = alignmentScaleDefault;
        cohesionScale = cohesionScaleDefault;
        SeparationScale = SeparationScaleDefault;
        targetScale = targetScaleDefault;
        obstacleAvoidanceScale = obstacleAvoidanceScaleDefault;
        maximumViewDistance = maximumViewDistanceDefault;
        maximumViewAngle = maximumViewAngleDefault;
        targetDetectDistance = targetDetectDistanceDefault;
        targetSpeed = targetSpeedDefault;
        targetMaxAcceleration = targetMaxAccelerationDefault;
        numObstacles = numObstaclesDefault;
        obstacleSpeed = obstacleSpeedDefault;
        obstacleMaxAcceleration = obstacleMaxAccelerationDefault;
        obstacleDetectDistance = obstacleDetectDistanceDefault;
        obstacleMinSize = obstacleMinSizeDefault;
        obstacleMaxSize = obstacleMaxSizeDefault;
        cameraDistanceToOrigin = cameraDistanceToOriginDefault;
        cameraRotationSpeed = cameraRotationSpeedDefault;
        Show();
    }

    private void Show(){
        numBoidsText.text = numBoids.ToString();
        spawnRangeText.text = spawnRange.ToString();
        roomSizeText.text = roomSize.ToString();
        maximumSpeedText.text = maximumSpeed.ToString();
        maximumAccelerationText.text = maximumAcceleration.ToString();
        turningSpeedText.text = turningSpeed.ToString();
        alignmentScaleText.text = alignmentScale.ToString();
        cohesionScaleText.text = cohesionScale.ToString();
        SeparationScaleText.text = SeparationScale.ToString();
        targetScaleText.text = targetScale.ToString();
        obstacleAvoidanceScaleText.text = obstacleAvoidanceScale.ToString();
        maximumViewDistanceText.text = maximumViewDistance.ToString();
        maximumViewAngleText.text = maximumViewAngle.ToString();
        targetDetectDistanceText.text = targetDetectDistance.ToString();
        targetSpeedText.text = targetSpeed.ToString();
        targetMaxAccelerationText.text = targetMaxAcceleration.ToString();
        numObstaclesText.text = numObstacles.ToString();
        obstacleSpeedText.text = obstacleSpeed.ToString();
        obstacleMaxAccelerationText.text = obstacleMaxAcceleration.ToString();
        obstacleDetectDistanceText.text = obstacleDetectDistance.ToString();
        obstacleMinSizeText.text = obstacleMinSize.ToString();
        obstacleMaxSizeText.text = obstacleMaxSize.ToString();
        cameraDistanceToOriginText.text = cameraDistanceToOrigin.ToString();
        cameraRotationSpeedText.text = cameraRotationSpeed.ToString();
    }

    public void SetData(){
        Brain b = GameObject.Find("Brain").GetComponent<Brain>();

        b.BOIDS_NUMBER = numBoids;
        b.SPAWN_RANGE = spawnRange;
        b.ROOM_SIZE = roomSize;
        b.BOID_MAX_SPEED = maximumSpeed;
        b.BOID_MAX_ACCELERATION_MAGNITUDE = maximumAcceleration;
        b.TURNING_SPEED = turningSpeed;
        b.BOID_ALIGNMENT_SCALE = alignmentScale;
        b.BOID_COHESION_SCALE = cohesionScale;
        b.BOID_SEPARATION_SCALE = SeparationScale;
        b.BOID_GOAL_SCALE = targetScale;
        b.BOID_OBSTACLE_AVOIDANCE_SCALE = obstacleAvoidanceScale;
        b.MAX_VIEW_DISTANCE = maximumViewDistance;
        b.MAX_VIEW_ANGLE = maximumViewAngle;
        b.GOAL_DETECTION_DIST = targetDetectDistance;
        b.GOAL_SPEED = targetSpeed;
        b.GOAL_MAX_ACCELERATION_MAGNITUDE = targetMaxAcceleration;
        b.OBSTACLES_NUMBER = numObstacles;
        b.OBSTACLE_SPEED = obstacleSpeed;
        b.OBSTACLE_MAX_ACCELERATION_MAGNITUDE = obstacleMaxAcceleration;
        b.OBSTACLE_TURNING_DIST = obstacleDetectDistance;
        b.OBSTACLE_MIN_SIZE = obstacleMinSize;
        b.OBSTACLE_MAX_SIZE = obstacleMaxSize;
        b.CAMERA_DISTANCE_TO_ORIGIN = cameraDistanceToOrigin;
        b.CAMERA_ROTATION_SPEED = cameraRotationSpeed;
    }

    public void GetNumBoids(string input){
        if(input.Length == 0){
            input = "0";
        }
        numBoids = int.Parse(input);
        Show();
    }
    public void GetSpawnRange(string input){
        if(input.Length == 0){
            input = "0";
        }
        spawnRange = float.Parse(input);
        Show();
    }
    public void GetRoomSize(string input){
        if(input.Length == 0){
            input = "0";
        }
        roomSize = float.Parse(input);
        Show();
    }
    public void GetMaximumSpeed(string input){
        if(input.Length == 0){
            input = "0";
        }
        maximumSpeed = float.Parse(input);
        Show();
    }
    public void GetMaximumAcceleration(string input){
        if(input.Length == 0){
            input = "0";
        }
        maximumAcceleration = float.Parse(input);
        Show();
    }
    public void GetTurningSpeed(string input){
        if(input.Length == 0){
            input = "0";
        }
        turningSpeed = float.Parse(input);
        Show();
    }
    public void GetAlignmentScale(string input){
        if(input.Length == 0){
            input = "0";
        }
        alignmentScale = float.Parse(input);
        Show();
    }
    public void GetCohesionScale(string input){
        if(input.Length == 0){
            input = "0";
        }
        cohesionScale = float.Parse(input);
        Show();
    }
    public void GetSeparationScale(string input){
        if(input.Length == 0){
            input = "0";
        }
        SeparationScale = float.Parse(input);
        Show();
    }
    public void GetTargetScale(string input){
        if(input.Length == 0){
            input = "0";
        }
        targetScale = float.Parse(input);
        Show();
    }
    public void GetObstacleAvoidanceScale(string input){
        if(input.Length == 0){
            input = "0";
        }
        obstacleAvoidanceScale = float.Parse(input);
        Show();
    }
    public void GetMaximumViewDistance(string input){
        if(input.Length == 0){
            input = "0";
        }
        maximumViewDistance = float.Parse(input);
        Show();
    }
    public void GetMaximumViewAngle(string input){
        if(input.Length == 0){
            input = "0";
        }
        maximumViewAngle = float.Parse(input);
        Show();
    }
    public void GetTargetDetectionDistance(string input){
        if(input.Length == 0){
            input = "0";
        }
        targetDetectDistance = float.Parse(input);
        Show();
    }
    public void GetTargetSpeed(string input){
        if(input.Length == 0){
            input = "0";
        }
        targetSpeed = float.Parse(input);
        Show();
    }
    public void GetTargetMaxAcceleration(string input){
        if(input.Length == 0){
            input = "0";
        }
        targetMaxAcceleration = float.Parse(input);
        Show();
    }
    public void GetNumObstacles(string input){
        if(input.Length == 0){
            input = "0";
        }
        numObstacles = int.Parse(input);
        Show();
    }
    public void GetObstacleSpeed(string input){
        if(input.Length == 0){
            input = "0";
        }
        obstacleSpeed = float.Parse(input);
        Show();
    }
    public void GetObstacleMaxAcceleration(string input){
        if(input.Length == 0){
            input = "0";
        }
        obstacleMaxAcceleration = float.Parse(input);
        Show();
    }
    public void GetObstacleDetectionDistance(string input){
        if(input.Length == 0){
            input = "0";
        }
        obstacleDetectDistance = float.Parse(input);
        Show();
    }
    public void GetObstacleMinSize(string input){
        if(input.Length == 0){
            input = "0";
        }
        obstacleMinSize = float.Parse(input);
        Show();
    }
    public void GetObstacleMaxSize(string input){
        if(input.Length == 0){
            input = "0";
        }
        obstacleMaxSize = float.Parse(input);
        Show();
    }
    public void GetCameraDistanceToOrigin(string input){
        if(input.Length == 0){
            input = "0";
        }
        cameraDistanceToOrigin = float.Parse(input);
        Show();
    }
    public void GetCameraRotationSpeed(string input){
        if(input.Length == 0){
            input = "0";
        }
        cameraRotationSpeed = float.Parse(input);
        Show();
    }
}
