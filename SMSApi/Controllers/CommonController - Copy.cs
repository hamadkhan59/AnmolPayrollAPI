using Logger;
using SMS_DAL;
using SMS_DAL.Reports;
using SMS_DAL.SmsRepository.IRepository;
using SMS_DAL.SmsRepository.RepositoryImp;
using SMSApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SMSApi.Controllers
{
    public class CommonController : ApiController
    {
        IStaffRepository staffRepo = new StaffRepositoryImp(new SC_WEBEntities2());
        ISecurityRepository secRepo = new SecurityRepositoryImp(new SC_WEBEntities2());
        //public string[] Get()
        //{
        //    return new string[]
        //    {
        //        "Hello",
        //        "World"
        //    };
        //}

        [HttpGet]
        public string AuthenticateUser(string userName, string password)
        {
            string errorCode = "000";
            try
            {
                User user = secRepo.AuthenticateUser(userName, password);
                if (user == null)
                    errorCode = "200";
            }
            catch
            {
                errorCode = "420";
            }
            return errorCode;
        }

        [HttpGet]
        public string AddStaffBioMatric(string staffId, string bioHash)
        {
            return staffRepo.AddStaffBioMatric(staffId, bioHash);
        }

        DateTime GetDate(string attendanceDate)
        {
            string [] attendanceData = attendanceDate.Split(' ');
            string [] date = attendanceData[0].Split('/');
            string [] time = attendanceData[1].Split(':');
            string am_pm = attendanceData[2];
            //string newDate = date[1] + "/" + date[0] + "/" + date[2];
            int year = int.Parse(date[2]);
            int month = int.Parse(date[0]);
            int day = int.Parse(date[1]);

            int hours = int.Parse(time[0]);
            int mins = int.Parse(time[1]);
            int secs = int.Parse(time[2]);


            //string dateTime = newDate + " " + time + " " + am_pm;
            if (am_pm.ToLower().Contains("pm"))
            {
                if(hours != 12)
                    hours += 12;
            }
            else if (am_pm.ToLower().Contains("am"))
            {
                if (hours == 12)
                    hours = 0;
            }
            DateTime dateAtt = new DateTime(year, month, day, hours, mins, secs);

            return dateAtt;
        }

        [HttpGet]
        public string GetBioMatrixLogCount()
        {
            int count = staffRepo.GetBioMatrixLogCount();
            return count.ToString();
        }

        [HttpGet]
        public void UpdateBioMatrixLogCount(int count)
        {
            staffRepo.UpdateBioMatrixLogCount(count);
        }

        [HttpGet]
        public string FetchLogsFromDB()
        {
            DataSet dataSet = (new DAL_Staff_Reports()).GetNewStaffAttendanceLogs();

            var table = dataSet.Tables[0];
            int count = 0;
            foreach (DataRow row in table.Rows)
            {
                string staffId = row[1].ToString();
                string attendanceDate = row[3].ToString();
                AddStaffAttendance(staffId, attendanceDate);
                count++;
            }

            return "SUCCESS";
        }

        [HttpGet]
        public string AddStaffAttendance(string staffId, string attendanceDate)
        {
            string error = AttendanceSettings.SUCCESS;
            //string datetime = GetDate(attendanceDate);
            try
            {
                //DateTime attendanceTime = DateTime.Parse(attendanceDate);
                DateTime attendanceTime = GetDate(attendanceDate);
                //if (attendanceTime.DayOfWeek == DayOfWeek.Sunday)
                //{
                //    error = "Error : Today is sunday";
                //}
                //else
                //{
                //if (AttendanceSettings.TodaysDate.Date != attendanceTime.Date)
                //{
                //    AttendanceSettings.TodaysDate = attendanceTime;
                //    AttendanceSettings.TodaysAttendance = false;
                //}
                var staffObj = staffRepo.GetStaffById(int.Parse(staffId));

                if (staffObj != null)
                {
                    int logId = staffRepo.GetAttendanceLogId(int.Parse(staffId), attendanceDate);
                    //int logId = 0;

                    if (logId == 0 || true)
                    {
                        //StaffAttendanceLog log = new StaffAttendanceLog();
                        //log.DateTime = attendanceTime;
                        //log.DateTimeString = attendanceDate;
                        //log.StaffId = int.Parse(staffId);
                        //staffRepo.AddStaffAttendnaceLogs(log);

                        StaffAttandance attObj = staffRepo.GetStaffDailyAttendance(attendanceTime, 0);

                        //if (attObj == null)
                        //{
                        //    LogWriter.WriteLog("Attendance is not found for the date : " + attendanceTime.Date + ", staff Id : " + staffId);
                        //    AttendanceSettings.TodaysAttendance = true;

                        //    List<Staff> staffList = staffRepo.GetAllStaff();
                        //    foreach (Staff staff in staffList)
                        //    {
                        //        StaffAttandance tempObj = new StaffAttandance();
                        //        tempObj.StaffId = staff.StaffId;
                        //        tempObj.Date = attendanceTime.Date;
                        //        tempObj.CreatedOn = DateTime.Now;
                        //        tempObj.Time = AttendanceSettings.TimeIn;
                        //        tempObj.OutTime = AttendanceSettings.TimeIn;
                        //        tempObj.Status = 2;

                        //        int attendance = staffRepo.AddStaffAttendnace(tempObj);
                        //    }
                        //}
                        attObj = staffRepo.SearchStaffDailyAttendance(attendanceTime, int.Parse(staffId));

                        if (attObj == null || attObj.Id > 67052)
                        {
                            if (attObj == null)
                            {
                                StaffAttandance tempObj = new StaffAttandance();
                                tempObj.StaffId = int.Parse(staffId);
                                tempObj.Date = attendanceTime.Date;
                                tempObj.CreatedOn = DateTime.Now;
                                tempObj.Time = AttendanceSettings.TimeIn;
                                tempObj.OutTime = AttendanceSettings.TimeIn;
                                tempObj.Status = 1;

                                int attendance = staffRepo.AddStaffAttendnace(tempObj);
                                attObj = tempObj;
                            }

                            LogWriter.WriteLog("Adding attendance");


                            string time = attendanceTime.Hour.ToString().PadLeft(2, '0') + ":" + attendanceTime.Minute.ToString().PadLeft(2, '0');
                            var attDetail = staffRepo.GetTopStaffAttendanceDetailByAttId(attObj.Id);
                            string type = AttendanceSettings.TIME_IN;
                            if (attDetail == null || attDetail.TimeOut != null)
                            {
                                LogWriter.WriteLog("Adding initial attendance detail");
                                attDetail = new StaffAttendanceDetail();
                                attDetail.AttendanceId = attObj.Id;
                                attDetail.TimeIn = time;
                                attDetail.TimeInString = attendanceDate;
                                staffRepo.AddStaffAttendnaceDetail(attDetail);
                            }
                            else
                            {
                                LogWriter.WriteLog("Adding next attendance detail");
                                if (attDetail.TimeOut == null)
                                {
                                    type = AttendanceSettings.TIME_OUT;
                                    int hours = int.Parse(time.Split(':')[0]);
                                    int mins = int.Parse(time.Split(':')[1]);
                                    if (hours >= 19 || hours <= 2)
                                    {
                                        if (mins >= 40)
                                            time = time.Split(':')[0] + ":" + mins;
                                        else
                                            time = time.Split(':')[0] + ":00";
                                    }

                                    attDetail.TimeOut = time;
                                    attDetail.TimeOutString = attendanceDate;
                                    staffRepo.UpdateStaffAttendanceDetail(attDetail);
                                }
                            }

                            attObj.Status = 1;
                            LogWriter.WriteLog("updating attendance status");
                            if (type == AttendanceSettings.TIME_IN)
                            {
                                if (attObj.Time == AttendanceSettings.TimeIn)
                                {
                                    attObj.Time = time;
                                    error = AttendanceSettings.TIME_IN;
                                }
                            }
                            else
                            {
                                int hours = int.Parse(time.Split(':')[0]);
                                int mins = int.Parse(time.Split(':')[1]);
                                if (hours >= 19 || hours <= 2)
                                {
                                    if (mins >= 40)
                                        time = time.Split(':')[0] + ":" + mins;
                                    else
                                        time = time.Split(':')[0] + ":00";
                                }
                                attObj.OutTime = time;
                                error = AttendanceSettings.TIME_OUT;
                            }
                            staffRepo.UpdateStaffAttendance(attObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = "===================================\n";
                error += ex.ToString();
                error += "\n===================================\n";
                error += ex.StackTrace.ToString();
                error += "\n===================================\n";
                LogWriter.WriteLog(error);

            }
            return error;
        }

        [HttpGet]
        public void AddStaffPaymentApproval(string staffId)
        {
            StaffPaymentApproval approvl = new StaffPaymentApproval();
            approvl.StaffId = int.Parse(staffId);
            approvl.PaymentFlag = true;
            approvl.CreatedOn = DateTime.Now;
            staffRepo.AddStaffPaymentApprovalHistory(approvl);
        }

        [HttpGet]
        public string MarkAttendance(string bioHash)
        {
            return "000";
        }

        [HttpGet]
        public string SearchStaffId(string staffId)
        {
            return staffRepo.GetAPIStaffById(staffId);
        }

        [HttpGet]
        public List<string> GetAllStaff()
        {
            return staffRepo.GetAPIAllStaff();
        }

        [HttpGet]
        public string ShowAttendanceProcess()
        {
            string response = "";
            try
            {
                string exePath = ConfigurationManager.AppSettings["BioMatrixExePath"].ToString();
                string directory = ConfigurationManager.AppSettings["BioMatrixExeDirectory"].ToString();
                string username = ConfigurationManager.AppSettings["Username"].ToString();
                string password = ConfigurationManager.AppSettings["Password"].ToString();
                var proc = new Process();
                proc.StartInfo = new ProcessStartInfo(exePath);
                proc.StartInfo.WorkingDirectory = directory;
                proc.StartInfo.UseShellExecute = true;
                

                proc.Start();

                //Process.Start(exePath);
                response = "Success";
                response += proc.StartInfo.WorkingDirectory;
            }
            catch (Exception exc)
            {
                response = exc.ToString();
            }

            return response;
        }

    }
}
