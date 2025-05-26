using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI;
using TestMod_SV.Api.Controllers;
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
        private readonly ApiRouter _router;
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
            // Thêm tiền tố với dấu + để xử lý tất cả các đường dẫn con
            _listener.Prefixes.Add(_url);
            _monitor = monitor;
            
            // Khởi tạo router và đăng ký các controller
            _router = new ApiRouter(monitor);
            _router.RegisterController("inventory", new InventoryApiController(monitor, inventoryService));
            
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
                    Task.Run(async () => await ProcessRequestAsync(context));
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
        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                // Chuyển yêu cầu đến router
                await _router.RouteRequestAsync(context);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi xử lý yêu cầu: {ex.Message}", LogLevel.Error);
                
                try
                {
                    // Gửi phản hồi lỗi
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes($"Lỗi máy chủ: {ex.Message}");
                    context.Response.ContentLength64 = buffer.Length;
                    
                    using (var output = context.Response.OutputStream)
                    {
                        output.Write(buffer, 0, buffer.Length);
                    }
                    
                    context.Response.Close();
                }
                catch
                {
                    // Bỏ qua lỗi khi gửi phản hồi lỗi
                }
            }
        }
    }
}
