using Haver.Data;
using Haver.Models;
using Haver.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Haver.Utilities
{
    public class NotificationGenerator
    {
        private readonly HaverContext _context;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationGenerator(HaverContext context, IMyEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;

        }

        public void NotifyUser()
        {
            var countersConstructor = new CalculateNCRTimeCounters(_context, _userManager);

            (List<NCR> qual24, List<NCR> qual48, List<NCR> qual5, List<NCR> qualTotal, List<NCR> lgActive) qualCounters = countersConstructor.QualCounters();
            (List<NCR> eng24, List<NCR> eng48, List<NCR> eng5, List<NCR> engTotal, List<NCR> lgActive) engCounters = countersConstructor.EngCounters();
            (List<NCR> oper24, List<NCR> oper48, List<NCR> oper5, List<NCR> operTotal, List<NCR> lgActive) operCounters = countersConstructor.OperCounters();
            (List<NCR> proc24, List<NCR> proc48, List<NCR> proc5, List<NCR> procTotal, List<NCR> lgActive) procCounters = countersConstructor.ProcCounters();
            (List<NCR> reinsp24, List<NCR> reinsp48, List<NCR> reinsp5, List<NCR> reinspTotal, List<NCR> lgActive) reinspCounters = countersConstructor.ReinspCounters();

            var usersInAdminRole = _userManager.GetUsersInRoleAsync("Admin").Result;
            var usersInQualInspRole = _userManager.GetUsersInRoleAsync("Quality Inspector").Result;
            var usersInEngineeringRole = _userManager.GetUsersInRoleAsync("Engineer").Result;
            var usersInOperationsRole = _userManager.GetUsersInRoleAsync("Operations Manager").Result;
            var usersInProcurementRole = _userManager.GetUsersInRoleAsync("Procurement").Result;

            //Concat NCRs of each phase
            var allQualNCRs = qualCounters.qual24.Concat(qualCounters.qual48)
                                                  .Concat(qualCounters.qual5)
                                                  .ToList();
            var allEngNCRs = engCounters.eng24.Concat(engCounters.eng48)
                                                  .Concat(engCounters.eng5)
                                                  .ToList();
            var allOperNCRs = operCounters.oper24.Concat(operCounters.oper48)
                                                  .Concat(operCounters.oper5)
                                                  .ToList();
            var allProcNCRs = procCounters.proc24.Concat(procCounters.proc48)
                                                  .Concat(procCounters.proc5)
                                                  .ToList();
            var allReinspNCRs = reinspCounters.reinsp24.Concat(reinspCounters.reinsp48)
                                                      .Concat(reinspCounters.reinsp5)
                                                      .ToList();
            var allLgActiveNCRs = qualCounters.lgActive.Concat(engCounters.lgActive)
                                                      .Concat(operCounters.lgActive)
                                                      .Concat(procCounters.lgActive)
                                                      .Concat(reinspCounters.lgActive)
                                                      .ToList();

            //Get NCRs with due Expected date of return
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            var expecDateRetNCRs = _context.NCRs.Include(ncr => ncr.Procurement)
                                                .Where(ncr => ncr.Procurement.ExpecDateOfReturn.Date <= nowToronto.Date)
                                                .Where(ncr => !ncr.IsNCRArchived)
                                                .Where(ncr => ncr.Status == "Active")
                                                .Where(ncr => ncr.ExpecDateReturnReminded == false)
                                                .ToList();

            //Send notifications to users that are not Admins
            SendNotificationsForNCRList(allQualNCRs, (List<IdentityUser>)usersInQualInspRole, "notLong");
            SendNotificationsForNCRList(allEngNCRs, (List<IdentityUser>)usersInEngineeringRole, "notLong");
            SendNotificationsForNCRList(allOperNCRs, (List<IdentityUser>)usersInOperationsRole, "notLong");
            SendNotificationsForNCRList(allProcNCRs, (List<IdentityUser>)usersInProcurementRole, "notLong");
            SendNotificationsForNCRList(allReinspNCRs, (List<IdentityUser>)usersInQualInspRole, "notLong");

            //Send notification for all NCRs that have been long active, just for admins
            SendNotificationsForNCRList(allLgActiveNCRs, (List<IdentityUser>)usersInAdminRole, "long", "noAdmin");

            //Send notification for Expected date of return if there is any
            SendNotificationsForNCRList(expecDateRetNCRs, (List<IdentityUser>)usersInProcurementRole, "expcDateReminder");
        }

        //Send notification for users that are not admin
        public void SendNotificationsForNCRList(List<NCR> ncrList, List<IdentityUser> userList, string action, string noAdmin = null, string emailContent = null)
        {
            var usersInAdminRole = _userManager.GetUsersInRoleAsync("Admin").Result;
            var usersInQualInspRole = _userManager.GetUsersInRoleAsync("Quality Inspector").Result;
            var usersInEngineeringRole = _userManager.GetUsersInRoleAsync("Engineer").Result;
            var usersInOperationsRole = _userManager.GetUsersInRoleAsync("Operations Manager").Result;
            var usersInProcurementRole = _userManager.GetUsersInRoleAsync("Procurement").Result;

            if(ncrList != null)
            {
                foreach (var ncr in ncrList)
                {
                    try
                    {
                        if (userList != null)
                        {
                            foreach (var user in userList)
                            {
                                if (!usersInAdminRole.Any(ad => ad.UserName == user.UserName))
                                {
                                    Notification notification = new Notification();
                                    var employee = _context.Employees.FirstOrDefault(e => e.Email == user.UserName);
                                    if (action == "notLong")
                                    {
                                        SendFillNotification(notification, ncr, employee);
                                    }
                                    else if (action == "long")
                                    {
                                        SendLongActiveNotification(notification, ncr, employee);
                                    }
                                    else if (action == "expcDateReminder")
                                    {
                                        ExpecDateReturnReminder(notification, ncr, employee);
                                        ncr.ExpecDateReturnReminded = true;
                                    }
                                    else if (action == "rejected")
                                    {
                                        SendNewActionNotification(notification, ncr, employee, "rejected",emailContent);
                                    }
                                    else if (action == "rejectedAgain")
                                    {
                                        SendNewActionNotification(notification, ncr, employee, "rejectedAgain", emailContent);
                                    }
                                    else if (action == "edited")
                                    {
                                        SendNewActionNotification(notification, ncr, employee, "edited");
                                    }
                                    else if (action == "close")
                                    {
                                        SendNewActionNotification(notification, ncr, employee, "close", emailContent);
                                    }
                                    else if (action == "fill")
                                    {
                                        SendNewActionNotification(notification, ncr, employee, "fill", emailContent);
                                    }
                                    else if (action == "create")
                                    {
                                        SendNewActionNotification(notification, ncr, employee, "create", emailContent);
                                    }
                                }
                            }
                        }
                    
                        _context.SaveChanges();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Could not complete Notification creation, NCR {ncr.NCRNum}");
                    }
                }

                if(noAdmin == null)
                {
                    SendNotificationsForNCRListAdmin(ncrList, (List<IdentityUser>)usersInAdminRole, action, emailContent);
                }
            }

        }

        //Send notification for users that are admin
        public void SendNotificationsForNCRListAdmin(List<NCR> ncrList, List<IdentityUser> userList, string action, string emailContent = null)
        {
            var usersInAdminRole = _userManager.GetUsersInRoleAsync("Admin").Result;

            if (ncrList != null)
            {
                foreach (var ncr in ncrList)
                {
                    try
                    {

                        if(userList != null)
                        {
                            foreach (var user in userList)
                            {
                                Notification notification = new Notification();
                                var employee = _context.Employees.FirstOrDefault(e => e.Email == user.UserName);

                                if (action == "notLong")
                                {
                                    SendFillNotification(notification, ncr, employee);
                                }
                                else if (action == "long")
                                {
                                    SendLongActiveNotification(notification, ncr, employee);
                                }
                                else if (action == "expcDateReminder")
                                {
                                    ExpecDateReturnReminder(notification, ncr, employee);
                                    ncr.ExpecDateReturnReminded = true;
                                }
                                else if (action == "rejected")
                                {
                                    SendNewActionNotification(notification, ncr, employee, "rejected", emailContent);
                                }
                                else if (action == "rejectedAgain")
                                {
                                    SendNewActionNotification(notification, ncr, employee, "rejectedAgain", emailContent);
                                }
                                else if (action == "edited")
                                {
                                    SendNewActionNotification(notification, ncr, employee, "edited");
                                }
                                else if (action == "close")
                                {
                                    SendNewActionNotification(notification, ncr, employee, "close", emailContent);
                                }
                                else if (action == "fill")
                                {
                                    SendNewActionNotification(notification, ncr, employee, "fill", emailContent);
                                }
                                else if (action == "create")
                                {
                                    SendNewActionNotification(notification, ncr, employee, "create", emailContent);
                                }
                            }
                        }

                        _context.SaveChanges();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Could not complete Notification creation, NCR {ncr.NCRNum}");
                    }
                }
            }
        }

        // notification for when users create fill or edit a section
        public void SendNewActionNotification(Notification notification, NCR ncr, Employee employee, string action = null, string emailContent = null)
        {
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            string prevPhase = "";
            string sectionToFill = "Create";
            if (ncr.Phase == "Engineering")
            {
                prevPhase = "Quality Representative";
                sectionToFill += "Engineering";
            }
            else if (ncr.Phase == "Operations")
            {
                prevPhase = "Engineering";
                sectionToFill += "Operations";
            }
            else if (ncr.Phase == "Procurement")
            {
                prevPhase = "Operations";
                sectionToFill += "Procurement";
            }
            else if (ncr.Phase == "Reinspection")
            {
                prevPhase = "Procurement";
                sectionToFill += "Reinspection";
            }
            //Alter When final
            string urlStart = "https://haverfinal2024.azurewebsites.net";
            notification.Type = "createFill";
            notification.EmployeeID = employee.ID;
            notification.CreateOn = nowToronto;

            if (action == "rejected")
            {
                notification.Title = $"NCR Rejected";
                notification.Message = $"A new NCR with number {ncr.NCRNum} was started and linked to the previous one. <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details of new NCR</a> or <a href='/NCR/CreateQualityRepresentative/{ncr.ID}'>Fill new NCR</a>";
                notification.Type = "rejected";
                var prevNCR = _context.NCRs.FirstOrDefault(n => n.ID == ncr.PrevNCRID);
                if (prevNCR != null)
                {
                    notification.Title = $"NCR {prevNCR.NCRNum}. Rejected";
                    notification.Message = $"A new NCR with number {ncr.NCRNum} was started and linked to the previous one (NCR {prevNCR.NCRNum}). <a href='{urlStart}/NCR/Details/{prevNCR.ID}'>View details of previous NCR</a> or <a href='{urlStart}/NCR/{sectionToFill}/{ncr.ID}'>View details of new NCR</a> ";
                }
            }
            else if (action == "rejectedAgain")
            {
                notification.Title = $"NCR {ncr.NCRNum}. Rejected twice";
                notification.Message = $"This NCR was firstly rejected, a new NCR was started and rejected again, so it was closed and the status set to 'Rejected again'. <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                notification.Type = "rejectedTwice";
                var prevNCR = _context.NCRs.FirstOrDefault(n => n.ID == ncr.PrevNCRID);
                if (prevNCR != null)
                {
                    notification.Message = $"This NCR was firstly rejected, a new NCR was started and rejected again, so it was closed and the status set to 'Rejected again'. <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a> or <a href='{urlStart}/NCR/Details/{prevNCR.ID}'>View details of previous NCR</a>";
                }
                //SendNotificationEmail(employee.Email, employee.Email, notification.Title, notification.Message);
            }
            else if (action == "edited")
            {
                notification.Title = $"NCR {ncr.NCRNum}. Edited";
                notification.Message = $"The '{ncr.SectionUpdated}' section was recently edited by {ncr.UpdatedBy}. <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                notification.Type = "edited";
                //SendNotificationEmail(employee.Email, employee.Email, notification.Title, notification.Message);
            }
            else if (action == "close")
            {
                notification.Title = $"NCR {ncr.NCRNum}. Closed";
                notification.Message = $"The NCR was closed. <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                notification.Type = "close";
                //SendNotificationEmail(employee.Email, employee.Email, notification.Title, notification.Message);
            }
            else if (action == "fill")
            {
                notification.Title = $"NCR {ncr.NCRNum}. Section filled";
                notification.Message = $"The {prevPhase} section was recently filled, the NCR is currently in {ncr.Phase} phase. <a href='{urlStart}/NCR/{sectionToFill}/{ncr.ID}'>Fill NCR</a> or <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                notification.Type = "createFill";
            }
            else if (action == "create")
            {
                notification.Title = $"NCR {ncr.NCRNum}. Started";
                notification.Message = $"A new NCR was created. <a href='{urlStart}/NCR/{sectionToFill}/{ncr.ID}'>Fill NCR</a> or <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                notification.Type = "createFill";
            }

            _context.Notifications.Add(notification);

            if(employee.Email != "qualityinsp@outlook.com" && employee.Email != "engineer@outlook.com" && employee.Email != "opmanager@outlook.com" && employee.Email != "admin@outlook.com") 
            {

                if(emailContent != null)
                {
                    if(action == "create" || action == "fill")
                    {
                        emailContent += $"\n<a href='{urlStart}/NCR/{sectionToFill}/{ncr.ID}'>Fill NCR</a> or <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                    }
                    else if(action == "rejected")
                    {
                        emailContent += $" Unfortunately, upon reinspection, the materials were still found to be non-compliant and cannot be accepted.\n<a href='{urlStart}/NCR/Details/{ncr.ID}'>View details of new NCR</a>";
                    }
                    else if (action == "rejectedAgain")
                    {
                        emailContent += $" Unfortunately, upon reinspection, the materials were still found to be non-compliant and cannot be accepted.\n<a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                    }
                    else if (action == "close")
                    {
                        emailContent += $" Im pleased to inform you that upon reinspection, the materials have been accepted.\n<a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                    }
                    SendNotificationEmail(employee.Email, employee.Email, notification.Title, emailContent);
                }
                else
                {
                    SendNotificationEmail(employee.Email, employee.Email, notification.Title, notification.Message);
                }
            }

        }

        // notification reminders for users to fill the NCR form
        public void SendFillNotification(Notification notification, NCR ncr, Employee employee)
        {
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
            int LastFilled = differenceLastFilled.Days;
            string timeFrame = "";
            if (LastFilled == 1)
            {
                timeFrame = "24h";
            }
            else if (LastFilled == 2)
            {
                timeFrame = "48h";
            }
            else if (LastFilled >= 2)
            {
                timeFrame = "3 days";
            }
            string urlStart = "https://haverfinal2024.azurewebsites.net";
            string sectionToFill = $"Create{ncr.Phase}";
            if(ncr.Phase == "Quality Representative")
            {
                sectionToFill = "CreateQualityRepresentative";
            }

            notification.Type = "overdueFill";
            notification.Title = $"NCR {ncr.NCRNum}. {timeFrame} reminder";
            notification.Message = $"NCR in {ncr.Phase} phase has not been filled for <p>{timeFrame}</p>. <a href='{urlStart}/NCR/{sectionToFill}/{ncr.ID}'>Fill NCR</a> or <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
            notification.CreateOn = nowToronto;
            notification.EmployeeID = employee.ID;
            _context.Notifications.Add(notification);

            //if (employee.Email != "qualityinsp@outlook.com" && employee.Email != "engineer@outlook.com" && employee.Email != "opmanager@outlook.com" && employee.Email != "admin@outlook.com")
            //{
            //    SendNotificationEmail(employee.Email, employee.Email, notification.Title, notification.Message);
            //}
        }

        // notification for NCRs that are active for a long time
        public void SendLongActiveNotification(Notification notification, NCR ncr, Employee employee)
        {
            try
            {
                var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                int LastFilled = differenceLastFilled.Days;
                string urlStart = "https://haverfinal2024.azurewebsites.net";
                var configurationVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);
                if (configurationVariables == null)
                {
                    throw new Exception("Configurations variables cannot be null.");
                }

                string sectionToFill = $"Create{ncr.Phase}";
                if (ncr.Phase == "Quality Representative")
                {
                    sectionToFill = "CreateQualityRepresentative";
                }

                notification.Type = "overdueNCR";
                notification.Title = $"NCR {ncr.NCRNum}. {configurationVariables.OverdueNCRsNotificationDays} days reminder";
                notification.Message = $"NCR in {ncr.Phase} phase is active for <p>{configurationVariables.OverdueNCRsNotificationDays} or more days</p>. <a href='{urlStart}/NCR/{sectionToFill}/{ncr.ID}'>Fill NCR</a> or <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
                notification.CreateOn = nowToronto;
                notification.EmployeeID = employee.ID;
                _context.Notifications.Add(notification);

                if (employee.Email != "qualityinsp@outlook.com" && employee.Email != "engineer@outlook.com" && employee.Email != "opmanager@outlook.com" && employee.Email != "admin@outlook.com")
                {
                    SendNotificationEmail(employee.Email, employee.Email, notification.Title, notification.Message);
                }
            }
            catch (Exception){

            }
        }

        public void ExpecDateReturnReminder(Notification notification, NCR ncr, Employee employee)
        {
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            string urlStart = "https://haverfinal2024.azurewebsites.net";

            notification.Type = "expecDateReturn";
            notification.Title = $"NCR {ncr.NCRNum}. Date of return reminder";
            notification.Message = $"The expected date of return for this NCR is {ncr.Procurement.ExpecDateOfReturn}</p>. <a href='{urlStart}/NCR/Details/{ncr.ID}'>View details</a>";
            notification.CreateOn = nowToronto;
            notification.EmployeeID = employee.ID;
            _context.Notifications.Add(notification);

            if (employee.Email != "qualityinsp@outlook.com" && employee.Email != "engineer@outlook.com" && employee.Email != "opmanager@outlook.com" && employee.Email != "admin@outlook.com")
            {
                SendNotificationEmail(employee.Email, employee.Email, notification.Title, notification.Message);
            }
        }

        private async void SendNotificationEmail(string name, string email, string Subject, string emailContent)
        {
            if ((string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent)) && email == null)
            {
                
            }
            else
            {
                try
                {
                    await _emailSender.SendOneAsync(name, email, Subject, emailContent);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }

}


//    if (qualCounters.qual24.Count > 0)
//{
//    foreach (var ncr in qualCounters.qual24)
//    {

//        Notification notification = new Notification();

//        //Send Notification to all Admins Role
//        foreach(var admin in usersInAdminRole)
//        {
//            var employee = _context.Employees.FirstOrDefault(e => e.Email == admin.UserName);
//            SendNotification(notification, ncr, employee, "24h");
//        }
//        //Send Notification to all users in QualRep role
//        foreach (var qualityRep in usersInQualRepRole)
//        {
//            // Check if the quality rep user is not also in the Admin role
//            if (!usersInAdminRole.Any(ad => ad.UserName == qualityRep.UserName))
//            {
//                var employee = _context.Employees.FirstOrDefault(e => e.Email == qualityRep.UserName);
//                SendNotification(notification, ncr, employee, "24h");
//            }
//        }

//    }
//}