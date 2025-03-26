using SQLite;

namespace budget.models
{
    [Table("User")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public DateTime birth { get; set; }
        public double Salary { get; set; }
        public double Balance { get; set; }
        public double SavingsBalance { get; set; }  

        public DateTime LastUpdated { get; set; }
   

        public User() { }
    }
}
