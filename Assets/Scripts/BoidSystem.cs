using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Rendering;


[BurstCompile]
public partial class BoidSystem : SystemBase
{
    private static NativeParallelMultiHashMap<int3, BoidComponent> organizedBoids;
    private static NativeArray<int3> cellDirection;

    private NativeArray<ObstacleComponent> obstaclesArray;

    private static float Magnitude(float3 v)
    {
        return math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }

    public static int3 GetCellPosition(float3 position, float size)
    {
        return new int3((int)math.floor(position.x / size), (int)math.floor(position.y / size), (int)math.floor(position.z / size));
    }

    public static float AngleBetweenVectors(float3 a, float3 b)
    {
        return math.degrees(math.acos(math.dot(math.normalize(a), math.normalize(b))));
    }

    protected override void OnCreate()
    {
        EntityQuery entityQuery = GetEntityQuery(ComponentType.ReadOnly<BoidComponent>());
        organizedBoids = new NativeParallelMultiHashMap<int3, BoidComponent>(0, Allocator.Persistent);

        // organize boids in cells, creating the list of adjacent cells first
        cellDirection = new NativeArray<int3>(27, Allocator.Persistent);
        int cnt = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    cellDirection[cnt] = new int3(i, j, k);
                    cnt++;
                }
            }
        }


    }

    protected override void OnUpdate()
    {
        // create parallel writer for native parallel hash map
        NativeParallelMultiHashMap<int3, BoidComponent>.ParallelWriter organizedBoidParallel = organizedBoids.AsParallelWriter();
        organizedBoids.Clear();
        EntityQuery entityQuery = GetEntityQuery(ComponentType.ReadOnly<BoidComponent>());
        if (entityQuery.CalculateEntityCount() > organizedBoids.Capacity)
        {
            organizedBoids.Capacity = entityQuery.CalculateEntityCount();
        }

        Entities.WithBurst().ForEach((ref BoidComponent b, ref Translation translation) =>
        {
            b.position = translation.Value;
            organizedBoidParallel.Add(GetCellPosition(translation.Value, b.maxViewDistance), b);
        }).ScheduleParallel();


        // find goal translation
        float3 goalPosition = float3.zero;
        int goalCnt = 0;
        Entities.
        ForEach((ref Goal goal, in Translation translation) =>
        {
            goalPosition += translation.Value;
            goalCnt++;
        }).Run();
        goalPosition /= goalCnt;

        // store all obstacles in a native array
        entityQuery = GetEntityQuery(ComponentType.ReadOnly<ObstacleComponent>());
        obstaclesArray = new NativeArray<ObstacleComponent>(entityQuery.CalculateEntityCount(), Allocator.Persistent);
        int cnt = 0;
        Entities.WithoutBurst().ForEach((in ObstacleComponent ob) =>
        {
            obstaclesArray[cnt] = ob;
            cnt++;
        }).Run();

        // make copy of static objects
        NativeParallelMultiHashMap<int3, BoidComponent> organizedBoidsCopy = organizedBoids;
        NativeArray<int3> cellDirectionCopy = cellDirection;
        NativeArray<ObstacleComponent> obstaclesArrayCopy = obstaclesArray;


        float deltaTime = Time.DeltaTime;

        // MAIN ALGORITHM
        Entities.
        WithBurst().
        WithReadOnly(organizedBoidsCopy).
        WithReadOnly(cellDirectionCopy).
        WithReadOnly(obstaclesArrayCopy).
        ForEach((ref BoidComponent b, ref Translation translation, ref Rotation rotation) =>
        {
            float3 alignment = float3.zero;
            float3 cohesion = float3.zero;
            float3 separation = float3.zero;
            float3 goal = float3.zero;
            BoidComponent boidComponent = default;
            NativeParallelMultiHashMapIterator<int3> iterator = default;
            float boidCnt = 0;

            // get boid position in cell
            int3 cellPos = GetCellPosition(translation.Value, b.maxViewDistance);

            // try get values from that cell
            for (int i = 0; i < 27; i++)
            {
                if (organizedBoidsCopy.TryGetFirstValue(cellPos + cellDirectionCopy[i], out boidComponent, out iterator))
                {
                    do
                    {
                        if (!translation.Value.Equals(boidComponent.position) &&
                        math.distance(translation.Value, boidComponent.position) < b.maxViewDistance &&
                        AngleBetweenVectors(b.velocity, boidComponent.position - translation.Value) < b.maxViewAngle / 2)
                        {
                            float3 dist = translation.Value - boidComponent.position;

                            alignment += boidComponent.velocity;
                            cohesion += boidComponent.position;
                            separation += (dist / math.distance(boidComponent.position, translation.Value));
                            boidCnt++;
                        }
                    } while (organizedBoidsCopy.TryGetNextValue(out boidComponent, ref iterator));
                }
            }


            // calculate total average alignment, cohesion and separation direction
            if (boidCnt > 0)
            {
                alignment = math.normalize(alignment / boidCnt - b.velocity) * b.alignmentScale;
                cohesion = math.normalize(cohesion / boidCnt - translation.Value - b.velocity) * b.cohesionScale;
                separation = math.normalize(separation / boidCnt - b.velocity) * b.seperationScale * -1;
            }
            // calculate whether goal is in view
            if (math.distance(goalPosition, translation.Value) < b.goalDetectionDist &&
            AngleBetweenVectors(goalPosition - translation.Value, b.velocity) < b.maxViewAngle)
            {
                goal = math.normalize(goalPosition - translation.Value - b.velocity) * b.goalScale;
            }

            // calculate wall distance and steer away from them
            float halfRoomSize = b.roomSize / 2;
            float minDistToWall = Mathf.Min(Mathf.Min(halfRoomSize - Mathf.Abs(translation.Value.x),
                                                            halfRoomSize - Mathf.Abs(translation.Value.y)),
                                                            halfRoomSize - Mathf.Abs(translation.Value.z));
            float3 wallAvoid = float3.zero;
            if (minDistToWall < b.wallTurningDist)
            {
                wallAvoid = -math.normalize(translation.Value) * (1 / (minDistToWall + 0.00000001f)) * b.obstacleAvoidanceScale;
            }

            // obstacle avoidance
            float3 obstacleAvoid = float3.zero;
            foreach (ObstacleComponent obstacleComponent in obstaclesArrayCopy)
            {
                float distObstacle = math.distance(translation.Value, obstacleComponent.position) - obstacleComponent.scale / 2;
                if (distObstacle < b.obstacleTurningDist)
                {
                    obstacleAvoid += math.normalize(translation.Value - obstacleComponent.position) * (1 / (distObstacle + 0.00000001f)) * b.obstacleAvoidanceScale;
                }
            }

            // update boid movement states
            b.acceleration += (alignment + cohesion + separation + goal + wallAvoid + obstacleAvoid);
            if (Magnitude(b.acceleration) > b.maxAccelerationMagnitude)
            {
                b.acceleration = math.normalize(b.acceleration) * b.maxAccelerationMagnitude;
            }
            b.velocity += b.acceleration * deltaTime;
            if (Magnitude(b.velocity) > b.maxSpeed)
            {
                b.velocity = math.normalize(b.velocity) * b.maxSpeed;
            }
            rotation.Value = math.slerp(rotation.Value, quaternion.LookRotation(math.normalize(b.velocity), math.forward()), 1);
            translation.Value += b.velocity * deltaTime;

            // clamp boids translation so no boids gets outside the room
            translation.Value.x = math.clamp(translation.Value.x, -halfRoomSize, halfRoomSize);
            translation.Value.y = math.clamp(translation.Value.y, -halfRoomSize, halfRoomSize);
            translation.Value.z = math.clamp(translation.Value.z, -halfRoomSize, halfRoomSize);

            // clamp boids translation so no boids stuck in obstacles
            foreach (ObstacleComponent obstacleComponent in obstaclesArrayCopy)
            {
                float distObstacle = math.distance(translation.Value, obstacleComponent.position) - obstacleComponent.scale / 2;
                if (distObstacle < 0)
                {
                    translation.Value = obstacleComponent.position + math.normalize(translation.Value - obstacleComponent.position) * (obstacleComponent.scale / 2);
                }
            }
        }).ScheduleParallel();
        obstaclesArray.Dispose();
    }

    protected override void OnDestroy()
    {
        organizedBoids.Dispose();
        cellDirection.Dispose();
    }
}
