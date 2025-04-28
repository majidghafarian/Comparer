 
namespace Domain
{
    [Flags]
    public enum AnckerType
    {
        None = 0,
        Up = 1,     // 2^0
        Down = 2,   // 2^1
        Right = 4,  // 2^2
        Left = 8    // 2^3
    }

}
