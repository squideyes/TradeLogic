namespace WickScalper.Common
{
    /// <summary>
    /// Example validator for a configuration object using the FluentValidator framework.
    /// This demonstrates how to use the FluentValidator to validate complex objects.
    ///
    /// Usage example:
    /// <code>
    /// var config = new SampleConfig { /* ... */ };
    /// var validator = new SampleConfigValidator();
    /// var result = validator.Validate(config);
    ///
    /// if (!result.IsValid)
    /// {
    ///     foreach (var error in result.Errors)
    ///         Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    /// }
    /// </code>
    /// </summary>
    public class SampleConfigValidator : FluentValidator<SampleConfig>
    {
        /// <summary>
        /// Initializes a new instance of the SampleConfigValidator class.
        /// Defines validation rules for all SampleConfig properties.
        /// </summary>
        public SampleConfigValidator()
        {
            RuleFor(x => x.TickSize)
                .GreaterThan(0m)
                .WithMessage("TickSize must be greater than 0");

            RuleFor(x => x.PointValue)
                .GreaterThan(0m)
                .WithMessage("PointValue must be greater than 0");

            RuleFor(x => x.IdPrefix)
                .NotEmpty()
                .WithMessage("IdPrefix cannot be null or empty");

            RuleFor(x => x.SlippageToleranceTicks)
                .GreaterThanOrEqual(0)
                .WithMessage("SlippageToleranceTicks cannot be negative");
        }
    }

    /// <summary>
    /// Sample configuration class for demonstration purposes.
    /// </summary>
    public class SampleConfig
    {
        public decimal TickSize { get; set; }
        public decimal PointValue { get; set; }
        public string IdPrefix { get; set; }
        public int SlippageToleranceTicks { get; set; }
    }
}

