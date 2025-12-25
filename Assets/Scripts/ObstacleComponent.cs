using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public struct ObstacleComponent : IComponentData
{
    public float speed;
    public float maxAccelerationMagnitude;
    public float3 acceleration;
    public float3 velocity;
    public float roomSize;
    public float scale;
    public float3 position;
}
