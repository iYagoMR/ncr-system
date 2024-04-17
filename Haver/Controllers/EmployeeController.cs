using Haver.CustomControllers;
using Haver.Data;
using Haver.Models;
using Haver.Utilities;
using Haver.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Text;
using System.Text.Encodings.Web;

namespace Haver.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : CognizantController
    {
        private readonly HaverContext _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly IEmailSender _iEmailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeeController(HaverContext context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender, IEmailSender iEmailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
            _iEmailSender = iEmailSender;
        }
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Select(e => new EmployeeAdminVM
                {
                    Email = e.Email,
                    Active = e.Active,
                    ID = e.ID,
                    FirstName = e.FirstName,
                    EmployeeThumbnail = e.EmployeeThumbnail,
                    LastName = e.LastName,
                    Phone = e.Phone,
                    RowVersion = e.RowVersion,
                }).ToListAsync();

            foreach (var e in employees)
            {
                var user = await _userManager.FindByEmailAsync(e.Email);
                if (user != null)
                {
                    e.UserRoles = (List<string>)await _userManager.GetRolesAsync(user);
                }
            };

            return View(employees);
        }
        // GET: Employee/Create
        public IActionResult Create()
        {
            EmployeeAdminVM employee = new EmployeeAdminVM();
            PopulateAssignedRoleData(employee);
            return View(employee);
        }

        // POST: Employee/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Phone," +
            "Email, Notification")] Employee employee, string[] selectedRoles)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(employee);
                    await _context.SaveChangesAsync();

                    InsertIdentityUser(employee.Email, selectedRoles, employee.Phone);

                    //Send Email to new Employee - commented out till email configured
                    await InviteUserToResetPassword(employee, null);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            //We are here because something went wrong and need to redisplay
            EmployeeAdminVM employeeAdminVM = new EmployeeAdminVM
            {
                Email = employee.Email,
                Active = employee.Active,
                ID = employee.ID,
                EmployeeThumbnail = employee.EmployeeThumbnail,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Phone = employee.Phone
            };
            foreach (var role in selectedRoles)
            {
                employeeAdminVM.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(employeeAdminVM);
            return View(employeeAdminVM);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Where(e => e.ID == id)
                .Select(e => new EmployeeAdminVM
                {
                    Email = e.Email,
                    Active = e.Active,
                    ID = e.ID,
                    EmployeeThumbnail = e.EmployeeThumbnail,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Phone = e.Phone,
                    RowVersion = e.RowVersion
                }).FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound();
            }

            //Get the user from the Identity system
            var user = await _userManager.FindByEmailAsync(employee.Email);
            if (user != null)
            {
                //Add the current roles
                var r = await _userManager.GetRolesAsync(user);
                employee.UserRoles = (List<string>)r;
            }
            PopulateAssignedRoleData(employee);

            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, bool Active, string[] selectedRoles, Byte[] RowVersion)
        {
            var employeeToUpdate = await _context.Employees
                .FirstOrDefaultAsync(m => m.ID == id);
            if (employeeToUpdate == null)
            {
                return NotFound();
            }

            _context.Entry(employeeToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == employeeToUpdate.Email);

            //Note the current Email and Active Status
            bool ActiveStatus = employeeToUpdate.Active;
            string databaseEmail = employeeToUpdate.Email;
            string databasePhone = employeeToUpdate.Phone;


            if (await TryUpdateModelAsync<Employee>(employeeToUpdate, "",
                e => e.FirstName, e => e.LastName, e => e.Phone, e => e.Email, e => e.Active))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    //Save successful so go on to related changes

                    //Check for changes in the Active state
                    if (employeeToUpdate.Active == false && ActiveStatus == true)
                    {
                        //Deactivating them so delete the IdentityUser
                        //This deletes the user's login from the security system
                        await DeleteIdentityUser(employeeToUpdate.Email);

                    }
                    else if (employeeToUpdate.Active == true && ActiveStatus == false)
                    {
                        //You reactivating the user, create them and
                        //give them the selected roles
                        InsertIdentityUser(employeeToUpdate.Email, selectedRoles, employeeToUpdate.Phone);
                    }
                    else if (employeeToUpdate.Active == true && ActiveStatus == true)
                    {
                        //No change to Active status so check for a change in Email
                        //If you Changed the email, Delete the old login and create a new one
                        //with the selected roles
                        if (employeeToUpdate.Email != databaseEmail)
                        {
                            //Add the new login with the selected roles
                            InsertIdentityUser(employeeToUpdate.Email, selectedRoles);

                            //This deletes the user's old login from the security system
                            await DeleteIdentityUser(databaseEmail);
                        }
                        //If you Changed the phone, update the number in the user account
                        if (employeeToUpdate.Phone != databasePhone)
                        {
                            var setPhoneUserAcc = await _userManager.SetPhoneNumberAsync(user, employeeToUpdate.Phone);
                        }
                        else
                        {
                            //Finially, Still Active and no change to Email so just Update
                            await UpdateUserRoles(selectedRoles, employeeToUpdate.Email);
                        }
                    }

                    TempData["SuccessAlert"] = "Employee was successfully edited.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeToUpdate.ID))
                    {
                        TempData["ErrorAlert"] = "This Employee was deleted";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorAlert"] = "This Employee was edited or modified by another user, try again and if problems persist see your system administrator";
                        return RedirectToAction("Index");
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            //We are here because something went wrong and need to redisplay
            EmployeeAdminVM employeeAdminVM = new EmployeeAdminVM
            {
                Email = employeeToUpdate.Email,
                Active = employeeToUpdate.Active,
                ID = employeeToUpdate.ID,
                FirstName = employeeToUpdate.FirstName,
                LastName = employeeToUpdate.LastName,
                Phone = employeeToUpdate.Phone,
                RowVersion = employeeToUpdate.RowVersion,
            };
            foreach (var role in selectedRoles)
            {
                employeeAdminVM.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(employeeAdminVM);
            return View(employeeAdminVM);
        }

        private void PopulateAssignedRoleData(EmployeeAdminVM employee)
        {//Prepare checkboxes for all Roles
            var allRoles = _identityContext.Roles;
            var currentRoles = employee.UserRoles;
            var viewModel = new List<RoleVM>();
            foreach (var r in allRoles)
            {
                viewModel.Add(new RoleVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Assigned = currentRoles.Contains(r.Name)
                });
            }
            ViewBag.Roles = viewModel;
        }

        private async Task UpdateUserRoles(string[] selectedRoles, string Email)
        {
            var _user = await _userManager.FindByEmailAsync(Email);//IdentityUser
            if (_user != null)
            {
                var UserRoles = (List<string>)await _userManager.GetRolesAsync(_user);//Current roles user is in

                if (selectedRoles == null)
                {
                    //No roles selected so just remove any currently assigned
                    foreach (var r in UserRoles)
                    {
                        await _userManager.RemoveFromRoleAsync(_user, r);
                    }
                }
                else
                {
                    //At least one role checked so loop through all the roles
                    //and add or remove as required

                    //We need to do this next line because foreach loops don't always work well
                    //for data returned by EF when working async.  Pulling it into an IList<>
                    //first means we can safely loop over the colleciton making async calls and avoid
                    //the error 'New transaction is not allowed because there are other threads running in the session'
                    IList<IdentityRole> allRoles = _identityContext.Roles.ToList<IdentityRole>();

                    foreach (var r in allRoles)
                    {
                        if (selectedRoles.Contains(r.Name))
                        {
                            if (!UserRoles.Contains(r.Name))
                            {
                                await _userManager.AddToRoleAsync(_user, r.Name);
                            }
                        }
                        else
                        {
                            if (UserRoles.Contains(r.Name))
                            {
                                await _userManager.RemoveFromRoleAsync(_user, r.Name);
                            }
                        }
                    }
                }
            }
        }

        private void InsertIdentityUser(string Email, string[] selectedRoles, string Phone = null)
        {
            //Create the IdentityUser in the IdentitySystem
            //Note: this is similar to what we did in ApplicationSeedData
            if (_userManager.FindByEmailAsync(Email).Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = Email,
                    Email = Email,
                    PhoneNumber = Phone,
                    EmailConfirmed = true //since we are creating it!
                };
                //Create a random password with a default 8 characters
                string password = MakePassword.Generate();
                IdentityResult result = _userManager.CreateAsync(user, password).Result;

                if (result.Succeeded)
                {
                    foreach (string role in selectedRoles)
                    {
                        _userManager.AddToRoleAsync(user, role).Wait();
                    }
                }
            }
            else
            {
                TempData["messageGreen"] = "The Account for " + Email + " was already in the system.";
            }
        }

        private async Task DeleteIdentityUser(string Email)
        {
            var userToDelete = await _identityContext.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();
            if (userToDelete != null)
            {
                _identityContext.Users.Remove(userToDelete);
                await _identityContext.SaveChangesAsync();
            }
        }

        private async Task InviteUserToResetPassword(Employee employee, string message)
        {
            try
            {
                //Send reset password link to new employee
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(employee.Email);

                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ResetPassword",
                        pageHandler: null,
                        values: new { area = "Identity", code, email = user.Email},
                        protocol: Request.Scheme);

                    await _iEmailSender.SendEmailAsync(
                        employee.Email,
                        "Set Password",
                        $"Hello {employee.FullName} please set your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                
                    TempData["SuccessAlert"] = "Invitation email sent to " + employee.FullName + " at " + employee.Email;
                }
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Could not send Invitation email to " + employee.FullName + " at " + employee.Email;
            }
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }
}

