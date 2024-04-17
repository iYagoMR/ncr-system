using Haver.Data;
using Haver.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Haver.Utilities
{
    public class CalculateNCRTimeCounters
    {
        private readonly HaverContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private List<NCR> activeNCRs;
        public CalculateNCRTimeCounters(HaverContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            activeNCRs = _context.NCRs.Where(ncr => ncr.Status == "Active")
                          .Include(ncr => ncr.QualityRepresentative)
                          .Include(ncr => ncr.Engineering)
                          .Include(ncr => ncr.Operations)
                          .Include(ncr => ncr.Procurement)
                          .Where(ncr => !ncr.IsNCRArchived)
                          .ToList();
        }

        public (List<NCR> qual24, List<NCR> qual48, List<NCR> qual5, List<NCR> qualTotal, List<NCR>lgActive) QualCounters()
        {
            var QualNCRs = activeNCRs.Where(ncr => ncr.Phase == "Quality Representative").ToList();

            List<NCR> qualTotal = new List<NCR>();
            List<NCR> qual24 = new List<NCR>();
            List<NCR> qual48 = new List<NCR>();
            List<NCR> qual5 = new List<NCR>();
            List<NCR> lgActive = new List<NCR>();
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            var configurationVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);
            if (configurationVariables == null)
            {
                throw new Exception("Configurations variables cannot be null.");
            }

            foreach (var ncr in QualNCRs)
            {
                try 
                {
                    TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                    int LastFilled = differenceLastFilled.Days;
                    qualTotal.Add(ncr);

                    //Count the amount of days at each category
                    if (LastFilled == 1)
                    {
                        qual24.Add(ncr);
                    }
                    else if (LastFilled == 2)
                    {
                        qual48.Add(ncr);
                    }
                    else if (LastFilled > 2)
                    {
                        qual5.Add(ncr);
                    }
                    else if (LastFilled >= configurationVariables.OverdueNCRsNotificationDays)
                    {
                        lgActive.Add(ncr);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to count NCR");
                }
            }

            return (qual24, qual48, qual5, qualTotal, lgActive);
        }
        public (List<NCR> eng24, List<NCR> eng48, List<NCR> eng5, List<NCR> engTotal, List<NCR> lgActive) EngCounters()
        {
            var EngNCRs = activeNCRs.Where(ncr => ncr.Phase == "Engineering").ToList();

            List<NCR> engTotal = new List<NCR>();
            List<NCR> eng24 = new List<NCR>();
            List<NCR> eng48 = new List<NCR>();
            List<NCR> eng5 = new List<NCR>();
            List<NCR> lgActive = new List<NCR>();
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            foreach (var ncr in EngNCRs)
            {
                try 
                {
                    int LastFilled = 0;
                    var configurationVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);
                    if (configurationVariables == null)
                    {
                        throw new Exception("Configurations variables cannot be null.");
                    }

                    // Converting DateOnly to DateTime by providing Time Info
                    if (ncr.QualityRepresentative != null)
                    {
                        TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                        LastFilled = differenceLastFilled.Days;
                        engTotal.Add(ncr);
                    }

                    //Count the amount of days at each category
                    if (LastFilled == 1)
                    {
                        eng24.Add(ncr);
                    }
                    else if (LastFilled == 2)
                    {
                        eng48.Add(ncr);
                    }
                    else if (LastFilled > 2)
                    {
                        eng5.Add(ncr);
                    }
                    else if (LastFilled >= configurationVariables.OverdueNCRsNotificationDays)
                    {
                        lgActive.Add(ncr);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to count NCR");
                }
            }

            return (eng24, eng48, eng5, engTotal, lgActive);
        }

        public (List<NCR> oper24, List<NCR> oper48, List<NCR> oper5, List<NCR> operTotal, List<NCR> lgActive) OperCounters()
        {
            var OperNCRs = activeNCRs.Where(ncr => ncr.Phase == "Operations").ToList();

            List<NCR> operTotal = new List<NCR>();
            List<NCR> oper24 = new List<NCR>();
            List<NCR> oper48 = new List<NCR>();
            List<NCR> oper5 = new List<NCR>();
            List<NCR> lgActive = new List<NCR>();
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            foreach (var ncr in OperNCRs)
            {
                try
                {
                    int LastFilled = 0;
                    var configurationVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);
                    if (configurationVariables == null)
                    {
                        throw new Exception("Configurations variables cannot be null.");
                    }

                    // Converting DateOnly to DateTime by providing Time Info
                    if (ncr.Engineering != null && ncr.QualityRepresentative.ConfirmingEng == false)
                    {
                        TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.Engineering.CreatedOn);
                        LastFilled = differenceLastFilled.Days;
                        operTotal.Add(ncr);
                    }
                    else if (ncr.Engineering == null && ncr.QualityRepresentative.ConfirmingEng == true)
                    {
                        TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                        LastFilled = differenceLastFilled.Days;
                        operTotal.Add(ncr);
                    }

                    //Count the amount of days at each category
                    if (LastFilled == 1)
                    {
                        oper24.Add(ncr);
                    }
                    else if (LastFilled == 2)
                    {
                        oper48.Add(ncr);
                    }
                    else if (LastFilled > 2)
                    {
                        oper5.Add(ncr);
                    }
                    else if (LastFilled >= configurationVariables.OverdueNCRsNotificationDays)
                    {
                        lgActive.Add(ncr);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to count NCR");
                }
            }

            return (oper24, oper48, oper5, operTotal, lgActive);
        }

        public (List<NCR> proc24, List<NCR> proc48, List<NCR> proc5, List<NCR> procTotal, List<NCR> lgActive) ProcCounters()
        {
            var ProcNCRs = activeNCRs.Where(ncr => ncr.Phase == "Procurement").ToList();

            List<NCR> procTotal = new List<NCR>();
            List<NCR> proc24 = new List<NCR>();
            List<NCR> proc48 = new List<NCR>();
            List<NCR> proc5 = new List<NCR>();
            List<NCR> lgActive = new List<NCR>();
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            foreach (var ncr in ProcNCRs)
            {
                try
                {
                    int LastFilled = 0;
                    var configurationVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);
                    if (configurationVariables == null)
                    {
                        throw new Exception("Configurations variables cannot be null.");
                    }

                    // Converting DateOnly to DateTime by providing Time Info
                    if (ncr.Operations != null)
                    {
                        TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.Operations.CreatedOn);
                        LastFilled = differenceLastFilled.Days;
                        procTotal.Add(ncr);
                    }

                    //Count the amount of days at each category
                    if (LastFilled == 1)
                    {
                        proc24.Add(ncr);
                    }
                    else if (LastFilled == 2)
                    {
                        proc48.Add(ncr);
                    }
                    else if (LastFilled > 2)
                    {
                        proc5.Add(ncr);
                    }
                    else if (LastFilled >= configurationVariables.OverdueNCRsNotificationDays)
                    {
                        lgActive.Add(ncr);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to count NCR");
                }
            }

            return (proc24, proc48, proc5, procTotal, lgActive);
        }

        public (List<NCR> reinsp24, List<NCR> reinsp48, List<NCR> reinsp5, List<NCR> reinspTotal, List<NCR> lgActive) ReinspCounters()
        {
            var ReinspNCRs = activeNCRs.Where(ncr => ncr.Phase == "Reinspection").ToList();

            List<NCR> reinspTotal = new List<NCR>();
            List<NCR> reinsp24 = new List<NCR>();
            List<NCR> reinsp48 = new List<NCR>();
            List<NCR> reinsp5 = new List<NCR>();
            List<NCR> lgActive = new List<NCR>();
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            foreach (var ncr in ReinspNCRs)
            {
                try
                {
                    int LastFilled = 0;
                    var configurationVariables = _context.ConfigurationVariables.FirstOrDefault(config => config.ID == 1);
                    if (configurationVariables == null)
                    {
                        throw new Exception("Configurations variables cannot be null.");
                    }

                    // Converting DateOnly to DateTime by providing Time Info
                    if (ncr.Procurement != null)
                    {
                        TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.Procurement.CreatedOn);
                        LastFilled = differenceLastFilled.Days;
                        reinspTotal.Add(ncr);
                    }

                    //Count the amount of days at each category
                    if (LastFilled == 1)
                    {
                        reinsp24.Add(ncr);
                    }
                    else if (LastFilled == 2)
                    {
                        reinsp48.Add(ncr);
                    }
                    else if (LastFilled > 2)
                    {
                        reinsp5.Add(ncr);
                    }
                    else if (LastFilled >= configurationVariables.OverdueNCRsNotificationDays)
                    {
                        lgActive.Add(ncr);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to count NCR");
                }
            }

            return (reinsp24, reinsp48, reinsp5, reinspTotal, lgActive);
        }
    }
}
