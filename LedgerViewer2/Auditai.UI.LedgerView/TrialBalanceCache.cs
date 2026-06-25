using System;
using System.Collections.Generic;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public class TrialBalanceCache : Dictionary<Account, Tuple<decimal, decimal, decimal>>
{
}
