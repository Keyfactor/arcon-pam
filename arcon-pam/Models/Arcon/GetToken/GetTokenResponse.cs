namespace arcon_pam.Models.Arcon.GetToken;

public sealed class GetTokenResponse : ArconResponse<GetTokenResponseData>
{
}

public sealed class GetTokenResponseData
{
    public string? AccessToken { get; set; }
    public string ExpiresIn { get; set; }
    public string? TokenType { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}
