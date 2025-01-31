using Domain.ValueObjects;

namespace Domain.Events;

public sealed record MfaEnabledDomainEvent(
    Guid Id, 
    Guid UserId,
    Email Email) : DomainEvent(Id);
