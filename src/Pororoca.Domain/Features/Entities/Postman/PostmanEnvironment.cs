#nullable disable warnings

using System;
using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Postman
{
    public class PostmanEnvironment
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public PostmanEnvironmentVariable[] Values { get; set; }

        [JsonPropertyName("_postman_variable_scope")]
        public string Scope { get; set; }

        [JsonPropertyName("_postman_exported_at")]
        public DateTimeOffset ExportedAt { get; set; }

        [JsonPropertyName("_postman_exported_using")]
        public string ExportedUsing { get; set; }
    }
}

#nullable enable warnings