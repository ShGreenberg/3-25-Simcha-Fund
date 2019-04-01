using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFundManager
{
    public class Contributor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cell { get; set; }
        public bool AlwaysInclude { get; set; }

        public decimal Balance { get; set; }

        public bool AlreadyContributed { get; set; }

    }
}
