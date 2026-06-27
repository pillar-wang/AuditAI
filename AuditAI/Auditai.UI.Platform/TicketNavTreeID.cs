using System;

namespace Auditai.UI.Platform;

public class TicketNavTreeID
{
    public int PageIndex;
    public string ColumnsKey;

    public static bool operator ==(TicketNavTreeID left, TicketNavTreeID right)
    {
        if ((object)left == null)
        {
            if ((object)right == null)
                return true;
            return false;
        }
        if ((object)right == null)
            return false;
        if (left.PageIndex == right.PageIndex)
            return left.ColumnsKey == right.ColumnsKey;
        return false;
    }

    public static bool operator !=(TicketNavTreeID left, TicketNavTreeID right)
    {
        if ((object)left == null && (object)right == null)
            return false;
        if ((object)left == null || (object)right == null)
            return true;
        if (left.PageIndex != right.PageIndex)
            return left.ColumnsKey != right.ColumnsKey;
        return false;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is TicketNavTreeID other))
            return false;
        if (PageIndex == other.PageIndex)
            return ColumnsKey == other.ColumnsKey;
        return false;
    }

    public override int GetHashCode()
    {
        if (ColumnsKey == null)
            return PageIndex;
        return PageIndex & ColumnsKey.GetHashCode();
    }
}