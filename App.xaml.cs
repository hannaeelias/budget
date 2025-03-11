using budget.models;

namespace budget
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                while (true)
                {
                    await CheckForUpdatesAsync();
                    await Task.Delay(TimeSpan.FromDays(1)); 
                }
            });
        }


        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new LoginPage()));
        }

        private async Task CheckForUpdatesAsync()
        {
            var dbContext = new AppDbContext();
            var user = await dbContext.GetUser();
            if (user == null) return;

            DateTime lastChecked = Preferences.Get("LastCheckedDate", DateTime.MinValue);
            DateTime now = DateTime.Now;

            
            if (lastChecked.Month != now.Month)
            {
                user.Salary += user.Salary; 
                await dbContext.UpdateUser(user);
            }

            var items = await dbContext.GetItemsForUser();
            foreach (var item in items.Where(i => i.IsRecurring && i.NextDueDate <= now))
            {
                item.NextDueDate = GetNextDueDate(item.RecurrenceInterval, item.NextDueDate);
                await dbContext.Update(item);
            }

            Preferences.Set("LastCheckedDate", now);
        }

        private DateTime GetNextDueDate(string recurrenceInterval, DateTime? currentDueDate)
        {
            if (currentDueDate == null) return DateTime.Now;
            return recurrenceInterval.ToLower() switch
            {
                "monthly" => currentDueDate.Value.AddMonths(1),
                "quarter" => currentDueDate.Value.AddMonths(3),
                "semester" => currentDueDate.Value.AddMonths(6),
                "weekly" => currentDueDate.Value.AddDays(7),
                "yearly" => currentDueDate.Value.AddYears(1),
                _ => currentDueDate.Value
            };
        }
    }
}