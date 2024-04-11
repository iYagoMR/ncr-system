using Haver.CustomControllers;
using Haver.Data;
using Haver.Models;
using Haver.Utilities;
using Haver.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Numerics;
using String = System.String;

namespace Haver.Controllers
{
    [Authorize]
    public class EmployeeAccountController : CognizantController
    {
        //Specialized controller just used to allow an 
        //Authenticated user to maintain their own account details.

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly HaverContext _context;

        public EmployeeAccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            HaverContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: EmployeeAccount
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Details));
        }

        // GET: EmployeeAccount/Details/5
        public async Task<IActionResult> Details()
        {

            var employee = await _context.Employees
               .Where(e => e.Email == User.Identity.Name)
               .Select(e => new EmployeeVM
               {
                   ID = e.ID,
                   FirstName = e.FirstName,
                   LastName = e.LastName,
                   Phone = e.Phone,
                   EmployeePhoto = e.EmployeePhoto,
               })
               .FirstOrDefaultAsync();
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: EmployeeAccount/Edit/5
        public async Task<IActionResult> Edit()
        {
            var employee = await _context.Employees
                .Where(e => e.Email == User.Identity.Name)
                .Include(e => e.EmployeePhoto)
                .Include(e => e.EmployeeThumbnail)
                .Select(e => new EmployeeVM
                {
                    ID = e.ID,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Phone = e.Phone,
                    EmployeePhoto = e.EmployeePhoto,
                    EmployeeThumbnail = e.EmployeeThumbnail,
                })
                .FirstOrDefaultAsync();
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: EmployeeAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string chkRemoveImage, IFormFile thePicture)
        {
            var employeeToUpdate = await _context.Employees
                .Include(e => e.EmployeePhoto)
                .Include(e => e.EmployeeThumbnail)
                .FirstOrDefaultAsync(m => m.ID == id);
            var user = await _userManager.GetUserAsync(User);

            //Note: Using TryUpdateModel we do not need to invoke the ViewModel
            //Only allow some properties to be updated
            if (await TryUpdateModelAsync<Employee>(employeeToUpdate, "", c => c.FirstName, c => c.LastName, c => c.Phone, c => c.EmployeePhoto))
            {
                try
                {
                    //If delete image selected
                    if (chkRemoveImage != null)
                    {
                        employeeToUpdate.EmployeePhoto = null;
                        employeeToUpdate.EmployeeThumbnail = null;
                    }
                    //If image was added
                    else if(chkRemoveImage == null && thePicture != null) 
                    {
                        await AddPicture(employeeToUpdate, thePicture);
                    }

                    _context.Update(employeeToUpdate);
                    await _context.SaveChangesAsync();
                    UpdateUserNameCookie(employeeToUpdate.FullName);
                    //Update user account phone number 
                    var setPhoneUserAcc = await _userManager.SetPhoneNumberAsync(user, employeeToUpdate.Phone);
                    await _signInManager.RefreshSignInAsync(user);
                    return RedirectToAction("Details");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    //Since we do not allow changing the email, we cannot introduce a duplicate
                    ModelState.AddModelError("", "Something went wrong in the database.");
                }
            }
            return View(employeeToUpdate);

        }

        private void UpdateUserNameCookie(string userName)
        {
            CookieHelper.CookieSet(HttpContext, "userName", userName, 960);
        }

        //Method to add Photos
        private async Task AddPicture(Employee employee, IFormFile thePicture)
        {
            //Get the picture and save it with the Customer (2 sizes)
            if (thePicture != null)
            {
                string mimeType = thePicture.ContentType;
                long fileLength = thePicture.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("image"))
                    {
                        using var memoryStream = new MemoryStream();
                        await thePicture.CopyToAsync(memoryStream);
                        var pictureArray = memoryStream.ToArray();//Gives us the Byte[]

                        //Check if we are replacing or creating new
                        if (employee.EmployeePhoto != null)
                        {
                            //We already have pictures so just replace the Byte[]
                            employee.EmployeePhoto.Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600);

                            //Get the Thumbnail so we can update it.  Remember we didn't include it
                            employee.EmployeeThumbnail = _context.EmployeeThumbnails.Where(p => p.EmployeeID == employee.ID).FirstOrDefault();
                            employee.EmployeeThumbnail.Content = ResizeImage.shrinkImageWebp(pictureArray, 75, 90);
                        }
                        else //No pictures saved so start new
                        {
                            employee.EmployeePhoto = new EmployeePhoto
                            {
                                Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600),
                                MimeType = "image/webp"
                            };
                            employee.EmployeeThumbnail = new EmployeeThumbnail
                            {
                                Content = ResizeImage.shrinkImageWebp(pictureArray, 75, 90),
                                MimeType = "image/webp"
                            };
                        }
                    }
                }
            }
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }

}

