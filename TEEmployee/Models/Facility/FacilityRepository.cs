using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace TEEmployee.Models.Facility
{
    public class FacilityRepository : IFacilityRepository
    {
        private IDbConnection _conn;
        public FacilityRepository()
        {
            string educationConnection = ConfigurationManager.ConnectionStrings["FacilityConnection"].ConnectionString;
            _conn = new SQLiteConnection(educationConnection);
        }

        /// <summary>
        /// 取得所有公用裝置
        /// </summary>
        /// <returns></returns>
        public List<Facility> GetDevices()
        {
            List<Facility> ret;
            string sql = @"SELECT * FROM facility";
            ret = _conn.Query<Facility>(sql).Where(x => x.available.Equals(true)).OrderBy(x => x.deviceID).ToList();

            return ret;
        }
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            bool ret = false;

            try
            {
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"DELETE FROM facility WHERE id=@id;";
                    _conn.Execute(sql, new { id }, tran);
                    tran.Commit();
                    ret = true;
                }
                _conn.Close();
            }
            catch (Exception ex) { string error = ex.Message + "\n" + ex.ToString(); }

            return ret;
        }
        /// <summary>
        /// 修改與新增, 同id則修改, 不同id則新增
        /// </summary>
        /// <param name="state"></param>
        /// <param name="reserve"></param>
        /// <returns></returns>
        public bool Send(string state, Facility reserve)
        {
            bool ret = false;
            try
            {
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"INSERT INTO facility (id, type, deviceID, deviceName, empno, name, contactTel, startTime, endTime, meetingDate, modifiedDate, modifiedUser, num, title, available, allDay)
                                 VALUES(@id, @type, @deviceID, @deviceName, @empno, @name, @contactTel, @startTime, @endTime, @meetingDate, @modifiedDate, @modifiedUser, @num, @title, @available, @allDay)
                                 ON CONFLICT(id)
                                 DO UPDATE SET id=@id, type=@type, deviceID=@deviceID, deviceName=@deviceName, empno=@empno, name=@name, contactTel=@contactTel, startTime=@startTime, endTime=@endTime, meetingDate=@meetingDate, modifiedDate=@modifiedDate, modifiedUser=@modifiedUser, num=@num, title=@title, available=@available, allDay=@allDay";
                    _conn.Execute(sql, reserve, tran);
                    tran.Commit();
                    ret = true;
                }
                _conn.Close();
            }
            catch (Exception ex) { string error = ex.Message + "\n" + ex.ToString(); _conn.Close(); }
            return ret;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}