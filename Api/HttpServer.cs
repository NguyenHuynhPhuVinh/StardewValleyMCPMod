using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI;
using TestMod_SV.Models;
using TestMod_SV.Services;

namespace TestMod_SV.Api
{
    /// <summary>
    /// Máy chủ HTTP đơn giản sử dụng HttpListener
    /// </summary>
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly IMonitor _monitor;
        private readonly IInventoryService _inventoryService;
        private readonly string _url;
        private readonly int _port;
        private bool _isRunning;
        private Thread? _serverThread;

        /// <summary>
        /// Khởi tạo máy chủ HTTP
        /// </summary>
        /// <param name="port">Cổng để lắng nghe</param>
        /// <param name="monitor">Monitor để ghi log</param>
        /// <param name="inventoryService">Dịch vụ quản lý túi đồ</param>
        public HttpServer(int port, IMonitor monitor, IInventoryService inventoryService)
        {
            _port = port;
            _url = $"http://localhost:{port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _monitor = monitor;
            _inventoryService = inventoryService;
            _isRunning = false;
        }

        /// <summary>
        /// Khởi động máy chủ HTTP
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            try
            {
                _listener.Start();
                _isRunning = true;
                _monitor.Log($"Máy chủ HTTP đã được khởi động tại {_url}", LogLevel.Info);
                _monitor.Log($"Bạn có thể truy cập API túi đồ tại {_url}api/inventory", LogLevel.Info);

                // Khởi động một thread mới để xử lý các yêu cầu
                _serverThread = new Thread(HandleRequests);
                _serverThread.Start();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi khởi động máy chủ HTTP: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Dừng máy chủ HTTP
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            try
            {
                _isRunning = false;
                _listener.Stop();
                _monitor.Log("Máy chủ HTTP đã được dừng", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi dừng máy chủ HTTP: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Xử lý các yêu cầu HTTP
        /// </summary>
        private void HandleRequests()
        {
            while (_isRunning)
            {
                try
                {
                    // Đợi yêu cầu
                    HttpListenerContext context = _listener.GetContext();
                    
                    // Xử lý yêu cầu trong một task riêng biệt
                    Task.Run(() => ProcessRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Bỏ qua ngoại lệ khi dừng listener
                    break;
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Lỗi khi xử lý yêu cầu: {ex.Message}", LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Xử lý một yêu cầu HTTP cụ thể
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string url = context.Request.Url?.AbsolutePath ?? "";
                _monitor.Log($"Nhận yêu cầu: {url}", LogLevel.Debug);

                // Xử lý các endpoint khác nhau
                if (url.Equals("/api/inventory", StringComparison.OrdinalIgnoreCase))
                {
                    HandleGetInventory(context);
                }
                else if (url.StartsWith("/api/inventory/item/", StringComparison.OrdinalIgnoreCase))
                {
                    HandleGetInventoryItem(context);
                }
                else
                {
                    // Endpoint không tồn tại
                    SendResponse(context, 404, "Endpoint không tồn tại");
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi xử lý yêu cầu: {ex.Message}", LogLevel.Error);
                SendResponse(context, 500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý yêu cầu lấy thông tin túi đồ
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        private void HandleGetInventory(HttpListenerContext context)
        {
            if (!StardewValley.Game1.hasLoadedGame)
            {
                SendResponse(context, 400, "Người chơi chưa vào thế giới game");
                return;
            }

            var inventory = _inventoryService.GetPlayerInventory();
            SendJsonResponse(context, 200, inventory);
        }

        /// <summary>
        /// Xử lý yêu cầu lấy thông tin một vật phẩm cụ thể
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        private void HandleGetInventoryItem(HttpListenerContext context)
        {
            if (!StardewValley.Game1.hasLoadedGame)
            {
                SendResponse(context, 400, "Người chơi chưa vào thế giới game");
                return;
            }

            string url = context.Request.Url?.AbsolutePath ?? "";
            string[] segments = url.Split('/');
            
            if (segments.Length < 5 || !int.TryParse(segments[4], out int slotNumber))
            {
                SendResponse(context, 400, "Vị trí vật phẩm không hợp lệ");
                return;
            }

            if (slotNumber < 1 || slotNumber > StardewValley.Game1.player.MaxItems)
            {
                SendResponse(context, 400, $"Vị trí không hợp lệ. Phải từ 1 đến {StardewValley.Game1.player.MaxItems}");
                return;
            }

            var item = _inventoryService.GetInventoryItem(slotNumber);
            
            if (item == null)
            {
                SendResponse(context, 404, $"Không tìm thấy vật phẩm ở vị trí {slotNumber}");
                return;
            }

            SendJsonResponse(context, 200, item);
        }

        /// <summary>
        /// Gửi phản hồi HTTP dạng văn bản
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="statusCode">Mã trạng thái HTTP</param>
        /// <param name="message">Thông điệp phản hồi</param>
        private void SendResponse(HttpListenerContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            context.Response.ContentLength64 = buffer.Length;
            
            using (Stream output = context.Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
            
            context.Response.Close();
        }

        /// <summary>
        /// Gửi phản hồi HTTP dạng JSON
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="statusCode">Mã trạng thái HTTP</param>
        /// <param name="data">Dữ liệu để chuyển đổi thành JSON</param>
        private void SendJsonResponse(HttpListenerContext context, int statusCode, object data)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            context.Response.ContentLength64 = buffer.Length;
            
            using (Stream output = context.Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
            
            context.Response.Close();
        }
    }
}
