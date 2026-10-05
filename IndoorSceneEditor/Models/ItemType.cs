namespace IndoorSceneEditor.Models
{
    /// <summary>
    /// 可交换的室内物品类型。物品之间可以互相替换（Swap），
    /// 替换时保留位置、旋转、缩放和颜色。
    /// </summary>
    public enum ItemType
    {
        Sofa,       // 沙发
        Table,      // 桌子
        Chair,      // 椅子
        Bed,        // 床
        Cabinet,    // 柜子
        Lamp,       // 落地灯
        Plant,      // 盆栽
        TV,         // 电视
        Shelf,      // 书架
        Rug         // 地毯
    }
}
