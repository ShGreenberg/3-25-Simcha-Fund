using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFundManager
{
    public class Transaction
    {
        public int ContributorId { get; set; }
        public int? Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int? SimchaId { get; set; }
        public bool Contribute { get; set; }
    }
}
