using Unity.Entities;

//��ǹ������ǹ���Զ�ģʽ
public enum WeaponType
{
    gun,
    shotgun,
    gunAutoshot
}
[GenerateAuthoringComponent]
public struct  Weapon : IComponentData
{
    //ǹ��λ��
    public Entity gunPoint;
    //��������
    public WeaponType weaponType;
    //�Ƿ������л�����
    public bool canSwitch;

    //��ǹ���
    public float firingInterval;
    //������¼ÿ�ο�ǹ��ʱ��
    public float shotTime;
}
