namespace App.Domain.Models
{
    class Command
    {
        public string Action { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
