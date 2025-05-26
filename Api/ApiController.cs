using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace StardewValleyMCP.Api
{
    /// <summary>
    /// Lớp cơ sở cho các controller API
    /// </summary>
    public abstract class ApiController
    {
        protected readonly IMonitor Monitor;

        /// <summary>
        /// Khởi tạo controller API
        /// </summary>
        /// <param name="monitor">Monitor để ghi log</param>
        protected ApiController(IMonitor monitor)
        {
            Monitor = monitor;
        }

        /// <summary>
        /// Xử lý yêu cầu HTTP
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="segments">Các phân đoạn của URL</param>
        /// <returns>Task</returns>
        public abstract Task ProcessRequestAsync(HttpListenerContext context, string[] segments);

        /// <summary>
        /// Gửi phản hồi HTTP dạng văn bản
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="statusCode">Mã trạng thái HTTP</param>
        /// <param name="message">Thông điệp phản hồi</param>
        protected void SendTextResponse(HttpListenerContext context, int statusCode, string message)
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

        /// <summary>
        /// Gửi phản hồi HTTP dạng JSON
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="statusCode">Mã trạng thái HTTP</param>
        /// <param name="data">Dữ liệu để chuyển đổi thành JSON</param>
        protected void SendJsonResponse(HttpListenerContext context, int statusCode, object data)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
            context.Response.ContentLength64 = buffer.Length;
            
            using (var output = context.Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
            
            context.Response.Close();
        }

        /// <summary>
        /// Gửi phản hồi HTTP dạng JSON với mã trạng thái 200 OK
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="data">Dữ liệu để chuyển đổi thành JSON</param>
        protected void Ok(HttpListenerContext context, object data)
        {
            SendJsonResponse(context, 200, data);
        }

        /// <summary>
        /// Gửi phản hồi HTTP dạng văn bản với mã trạng thái 400 Bad Request
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="message">Thông điệp lỗi</param>
        protected void BadRequest(HttpListenerContext context, string message)
        {
            SendTextResponse(context, 400, message);
        }

        /// <summary>
        /// Gửi phản hồi HTTP dạng văn bản với mã trạng thái 404 Not Found
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="message">Thông điệp lỗi</param>
        protected void NotFound(HttpListenerContext context, string message)
        {
            SendTextResponse(context, 404, message);
        }

        /// <summary>
        /// Gửi phản hồi HTTP dạng văn bản với mã trạng thái 500 Internal Server Error
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="message">Thông điệp lỗi</param>
        protected void InternalServerError(HttpListenerContext context, string message)
        {
            SendTextResponse(context, 500, message);
        }
    }
}
