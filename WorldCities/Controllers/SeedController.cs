using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorldCities.Data;
using WorldCities.Data.Models;
using OfficeOpenXml;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;

namespace WorldCities.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        public SeedController(ApplicationDbContext context, IWebHostEnvironment env, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _env = env;
        }
        [HttpGet]
        public async Task<ActionResult> Import()
        {
            var path = Path.Combine(_env.ContentRootPath, String.Format("Data/Source/worldcities.xlsx"));
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var ep = new ExcelPackage(stream))
                {
                    var ws = ep.Workbook.Worksheets[0];
                    var nCountries = 0;
                    var nCities = 0;
                    var lstCountries = _context.Countries.ToList();
                    for (int i = 2; i <= ws.Dimension.End.Row; i++)
                    {
                        var row = ws.Cells[i, 1, i, ws.Dimension.End.Column];
                        var name = row[i, 5].GetValue<string>();
                        if(lstCountries.Where(c => c.Name == name).Count() == 0)
                        {
                            var country = new Country();
                            country.Name = name;
                            country.ISO2 = row[i, 6].GetValue<string>();
                            country.ISO3 = row[i, 7].GetValue<string>();
                            _context.Countries.Add(country);
                            await _context.SaveChangesAsync();
                            lstCountries.Add(country);
                            nCountries++;
                        }
                    }
                    for(int i = 2; i < ws.Dimension.End.Row; i++)
                    {
                        var row = ws.Cells[i, 1, i, ws.Dimension.End.Column];
                        var city = new City();
                        city.Name = row[i, 1].GetValue<string>();
                        city.Name_ASCII = row[i, 2].GetValue<string>();
                        city.Lat = row[i, 3].GetValue<decimal>();
                        city.Lon = row[i, 4].GetValue<decimal>();
                        var countryName = row[i, 5].GetValue<string>();
                        var country = lstCountries.FirstOrDefault(c => c.Name == countryName);
                        city.CountryId = country.Id;
                        _context.Cities.Add(city);
                        await _context.SaveChangesAsync();
                        nCities++;
                    }
                    return new JsonResult(new
                    {
                        Cities = nCities,
                        Countries = nCountries
                    });
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> CreateDefaultUsers()
        {
            // setup the default role names
            string role_RegisteredUser = "RegisteredUser";
            string role_Administrator = "Administrator";

            // create the default roles (if they don't exist yet)
            if (await _roleManager.FindByNameAsync(role_RegisteredUser) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role_RegisteredUser));
            }
            if(await _roleManager.FindByNameAsync(role_Administrator) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role_Administrator));
            }

            // create a list to track the newly added users
            var addedUserList = new List<ApplicationUser>();

            // check if the admin user already exists
            var email_Admin = "admin@email.com";
            if(await _userManager.FindByNameAsync(email_Admin) == null)
            {
                // create the new user
                var user_Admin = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = email_Admin,
                    Email = email_Admin
                };

                // insert the admin user into the db
                await _userManager.CreateAsync(user_Admin, "MySecr3t$");

                // assign the "RegisteredUser" and "Administrator" roles
                await _userManager.AddToRoleAsync(user_Admin, role_Administrator);
                await _userManager.AddToRoleAsync(user_Admin, role_RegisteredUser);

                // confirm the email and remove lockout
                user_Admin.EmailConfirmed = true;
                user_Admin.LockoutEnabled = false;

                // add the admin user to the added users list
                addedUserList.Add(user_Admin);
            }
            var email_User = "user@email.com";
            if (await _userManager.FindByNameAsync(email_User) == null)
            {
                // create the new user
                var user_User = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = email_User,
                    Email = email_User
                };

                // insert the standard user into the db
                await _userManager.CreateAsync(user_User, "MySecr3t$");

                // assign the "RegisteredUser" role
                await _userManager.AddToRoleAsync(user_User, role_RegisteredUser);

                // confirm the email and remove lockout
                user_User.EmailConfirmed = true;
                user_User.LockoutEnabled = false;

                // add the standard user to the added users list
                addedUserList.Add(user_User);
            }
            // iff we added at least one user, save changes
            if(addedUserList.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new
            {
                Count = addedUserList.Count,
                Users = addedUserList
            });
        }
    }
}
