using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class BulletSystem : SystemBase
{
    protected override void OnUpdate()
    {

        float deltaTime = Time.DeltaTime;
        //����ֻ��Ϊ��չʾ�Զ���EntityCommandBuffer���÷���Ϊ�����ܿ���һ�㻹����ϵͳ�ṩ��ecb
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        Entities.
            ForEach((Entity entity, ref Bullet bullet, ref Translation translation,in Rotation rot) =>
        {
            translation.Value += bullet.flySpeed * deltaTime * math.forward(rot.Value);
            bullet.lifetime-=deltaTime;
            if (bullet.lifetime <=0)
            {
                translation.Value = new float3(0, 100, 0);
                DeleteTag deleteTag = new DeleteTag
                {
                    delayTime=1f
                };
                ecb.AddComponent(entity, deleteTag);
             
            }
           
        }).Run();

        ecb.Playback(World.DefaultGameObjectInjectionWorld.EntityManager);
        ecb.Dispose();
    }
}
