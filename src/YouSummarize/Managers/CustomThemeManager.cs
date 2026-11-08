using MudBlazor;

namespace YouSummarize.Managers
{
    public static class CustomThemeManager
    {
        // Thème pour Travel Planner (YouSummarize)
        public static readonly MudTheme TravelPlannerTheme = new()
        {
            LayoutProperties = new()
            {
                DefaultBorderRadius = "8px",
            }
        };

        // Thème pour Travel Planner Admin
        public static readonly MudTheme TravelPlannerAdminTheme = new()
        {
            LayoutProperties = new()
            {
                DefaultBorderRadius = "8px",
            }
        };
    }
}