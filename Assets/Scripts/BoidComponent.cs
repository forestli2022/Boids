using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct BoidComponent : IComponentData
{
    public float speed;
    // spawn
    public int boidsNumber;

    // translation
    public float3 position;
    public float3 velocity;
    public float3 acceleration;
    public float maxSpeed;
    public float maxAccelerationMagnitude;
    public float turningSpeed;

    // boid scales
    public float alignmentScale;
    public float cohesionScale;
    public float seperationScale;
    public float goalScale;
    public float obstacleAvoidanceScale;

    // view
    public float maxViewDistance;
    public float maxViewAngle;
    public float goalDetectionDist;

    // wall limits
    public float roomSize;
    public float wallTurningDist;

    // obstacle avoidance
    public float obstacleTurningDist;
}
