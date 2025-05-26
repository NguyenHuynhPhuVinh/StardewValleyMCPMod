using System;
using System.Net;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewValleyMCP.Services;

namespace StardewValleyMCP.Api.Controllers
{
    /// <summary>
    /// Controller API cho túi đồ
    /// </summary>
    public class InventoryApiController : ApiController
    {
        private readonly IInventoryService _inventoryService;

        /// <summary>
        /// Khởi tạo controller API túi đồ
        /// </summary>
        /// <param name="monitor">Monitor để ghi log</param>
        /// <param name="inventoryService">Dịch vụ quản lý túi đồ</param>
        public InventoryApiController(IMonitor monitor, IInventoryService inventoryService) 
            : base(monitor)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Xử lý yêu cầu HTTP
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="segments">Các phân đoạn của URL</param>
        /// <returns>Task</returns>
        public override Task ProcessRequestAsync(HttpListenerContext context, string[] segments)
        {
            // Kiểm tra phương thức HTTP
            if (context.Request.HttpMethod != "GET")
            {
                BadRequest(context, "Chỉ hỗ trợ phương thức GET");
                return Task.CompletedTask;
            }

            // Kiểm tra người chơi đã vào thế giới game chưa
            if (!Game1.hasLoadedGame)
            {
                BadRequest(context, "Người chơi chưa vào thế giới game");
                return Task.CompletedTask;
            }

            try
            {
                // Ghi log các phân đoạn URL để debug
                Monitor.Log($"InventoryApiController nhận phân đoạn: {string.Join(", ", segments)}", LogLevel.Debug);
                
                // Xử lý các endpoint khác nhau
                // Kiểm tra xem URL có kết thúc bằng /api/inventory không
                if (segments.Length >= 2 && segments[segments.Length - 2] == "api" && segments[segments.Length - 1] == "inventory")
                {
                    GetInventory(context);
                    return Task.CompletedTask;
                }
                
                // Kiểm tra xem URL có kết thúc bằng /api/inventory/item/{slotNumber} không
                if (segments.Length >= 4 && 
                    segments[segments.Length - 4] == "api" && 
                    segments[segments.Length - 3] == "inventory" && 
                    segments[segments.Length - 2] == "item")
                {
                    GetInventoryItem(context, segments[segments.Length - 1]);
                    return Task.CompletedTask;
                }
                
                // Nếu không phải các trường hợp trên, trả về lỗi
                NotFound(context, "Endpoint không tồn tại");
                Monitor.Log($"Endpoint không tồn tại: {string.Join("/", segments)}", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Lỗi khi xử lý yêu cầu: {ex.Message}", LogLevel.Error);
                InternalServerError(context, $"Lỗi máy chủ: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Xử lý yêu cầu lấy thông tin túi đồ
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        private void GetInventory(HttpListenerContext context)
        {
            var inventory = _inventoryService.GetPlayerInventory();
            Ok(context, inventory);
        }

        /// <summary>
        /// Xử lý yêu cầu lấy thông tin một vật phẩm cụ thể
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="slotNumberStr">Chuỗi vị trí vật phẩm</param>
        private void GetInventoryItem(HttpListenerContext context, string slotNumberStr)
        {
            if (!int.TryParse(slotNumberStr, out int slotNumber))
            {
                BadRequest(context, "Vị trí vật phẩm không hợp lệ");
                return;
            }

            if (slotNumber < 1 || slotNumber > Game1.player.MaxItems)
            {
                BadRequest(context, $"Vị trí không hợp lệ. Phải từ 1 đến {Game1.player.MaxItems}");
                return;
            }

            var item = _inventoryService.GetInventoryItem(slotNumber);
            
            if (item == null)
            {
                NotFound(context, $"Không tìm thấy vật phẩm ở vị trí {slotNumber}");
                return;
            }

            Ok(context, item);
        }
    }
}
