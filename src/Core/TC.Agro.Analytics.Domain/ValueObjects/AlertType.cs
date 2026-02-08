using System.Collections.Generic;

namespace TC.Agro.Analytics.Domain.ValueObjects
{
    public sealed record AlertType
    {
        public string Value { get; }

        private AlertType(string value) => Value = value;

        public static AlertType HighTemperature => new("HighTemperature");
        public static AlertType LowSoilMoisture => new("LowSoilMoisture");
        public static AlertType LowBattery => new("LowBattery");

        public static Result<AlertType> Create(string value)
        {
            var normalized = value?.Trim();
            return normalized switch
            {
                "HighTemperature" => Result.Success(HighTemperature),
                "LowSoilMoisture" => Result.Success(LowSoilMoisture),
                "LowBattery" => Result.Success(LowBattery),
                _ => Result.Invalid(new ValidationError
                {
                    Identifier = "AlertType.Invalid",
                    ErrorMessage = $"Invalid alert type: '{value}'. Must be HighTemperature, LowSoilMoisture, or LowBattery."
                })
            };
        }

        public static IEnumerable<AlertType> GetAll()
        {
            yield return HighTemperature;
            yield return LowSoilMoisture;
            yield return LowBattery;
        }

        public bool IsHighTemperature => this == HighTemperature;
        public bool IsLowSoilMoisture => this == LowSoilMoisture;
        public bool IsLowBattery => this == LowBattery;

        public static implicit operator string(AlertType type) => type.Value;

        public override string ToString() => Value;
    }
}
