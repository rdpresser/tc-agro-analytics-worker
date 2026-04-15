using FluentValidation;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetAlertHistory;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetSensorStatus;

namespace TC.Agro.Analytics.Tests.Application.Validators;

/// <summary>
/// Unit tests for all analytics query validators.
/// Covers all validation rules: required fields, ranges, enum values, and pagination limits.
/// </summary>
public sealed class AnalyticsQueryValidatorTests
{
    // ══════════════════════════════════════════
    // GetAlertHistoryQueryValidator
    // ══════════════════════════════════════════

    public sealed class GetAlertHistoryQueryValidatorTests
    {
        private readonly GetAlertHistoryQueryValidator _validator = new();

        [Fact]
        public void Validate_WithValidQuery_ShouldPass()
        {
            var query = new GetAlertHistoryQuery
            {
                SensorId = Guid.NewGuid(),
                Days = 30,
                PageNumber = 1,
                PageSize = 10
            };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithEmptySensorId_ShouldFail()
        {
            var query = new GetAlertHistoryQuery { SensorId = Guid.Empty, Days = 30 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.SensorId));
        }

        [Fact]
        public void Validate_WithZeroDays_ShouldFail()
        {
            var query = new GetAlertHistoryQuery { SensorId = Guid.NewGuid(), Days = 0 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.Days));
        }

