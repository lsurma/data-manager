namespace DataManager.Application.Contracts.Common;

/// <summary>
/// Represents a file downloaded from the API with its metadata
/// </summary>
public class DownloadedFile
{
    /// <summary>
    /// The file content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The MIME type of the file (e.g., "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// The suggested filename for the downloaded file
    /// </summary>
    public string? FileName { get; set; }
}
