using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public sealed class Department : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Team> Teams { get; set; } = [];
}

public sealed class Team : Entity
{
    public required string Name { get; set; }
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
}

public sealed class Vendor : Entity
{
    public required string Name { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? ContractReference { get; set; }
    public string? SlaDetails { get; set; }
}

public sealed class SoftwareApplication : Entity
{
    public required string Name { get; set; }
    public bool IsThirdParty { get; set; }
    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public string? BusinessOwner { get; set; }
    public string? TechnicalOwner { get; set; }
    public string? SupportTeam { get; set; }
    public string? Criticality { get; set; }
    public string? Technology { get; set; }
    public string? CurrentVersion { get; set; }
    public bool IsProduction { get; set; }
}
