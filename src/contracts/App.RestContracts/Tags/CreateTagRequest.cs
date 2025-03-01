namespace App.RestContracts.Tags
{
    public class CreateTagRequest
    {
        public string Name { get; set; }
        public List<Guid>? UserIds { get; set; }
    }
}
