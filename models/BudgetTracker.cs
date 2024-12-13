namespace budget.models
{
    public class BudgetTracker
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal RemainingBudget { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
