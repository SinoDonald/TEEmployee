using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using TEEmployee.Models.IssueV2;

namespace TEEmployee.Models.Ballot
{
    public class BallotService
    {
        private IBallotRepository _ballotRepository;
        private IUserRepository _userRepository;

        public BallotService()
        {
            _ballotRepository = new BallotRepository();
            _userRepository = new UserRepository();
        }

        public List<User> GetAllEmployeeCandidates()
        {
            var users = _userRepository.GetAll();
            users.RemoveAll(x => x.department_manager || x.group_manager || x.empno == "6843");
            return users;
        }

        public Ballot GetBallotByUserAndEvent(string empno, string event_name)
        {
            var ret = _ballotRepository.GetBallotByUserAndEvent(empno, event_name);
            return ret;
        }

        public bool CreateBallot(Ballot ballot)
        {
            var ret = _ballotRepository.InsertBallot(ballot);
            return ret;
        }

        public bool DeleteAll()
        {
            var ret = _ballotRepository.DeleteAll();
            return ret;
        }

        public byte[] DownloadEmployeeVoteExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var users = _userRepository.GetAll();
            var nameDict = users.ToDictionary(x => x.empno, x => x.name);
            users.RemoveAll(x => x.empno == "6843");

            var voters_list = users.Select(x => x.empno).ToList();
            users.RemoveAll(x => x.department_manager || x.group_manager);
            var candidates = users.Select(x => x.empno).ToList();

            var ballots = _ballotRepository.GetBallotsByEvent("2025emp");
            var votes = candidates.ToDictionary(x => x, x => 0);

            foreach (var ballot in ballots)
            {
                string[] choices = ballot.choices.Split(';');

                for (int i = 0; i < choices.Length; i++) {
                    votes[choices[i]]++;
                }

                voters_list.Remove(ballot.empno);
            }

            using (var package = new ExcelPackage())
            {
                var ws1 = package.Workbook.Worksheets.Add("票選結果");
                var ws2 = package.Workbook.Worksheets.Add("未投名單");

                // votes
                int row = 1;
                foreach (var v in votes)
                {
                    ws1.Cells[row, 1].Value = v.Key;
                    ws1.Cells[row, 2].Value = nameDict[v.Key];
                    ws1.Cells[row, 3].Value = v.Value;
                    row++;
                }

                // Non voter list
                for (int i = 0; i < voters_list.Count; i++)
                {
                    ws2.Cells[i + 1, 1].Value = voters_list[i];
                    ws2.Cells[i + 1, 2].Value = nameDict[voters_list[i]];
                }

                var excelData = package.GetAsByteArray();  // byte or stream


                return excelData;
            }

        }

        public void Dispose()
        {
            _ballotRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}