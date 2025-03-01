namespace App.Infrastructure.Models.Trello
{
    public class TrelloApiRequest
    {
        public string Url { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
