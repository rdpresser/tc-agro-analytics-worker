namespace TC.Agro.Analytics.Domain.ValueObjects
{
    public sealed record AlertSeverity
    {
        public string Value { get; }
        public int Level { get; }

        private AlertSeverity(string value, int level)
        {
            Value = value;
            Level = level;
        }

        public static AlertSeverity Low => new("Low", 1);
        public static AlertSeverity Medium => new("Medium", 2);
        public static AlertSeverity High => new("High", 3);
        public static AlertSeverity Critical => new("Critical", 4);

        public static Result<AlertSeverity> Create(string value)
        {
            var normalized = value?.Trim();
            return normalized switch
            {
                "Low" => Result.Success(Low),
                "Medium" => Result.Success(Medium),
                "High" => Result.Success(High),
                "Critical" => Result.Success(Critical),
                _ => Result.Invalid(new ValidationError
                {
                    Identifier = "AlertSeverity.Invalid",
                    ErrorMessage = $"Invalid alert severity: '{value}'. Must be Low, Medium, High, or Critical."
                })
            };
        }

        public static IEnumerable<AlertSeverity> GetAll()
        {
            yield return Low;
            yield return Medium;
            yield return High;
            yield return Critical;
        }

        public bool IsLow => this == Low;
        public bool IsMedium => this == Medium;
        public bool IsHigh => this == High;
        public bool IsCritical => this == Critical;

        public static bool operator >(AlertSeverity left, AlertSeverity right)
            => left.Level > right.Level;

        public static bool operator <(AlertSeverity left, AlertSeverity right)
            => left.Level < right.Level;

        public static bool operator >=(AlertSeverity left, AlertSeverity right)
            => left.Level >= right.Level;

        public static bool operator <=(AlertSeverity left, AlertSeverity right)
            => left.Level <= right.Level;

        public static implicit operator string(AlertSeverity severity) => severity.Value;

        public override string ToString() => Value;
    }
}
