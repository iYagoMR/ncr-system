using Haver.Data;
using Haver.Models;
using Haver.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haver.Utilities
{
    public class AutoArchive
    {
        private readonly HaverContext _context;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public AutoArchive(HaverContext context, IMyEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public void ArchiveOldNCRs()
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                AutoArchive authoArchive = new AutoArchive(_context, _emailSender, _userManager);
                var configurationsVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);

                // Calculate the date five years ago
                var years = 5;
                if (configurationsVariables != null)
                {
                    years = configurationsVariables.ArchiveNCRsYears;
                }
                var timeFrame = now.AddYears(-years);

                // Get old inactive NCRs
                var ncrs = _context.NCRs
                    .Where(ncr => !ncr.IsNCRArchived)
                    .Where(ncr => ncr.Status != "Active")
                    .Where(ncr => ncr.CreatedOn.Value.Date <= timeFrame.Date)
                    .ToList();

                if (ncrs != null && ncrs.Any())
                {
                    foreach (var ncr in ncrs)
                    {
                        try
                        {
                            ncr.IsNCRArchived = true;

                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
