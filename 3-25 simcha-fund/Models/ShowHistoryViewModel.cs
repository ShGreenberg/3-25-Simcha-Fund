using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFundManager;

namespace _3_25_simcha_fund.Models
{
    public class ShowHistoryViewModel
    {
        public IEnumerable<Transaction> Transactions { get; set; }
        public string ContributorName { get; set; }
        public Dictionary<int, Simcha> Simchos { get; set; }
    }
}