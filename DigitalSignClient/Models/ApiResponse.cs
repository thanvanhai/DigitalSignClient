using System;

namespace DigitalSignClient.Models
{
    /// <summary>
    /// Generic wrapper cho API responses
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Response khi tạo mới resource (trả về Id)
    /// </summary>
    public class CreatedResponse
    {
        public Guid Id { get; set; }
    }
}