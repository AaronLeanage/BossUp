using Unity.Entities;

namespace BossUp.Components
{
    /// <summary>
    /// Tags a customer group entity as a boss group. Customer groups never survive into a save
    /// (the game saves between days), so this never ends up in save data.
    /// </summary>
    public struct CBossGroup : IComponentData
    {
    }
}
