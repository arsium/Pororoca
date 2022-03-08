﻿#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman
{
    public enum PostmanRequestBodyFormDataParamType
    {
        Text,
        File
    }

    public class PostmanRequestBodyFormDataParam
    {
        public string Key { get; set; }

        public string? Value { get; set; }

        public string? ContentType { get; set; }

        public PostmanRequestBodyFormDataParamType Type { get; set; }

        /// <summary>
        /// string list of file paths.
        /// </summary>
        public object? Src { get; set; }

        public bool? Disabled { get; set; }

    }
}

#nullable enable warnings