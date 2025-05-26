using StardewValley;
using System;
using System.Linq;
using TestMod_SV.Models;
using StardewModdingAPI;

namespace TestMod_SV.Services
{
    /// <summary>
    /// Dịch vụ quản lý túi đồ
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IMonitor _monitor;

        public InventoryService(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Lấy thông tin túi đồ của người chơi
        /// </summary>
        /// <returns>Thông tin túi đồ</returns>
        public InventoryModel GetPlayerInventory()
        {
            _monitor.Log("Đang lấy thông tin túi đồ qua API", StardewModdingAPI.LogLevel.Info);

            var inventory = new InventoryModel
            {
                PlayerName = Game1.player.Name,
                TotalItems = Game1.player.Items.Count(item => item != null),
                MaxItems = Game1.player.MaxItems,
                Timestamp = DateTime.Now
            };

            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                var item = Game1.player.Items[i];
                if (item != null)
                {
                    inventory.Items.Add(ConvertToInventoryItemModel(item, i + 1));
                }
            }

            return inventory;
        }

        /// <summary>
        /// Lấy thông tin chi tiết về một vật phẩm cụ thể trong túi đồ
        /// </summary>
        /// <param name="slotNumber">Vị trí của vật phẩm (bắt đầu từ 1)</param>
        /// <returns>Thông tin chi tiết về vật phẩm</returns>
        public InventoryItemModel? GetInventoryItem(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > Game1.player.Items.Count)
            {
                return null;
            }

            var item = Game1.player.Items[slotNumber - 1];
            if (item == null)
            {
                return null;
            }

            return ConvertToInventoryItemModel(item, slotNumber);
        }

        /// <summary>
        /// Chuyển đổi từ đối tượng Item của game sang mô hình InventoryItemModel
        /// </summary>
        /// <param name="item">Đối tượng Item của game</param>
        /// <param name="slotNumber">Vị trí của vật phẩm</param>
        /// <returns>Mô hình InventoryItemModel</returns>
        private InventoryItemModel ConvertToInventoryItemModel(StardewValley.Item item, int slotNumber)
        {
            string category = "Khác";

            if (item is StardewValley.Object obj)
            {
                category = GetCategoryName(obj);
            }
            else if (item is StardewValley.Tool)
            {
                category = "Công cụ";
            }
            else if (item is StardewValley.Objects.Furniture)
            {
                category = "Nội thất";
            }
            else if (item is StardewValley.Objects.Ring)
            {
                category = "Nhẫn";
            }
            else if (item is StardewValley.Objects.Boots)
            {
                category = "Giày";
            }
            else if (item is StardewValley.Objects.Hat)
            {
                category = "Mũ";
            }

            return new InventoryItemModel
            {
                SlotNumber = slotNumber,
                Name = item.Name,
                Stack = item.Stack,
                ItemId = item.ParentSheetIndex,
                Quality = item is StardewValley.Object obj2 ? obj2.Quality : 0,
                Category = category
            };
        }

        /// <summary>
        /// Lấy tên danh mục dựa trên loại vật phẩm
        /// </summary>
        /// <param name="obj">Đối tượng vật phẩm</param>
        /// <returns>Tên danh mục</returns>
        private string GetCategoryName(StardewValley.Object obj)
        {
            // Sử dụng các giá trị số thay vì hằng số không tồn tại
            if (obj.Category == -81) // VegetableCategory
                return "Rau củ";
            if (obj.Category == -79) // FruitsCategory
                return "Trái cây";
            if (obj.Category == -74) // SeedsCategory
                return "Hạt giống";
            if (obj.Category == -12) // mineralsCategory
                return "Khoáng sản";
            if (obj.Category == -26) // artisanGoodsCategory
                return "Hàng thủ công";
            if (obj.Category == -7) // foodCategory
                return "Thức ăn";
            if (obj.Category == -4) // fishCategory
                return "Cá";
            if (obj.Category == -22) // meatCategory
                return "Thịt";
            if (obj.Category == -20) // junkCategory
                return "Rác";
            if (obj.Category == -16) // resourceCategory
                return "Tài nguyên";
            if (obj.Category == -8) // craftingCategory
                return "Vật liệu chế tạo";
            if (obj.Category == -9) // bigCraftablesCategory
                return "Vật phẩm lớn";
            if (obj.Category == -24) // furnitureCategory
                return "Nội thất";
            if (obj.Category == -5) // ingredientsCategory
                return "Nguyên liệu";

            return "Khác";
        }
    }
}
