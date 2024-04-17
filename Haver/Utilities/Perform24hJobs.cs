using Haver.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Quartz;
using Microsoft.Identity.Client;
using Haver.Models;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using System.Reflection.Metadata;
using System.Linq.Expressions;

namespace Haver.Utilities
{
    [DisallowConcurrentExecution]
    public class Perform24hJobs : IJob
    {
        private readonly IMyEmailSender _emailSender;
        private readonly HaverContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public Perform24hJobs(HaverContext context, IMyEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            NotificationGenerator notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);
            AutoArchive authoArchive = new AutoArchive(_context, _emailSender, _userManager);

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            var configurationsVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);
            var tomorrowStart = now.Date.AddDays(1);

            try
            {
                if(configurationsVariables == null)
                {
                    throw new("Configurations variables cannot be null");
                }

                //Notifications
                try
                {
                    if (configurationsVariables != null)
                    {
                        if (configurationsVariables.DateToRunNotificationJob <= now)
                        {
                            notificationGenerator.NotifyUser();
                            _context.SaveChanges();
                            //Update the date to next day
                            configurationsVariables.DateToRunNotificationJob = tomorrowStart;
                            _context.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Notification Job was not succesfully performed.");
                }
                //AutoArchive
                try
                {
                    if (configurationsVariables != null)
                    {
                        if (configurationsVariables.DateToRunArchiveJob <= now)
                        {
                            authoArchive.ArchiveOldNCRs();
                            _context.SaveChanges();
                            //Update the date to next day
                            configurationsVariables.DateToRunArchiveJob = tomorrowStart;
                            _context.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Archiving Job was not succesfully performed.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
