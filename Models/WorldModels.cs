using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StardewValleyMCP.Models
{
    /// <summary>
    /// Yêu cầu quét vật thể xung quanh
    /// </summary>
    public class ScanObjectsRequest
    {
        /// <summary>
        /// Bán kính quét (tính bằng ô)
        /// </summary>
        [JsonPropertyName("radius")]
        public int Radius { get; set; } = 5; // Giá trị mặc định là 5 ô
    }

    /// <summary>
    /// Thông tin về một vật thể trong thế giới game
    /// </summary>
    public class WorldObjectModel
    {
        /// <summary>
        /// Tên của vật thể
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Loại vật thể
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Vị trí X của vật thể
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Vị trí Y của vật thể
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Khoảng cách từ người chơi đến vật thể (tính bằng ô)
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Thông tin bổ sung về vật thể (nếu có)
        /// </summary>
        public Dictionary<string, object> AdditionalInfo { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Kết quả quét vật thể xung quanh
    /// </summary>
    public class ScanObjectsResponse
    {
        /// <summary>
        /// Vị trí X của người chơi
        /// </summary>
        public int PlayerX { get; set; }

        /// <summary>
        /// Vị trí Y của người chơi
        /// </summary>
        public int PlayerY { get; set; }

        /// <summary>
        /// Tên của địa điểm hiện tại
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// Bán kính quét (tính bằng ô)
        /// </summary>
        public int Radius { get; set; }

        /// <summary>
        /// Danh sách các vật thể được tìm thấy
        /// </summary>
        public List<WorldObjectModel> Objects { get; set; } = new List<WorldObjectModel>();

        /// <summary>
        /// Thời gian quét
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
