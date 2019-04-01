using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFundManager;
namespace _3_25_simcha_fund.Models
{
    public class IndexViewModel
    {
        public IEnumerable<Simcha> Simchos { get; set; }
        public int MaxContributors { get; set; }

        public Dictionary<int, int> NumContributors { get; set; }
        public Dictionary<int, decimal> Totals { get; set; }
    }
}