namespace TC.Agro.Analytics.Domain.ValueObjects
{
    public sealed record AlertStatus
    {
        public string Value { get; }

        private AlertStatus(string value) => Value = value;

        public static AlertStatus Pending => new("Pending");
        public static AlertStatus Acknowledged => new("Acknowledged");
        public static AlertStatus Resolved => new("Resolved");

        public static Result<AlertStatus> Create(string value)
        {
            var normalized = value?.Trim();
            return normalized switch
            {
                "Pending" => Result.Success(Pending),
                "Acknowledged" => Result.Success(Acknowledged),
                "Resolved" => Result.Success(Resolved),
                _ => Result.Invalid(new ValidationError
                {
                    Identifier = "AlertStatus.Invalid",
                    ErrorMessage = $"Invalid alert status: '{value}'. Must be Pending, Acknowledged, or Resolved."
                })
            };
        }

        public static IEnumerable<AlertStatus> GetAll()
        {
            yield return Pending;
            yield return Acknowledged;
            yield return Resolved;
        }

        public bool IsPending => this == Pending;
        public bool IsAcknowledged => this == Acknowledged;
        public bool IsResolved => this == Resolved;

        public static implicit operator string(AlertStatus status) => status.Value;

        public override string ToString() => Value;
    }
}
