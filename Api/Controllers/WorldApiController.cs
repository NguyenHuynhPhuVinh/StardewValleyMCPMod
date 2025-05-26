using System.Net;
using StardewModdingAPI;
using StardewValley;
using StardewValleyMCP.Models;
using StardewValleyMCP.Services;

namespace StardewValleyMCP.Api.Controllers
{
    /// <summary>
    /// Controller API cho thế giới game
    /// </summary>
    public class WorldApiController : ApiController
    {
        private readonly IWorldService _worldService;

        /// <summary>
        /// Khởi tạo controller API thế giới game
        /// </summary>
        /// <param name="monitor">Monitor để ghi log</param>
        /// <param name="worldService">Dịch vụ quản lý thế giới game</param>
        public WorldApiController(IMonitor monitor, IWorldService worldService) 
            : base(monitor)
        {
            _worldService = worldService;
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
        /// Quét các vật thể xung quanh người chơi
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="request">Yêu cầu từ body JSON</param>
        [ApiEndpoint("world/scan", "POST")]
        private void ScanObjects(HttpListenerContext context, [FromBody] ScanObjectsRequest request)
        {
            if (!CheckPlayerInGame(context)) return;
            
            // Nếu request là null, sử dụng giá trị mặc định
            int radius = request?.Radius ?? 5;
            
            // Giới hạn bán kính để tránh quá tải
            if (radius < 1) radius = 1;
            if (radius > 20) radius = 20;
            
            // Gọi dịch vụ để quét vật thể
            var result = _worldService.ScanObjects(radius);
            
            // Trả về kết quả
            Ok(context, result);
        }
    }
}
