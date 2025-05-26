using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewValleyMCP.Models;
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
        public override async Task ProcessRequestAsync(HttpListenerContext context, string[] segments)
        {
            // Kiểm tra người chơi đã vào thế giới game chưa
            if (!Game1.hasLoadedGame)
            {
                BadRequest(context, "Người chơi chưa vào thế giới game");
                return;
            }

            try
            {
                // Ghi log các phân đoạn URL để debug
                Monitor.Log($"InventoryApiController nhận phân đoạn: {string.Join(", ", segments)}", LogLevel.Debug);
                Monitor.Log($"Phương thức HTTP: {context.Request.HttpMethod}", LogLevel.Debug);
                
                // Xử lý các endpoint khác nhau
                // Kiểm tra xem URL có kết thúc bằng /api/inventory không
                if (segments.Length >= 2 && segments[segments.Length - 2] == "api" && segments[segments.Length - 1] == "inventory")
                {
                    if (context.Request.HttpMethod == "GET")
                    {
                        GetInventory(context);
                    }
                    else
                    {
                        BadRequest(context, "Endpoint này chỉ hỗ trợ phương thức GET");
                    }
                    return;
                }
                
                // Kiểm tra xem URL có kết thúc bằng /api/inventory/item không
                if (segments.Length >= 3 && 
                    segments[segments.Length - 3] == "api" && 
                    segments[segments.Length - 2] == "inventory" && 
                    segments[segments.Length - 1] == "item")
                {
                    if (context.Request.HttpMethod == "POST")
                    {
                        await GetInventoryItemByPostAsync(context);
                    }
                    else
                    {
                        BadRequest(context, "Endpoint này chỉ hỗ trợ phương thức POST");
                    }
                    return;
                }
                
                // Đã bỏ endpoint GET /api/inventory/item/{slotNumber}
                
                // Nếu không phải các trường hợp trên, trả về lỗi
                NotFound(context, "Endpoint không tồn tại");
                Monitor.Log($"Endpoint không tồn tại: {string.Join("/", segments)}", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Lỗi khi xử lý yêu cầu: {ex.Message}", LogLevel.Error);
                InternalServerError(context, $"Lỗi máy chủ: {ex.Message}");
            }
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

        // Đã xóa phương thức GetInventoryItem

        /// <summary>
        /// Xử lý yêu cầu lấy thông tin một vật phẩm cụ thể thông qua POST với body JSON
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        private async Task GetInventoryItemByPostAsync(HttpListenerContext context)
        {
            try
            {
                // Đọc body của request
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                
                Monitor.Log($"Nhận body: {requestBody}", LogLevel.Debug);
                
                // Kiểm tra body có dữ liệu không
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    BadRequest(context, "Body không được để trống");
                    return;
                }
                
                // Phân tích JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                try
                {
                    var request = JsonSerializer.Deserialize<InventoryItemRequest>(requestBody, options);
                    
                    if (request == null)
                    {
                        BadRequest(context, "Không thể phân tích dữ liệu JSON");
                        return;
                    }
                    
                    if (request.SlotNumber < 1 || request.SlotNumber > Game1.player.MaxItems)
                    {
                        BadRequest(context, $"Vị trí không hợp lệ. Phải từ 1 đến {Game1.player.MaxItems}");
                        return;
                    }
                    
                    var item = _inventoryService.GetInventoryItem(request.SlotNumber);
                    
                    if (item == null)
                    {
                        NotFound(context, $"Không tìm thấy vật phẩm ở vị trí {request.SlotNumber}");
                        return;
                    }
                    
                    Ok(context, item);
                }
                catch (JsonException ex)
                {
                    Monitor.Log($"Lỗi phân tích JSON: {ex.Message}", LogLevel.Error);
                    BadRequest(context, $"Lỗi phân tích JSON: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Lỗi khi xử lý yêu cầu POST: {ex.Message}", LogLevel.Error);
                InternalServerError(context, $"Lỗi máy chủ: {ex.Message}");
            }
        }
    }
}
