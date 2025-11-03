using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Web;
using Dapper;
using TEEmployee.Models.IssueV2;

namespace TEEmployee.Models.Ballot
{
    public class BallotRepository : IBallotRepository
    {
        private IDbConnection _conn;
        public BallotRepository()
        {
            string ballotConnection = ConfigurationManager.ConnectionStrings["BallotConnection"].ConnectionString;
            _conn = new SQLiteConnection(ballotConnection);
        }

        public List<Ballot> GetBallotsByEvent(string event_name)
        {
            string sql = @"SELECT * FROM Ballot WHERE event_name=@event_name ";
            var ret = _conn.Query<Ballot>(sql, new { event_name }).ToList();

            return ret;
        }

        public Ballot GetBallotByUserAndEvent(string empno, string event_name)
        {
            string sql = @"SELECT * FROM Ballot WHERE empno=@empno AND event_name=@event_name ";
            var ret = _conn.Query<Ballot>(sql, new { empno, event_name }).FirstOrDefault();

            return ret;
        }

        public bool InsertBallot(Ballot ballot)
        {
            int ret = 0;

            string sql = @"
                INSERT INTO Ballot (empno, event_name, choices) 
                VALUES(@empno, @event_name, @choices)                
                ";

            //ON CONFLICT(empno, event_name)
            //    DO NOTHING

            ret = _conn.Execute(sql, ballot);
            return ret > 0;
        }

        public bool DeleteAll()
        {
            string sql = @"DELETE FROM Ballot";
            int ret = _conn.Execute(sql);

            return ret > 0;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        
    }
}