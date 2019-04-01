using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SimchaFundManager;
using _3_25_simcha_fund.Models;

namespace _3_25_simcha_fund.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            IndexViewModel vm = new IndexViewModel();
            vm.Simchos = mgr.GetSimchos();
            vm.MaxContributors = mgr.MaxContributors();
            vm.NumContributors = mgr.ContributorCountDictionary(vm.Simchos);
            vm.Totals = mgr.GetTotals(vm.Simchos);
            return View(vm);
        }

        public ActionResult AddSimcha(Simcha simcha)
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            mgr.AddSimcha(simcha);
            return Redirect("/");
        }

        public ActionResult Contributions(int id)
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            ContributionsViewModel vm = new ContributionsViewModel();
            vm.SimchaId = id;
            Dictionary<int, List<Contributor>> test = new Dictionary<int, List<Contributor>>();
            List<Simcha> simchas = mgr.GetSimchos().ToList();
            if (Session["contributorsForSimcha"] == null)
            {
                foreach (Simcha s in simchas)
                {
                    test.Add(s.Id, mgr.GetContributorsForSpecificSimcha(s.Id).ToList());
                }
                Session["contributorsForSimcha"] = test;
            }
            var fromSession = (Dictionary<int,List<Contributor>>)Session["contributorsForSimcha"];
            var trial = mgr.GetContributors();
            foreach(Contributor c in trial)
            {
                if (fromSession[id].FirstOrDefault(t => t.Id == c.Id) != null)
                {
                    c.AlreadyContributed = true;
                }
            }
            vm.Contributors = trial;
            return View(vm);
        }

        public ActionResult Contributors()
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            ContributorsViewModel vm = new ContributorsViewModel();
            vm.Contributors = mgr.GetContributors();
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditContributor(Contributor contributor)
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            mgr.UpdateContributor(contributor);
            return Redirect("/home/contributors");
        }

        [HttpPost]
        public ActionResult AddContributor(Contributor c, Transaction t)
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            t.ContributorId = mgr.AddContributor(c);
            mgr.AddDeposit(t);
            return Redirect("/home/contributors");
        }

        [HttpPost]
        public ActionResult Deposit(Transaction t)
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            mgr.AddDeposit(t);
            return Redirect("/home/contributors");
        }

        [HttpPost]
        public ActionResult UpdateContributions(List<Transaction> contributions, int id)
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            var fromSession = (Dictionary<int, List<Contributor>>)Session["contributorsForSimcha"];
            List<Transaction> transactions = new List<Transaction>();
            List<Transaction> delete = new List<Transaction>();
            int x = 0;
            foreach(Transaction t in contributions)
            {

                if (t.Contribute)
                {
                    transactions.Add(t);
                    x++;
                }
                else
                {
                    var con = mgr.GetContributorsForSpecificSimcha((int)t.SimchaId);
                    if(con.FirstOrDefault(c => c.Id == t.ContributorId) != null)
                    {
                        delete.Add(t);
                    }
                }
            }
            mgr.AddContribution(transactions);
            mgr.DeleteTrans(delete);
            
            fromSession[id].AddRange(mgr.GetContributorsForSpecificSimcha((int)id));
           for (int i = 0; i < x; i++)
            {
                fromSession[id][i].AlreadyContributed = true;
            }
            Session["contributorsForSimcha"] = fromSession;
            return Redirect("/");
        }

        public ActionResult ShowHistory(int id)
        {
            DbManager mgr = new DbManager(Properties.Settings.Default.ConStr);
            ShowHistoryViewModel vm = new ShowHistoryViewModel();
            vm.Transactions = mgr.GetHistory(id);
            vm.Simchos = mgr.GetSimchosForContributor(id);
            vm.ContributorName = mgr.GetContributorName(id);
            return View(vm);
        }
    }
}