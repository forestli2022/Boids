using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[BurstCompile]
public partial class ObstacleSystem : SystemBase
{
    private static float Magnitude(float3 v){
        return math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.
        WithBurst().
        ForEach((ref ObstacleComponent obstacleComponent, ref Translation translation, in Scale scale)=> 
        {
            obstacleComponent.acceleration += (float3)Random.insideUnitSphere * obstacleComponent.maxAccelerationMagnitude;
        }).Run();

        Entities.
        WithBurst().
        ForEach((ref ObstacleComponent obstacleComponent, ref Translation translation, in Scale scale) =>
        {
            float halfRoomSize = obstacleComponent.roomSize / 2 - scale.Value / 2;

            float minDistToWall = Mathf.Min(Mathf.Min(halfRoomSize - Mathf.Abs(translation.Value.x), 
                                                            halfRoomSize - Mathf.Abs(translation.Value.y)), 
                                                            halfRoomSize - Mathf.Abs(translation.Value.z));
            if(minDistToWall < scale.Value){
                obstacleComponent.acceleration -= translation.Value;
            }

            if(Magnitude(obstacleComponent.acceleration) > obstacleComponent.maxAccelerationMagnitude){
                obstacleComponent.acceleration = math.normalize(obstacleComponent.acceleration) * obstacleComponent.maxAccelerationMagnitude;
            }

            obstacleComponent.velocity += obstacleComponent.acceleration * deltaTime;
            obstacleComponent.velocity = math.normalize(obstacleComponent.velocity) * obstacleComponent.speed;
            translation.Value += obstacleComponent.velocity * deltaTime;

            translation.Value = new float3(math.clamp(translation.Value.x, -halfRoomSize, halfRoomSize),
                                            math.clamp(translation.Value.y, -halfRoomSize, halfRoomSize),
                                            math.clamp(translation.Value.z, -halfRoomSize, halfRoomSize));

            obstacleComponent.scale = scale.Value;
            obstacleComponent.position = translation.Value;
        }).ScheduleParallel();
    }
}
