using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace StardewValleyMCP.Api
{
    /// <summary>
    /// Bộ định tuyến API
    /// </summary>
    public class ApiRouter
    {
        private readonly Dictionary<string, ApiController> _controllers;
        private readonly IMonitor _monitor;

        /// <summary>
        /// Khởi tạo bộ định tuyến API
        /// </summary>
        /// <param name="monitor">Monitor để ghi log</param>
        public ApiRouter(IMonitor monitor)
        {
            _controllers = new Dictionary<string, ApiController>(StringComparer.OrdinalIgnoreCase);
            _monitor = monitor;
        }

        /// <summary>
        /// Đăng ký một controller cho một đường dẫn cụ thể
        /// </summary>
        /// <param name="path">Đường dẫn cơ sở (ví dụ: "inventory")</param>
        /// <param name="controller">Controller để xử lý yêu cầu</param>
        public void RegisterController(string path, ApiController controller)
        {
            _controllers[path] = controller;
            _monitor.Log($"Đã đăng ký controller cho đường dẫn: /api/{path}", LogLevel.Debug);
        }

        /// <summary>
        /// Xử lý một yêu cầu HTTP
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <returns>Task</returns>
        public async Task RouteRequestAsync(HttpListenerContext context)
        {
            try
            {
                string url = context.Request.Url?.AbsolutePath ?? "";
                _monitor.Log($"Nhận yêu cầu: {url}", LogLevel.Debug);

                // Phân tích URL thành các phân đoạn
                string[] segments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                // Ghi log các phân đoạn để debug
                _monitor.Log($"Phân đoạn URL: {string.Join(", ", segments)}", LogLevel.Debug);

                // Kiểm tra xem URL có bắt đầu bằng /api không
                if (segments.Length < 1)
                {
                    SendResponse(context, 404, "Endpoint không tồn tại");
                    return;
                }
                
                // Kiểm tra phần đầu của URL
                string firstSegment = segments[0].ToLower();
                if (firstSegment != "api" && segments.Length >= 2 && segments[1].ToLower() != "api")
                {
                    SendResponse(context, 404, "Endpoint không tồn tại");
                    return;
                }

                // Xác định vị trí của tên controller trong URL
                int controllerIndex = firstSegment == "api" ? 1 : 2;
                
                // Kiểm tra xem có đủ phân đoạn để lấy tên controller không
                if (segments.Length <= controllerIndex)
                {
                    // Nếu không có tên controller, trả về danh sách các API có sẵn
                    string availableApis = string.Join(", ", _controllers.Keys);
                    SendResponse(context, 200, $"API có sẵn: {availableApis}");
                    return;
                }
                
                // Lấy tên controller từ URL
                string controllerName = segments[controllerIndex];
                _monitor.Log($"Tên controller: {controllerName}", LogLevel.Debug);

                // Tìm controller phù hợp
                if (_controllers.TryGetValue(controllerName, out ApiController controller))
                {
                    // Chuyển yêu cầu đến controller
                    await controller.ProcessRequestAsync(context, segments);
                }
                else
                {
                    SendResponse(context, 404, $"Không tìm thấy API cho đường dẫn: {controllerName}");
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi xử lý yêu cầu: {ex.Message}", LogLevel.Error);
                SendResponse(context, 500, $"Lỗi máy chủ: {ex.Message}");
            }
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
            
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            context.Response.ContentLength64 = buffer.Length;
            
            using (var output = context.Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
            
            context.Response.Close();
        }
    }
}
