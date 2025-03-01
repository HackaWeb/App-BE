namespace App.Domain.Models;

public class OperationResult
{
    public bool Success { get; set; }

    public string ProjectId { get; set; }

    public string ErrorMessage { get; set; }
}