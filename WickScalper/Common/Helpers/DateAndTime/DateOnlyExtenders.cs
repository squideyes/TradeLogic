namespace WickScalper.Common
{
    public static class DateOnlyExtenders
    {
        public static string Format(this DateOnly value) =>
            value.ToString("MM/dd/yyyy");
    }
}
