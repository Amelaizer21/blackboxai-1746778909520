using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RosewoodSecurity.Models
{
    [Serializable]
    public class ApiException : Exception
    {
        public string Code { get; }
        public List<ValidationError> ValidationErrors { get; }
        public string RequestId { get; }
        public int StatusCode { get; }

        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, string code, int statusCode = 500) : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }

        public ApiException(string message, string code, List<ValidationError> validationErrors, int statusCode = 400)
            : base(message)
        {
            Code = code;
            ValidationErrors = validationErrors;
            StatusCode = statusCode;
        }

        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Code = info.GetString(nameof(Code));
            ValidationErrors = (List<ValidationError>)info.GetValue(nameof(ValidationErrors), typeof(List<ValidationError>));
            RequestId = info.GetString(nameof(RequestId));
            StatusCode = info.GetInt32(nameof(StatusCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Code), Code);
            info.AddValue(nameof(ValidationErrors), ValidationErrors);
            info.AddValue(nameof(RequestId), RequestId);
            info.AddValue(nameof(StatusCode), StatusCode);
        }
    }

    [Serializable]
    public class AuthenticationException : Exception
    {
        public string Code { get; }

        public AuthenticationException(string message) : base(message)
        {
        }

        public AuthenticationException(string message, string code) : base(message)
        {
            Code = code;
        }

        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Code = info.GetString(nameof(Code));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Code), Code);
        }
    }

    [Serializable]
    public class AuthorizationException : Exception
    {
        public string RequiredPermission { get; }

        public AuthorizationException(string message) : base(message)
        {
        }

        public AuthorizationException(string message, string requiredPermission) : base(message)
        {
            RequiredPermission = requiredPermission;
        }

        public AuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            RequiredPermission = info.GetString(nameof(RequiredPermission));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(RequiredPermission), RequiredPermission);
        }
    }

    [Serializable]
    public class ValidationException : Exception
    {
        public List<ValidationError> ValidationErrors { get; }

        public ValidationException(string message) : base(message)
        {
            ValidationErrors = new List<ValidationError>();
        }

        public ValidationException(string message, List<ValidationError> validationErrors) : base(message)
        {
            ValidationErrors = validationErrors;
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            ValidationErrors = new List<ValidationError>();
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ValidationErrors = (List<ValidationError>)info.GetValue(nameof(ValidationErrors), typeof(List<ValidationError>));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ValidationErrors), ValidationErrors);
        }
    }

    [Serializable]
    public class ConfigurationException : Exception
    {
        public string ConfigurationKey { get; }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, string configurationKey) : base(message)
        {
            ConfigurationKey = configurationKey;
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ConfigurationKey = info.GetString(nameof(ConfigurationKey));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ConfigurationKey), ConfigurationKey);
        }
    }

    [Serializable]
    public class ConcurrencyException : Exception
    {
        public string EntityType { get; }
        public string EntityId { get; }

        public ConcurrencyException(string message) : base(message)
        {
        }

        public ConcurrencyException(string message, string entityType, string entityId) : base(message)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            EntityType = info.GetString(nameof(EntityType));
            EntityId = info.GetString(nameof(EntityId));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(EntityType), EntityType);
            info.AddValue(nameof(EntityId), EntityId);
        }
    }

    [Serializable]
    public class OfflineModeException : Exception
    {
        public OfflineModeException(string message) : base(message)
        {
        }

        public OfflineModeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OfflineModeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
