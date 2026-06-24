using System;
using System.Text.Json.Serialization;

namespace YourCompany.Dapper.PostgreSQL
{
    internal partial struct NpgsqlDynamicJsonAccountChanges
    {
        [JsonInclude, JsonPropertyName("resolving_from_id")]
        internal long? ResolvingFromId { get; set; }

        [JsonInclude, JsonPropertyName("balance_set")]
        internal decimal? BalanceSet { get; set; }

        [JsonInclude, JsonPropertyName("balance_increment")]
        internal decimal? BalanceIncrement { get; set; }

        [JsonInclude, JsonPropertyName("resolving_from_or_inserting_public_id")]
        internal Guid? ResolvingFromOrInsertingPublicId { get; set; }

        [JsonSerializable(typeof(NpgsqlDynamicJsonAccountChanges))]
        internal partial class GeneratedForInternalParentRequiringPartialAndJsonIncludeOnInternalMembers : JsonSerializerContext { }
    }
}