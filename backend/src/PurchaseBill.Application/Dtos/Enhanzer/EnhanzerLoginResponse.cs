using System.Text.Json.Serialization;

namespace PurchaseBill.Application.Dtos.Enhanzer;

/// <summary>
/// Envelope returned by the Enhanzer POS_Api/Invoke endpoint. Confirmed against the staging
/// API for three cases:
///
///   1. Valid credentials  -> Status_Code=200, Response_Body[0] has User_Locations etc.
///   2. Wrong password     -> Status_Code=200, Response_Body[0] has only Email + Doc_Msg.
///   3. Unknown company/user -> Status_Code=401, Response_Body=null,
///                               Message="Un-Authorize POS API Request.".
///
/// Response_Body's shape differs between success and failure, so it is modelled as one class
/// with the fields that are relevant to us all nullable; a login is only treated as successful
/// when User_Locations is present and Doc_Msg is absent.
/// </summary>
public class EnhanzerApiEnvelope
{
    [JsonPropertyName("Status_Code")]
    public int StatusCode { get; init; }

    [JsonPropertyName("Message")]
    public string? Message { get; init; }

    [JsonPropertyName("Response_Body")]
    public List<EnhanzerLoginResult>? ResponseBody { get; init; }
}

public class EnhanzerLoginResult
{
    [JsonPropertyName("Email")]
    public string? Email { get; init; }

    [JsonPropertyName("Doc_Msg")]
    public string? DocMsg { get; init; }

    [JsonPropertyName("User_Code")]
    public string? UserCode { get; init; }

    [JsonPropertyName("User_Display_Name")]
    public string? UserDisplayName { get; init; }

    [JsonPropertyName("Company_Code")]
    public string? CompanyCode { get; init; }

    [JsonPropertyName("User_Locations")]
    public List<EnhanzerUserLocation>? UserLocations { get; init; }
}

public class EnhanzerUserLocation
{
    [JsonPropertyName("Location_Code")]
    public required string LocationCode { get; init; }

    [JsonPropertyName("Location_Name")]
    public required string LocationName { get; init; }
}
