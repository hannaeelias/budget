namespace budget.models
{
    public class User
    {
        //not added yet will add in future 
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
