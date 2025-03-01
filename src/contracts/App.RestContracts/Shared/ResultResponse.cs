namespace App.RestContracts.Shared;

public class ResultResponse
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}