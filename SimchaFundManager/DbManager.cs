using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SimchaFundManager
{
    public class DbManager
    {
        private string _connString;
        public DbManager(string connString)
        {
            _connString = connString;
        }

        

        public void AddSimcha(Simcha simcha)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Simchos VALUES (@name, @date)";
            cmd.Parameters.AddWithValue("@name", simcha.SimchaName);
            cmd.Parameters.AddWithValue("@date", simcha.Date);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();


        }

        public int AddContributor(Contributor contributor)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Contributors VALUES(@name, @cell, @alwaysinclude)
                                SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@name", contributor.Name);
            cmd.Parameters.AddWithValue("@cell", contributor.Cell);
            cmd.Parameters.AddWithValue("@alwaysinclude", contributor.AlwaysInclude);
            conn.Open();
            int id = (int)(decimal)cmd.ExecuteScalar();
            conn.Close();
            conn.Dispose();
            return id;

        }

        public void AddDeposit(Transaction t)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Transactions (ContributorId, Amount, Date) VALUES(@contributorid, @amount, @date)";
            cmd.Parameters.AddWithValue("@contributorid", t.ContributorId);
            cmd.Parameters.AddWithValue("@amount", t.Amount);
            cmd.Parameters.AddWithValue("@date", t.Date);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
        }

        public void AddContribution(List<Transaction> contributions)
        {

            if(contributions.Count == 0)
            {
                return;
            }
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Transactions VALUES(@amount, @date, @contributorid, @simchaid)
                                INSERT INTO SimchosContributors VALUES (@simchaid, @contributorid, @amount)";
            conn.Open();
            List<Contributor> contr = GetContributorsForSpecificSimcha((int)contributions[0].SimchaId).ToList();

            foreach (Transaction t in contributions)
            {

                if(contr.Count == 0 || !contr.Any(c => c.Id == t.ContributorId))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@contributorid", t.ContributorId);
                        cmd.Parameters.AddWithValue("@amount", -t.Amount);
                        cmd.Parameters.AddWithValue("@date", t.Date);
                        cmd.Parameters.AddWithValue("@simchaid", t.SimchaId);
                        cmd.ExecuteNonQuery();
                    }

            }
           
            conn.Close();
            conn.Dispose();
        }
        //doesn't include date
        public void UpdateContributor(Contributor contributor)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            //doesn't include the date
            cmd.CommandText = @"UPDATE Contributors set name = @name, cell = @cell, alwaysinclude = @include
                                WHERE id = @id";
            cmd.Parameters.AddWithValue("@name", contributor.Name);
            cmd.Parameters.AddWithValue("@cell", contributor.Cell);
            cmd.Parameters.AddWithValue("@include", contributor.AlwaysInclude);
            cmd.Parameters.AddWithValue("id", contributor.Id);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
           
        }

        public void DeleteTrans(List<Transaction> transactions)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Delete Transactions WHERE SimchaId = @simchaid AND Contributorid = @contributorId
                                DELETE SimchosContributors WHERE  SimchaId = @simchaid AND Contributorid = @contributorId";
            conn.Open();
            foreach (Transaction t in transactions)
            {
                
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@contributorid", t.ContributorId);
                        cmd.Parameters.AddWithValue("@amount", -t.Amount);
                        cmd.Parameters.AddWithValue("@date", t.Date);
                        cmd.Parameters.AddWithValue("@simchaid", t.SimchaId);
                        cmd.ExecuteNonQuery();
            }

            conn.Close();
            conn.Dispose();
        }

        public int MaxContributors()
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(Id) FROM Contributors";
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            conn.Close();
            conn.Dispose();
            return count;
        }

        public Dictionary<int, int> ContributorCountDictionary(IEnumerable<Simcha> simchas)
        {
            Dictionary<int, int> simchaDict = new Dictionary<int, int>();
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(sc.ContributorId) FROM SimchosContributors sc
                                    inner join Contributors c on sc.ContributorId = c.Id
                                        WHERE sc.SimchaId = @id";
            conn.Open();
            foreach (Simcha s in simchas)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", s.Id);
                int amount = (int)cmd.ExecuteScalar();
                simchaDict.Add(s.Id, amount);
            }
            return simchaDict;
        }

        public Dictionary<int,decimal> GetTotals(IEnumerable<Simcha> simchos)
        {
            Dictionary<int, decimal> totals = new Dictionary<int, decimal>();
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT SUM(Amount) FROM Transactions WHERE SimchaId = @id";
            conn.Open();
            foreach(Simcha s in simchos)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", s.Id);
                object o = cmd.ExecuteScalar();
                if(o == DBNull.Value)
                {
                    totals.Add(s.Id, 0);
                }
                else
                {
                    decimal total = (decimal)o;
                    totals.Add(s.Id, -total);
                }
              
            }

            conn.Close();
            conn.Dispose();
            return totals;
        }

        public IEnumerable<Simcha> GetSimchos()
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Simchos";
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Simcha> simchos = new List<Simcha>();
            while (reader.Read())
            {
                simchos.Add(new Simcha
                {
                    SimchaName = (string)reader["SimchaName"],
                    Date = (DateTime)reader["Date"],
                    Id = (int)reader["Id"]
                });
            }
            conn.Close();
            conn.Dispose();
            return simchos;

        }

        public List<Contributor> GetContributorsForSpecificSimcha(int id)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from Contributors c
                                join SimchosContributors sc
                                on sc.ContributorId = c.Id
                                where sc.SimchaId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Contributor> contributors = new List<Contributor>();
            while (reader.Read())
            {
                contributors.Add(new Contributor
                {
                    Name = (string)reader["name"],
                    Cell = (string)reader["cell"],
                    AlwaysInclude = (bool)reader["AlwaysInclude"],
                    Id = (int)reader["Id"]
                });
            }
            conn.Close();
            conn.Dispose();
            return contributors;
        }

        public IEnumerable<Contributor> GetContributors()
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT  SUM(t.Amount)as'balance', c.*  FROM Contributors c
                                Join Transactions t
                                on t.ContributorId = c.Id
                                group by c.Id, c.AlwaysInclude, c.Name, c.Cell"; 
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Contributor> contributors = new List<Contributor>();
            while (reader.Read())
            {
                contributors.Add(new Contributor
                {
                    Name = (string)reader["name"],
                    Cell = (string)reader["cell"],
                    AlwaysInclude = (bool)reader["AlwaysInclude"],
                    Id = (int)reader["Id"],
                    Balance = (decimal)reader["Balance"]
                });
            }
            conn.Close();
            conn.Dispose();
            return contributors;

        }

        public List<Transaction> GetHistory(int id)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Transactions WHERE Contributorid = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            List<Transaction> transactions = new List<Transaction>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Transaction t = new Transaction
                {
                    Amount = (decimal)reader["Amount"],
                    Date = (DateTime)reader["date"],
                    ContributorId = (int)reader["ContributorId"],
                    Id = (int?)reader["id"]
                };
                object simcha = reader["SimchaId"];
                if(simcha != DBNull.Value)
                {
                    t.SimchaId = (int)simcha;
                }
                transactions.Add(t);
            }
            conn.Close();
            conn.Dispose();
            return transactions;
        }

        public string GetContributorName(int id)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NAME FROM Contributors" +
                " WHERE ID = @ID";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            string name = (string)cmd.ExecuteScalar();
            conn.Close();
            conn.Dispose();
            return name;
        }
   
        public Dictionary<int, Simcha> GetSimchosForContributor(int id)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Simchos s
                                join SimchosContributors sc
                                on sc.SimchaId = s.Id
                                WHERE sc.ContributorId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            Dictionary<int, Simcha> simchos = new Dictionary<int, Simcha>();
            while (reader.Read())
            {
                Simcha s = new Simcha
                {
                    SimchaName = (string)reader["SimchaName"],
                    Date = (DateTime)reader["Date"],
                    Id = (int)reader["Id"]
                };
                simchos.Add(s.Id, s);
            }
            conn.Close();
            conn.Dispose();
            return simchos;
        }

        #region not use
        public int ContributorCount(int id)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(c.Id) FROM SimchosContributors sc
                                    inner join Contributors c on sc.ContributorId = c.Id
                                        WHERE sc.Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            int amount = (int)cmd.ExecuteScalar();
            conn.Close();
            conn.Dispose();
            return amount;
        }
        public IEnumerable<Contributor> GetContributorsForSimcha(int id)
        {
            SqlConnection conn = new SqlConnection(_connString);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Contributors 
                                WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Contributor> contributors = new List<Contributor>();
            while (reader.Read())
            {
                contributors.Add(new Contributor
                {
                    Name = (string)reader["name"],
                    Cell = (string)reader["cell"],
                    AlwaysInclude = (bool)reader["AlwaysInclude"],
                    Id = (int)reader["Id"]
                });
            }
            conn.Close();
            conn.Dispose();
            return contributors;

        }
        #endregion

    }
}
