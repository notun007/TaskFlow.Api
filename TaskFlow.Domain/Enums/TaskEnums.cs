namespace TaskFlow.Domain.Enums;

public enum TaskType { Bug, Requirement, ChangeRequest, Enhancement, Incident, MeetingAction, Testing, Deployment, Maintenance }
public enum TaskStatus { Draft, Submitted, Triaged, Approved, Assigned, InProgress, PendingInformation, PendingVendor, ReadyForTesting, Uat, Resolved, Closed, Rejected, Cancelled, Reopened }
public enum Priority { Critical, High, Medium, Low }
public enum Severity { Critical, High, Medium, Low }
public enum ResponsibilityType { Reporter, Owner, Assignee, SupportingTeam, Vendor, VendorContact, Approver, Tester, UatOwner }
