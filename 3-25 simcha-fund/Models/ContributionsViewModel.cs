using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFundManager;

namespace _3_25_simcha_fund.Models
{
    public class ContributionsViewModel
    {
        public IEnumerable<Contributor> Contributors { get; set; }
        public string SimchaName { get; set; }
        public int SimchaId { get; set; }
        public int counter = 0;
    }
}