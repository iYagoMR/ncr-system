using Haver.Data;
using Haver.Models;
using Haver.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Haver.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly HaverContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationController(HaverContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                //Get user's notifications
                var notification = _context.Notifications.FirstOrDefault(n => n.ID == id);

                if (notification == null)
                {
                    return BadRequest();
                }

                _context.Notifications.Remove(notification);

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public IActionResult LoadNotifications()
        {
            try
            {
                //Get user's notifications
                var notifications = _context.Notifications
                    .Where(n => n.Employee.Email == User.Identity.Name)
                    .OrderByDescending(n => n.CreateOn)
                    .ToList();

                if (notifications.Count() < 0)
                {
                    return BadRequest();
                }

                var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                List<NotificationVM> notificationsList = new List<NotificationVM>();

                foreach (var notification in notifications)
                {
                    NotificationVM notificationVM = new NotificationVM();

                    notificationVM.ID = notification.ID;
                    notificationVM.Type = notification.Type;
                    notificationVM.Title = notification.Title;
                    notificationVM.Message = notification.Message;
                    notificationVM.Date = notification.CreateOn.ToString("MM-dd-yyyy HH:mm:ss");

                    notificationsList.Add(notificationVM);
                }

                return Ok(notificationsList);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public async Task<IActionResult> HandleNotificationView()
        {
            try
            {
                //Get user's notifications
                var notifications = _context.Notifications
                    .Where(n => n.Employee.Email == User.Identity.Name)
                    .Where(n => n.wasSeen == false)
                    .ToList();

                if (notifications.Count() < 0)
                {
                    return BadRequest();
                }

                // Update the 'wasSeen' property for all notifications
                foreach (var notification in notifications)
                {
                    notification.wasSeen = true;
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public int LoadNotificationCounter()
        {
            try
            {
                //Get user's notifications
                var notifications = _context.Notifications
                    .Where(n => n.Employee.Email == User.Identity.Name)
                    .Where(n => n.wasSeen == false)
                    .ToList();

                if (notifications.Count() < 0)
                {
                    return 0;
                }

                int counter = notifications.Count();

                return counter;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
