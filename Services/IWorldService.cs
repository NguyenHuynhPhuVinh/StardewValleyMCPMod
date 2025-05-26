using StardewValleyMCP.Models;

namespace StardewValleyMCP.Services
{
    /// <summary>
    /// Interface cho dịch vụ quản lý thế giới game
    /// </summary>
    public interface IWorldService
    {
        /// <summary>
        /// Quét các vật thể xung quanh người chơi
        /// </summary>
        /// <param name="radius">Bán kính quét (tính bằng ô)</param>
        /// <returns>Thông tin về các vật thể xung quanh</returns>
        ScanObjectsResponse ScanObjects(int radius);
    }
}
