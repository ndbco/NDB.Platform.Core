namespace NDB.Platform.Http;

/// <summary>Content type for HTTP request bodies.</summary>
public enum RequestContentType
{
    /// <summary>application/json.</summary>
    Json,

    /// <summary>application/x-www-form-urlencoded.</summary>
    FormUrlEncoded,

    /// <summary>multipart/form-data.</summary>
    Multipart
}
