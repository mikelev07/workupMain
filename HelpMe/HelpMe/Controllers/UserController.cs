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
using HelpMe.Hubs;
using HelpMe.Helpers;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security;

namespace HelpMe.Controllers
{
    public static class Extensions
    {
        public static void AddUpdateClaim(this IPrincipal currentPrincipal, string key, string value)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return;

           
            var existingClaim = identity.FindFirst(key);
            if (existingClaim != null)
                identity.RemoveClaim(existingClaim);

         
            identity.AddClaim(new Claim(key, value));
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identity), new AuthenticationProperties() { IsPersistent = true });
        }

        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            return claim.Value;
        }
    }
    public class UserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        private const int pageSize = 15;
        private const int customsPageSize = 5;    //for Details.cshtml in Works History tab
        private const int reviewsPageSize = 5;    //for Details.cshtml in Reviews tab

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
                users = (users ?? db.Users.Include(c=>c.Skills).Where(u=>u.Roles.Any(r=>r.RoleId== "b7da93e6-5276-4947-8406-bd5e2a60983c"))).Where(b => b.UserName.StartsWith(name));
                //worksUsersCount = users.Count();
            }
            if (taskId != 0 && taskId != null)
            {
                users = (users ?? db.Users.Include(c => c.Skills).Where(u => u.Roles.Any(r => r.RoleId == "b7da93e6-5276-4947-8406-bd5e2a60983c"))).Where(m => m.TaskCategories.Any(b => b.Id == taskId));
                //worksUsersCount = users.Count();

                if (skillId != 0 && skillId != null)
                {
                    users = users.Where(m => m.Skills.Any(b => b.Id == skillId));
                    //worksUsersCount = users.Count();
                }
            }

            if(isOnline==true)
            {
                users = (users ?? db.Users.Include(c => c.Skills).Where(u => u.Roles.Any(r => r.RoleId == "b7da93e6-5276-4947-8406-bd5e2a60983c"))).Where(m => m.IsOnline == isOnline);
            }

            if (isNotBusy == true)
            {
                users = (users ?? db.Users.Include(c => c.Skills).Where(u => u.Roles.Any(r => r.RoleId == "b7da93e6-5276-4947-8406-bd5e2a60983c"))).Where(m => m.IsNotBusy == isNotBusy);
            }

            users = (users ?? db.Users.Include(c => c.Skills).Where(u => u.Roles.Any(r => r.RoleId == "b7da93e6-5276-4947-8406-bd5e2a60983c"))).Where(m=>(m.PositiveThumbs+m.NegativeThumbs)>=worksCount);
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
                                      user.Skills,
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
                                      Skills = p.Skills,
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
            //by rating (default)
            if (sortId == 1)
            {
                usersWithRoles = usersWithRoles.OrderByDescending(m => HtmlExtensions.FindUserRating(m.PositiveThumbs, m.NegativeThumbs)).ThenByDescending(m => m.PositiveThumbs + m.NegativeThumbs);
            }
            //by finished works 
            if (sortId == 2)
            {
                usersWithRoles = usersWithRoles.OrderByDescending(m => m.Customs.Count(c=>c.DoneInTime==true));
            }
            //by registration date ascending order
            if(sortId==3)
            {
                usersWithRoles = usersWithRoles.OrderBy(m => m.RegistrationDate);
            }

            //by registration date descending order
            if (sortId == 4)
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
                    reviews = reviews.OrderByDescending(r => r.Date);
                    break;
                case 3://по рейтингу
                    reviews = reviews.OrderByDescending(r => r.Rating);
                    break;
                default://по новым(по умолчанию)
                    reviews = reviews.OrderBy(r => r.Date);
                    break;
            }
            return reviews;
        }


        // GET: User/Details/5
        public async Task<ActionResult> Details(string userName, 
            bool? customsPaginated, int? customsPage, 
            bool? reviewsSortSelected, int? reviewSortType, bool? reviewsPaginated, int? reviewsPage)
        {

            /*  if (await UserManager.IsInRoleAsync(userId, "admin"))
            {
             * 
             * 
             */

                if (userName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await db.Users
                .Include(u => u.Customs)
                .Include(u => u.Reviews)
                .Include(u => u.TaskCategories)
                .Include(u => u.Skills.Select(c=>c.TaskCategory))
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
            reviewsSortSelected = null;
            reviewSortType = null;
            reviewsPaginated = null;
            reviewsPage = null;


            Session["UserName"] = user.UserName;

            Session["CustomsPage"] = 1;
            Session["CustomsPageSize"] = customsPageSize;

            Session["ReviewsPage"] = 1;
            Session["ReviewsPageSize"] = reviewsPageSize;

            //Если данный пользователь - заказчик, то подгрузим его ОТКРЫТЫЕ заказы
            var customerId = User.Identity.GetUserId();
            if(customerId != null)
            {
                var customsList = await db.Customs.Where(c => c.UserId == customerId && c.Status==CustomStatus.Open).ToListAsync();
                ViewData["CustomsList"] = new SelectList(customsList, "Id", "Name");
            }




            return View(user);

        }

        public async Task<bool> ChangeStatus(string value)
        {
            string usId = User.Identity.GetUserId();
            var userFrom = await db.Users.FirstOrDefaultAsync(x => x.Id == usId);

            db.Entry(userFrom).State = EntityState.Modified;

            if (value != "dontworry")
            {
                userFrom.IsNotBusy = true;
                User.AddUpdateClaim("status", "true");
            }
            else
            {
                userFrom.IsNotBusy = false;
                User.AddUpdateClaim("status", "false");
            }

            await db.SaveChangesAsync();

            return true;
        }



        public async Task<bool> AttractToCustom(string yourMessage, int customId, string userFromName, string userToName, string sentMessage)
        {
           // bool hasAlreadyNotification = await db.Notifications.AnyAsync(n => n.Url == ("/Custom/Details/" + customId) && n.ExUserName==userToName);
           // if (hasAlreadyNotification)
           // {
           //     return false;
            //}

            var userFrom = await db.Users.FirstOrDefaultAsync(x => x.UserName == userFromName);
            var userFromId = userFrom.Id;
            var userTo = await db.Users.FirstOrDefaultAsync(x => x.UserName == userToName);
            var userToId = userTo.Id;
            string url = "/Custom/Details/" + customId;
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

       

            return true;
        }

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
