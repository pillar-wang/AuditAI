using System;
using System.Collections.Generic;
using Leqisoft.Model;

namespace Leqisoft.UI.LedgerView;

public class TrialBalanceCache : Dictionary<Account, Tuple<decimal, decimal, decimal>>
{
}
