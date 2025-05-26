using System;
using System.Net;
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
        /// Kiểm tra xem người chơi đã vào thế giới game chưa
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <returns>True nếu người chơi đã vào thế giới game</returns>
        private bool CheckPlayerInGame(HttpListenerContext context)
        {
            if (!Game1.hasLoadedGame)
            {
                BadRequest(context, "Người chơi chưa vào thế giới game");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Xử lý yêu cầu lấy thông tin túi đồ
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        [ApiEndpoint("inventory", "GET")]
        private void GetInventory(HttpListenerContext context)
        {
            if (!CheckPlayerInGame(context)) return;
            
            var inventory = _inventoryService.GetPlayerInventory();
            Ok(context, inventory);
        }

        /// <summary>
        /// Xử lý yêu cầu lấy thông tin một vật phẩm cụ thể thông qua POST với body JSON
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="request">Yêu cầu từ body JSON</param>
        [ApiEndpoint("inventory/item", "POST")]
        private void GetInventoryItemByPost(HttpListenerContext context, [FromBody] InventoryItemRequest request)
        {
            if (!CheckPlayerInGame(context)) return;
            
            if (request == null)
            {
                BadRequest(context, "Không thể phân tích dữ liệu JSON hoặc body trống");
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
    }
}
