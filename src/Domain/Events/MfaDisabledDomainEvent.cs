using Domain.ValueObjects;

namespace Domain.Events;

public sealed record MfaDisabledDomainEvent(
    Guid Id, 
    Guid UserId,
    Email Email) : DomainEvent(Id);
