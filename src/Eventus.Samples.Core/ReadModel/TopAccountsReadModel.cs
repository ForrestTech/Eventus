using System.Collections.Generic;
using System.Linq;

namespace Eventus.Samples.Core.ReadModel
{
    public class TopAccountsReadModel
    {
        public TopAccountsReadModel()
        {
            Accounts = new List<AccountSummary>();    
        }

        public List<AccountSummary> Accounts { get; set; }

        public List<AccountSummary> TopAccounts
        {
            get
            {
                return Accounts.OrderByDescending(x => x.Balance)
                    .Take(10)
                    .ToList();
            }
        }
    }
}