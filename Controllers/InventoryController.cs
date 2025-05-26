using Microsoft.AspNetCore.Mvc;
using StardewValley;
using TestMod_SV.Models;
using TestMod_SV.Services;

namespace TestMod_SV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Lấy thông tin túi đồ của người chơi
        /// </summary>
        /// <returns>Thông tin túi đồ</returns>
        [HttpGet]
        public ActionResult<InventoryModel> GetInventory()
        {
            if (!Game1.hasLoadedGame)
            {
                return BadRequest("Người chơi chưa vào thế giới game");
            }

            var inventory = _inventoryService.GetPlayerInventory();
            return Ok(inventory);
        }

        /// <summary>
        /// Lấy thông tin chi tiết về một vật phẩm cụ thể trong túi đồ
        /// </summary>
        /// <param name="slotNumber">Vị trí của vật phẩm (bắt đầu từ 1)</param>
        /// <returns>Thông tin chi tiết về vật phẩm</returns>
        [HttpGet("item/{slotNumber}")]
        public ActionResult<InventoryItemModel> GetInventoryItem(int slotNumber)
        {
            if (!Game1.hasLoadedGame)
            {
                return BadRequest("Người chơi chưa vào thế giới game");
            }

            if (slotNumber < 1 || slotNumber > Game1.player.MaxItems)
            {
                return BadRequest($"Vị trí không hợp lệ. Phải từ 1 đến {Game1.player.MaxItems}");
            }

            var item = _inventoryService.GetInventoryItem(slotNumber);
            
            if (item == null)
            {
                return NotFound($"Không tìm thấy vật phẩm ở vị trí {slotNumber}");
            }

            return Ok(item);
        }
    }
}
