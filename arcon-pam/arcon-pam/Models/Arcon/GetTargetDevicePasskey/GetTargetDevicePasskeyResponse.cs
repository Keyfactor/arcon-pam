namespace arcon_pam.Models.Arcon.GetTargetDevicePasskey;

public sealed class GetTargetDevicePasskeyResponse : ArconResponse<IReadOnlyCollection<GetTargetDevicePasskeyResponseItem>>
{
}

public sealed class GetTargetDevicePasskeyResponseItem
{
    public string ServerIp { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string AliasName { get; set; }
    public string Message { get; set; }
}
