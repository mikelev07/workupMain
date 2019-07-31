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
using PagedList;

namespace HelpMe.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        private const int pageSize = 3;
        private const int customsPageSize = 3;    //for Details.cshtml in Works History tab
        private const int reviewsPageSize = 3;    //for Details.cshtml in Reviews tab

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
        public ActionResult Index(int? id, string name, int? taskId, int? skillId, bool? isOnline, bool? isNotBusy, int worksCount=0, int sortId=1)
        {
            IQueryable<User> users = null;

            //int worksUsersCount = 0;
            //worksUsersCount = users.Count();

            if (!String.IsNullOrEmpty(name))
            {
                users = (users ?? db.Users).Where(b => b.UserName.StartsWith(name));
                //worksUsersCount = users.Count();
            }
            if (taskId != 0 && taskId != null)
            {
                users = (users ?? db.Users).Where(m => m.TaskCategories.Any(b => b.Id == taskId));
                //worksUsersCount = users.Count();

                if (skillId != 0 && skillId != null)
                {
                    users = users.Where(m => m.Skills.Any(b => b.Id == skillId));
                    //worksUsersCount = users.Count();
                }
            }

            if(isOnline==true)
            {
                users = (users ?? db.Users).Where(m => m.IsOnline == isOnline);
            }

            if (isNotBusy == true)
            {
                users = (users ?? db.Users).Where(m => m.IsNotBusy == isNotBusy);
            }

            users = (users ?? db.Users).Where(m=>(m.PositiveThumbs+m.NegativeThumbs)>=worksCount);
            //worksUsersCount = users.Count();

            var roles = RoleManager.Roles.ToList();
            var usersWithRoles = (from user in users
                                  select new
                                  {
                                      user.Id,
                                      Username = user.UserName,
                                      ImagePath = user.ImagePath,
                                      user.RegistrationDate,
                                      user.IsOnline,
                                      user.IsNotBusy,
                                      user.Email,
                                      user.TaskCategories,
                                      user.Reviews,
                                      user.Customs,
                                      user.PositiveThumbs,
                                      user.NegativeThumbs
                                  }).ToList().Select(p => new UserViewModel()
                                  {
                                      Id = p.Id,
                                      Username = p.Username,
                                      Email = p.Email,
                                      ImagePath = p.ImagePath ?? "~/Content/Custom/images/user-avatar-big-01.jpg",
                                      RegistrationDate = p.RegistrationDate,
                                      IsOnline = p.IsOnline,
                                      IsNotBusy = p.IsNotBusy,
                                      TaskCategories = p.TaskCategories,
                                      Reviews = p.Reviews,
                                      Customs = p.Customs,
                                      PositiveThumbs = p.PositiveThumbs,
                                      NegativeThumbs = p.NegativeThumbs
                                  });

            usersWithRoles = SortUsers(usersWithRoles, sortId);

            //worksUsersCount = usersWithRoles.Count();
            ViewBag.Tasks = new SelectList(db.TaskCategories, "Id", "Name");
            //ViewBag.Skills = new SelectList(db.Skills,"Id","Name");
            TempData["UsersFound"] = usersWithRoles.Count();

            int page = id ?? 0;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_UsersPage", GetUsersPage(usersWithRoles,page));
            }

            return View(GetUsersPage(usersWithRoles, page));
        }

        private IEnumerable<UserViewModel> GetUsersPage(IEnumerable<UserViewModel> usersWithRoles, int page)
        {

            var usersToSkip = page * pageSize;
            var users = usersWithRoles.Skip(usersToSkip).Take(pageSize);
            return users.ToList();
        }

        private IEnumerable<UserViewModel> SortUsers(IEnumerable<UserViewModel> usersWithRoles, int sortId)
        {
            //by rating 
            if (sortId == 1)
            {
                usersWithRoles = usersWithRoles.OrderByDescending(m => (double)(m.PositiveThumbs - m.NegativeThumbs) / (m.PositiveThumbs + m.NegativeThumbs)).
                    OrderByDescending(m => (m.PositiveThumbs + m.NegativeThumbs));
            }

            //by registration date ascending order
            if(sortId==2)
            {
                usersWithRoles = usersWithRoles.OrderBy(m => m.RegistrationDate);
            }

            //by registration date descending order
            if (sortId == 3)
            {
                usersWithRoles = usersWithRoles.OrderByDescending(m => m.RegistrationDate);
            }
            return usersWithRoles;
        }

        public int GetUsersCount()
        {
            return (int)TempData["UsersFound"];
        }

        public JsonResult GetSkills(int id)
        {
            var skills = db.Skills.Where(m => m.TaskCategoryId == id).ToList();
            return Json(skills, JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<Review> SortUserReviews(IEnumerable<Review> reviews, int sortType)
        {
            switch (sortType)
            {
                case 2://по давним
                    reviews = reviews.OrderBy(r => r.Date);
                    break;
                case 3://по рейтингу
                    reviews = reviews.OrderByDescending(r => r.Rating);
                    break;
                default://по новым(по умолчанию)
                    reviews = reviews.OrderByDescending(r => r.Date);
                    break;
            }
            return reviews;
        }


        // GET: User/Details/5
        public async Task<ActionResult> Details(string userName, 
            bool? customsPaginated, int? customsPage, 
            bool? reviewsSortSelected, int? reviewSortType, bool? reviewsPaginated, int? reviewsPage)
        {
            if (userName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await db.Users
                .Include(u => u.Customs)
                .Include(u => u.Reviews)
                .Include(u => u.TaskCategories)
                .Include(u => u.Skills)
                .SingleAsync(u => u.UserName == userName);

            if (user == null)
            {
                return HttpNotFound();
            }


            if (reviewSortType != null) // сбрасываем страницу отзывов на 1, если была выбрана новая сортировка
            {
                Session["ReviewsPage"] =  1;
            }
            else
            {
                Session["ReviewsPage"] = reviewsPage ?? Session["ReviewsPage"] ?? 1;
            }

            Session["CustomsPage"] = customsPage ?? Session["CustomsPage"] ?? 1;

            Session["ReviewsSortType"] = reviewSortType ?? Session["ReviewsSortType"] ?? 1;
            
            if (Request.IsAjaxRequest())
            {
                if (customsPaginated == true)
                {
                    return PartialView("_DetailsCustomsPage",
                        user.Customs?.OrderByDescending(c => c.EndingDate)
                        .ToPagedList((int)Session["CustomsPage"], customsPageSize));
                }
                if (reviewsPaginated == true || reviewsSortSelected==true)
                {
                    var sortReviews = SortUserReviews(user.Reviews, (int)Session["ReviewsSortType"]);
                    var pagedReviews = sortReviews.ToPagedList((int)Session["ReviewsPage"], reviewsPageSize);
                    return PartialView("_DetailsReviewsPage", pagedReviews);
                }
            }

            customsPaginated = null;
            customsPage = null;
            reviewsPaginated = null;
            reviewsPage = null;


            Session["UserName"] = user.UserName;

            Session["CustomsPage"] = 1;
            Session["CustomsPageSize"] = customsPageSize;

            Session["ReviewsPage"] = 1;
            Session["ReviewsPageSize"] = reviewsPageSize;

            return View(user);

        }

        //private IEnumerable<UserViewModel> GetReviewsPage(IEnumerable<UserViewModel> usersWithRoles, int page)
        //{

        //    var usersToSkip = page * pageSize;
        //    var users = usersWithRoles.Skip(usersToSkip).Take(pageSize);
        //    return users.ToList();
        //}

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
