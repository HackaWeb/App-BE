namespace App.RestContracts.Models
{
    public class TagModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<UserModel> Users { get; set; }
    }
}
