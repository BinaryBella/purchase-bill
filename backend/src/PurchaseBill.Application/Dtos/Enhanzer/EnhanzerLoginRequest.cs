using System.Text.Json.Serialization;

namespace PurchaseBill.Application.Dtos.Enhanzer;

/// <summary>
/// Envelope for the Enhanzer POS_Api/Invoke endpoint, matching the sample request in the
/// assignment brief:
/// {
///   "API_Action": "GetLoginData", "Device_Id": "D001", "Sync_Time": "",
///   "Company_Code": "{email}", "API_Body": { "Username": "{email}", "Pw": "{password}" }
/// }
/// </summary>
public class EnhanzerLoginRequest
{
    [JsonPropertyName("API_Action")]
    public string ApiAction { get; init; } = "GetLoginData";

    [JsonPropertyName("Device_Id")]
    public string DeviceId { get; init; } = "D001";

    [JsonPropertyName("Sync_Time")]
    public string SyncTime { get; init; } = string.Empty;

    [JsonPropertyName("Company_Code")]
    public required string CompanyCode { get; init; }

    [JsonPropertyName("API_Body")]
    public required EnhanzerLoginBody ApiBody { get; init; }
}

public class EnhanzerLoginBody
{
    [JsonPropertyName("Username")]
    public required string Username { get; init; }

    [JsonPropertyName("Pw")]
    public required string Pw { get; init; }
}
