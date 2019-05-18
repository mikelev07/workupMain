using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HelpMe.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace HelpMe.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        private int pageSize = 3;

        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager)
        {
            UserManager = userManager;

        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        // GET: ApplicationUser
        public async Task<ActionResult> Index(int? id)
        {
            int page = id ?? 0;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_UsersPage", GetUsersPage(page));
            }

            var roles = RoleManager.Roles.ToList();
            var Users = await db.Users.Include(m => m.TaskCategories).OrderByDescending(m => (double)(m.PositiveThumbs - m.NegativeThumbs) / (m.PositiveThumbs + m.NegativeThumbs)).
                    OrderByDescending(m => (m.PositiveThumbs + m.NegativeThumbs)).ToListAsync();

            var usersWithRoles = (from user in Users.ToList()
                                  select new
                                  {
                                      user.Id,
                                      Username = user.UserName,
                                      ImagePath = user.ImagePath,
                                      user.Email,
                                      user.TaskCategories,
                                      user.PositiveThumbs,
                                      user.NegativeThumbs,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in roles on userRole.RoleId equals role.Id
                                                   select role.Name).ToList()
                                  }).ToList().Select(p => new UserViewModel()
                                  {
                                      Id = p.Id,
                                      Username = p.Username,
                                      Email = p.Email,
                                      ImagePath = p.ImagePath ?? "~/Content/Custom/images/user-avatar-big-01.jpg",
                                      Role = string.Join(",", p.RoleNames),
                                      TaskCategories = p.TaskCategories,
                                      PositiveThumbs = p.PositiveThumbs,
                                      NegativeThumbs = p.NegativeThumbs
                                  });

            ViewBag.Tasks = new SelectList(db.TaskCategories, "Id", "Name");
            //ViewBag.Skills = new SelectList(db.Skills,"Id","Name");

            return View(GetUsersPage(page));
        }

        public IEnumerable<UserViewModel> GetUsersPage(int page = 1, int sortId = 1)
        {
            var usersToSkip = page * pageSize;
            var users = db.Users.OrderByDescending(m => (double)(m.PositiveThumbs - m.NegativeThumbs) / (m.PositiveThumbs + m.NegativeThumbs)).
                    OrderByDescending(m => (m.PositiveThumbs + m.NegativeThumbs));
            var roles = RoleManager.Roles.ToList();
            var usersWithRoles = (from user in users.ToList()
                                  select new
                                  {
                                      user.Id,
                                      Username = user.UserName,
                                      ImagePath = user.ImagePath,
                                      user.Email,
                                      user.TaskCategories,
                                      user.PositiveThumbs,
                                      user.NegativeThumbs,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in roles on userRole.RoleId equals role.Id
                                                   select role.Name).ToList()
                                  }).ToList().Select(p => new UserViewModel()
                                  {
                                      Id = p.Id,
                                      Username = p.Username,
                                      Email = p.Email,
                                      ImagePath = p.ImagePath ?? "~/Content/Custom/images/user-avatar-big-01.jpg",
                                      Role = string.Join(",", p.RoleNames),
                                      TaskCategories = p.TaskCategories,
                                      PositiveThumbs = p.PositiveThumbs,
                                      NegativeThumbs = p.NegativeThumbs
                                  });
            return usersWithRoles.OrderBy(t => t.Id).Skip(usersToSkip).Take(pageSize).ToList();
        }

        public ActionResult Filtrate(string name, int? taskId, int? skillId, int? worksCount, int? sortId)
        {
            //var roles = RoleManager.Roles.ToList();
            IQueryable<User> users = null;
            //int worksUsersCount = 0;
            if (worksCount != null)
            {
                users = db.Users.Where(m => (m.PositiveThumbs + m.NegativeThumbs) >= worksCount);
                //worksUsersCount = users.Count();
            }
            if (name != "")
            {
                users = users ?? db.Users;
                users = users.Where(b => b.UserName.StartsWith(name));
                //worksUsersCount = users.Count();
            }
            if (taskId != 0 && taskId != null)
            {
                users = users ?? db.Users;
                users = users.Where(m => m.TaskCategories.Any(b => b.Id == taskId));
                //worksUsersCount = users.Count();

                if (skillId != 0 && skillId != null)
                {
                    users = users.Where(m => m.Skills.Any(b => b.Id == skillId));
                    //worksUsersCount = users.Count();
                }
            }
            if (sortId != null)
            {
                users = users ?? db.Users;
                users = SortUsers(users);
                //worksUsersCount = users.Count();
            }

            var usersWithRoles = (from user in users
                                  select new
                                  {
                                      user.Id,
                                      Username = user.UserName,
                                      ImagePath = user.ImagePath,
                                      user.Email,
                                      user.TaskCategories,
                                      user.PositiveThumbs,
                                      user.NegativeThumbs
                                  }).ToList().Select(p => new UserViewModel()
                                  {
                                      Id = p.Id,
                                      Username = p.Username,
                                      Email = p.Email,
                                      ImagePath = p.ImagePath ?? "~/Content/Custom/images/user-avatar-big-01.jpg",
                                      TaskCategories = p.TaskCategories,
                                      PositiveThumbs = p.PositiveThumbs,
                                      NegativeThumbs = p.NegativeThumbs
                                  });
            return PartialView(usersWithRoles);
        }

        private IQueryable<User> SortUsers(IQueryable<User> users, int sortId = 1)
        {
            //by rating 
            if (sortId == 1)
            {
                users = users.OrderByDescending(m => (double)(m.PositiveThumbs - m.NegativeThumbs) / (m.PositiveThumbs + m.NegativeThumbs)).
                    OrderByDescending(m => (m.PositiveThumbs + m.NegativeThumbs));
            }
            return users;
        }
        private List<User> SortUsers(List<User> users, int sortId = 1)
        {
            //by rating 
            if (sortId == 1)
            {
                users = users.OrderByDescending(m => (double)(m.PositiveThumbs - m.NegativeThumbs) / (m.PositiveThumbs + m.NegativeThumbs)).
                    OrderByDescending(m => (m.PositiveThumbs + m.NegativeThumbs)).ToList();
            }
            return users;
        }

        public JsonResult GetSkills(int id)
        {
            var skills = db.Skills.Where(m => m.TaskCategoryId == id).ToList();
            return Json(skills, JsonRequestBehavior.AllowGet);
        }


        // GET: User/Details/5
        public async Task<ActionResult> Details(string userName)
        {
            if (userName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await db.Users.Include(u => u.Reviews).SingleAsync(u => u.UserName == userName);

            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        /*
           // POST: ApplicationUser/Edit/5
           // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
           // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
           [HttpPost]
           [ValidateAntiForgeryToken]
           public async Task<ActionResult> Edit([Bind(Include = "Id,Age,Description,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
           {
               if (ModelState.IsValid)
               {
                   db.Entry(applicationUser).State = EntityState.Modified;
                   await db.SaveChangesAsync();
                   return RedirectToAction("Index");
               }
               return View(applicationUser);
           }

           // GET: ApplicationUser/Delete/5
           /*
           public async Task<ActionResult> Delete(string id)
           {
               if (id == null)
               {
                   return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
               }
              // ApplicationUser applicationUser = await db.Users.FindAsync(id);
             //  if (applicationUser == null)
               {
                   return HttpNotFound();
               }
              // return View(applicationUser);
           }

           // POST: ApplicationUser/Delete/5
           [HttpPost, ActionName("Delete")]
           [ValidateAntiForgeryToken]
           public async Task<ActionResult> DeleteConfirmed(string id)
           {
              // ApplicationUser applicationUser = await db.Users.FindAsync(id);
             //  db.Users.Remove(applicationUser);
               await db.SaveChangesAsync();
               return RedirectToAction("Index");
           } */

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
