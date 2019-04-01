using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _3_25_simcha_fund.Models
{
    public class ContributionsAmount
    {
        public int ContributorId { get; set; }
        public decimal Amount { get; set; }
        public int SimchaId { get; set; }
        public bool Include { get; set; }
        public DateTime Date { get; set; }
    }
}