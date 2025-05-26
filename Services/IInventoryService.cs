using StardewValleyMCP.Models;

namespace StardewValleyMCP.Services
{
    /// <summary>
    /// Interface cho dịch vụ quản lý túi đồ
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Lấy thông tin túi đồ của người chơi
        /// </summary>
        /// <returns>Thông tin túi đồ</returns>
        InventoryModel GetPlayerInventory();

        /// <summary>
        /// Lấy thông tin chi tiết về một vật phẩm cụ thể trong túi đồ
        /// </summary>
        /// <param name="slotNumber">Vị trí của vật phẩm (bắt đầu từ 1)</param>
        /// <returns>Thông tin chi tiết về vật phẩm</returns>
        InventoryItemModel? GetInventoryItem(int slotNumber);
    }
}