        [Fact]
        public void Validate_WithNegativeDays_ShouldFail()
        {
            var query = new GetAlertHistoryQuery { SensorId = Guid.NewGuid(), Days = -1 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.Days));
        }

        [Fact]
        public void Validate_WithDaysExceeding365_ShouldFail()
        {
            var query = new GetAlertHistoryQuery { SensorId = Guid.NewGuid(), Days = 366 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.Days));
        }

        [Fact]
        public void Validate_WithAlertTypeTooLong_ShouldFail()
        {
            var query = new GetAlertHistoryQuery
            {
                SensorId = Guid.NewGuid(),
                Days = 30,
                AlertType = new string('x', 51)
            };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.AlertType));
        }

        [Fact]
        public void Validate_WithNullAlertType_ShouldPass()
        {
            var query = new GetAlertHistoryQuery
            {
                SensorId = Guid.NewGuid(),
                Days = 30,
                AlertType = null
            };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithStatusTooLong_ShouldFail()
        {
            var query = new GetAlertHistoryQuery
            {
                SensorId = Guid.NewGuid(),
                Days = 30,
                Status = new string('s', 21)
            };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.Status));
        }

        [Fact]
        public void Validate_WithZeroPageNumber_ShouldFail()
        {
            var query = new GetAlertHistoryQuery { SensorId = Guid.NewGuid(), Days = 30, PageNumber = 0 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.PageNumber));
        }

        [Fact]
        public void Validate_WithZeroPageSize_ShouldFail()
        {
            var query = new GetAlertHistoryQuery { SensorId = Guid.NewGuid(), Days = 30, PageNumber = 1, PageSize = 0 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.PageSize));
        }

        [Fact]
        public void Validate_WithPageSizeExceedingMax_ShouldFail()
        {
            var query = new GetAlertHistoryQuery
            {
                SensorId = Guid.NewGuid(),
                Days = 30,
                PageNumber = 1,
                PageSize = 10_000
            };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.PageSize));
        }

        [Fact]
        public void Validate_WithBoundaryDays_ShouldPass()
        {
            var query1 = new GetAlertHistoryQuery { SensorId = Guid.NewGuid(), Days = 1, PageNumber = 1, PageSize = 10 };
            var query365 = new GetAlertHistoryQuery { SensorId = Guid.NewGuid(), Days = 365, PageNumber = 1, PageSize = 10 };

            _validator.Validate(query1).IsValid.ShouldBeTrue();
            _validator.Validate(query365).IsValid.ShouldBeTrue();
        }
    }

    // ══════════════════════════════════════════
    // GetPendingAlertsQueryValidator
    // ══════════════════════════════════════════

    public sealed class GetPendingAlertsQueryValidatorTests
    {
        private readonly GetPendingAlertsQueryValidator _validator = new();

        [Fact]
        public void Validate_WithValidQuery_ShouldPass()
        {
            var query = new GetPendingAlertsQuery { PageNumber = 1, PageSize = 10 };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithOwnerIdAsEmptyGuid_ShouldFail()
        {
            var query = new GetPendingAlertsQuery { OwnerId = Guid.Empty, PageNumber = 1, PageSize = 10 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsQuery.OwnerId));
        }

        [Fact]
        public void Validate_WithValidOwnerId_ShouldPass()
        {
            var query = new GetPendingAlertsQuery { OwnerId = Guid.NewGuid(), PageNumber = 1, PageSize = 10 };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithNullOwnerId_ShouldPass()
        {
            var query = new GetPendingAlertsQuery { OwnerId = null, PageNumber = 1, PageSize = 10 };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Theory]
        [InlineData("high")]
        [InlineData("medium")]
        [InlineData("low")]
        [InlineData("critical")]
        [InlineData("warning")]
        [InlineData("info")]
        [InlineData("HIGH")]
        public void Validate_WithValidSeverity_ShouldPass(string severity)
        {
            var query = new GetPendingAlertsQuery { PageNumber = 1, PageSize = 10, Severity = severity };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithInvalidSeverity_ShouldFail()
        {
            var query = new GetPendingAlertsQuery { PageNumber = 1, PageSize = 10, Severity = "EXTREME" };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsQuery.Severity));
        }

        [Theory]
        [InlineData("pending")]
        [InlineData("acknowledged")]
        [InlineData("resolved")]
        [InlineData("all")]
        [InlineData("PENDING")]
        public void Validate_WithValidStatus_ShouldPass(string status)
        {
            var query = new GetPendingAlertsQuery { PageNumber = 1, PageSize = 10, Status = status };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithInvalidStatus_ShouldFail()
        {
            var query = new GetPendingAlertsQuery { PageNumber = 1, PageSize = 10, Status = "open" };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsQuery.Status));
        }

        [Fact]
        public void Validate_WithNullSeverityAndStatus_ShouldPass()
        {
            var query = new GetPendingAlertsQuery { PageNumber = 1, PageSize = 10, Severity = null, Status = null };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithZeroPageSize_ShouldFail()
        {
            var query = new GetPendingAlertsQuery { PageNumber = 1, PageSize = 0 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsQuery.PageSize));
        }
    }

    // ══════════════════════════════════════════
    // GetPendingAlertsSummaryQueryValidator
    // ══════════════════════════════════════════

    public sealed class GetPendingAlertsSummaryQueryValidatorTests
    {
        private readonly GetPendingAlertsSummaryQueryValidator _validator = new();

        [Fact]
        public void Validate_WithValidQuery_ShouldPass()
        {
            var query = new GetPendingAlertsSummaryQuery { WindowHours = 24 };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithZeroWindowHours_ShouldFail()
        {
            var query = new GetPendingAlertsSummaryQuery { WindowHours = 0 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsSummaryQuery.WindowHours));
        }

        [Fact]
        public void Validate_WithNegativeWindowHours_ShouldFail()
        {
            var query = new GetPendingAlertsSummaryQuery { WindowHours = -1 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsSummaryQuery.WindowHours));
        }

        [Fact]
        public void Validate_WithWindowHoursExceeding720_ShouldFail()
        {
            var query = new GetPendingAlertsSummaryQuery { WindowHours = 721 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsSummaryQuery.WindowHours));
        }

        [Fact]
        public void Validate_WithBoundaryWindowHours_ShouldPass()
        {
            var q1 = new GetPendingAlertsSummaryQuery { WindowHours = 1 };
            var q720 = new GetPendingAlertsSummaryQuery { WindowHours = 720 };

            _validator.Validate(q1).IsValid.ShouldBeTrue();
            _validator.Validate(q720).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithOwnerIdAsEmptyGuid_ShouldFail()
        {
            var query = new GetPendingAlertsSummaryQuery { OwnerId = Guid.Empty, WindowHours = 24 };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsSummaryQuery.OwnerId));
        }

        [Fact]
        public void Validate_WithNullOwnerId_ShouldPass()
        {
            var query = new GetPendingAlertsSummaryQuery { OwnerId = null, WindowHours = 24 };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }
    }

    // ══════════════════════════════════════════
    // GetSensorStatusQueryValidator
    // ══════════════════════════════════════════

    public sealed class GetSensorStatusQueryValidatorTests
    {
        private readonly GetSensorStatusQueryValidator _validator = new();

        [Fact]
        public void Validate_WithValidSensorId_ShouldPass()
        {
            var query = new GetSensorStatusQuery { SensorId = Guid.NewGuid() };

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WithEmptySensorId_ShouldFail()
        {
            var query = new GetSensorStatusQuery { SensorId = Guid.Empty };

            var result = _validator.Validate(query);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(GetSensorStatusQuery.SensorId));
        }
    }
}
