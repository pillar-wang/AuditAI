using System;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public class TicketNavTreeProjectID
{
    public Id64 TicketId;
    public Guid ProjectId;

    public TicketNavTreeProjectID(Guid projectId, Id64 ticketId)
    {
        ProjectId = projectId;
        TicketId = ticketId;
    }

    public static bool operator ==(TicketNavTreeProjectID left, TicketNavTreeProjectID right)
    {
        return left.TicketId == right.TicketId && left.ProjectId == right.ProjectId;
    }

    public static bool operator !=(TicketNavTreeProjectID left, TicketNavTreeProjectID right)
    {
        return left.TicketId != right.TicketId || left.ProjectId != right.ProjectId;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is TicketNavTreeProjectID other))
            return false;
        return TicketId == other.TicketId && ProjectId == other.ProjectId;
    }

    public override int GetHashCode()
    {
        return TicketId.GetHashCode() & ProjectId.GetHashCode();
    }
}