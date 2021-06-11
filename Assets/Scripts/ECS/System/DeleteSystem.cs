using Unity.Entities;
using Unity.Jobs;


public class DeleteSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    }
    protected override void OnUpdate()
    {
         // ����һ��ECS����ת���ɿɲ��е�
        var ecb = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        Entities
           .ForEach((Entity entity, int entityInQueryIndex, in DeleteTag deleteTag) =>
      {
          if (deleteTag.lifeTime <=0f)
          {
              ecb.DestroyEntity(entityInQueryIndex, entity);
            
          }
      }).ScheduleParallel();

        // ��֤ECB system������ǰ���Job
        endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}
