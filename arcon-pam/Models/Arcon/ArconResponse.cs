namespace arcon_pam.Models.Arcon;

public abstract class ArconResponse<T>
{
    public string Program { get; set; }
    public string Version { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }
    public T Result { get; set; }
}
