namespace budget.models
{
    public class Location
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Notes { get; set; }
    }
}
