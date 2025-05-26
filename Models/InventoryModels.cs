using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StardewValleyMCP.Models
{
    /// <summary>
    /// Mô hình đại diện cho một vật phẩm trong túi đồ
    /// </summary>
    public class InventoryItemModel
    {
        /// <summary>
        /// Vị trí của vật phẩm trong túi đồ (bắt đầu từ 1)
        /// </summary>
        public int SlotNumber { get; set; }
        
        /// <summary>
        /// Tên của vật phẩm
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Số lượng vật phẩm
        /// </summary>
        public int Stack { get; set; }
        
        /// <summary>
        /// ID của vật phẩm
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// Chất lượng của vật phẩm (0 = thông thường, 1 = bạc, 2 = vàng, 3 = iridium)
        /// </summary>
        public int Quality { get; set; }
        
        /// <summary>
        /// Loại vật phẩm
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mô hình đại diện cho toàn bộ túi đồ
    /// </summary>
    public class InventoryModel
    {
        /// <summary>
        /// Tên người chơi
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;
        
        /// <summary>
        /// Tổng số vật phẩm trong túi đồ
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// Dung lượng tối đa của túi đồ
        /// </summary>
        public int MaxItems { get; set; }
        
        /// <summary>
        /// Danh sách các vật phẩm trong túi đồ
        /// </summary>
        public List<InventoryItemModel> Items { get; set; } = new List<InventoryItemModel>();
        
        /// <summary>
        /// Thời gian lấy dữ liệu
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Mô hình yêu cầu lấy thông tin vật phẩm bằng phương thức POST
    /// </summary>
    public class InventoryItemRequest
    {
        /// <summary>
        /// Vị trí của vật phẩm trong túi đồ (bắt đầu từ 1)
        /// </summary>
        [JsonPropertyName("slotNumber")]
        public int SlotNumber { get; set; }
    }
}
