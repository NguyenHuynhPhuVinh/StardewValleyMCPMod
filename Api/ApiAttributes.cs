using System;

namespace StardewValleyMCP.Api
{
    /// <summary>
    /// Thuộc tính đánh dấu một phương thức là một endpoint API
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ApiEndpointAttribute : Attribute
    {
        /// <summary>
        /// Đường dẫn của endpoint
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Phương thức HTTP (GET, POST, PUT, DELETE)
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Khởi tạo thuộc tính ApiEndpoint
        /// </summary>
        /// <param name="path">Đường dẫn của endpoint</param>
        /// <param name="method">Phương thức HTTP</param>
        public ApiEndpointAttribute(string path, string method)
        {
            Path = path;
            Method = method.ToUpper();
        }
    }

    /// <summary>
    /// Thuộc tính đánh dấu một tham số là từ body của request
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class FromBodyAttribute : Attribute
    {
    }

    /// <summary>
    /// Thuộc tính đánh dấu một tham số là từ URL
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class FromUrlAttribute : Attribute
    {
        /// <summary>
        /// Tên tham số trong URL
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Khởi tạo thuộc tính FromUrl
        /// </summary>
        /// <param name="name">Tên tham số trong URL</param>
        public FromUrlAttribute(string name)
        {
            Name = name;
        }
    }
}
