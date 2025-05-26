using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
        private readonly Dictionary<string, EndpointInfo> _endpoints = new Dictionary<string, EndpointInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Khởi tạo controller API
        /// </summary>
        /// <param name="monitor">Monitor để ghi log</param>
        protected ApiController(IMonitor monitor)
        {
            Monitor = monitor;
            RegisterEndpoints();
        }

        /// <summary>
        /// Đăng ký các endpoint từ các phương thức có thuộc tính ApiEndpoint
        /// </summary>
        private void RegisterEndpoints()
        {
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<ApiEndpointAttribute>() != null);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<ApiEndpointAttribute>();
                if (attribute != null)
                {
                    var key = $"{attribute.Method}:{attribute.Path}";
                    _endpoints[key] = new EndpointInfo
                    {
                        Method = method,
                        HttpMethod = attribute.Method,
                        Path = attribute.Path
                    };
                    
                    Monitor.Log($"Đã đăng ký endpoint: {attribute.Method} {attribute.Path}", LogLevel.Debug);
                }
            }
        }

        /// <summary>
        /// Xử lý yêu cầu HTTP
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="segments">Các phân đoạn của URL</param>
        /// <returns>Task</returns>
        public async Task ProcessRequestAsync(HttpListenerContext context, string[] segments)
        {
            try
            {
                string path = string.Join("/", segments);
                string httpMethod = context.Request.HttpMethod.ToUpper();
                
                Monitor.Log($"Tìm endpoint cho: {httpMethod} {path}", LogLevel.Debug);
                
                // Tìm endpoint phù hợp
                foreach (var endpoint in _endpoints)
                {
                    var info = endpoint.Value;
                    if (httpMethod == info.HttpMethod && IsPathMatch(path, info.Path))
                    {
                        Monitor.Log($"Đã tìm thấy endpoint: {info.HttpMethod} {info.Path}", LogLevel.Debug);
                        
                        // Chuẩn bị tham số cho phương thức
                        var parameters = await PrepareParametersAsync(context, info.Method, path, info.Path);
                        
                        // Gọi phương thức
                        var result = info.Method.Invoke(this, parameters);
                        
                        // Xử lý kết quả nếu là Task
                        if (result is Task task)
                        {
                            await task;
                        }
                        
                        return;
                    }
                }
                
                // Không tìm thấy endpoint phù hợp
                NotFound(context, $"Không tìm thấy endpoint: {httpMethod} {path}");
            }
            catch (Exception ex)
            {
                Monitor.Log($"Lỗi khi xử lý yêu cầu: {ex.Message}", LogLevel.Error);
                InternalServerError(context, $"Lỗi máy chủ: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Kiểm tra xem đường dẫn có khớp với mẫu không
        /// </summary>
        /// <param name="path">Đường dẫn thực tế</param>
        /// <param name="pattern">Mẫu đường dẫn</param>
        /// <returns>True nếu khớp</returns>
        private bool IsPathMatch(string path, string pattern)
        {
            // Cách đơn giản: kiểm tra xem path có kết thúc bằng pattern không
            // Có thể cải tiến để hỗ trợ các tham số đường dẫn phức tạp hơn
            return path.EndsWith(pattern, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Chuẩn bị các tham số cho phương thức endpoint
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="method">Phương thức cần gọi</param>
        /// <param name="path">Đường dẫn thực tế</param>
        /// <param name="pattern">Mẫu đường dẫn</param>
        /// <returns>Mảng các tham số</returns>
        private async Task<object[]> PrepareParametersAsync(HttpListenerContext context, MethodInfo method, string path, string pattern)
        {
            var parameters = method.GetParameters();
            var result = new object[parameters.Length];
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                
                // Nếu tham số là HttpListenerContext
                if (param.ParameterType == typeof(HttpListenerContext))
                {
                    result[i] = context;
                    continue;
                }
                
                // Nếu tham số có thuộc tính FromBody
                var fromBodyAttr = param.GetCustomAttribute<FromBodyAttribute>();
                if (fromBodyAttr != null)
                {
                    string requestBody;
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                    
                    if (string.IsNullOrWhiteSpace(requestBody))
                    {
                        result[i] = null;
                    }
                    else
                    {
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };
                            
                            result[i] = JsonSerializer.Deserialize(requestBody, param.ParameterType, options);
                        }
                        catch
                        {
                            result[i] = null;
                        }
                    }
                    
                    continue;
                }
                
                // Nếu tham số có thuộc tính FromUrl
                var fromUrlAttr = param.GetCustomAttribute<FromUrlAttribute>();
                if (fromUrlAttr != null)
                {
                    // Đơn giản hóa: lấy phần cuối của URL làm giá trị tham số
                    // Có thể cải tiến để hỗ trợ các tham số đường dẫn phức tạp hơn
                    var segments = path.Split('/');
                    if (segments.Length > 0)
                    {
                        string value = segments[segments.Length - 1];
                        result[i] = Convert.ChangeType(value, param.ParameterType);
                    }
                    
                    continue;
                }
                
                // Nếu không có thuộc tính đặc biệt, đặt giá trị mặc định
                result[i] = param.HasDefaultValue ? param.DefaultValue : null;
            }
            
            return result;
        }
        
        /// <summary>
        /// Lớp chứa thông tin về một endpoint
        /// </summary>
        private class EndpointInfo
        {
            public MethodInfo Method { get; set; }
            public string HttpMethod { get; set; }
            public string Path { get; set; }
        }

        /// <summary>
        /// Gửi phản hồi HTTP dạng văn bản
        /// </summary>
        /// <param name="context">Context của yêu cầu HTTP</param>
        /// <param name="statusCode">Mã trạng thái HTTP</param>
        /// <param name="message">Thông điệp phản hồi</param>
        protected void SendTextResponse(HttpListenerContext context, int statusCode, string message)
        {
            try
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
            catch (Exception ex)
            {
                Monitor.Log($"Lỗi khi gửi phản hồi: {ex.Message}", LogLevel.Error);
            }
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
