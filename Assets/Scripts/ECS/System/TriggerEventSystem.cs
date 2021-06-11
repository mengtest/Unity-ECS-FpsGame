using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Physics;
using Unity.Burst;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

public class TriggerEventSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = endSimulationEcbSystem.CreateCommandBuffer();
        //��������boolֵ�������ж��Ƿ񲥷ű����л��߱���ɱ����Ч
        NativeArray<bool> isbehit = new NativeArray<bool>(2, Allocator.TempJob);
       
        TriggerJob triggerJob = new TriggerJob
        {
            #region ������������Group
            PhysicVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(),
            EnemyGroup = GetComponentDataFromEntity<Enemy>(),
            BeatBackGroup = GetComponentDataFromEntity<BeatBack>(),
            RotationGroup = GetComponentDataFromEntity<Rotation>(),
            HpGroup = GetComponentDataFromEntity<Hp>(),
            BulletGroup = GetComponentDataFromEntity<Bullet>(),
            DeleteGroup = GetComponentDataFromEntity<DeleteTag>(),
            TranslationGroup = GetComponentDataFromEntity<Translation>(),
            ecb = ecb,
            PhysicsColliderGroup = GetComponentDataFromEntity<PhysicsCollider>(),
            CharacterGroup = GetComponentDataFromEntity<Character>(),
            boom = FPSGameManager.instance.boomEntity,
            isbehit = isbehit,
            #endregion
        };
        Dependency = triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld,this.Dependency );
        Dependency.Complete();

        if (isbehit[0])
        {
            isbehit[0] = false;
            FPSGameManager.instance.PlayBehit();
        }
        
        if (isbehit[1])
        {
            isbehit[1] = false;
            FPSGameManager.instance.PlayBoom();
        }
        isbehit.Dispose();
    }

    [BurstCompile]
    private struct TriggerJob :ITriggerEventsJob
    {
        #region ����group

        public ComponentDataFromEntity<PhysicsVelocity> PhysicVelocityGroup;

        public ComponentDataFromEntity<Enemy> EnemyGroup;
        public ComponentDataFromEntity<BeatBack> BeatBackGroup;
        public ComponentDataFromEntity<Rotation> RotationGroup;
        public ComponentDataFromEntity<Hp> HpGroup;

        public ComponentDataFromEntity<Bullet> BulletGroup;
        public ComponentDataFromEntity<DeleteTag> DeleteGroup;
        public ComponentDataFromEntity<Translation> TranslationGroup;
        public ComponentDataFromEntity<Character> CharacterGroup;

        public EntityCommandBuffer ecb;

        public ComponentDataFromEntity<PhysicsCollider> PhysicsColliderGroup;

        public Entity boom;

        public NativeArray<bool> isbehit;
        #endregion

        public void Execute(TriggerEvent triggerEvent)
        {

            if (EnemyGroup.HasComponent(triggerEvent.EntityA))
            {
                //������������ײЧ��
                if (!BulletGroup.HasComponent(triggerEvent.EntityB) && BeatBackGroup.HasComponent(triggerEvent.EntityB))
                {

                    #region ����

                    BeatBack beatBack1 = BeatBackGroup[triggerEvent.EntityB];

                    if (beatBack1.curVelocity > 0.1f)
                    {
                        beatBack1.velocity += (5f - beatBack1.curVelocity) * 0.1f;

                    }
                    else
                    {
                        beatBack1.velocity = 5f;
                    }
                    if (RotationGroup.HasComponent(triggerEvent.EntityB))
                    {
                        Rotation rotation = RotationGroup[triggerEvent.EntityB];
                        beatBack1.rotation = rotation;
                    }

                    BeatBackGroup[triggerEvent.EntityB] = beatBack1;
                    #endregion
                    return;
                }
                isbehit[0] = true;

                #region ɾ���ӵ�

                float3 boomPos = float3.zero;
                if (TranslationGroup.HasComponent(triggerEvent.EntityA))
                {
                    Translation temp = TranslationGroup[triggerEvent.EntityB];
                    boomPos = temp.Value;
                    temp.Value = new float3(0, 100, 0);
                    TranslationGroup[triggerEvent.EntityB] = temp;
                    if (DeleteGroup.HasComponent(triggerEvent.EntityB))
                    {
                       DeleteTag temp1 = DeleteGroup[triggerEvent.EntityB];
                       temp1.lifeTime = 0f;
                        DeleteGroup[triggerEvent.EntityB] = temp1;
                    }
                   
                }

                #endregion

                #region �ӵ����˵���Ч��
                if (BeatBackGroup.HasComponent(triggerEvent.EntityA))
                {
                    BeatBack beatBack = BeatBackGroup[triggerEvent.EntityA];

                    if (beatBack.curVelocity > 0.1f)
                    {
                        beatBack.velocity += (5f - beatBack.curVelocity) * 0.1f;

                    }
                    else
                    {
                        beatBack.velocity = 5f;
                    }
                    if (RotationGroup.HasComponent(triggerEvent.EntityB))
                    {
                        Rotation rotation = RotationGroup[triggerEvent.EntityB];
                        beatBack.rotation = rotation;
                    }

                    BeatBackGroup[triggerEvent.EntityA] = beatBack;
                }

                #endregion

                #region ��Ѫ�����ɱ�ը����ʵ��
                if (HpGroup.HasComponent(triggerEvent.EntityA))
                {
                    Hp hp = HpGroup[triggerEvent.EntityA];
                    hp.HpValue--;
                    HpGroup[triggerEvent.EntityA] = hp;
                    if (hp.HpValue == 0)
                    {
                        //����������Ч
                        isbehit[1] = true;
                        Entity boomEntity = ecb.Instantiate(boom);
                        Translation translation = new Translation
                        {
                            Value = boomPos
                        };
                        ecb.SetComponent(boomEntity, translation);
                    }
                }
            

                #endregion
            }

            if (EnemyGroup.HasComponent(triggerEvent.EntityB))
            {

                if (!BulletGroup.HasComponent(triggerEvent.EntityA) && BeatBackGroup.HasComponent(triggerEvent.EntityA))
                {

                    #region ����
                    BeatBack beatBack1 = BeatBackGroup[triggerEvent.EntityA];

                    if (beatBack1.curVelocity > 0.1f)
                    {
                        beatBack1.velocity += (6f - beatBack1.curVelocity) * 0.1f;

                    }
                    else
                    {
                        beatBack1.velocity = 6f;
                    }
                    if (RotationGroup.HasComponent(triggerEvent.EntityA))
                    {
                        Rotation rotation = RotationGroup[triggerEvent.EntityA];
                        beatBack1.rotation = rotation;
                    }

                    BeatBackGroup[triggerEvent.EntityA] = beatBack1;
                    #endregion
                    return;
                }
                //���ű�������Ч
                isbehit[0] = true;

                #region ɾ���ӵ�
                float3 boomPos = float3.zero;
                if (TranslationGroup.HasComponent(triggerEvent.EntityA))
                {
                    Translation temp = TranslationGroup[triggerEvent.EntityA];
                    boomPos = temp.Value;
                    temp.Value = new float3(0, 100, 0);
                    TranslationGroup[triggerEvent.EntityA] = temp;
                    if (DeleteGroup.HasComponent(triggerEvent.EntityA))
                    {
                        DeleteTag temp1 = DeleteGroup[triggerEvent.EntityA];
                        temp1.lifeTime = 0f;
                        DeleteGroup[triggerEvent.EntityA] = temp1;
                    }
                }


                #endregion

                #region ����
                if (BeatBackGroup.HasComponent(triggerEvent.EntityB))
                {
                    BeatBack beatBack = BeatBackGroup[triggerEvent.EntityB];
                    if (beatBack.curVelocity > 0.1f)
                    {
                        beatBack.velocity = (6f - beatBack.curVelocity) * 0.1f;

                    }
                    else
                    {
                        beatBack.velocity = 6f;
                    }
                    if (RotationGroup.HasComponent(triggerEvent.EntityA))
                    {
                        Rotation rotation = RotationGroup[triggerEvent.EntityA];
                        beatBack.rotation = rotation;
                    }
                    BeatBackGroup[triggerEvent.EntityB] = beatBack;
                }


                #endregion

                #region ��Ѫ�����ɱ�ը����ʵ��
                if (HpGroup.HasComponent(triggerEvent.EntityB))
                {
                    Hp hp = HpGroup[triggerEvent.EntityB];
                    hp.HpValue--;
                    HpGroup[triggerEvent.EntityB] = hp;

                    if (hp.HpValue == 0)
                    {
                        //����������Ч
                        isbehit[1] = true;
                        Entity boomEntity = ecb.Instantiate(boom);
                        Translation translation = new Translation
                        {
                            Value = boomPos
                        };
                        ecb.SetComponent(boomEntity, translation);
                    }
                }

                #endregion

            }
        }
    }

}
