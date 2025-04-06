using SQLite;

namespace budget.models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double EstimatedCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsSelected { get; set; }
        public bool IsRecurring { get; set; } 
        public string RecurrenceInterval { get; set; } = "None"; 
        public DateTime? NextDueDate { get; set; }
        public bool IsProcessed { get; set; }
        public User? User { get; set; }

    }
}
