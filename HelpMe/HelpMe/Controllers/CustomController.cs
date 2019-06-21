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
using Microsoft.AspNet.Identity;
using HelpMe.Hubs;
using System.IO;
using PagedList;
using System.Runtime.Remoting.Contexts;
using Microsoft.AspNet.SignalR;


namespace HelpMe.Controllers
{
    public class CustomController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        int pageSize = 4; // количество объектов на страницу

        // GET: Custom
        public ActionResult Index(int? id, string name, int? typeTaskId, int? taskCategoryId, int? skillId,
            int? minPrice, int? maxPrice, bool? customsWithoutOffers, int sortId=1)
        {
            //подгружаем только открытые заявки
            var customViewModels = db.Customs/*.Where(c=>c.Status==CustomStatus.Open)*/.Include(c => c.Comments).Include(c => c.User)
                .Include(c => c.TypeTask).Include(c => c.CategoryTask).Include(c => c.Skill);
            //var count = customViewModels.Count();

            if (!String.IsNullOrEmpty(name))
            {
                customViewModels = customViewModels.Where(m => m.Name.Contains(name));
            }

            if (taskCategoryId != null && taskCategoryId != 0)
            {
                customViewModels = customViewModels.Where(m => m.CategoryTaskId == taskCategoryId);

                if (skillId != null && skillId != 0)
                {
                    customViewModels = customViewModels.Where(m => m.SkillId == skillId);
                }
            }

            if(minPrice!=null && minPrice!=0)
            {
                customViewModels = customViewModels.Where(m => m.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                customViewModels = customViewModels.Where(m => m.Price <= maxPrice);
            }

            if(customsWithoutOffers==true)
            {
                customViewModels = customViewModels.Where(m => m.Comments.Count == 0);
            }
            //count = customViewModels.Count();

            customViewModels = SortCustoms(customViewModels, sortId);

            int page = id ?? 0;
            //IEnumerable<CustomViewModel> customPerPages = customViewModels.Skip((page - 1) * pageSize).Take(pageSize);
            //PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = customViewModels.Count() };


            //CustomIndexViewModel ivm = new CustomIndexViewModel { PageInfo = pageInfo, Customs = customPerPages };

            //count = customViewModels.Count();

            ViewBag.Tasks = new SelectList(db.TaskCategories, "Id", "Name");
            TempData["CustomsFound"] = customViewModels.Count();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_CustomsPage", GetCustomsPage(customViewModels, page));
            }

            return View(GetCustomsPage(customViewModels, page));
        }

        private IEnumerable<CustomViewModel> GetCustomsPage(IQueryable<CustomViewModel> customs, int page)
        {

            var customsToSkip = page * pageSize;
            var customsToPage = customs.Skip(customsToSkip).Take(pageSize);
            return customsToPage.ToList();
        }

        public IQueryable<CustomViewModel> SortCustoms(IQueryable<CustomViewModel> customs, int sortId)
        {
            //by new customs
            if(sortId==1)
            {
                customs =  customs.OrderByDescending(m => m.StartDate);
            }
            //by old customs
            if(sortId==2)
            {
                customs = customs.OrderBy(m => m.StartDate);
            }
            //by price descending
            if (sortId == 3)
            {
                customs = customs.OrderByDescending(m => m.Price);
            }
            //by price ascending
            if(sortId==4)
            {
                customs = customs.OrderBy(m => m.Price);
            }
            
            return customs;
        }

        public int GetCustomsCount()
        {
            return (int)TempData["CustomsFound"];
        }

        public ActionResult Tasks()
        {
            var customViewModels = db.Customs.Include(c => c.Comments).Include(c => c.User).OrderBy(x => x.Id);
            return View();
        }

        public ActionResult Bidders()
        {
            var customViewModels = db.Customs.Include(c => c.Comments).Include(c => c.User).OrderBy(x => x.Id);
            return View();
        }

        public ActionResult LoadAttaches(int? id)
        {
            var allAttaches = db.Attachments.Include(c => c.CustomViewModel).Where(c => c.CustomViewModelId == id).ToList();
            return PartialView(allAttaches);
        }


        public async Task<JsonResult> DeleteAttach(int? id)
        {

            AttachModel attach = await db.Attachments.FindAsync(id);
            var customId = attach.CustomViewModelId;
            db.Attachments.Remove(attach);
            await db.SaveChangesAsync();
            return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> Upload()
        {
            var idCustom = Convert.ToInt32(Request.Form["CustomViewModelId"]);
            CustomViewModel customViewModel = await db.Customs.Include(c => c.User).Include(a => a.Attachments).FirstOrDefaultAsync(c => c.Id == idCustom);
            var count = customViewModel.Attachments.Count;
            if (customViewModel.ExecutorId == User.Identity.GetUserId() && count < 10)
            {
                AttachModel attach = new AttachModel();

                foreach (string file in Request.Files)
                {
                    var upload = Request.Files[file];

                    if (upload != null)
                    {
                        // получаем имя файла
                        string fileName = System.IO.Path.GetFileName(upload.FileName);
                        // сохраняем файл в папку Files в проекте
                        upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                        string path = Server.MapPath("~/Files/" + fileName);
                        // сохраняем файл в папку Files в проекте
                        upload.SaveAs(path);
                        attach.Id = 1;
                        attach.ExecutorPrice = Convert.ToInt32(Request.Form["ExecutorPrice"]);
                        attach.CustomViewModelId = Convert.ToInt32(Request.Form["CustomViewModelId"]);
                        attach.AttachFilePath = path;
                        attach.AttachStatus = AttachStatus.NotPurchased;
                        attach.UserId = User.Identity.GetUserId();
                        db.Attachments.Add(attach);
                        SendMessage("Вы загрузили решение", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, "загрузил решение");
                        await db.SaveChangesAsync();
                    }
                }
            }
            return Json("Файл загружен");
        }

        public async Task<ActionResult> Buy(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // optimaize
            AttachModel attachViewModel = await db.Attachments.Include(c => c.User).Include(c => c.CustomViewModel)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            if (attachViewModel.AttachStatus != AttachStatus.Purchased)
            {

                string myId = User.Identity.GetUserId();
                Wallet wallet = db.Wallets.Where(x => x.UserId == myId).FirstOrDefault();

                if (wallet.Summ - attachViewModel.ExecutorPrice > 0)
                {
                    wallet.Summ -= attachViewModel.ExecutorPrice;
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                attachViewModel.AttachStatus = AttachStatus.Purchased;
                if (attachViewModel.CustomViewModel.Attachments.Where(c => c.AttachStatus == AttachStatus.Purchased).Count() == 0)
                    attachViewModel.CustomViewModel.Status = CustomStatus.CheckCustom;
                db.Entry(wallet).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return Content("<div class='task-tags'><span>Работа куплена </span></div>", "text/html");
                // return RedirectToAction("Details", "Custom", new { id = attachViewModel.CustomViewModel.Id });
            }
            else
            {
                ViewBag.DangerText = "Решение куплено!";
            }
            return RedirectToAction("Details", "Custom", new { id = attachViewModel.CustomViewModel.Id });

        }


        /// <summary>
        /// Выбрать исполнителя
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ActionResult> ChooseExecutor(int? id, string userId)
        {
            if (id == null && userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments).Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);


            if (customViewModel.UserId != customViewModel.ExecutorId)
            {
                customViewModel.ExecutorId = userId;
                CommentViewModel comment = await db.Comments.Include(c => c.CustomViewModel).Include(c => User).FirstOrDefaultAsync(c => c.UserId == customViewModel.ExecutorId);
                db.Entry(customViewModel).State = EntityState.Modified;
                customViewModel.ExecutorPrice = comment.OfferPrice;
                customViewModel.Status = CustomStatus.Check; // заявка выполняется
            }
            string uName = db.Users.Where(c => c.Id == customViewModel.UserId).FirstOrDefault().UserName;
            string exName = db.Users.Where(c => c.Id == customViewModel.ExecutorId).FirstOrDefault().UserName;
            SendMessage("Вы выбрали исполнителем " + exName, customViewModel.Id, uName, exName, "выбрал вас исполнителем");
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        [HttpPost]
        public async Task<ActionResult> CustomSearch(string name)
        {
            var allCustoms = await db.Customs.Where(a => a.Name.Contains(name)).ToListAsync();
            if (allCustoms.Count <= 0)
            {
                return HttpNotFound();
            }
            return PartialView(allCustoms);
        }

        private void SendMessage(string message, int id, string userName, string exUsername, string descr)
        {
            // Получаем контекст хаба
            var uId = db.Users.Where(x => x.UserName == userName).FirstOrDefault().Id;
            var exId = db.Users.Where(x => x.UserName == exUsername).FirstOrDefault().Id;
            string url = "/Custom/Details/" + id;
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            Notification notification = new Notification { Id = 1, Title = message, Status = NotificationStatus.Unreading, Url = url, UserName = userName, ExUserName = exUsername, UserId = exId, Description = descr };
            Notification notification2 = new Notification { Id = 2, Title = message, Status = NotificationStatus.Unreading, Url = url, UserName = userName, ExUserName = exUsername, UserId = uId, Description = descr };
            db.Notifications.Add(notification);
            db.Notifications.Add(notification2);
            db.SaveChanges();
            context.Clients.User(uId).displayMessage(message);
            context.Clients.User(exId).displayMessage(message);
        }

        [HttpPost]
        public async Task<ActionResult> SendSolution(int? id, HttpPostedFileBase upload)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomViewModel customViewModel = await db.Customs.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);

            if (customViewModel.ExecutorId == User.Identity.GetUserId())
            {
                if (ModelState.IsValid && upload != null)
                {
                    // получаем имя файла
                    string fileName = System.IO.Path.GetFileName(upload.FileName);
                    string path = Server.MapPath("~/Files/" + fileName);
                    // сохраняем файл в папку Files в проекте
                    upload.SaveAs(path);
                    db.Entry(customViewModel).State = EntityState.Modified;
                    customViewModel.FilePath = path;
                    customViewModel.Status = CustomStatus.NeedBuy; // вложения требует покупки
                                                                   //  customViewModel.AttachStatus = AttachStatus.NotPurchased; // решение не куплено
                    await db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id }); ;
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetComment(int id)
        {
            var results = db.Comments.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description
            }).Where(e => e.Id == id).FirstOrDefault();

            return new JsonResult() { Data = results, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private string FindExecutorRating(User user)
        {
            var likes = user.PositiveThumbs;
            var dislikes = user.NegativeThumbs;
            var rate = "0.0";
            if (likes + dislikes > 0)
            {
                rate = Math.Round((double)likes / (likes + dislikes) * 5, 1).ToString().Replace(',', '.');
            }
            if (rate.Length == 1)
            {
                rate = rate + ".0";
            }
            return rate;
        }

        /// <summary>
        /// Разместить предложение
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="CustomViewModelId"></param>
        /// <param name="OfferPrice"></param>
        /// <param name="Days"></param>
        /// <param name="Hours"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult People(string Description, int CustomViewModelId, int OfferPrice, int Days, int Hours)
        {
            var comment = new CommentViewModel { Description = Description, OfferPrice = OfferPrice, Days = Days, Hours = Hours, UserId = User.Identity.GetUserId(), CustomViewModelId = CustomViewModelId };
            CustomViewModel customViewModel = db.Customs.Include(c => c.Comments).FirstOrDefault(c => c.Id == CustomViewModelId);

            var thisUserId = User.Identity.GetUserId();
            var user = db.Users.Where(u => u.Id == thisUserId).FirstOrDefault();
            var imagePath = user.ImagePath ?? "~/Content/Custom/images/user-avatar-big-01.jpg";
            imagePath = Url.Content(imagePath);

            if (customViewModel.Comments.Where(c => c.UserId == User.Identity.GetUserId()).Count() >= 1)
            {
                return null;
            }
            else
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Json(new
                {
                    Id = comment.Id,
                    Description = comment.Description,
                    OfferPrice = comment.OfferPrice,
                    CustomViewModelId = comment.CustomViewModelId,
                    UserId = comment.UserId,
                    ExecutorRating = FindExecutorRating(user),//for js method 
                    ExecutorImagePath = imagePath//for js method 
                }, JsonRequestBehavior.AllowGet);
                // return new JsonResult() { Data = comment, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public FileResult GetFileAttach(string path, int? id)
        {
            AttachModel attachViewModel = db.Attachments.Include(c => c.User).Include(c => c.CustomViewModel)
                                                            .FirstOrDefault(c => c.Id == id);
            // Тип файла - content-type
            if (attachViewModel.AttachStatus == AttachStatus.Purchased && attachViewModel.CustomViewModel.ExecutorId != User.Identity.GetUserId())
            {
                string file_type = "application/" + Path.GetExtension(path);
                string file_name = Path.GetFileName(path);

                return File(path, file_type, file_name);
            }
            else
            {
                return null;
            }

        }

        public FileResult GetFile(string path)
        {
            string file_type = "application/" + Path.GetExtension(path);
            string file_name = Path.GetFileName(path);

            return File(path, file_type, file_name);
        }

        // GET: Custom/Details/5
        public async Task<ActionResult> Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // optimaize
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                              .Include(c => c.CategoryTask)
                                                              .Include(c => c.TypeTask)
                                                              .Include(c => c.User)
                                                              .Include(c => c.Attachments)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            if (customViewModel == null)
            {
                return HttpNotFound();
            }

            return View(customViewModel);
        }

        // GET: Custom/Create
        public ActionResult Create()
        {
            SelectList types = new SelectList(db.CustomTypes, "Id", "Name"); // выбор типа задачи
            ViewBag.Types = types;
            SelectList categories = new SelectList(db.TaskCategories, "Id", "Name"); // выбор типа задачи
            ViewBag.Categories = categories;
            return View();
        }

        // GET: Custom/Create
        public async Task<ActionResult> Accept(int? id)
        {
            // optimaize
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                              .Include(c => c.CategoryTask)
                                                              .Include(c => c.TypeTask)
                                                              .Include(c => c.User)
                                                              .Include(c => c.Attachments)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            db.Entry(customViewModel).State = EntityState.Modified;
            customViewModel.Status = CustomStatus.Close;
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        public async Task<ActionResult> Revision(int? id)
        {
            // optimaize
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                              .Include(c => c.CategoryTask)
                                                              .Include(c => c.TypeTask)
                                                              .Include(c => c.User)
                                                              .Include(c => c.Attachments)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            db.Entry(customViewModel).State = EntityState.Modified;
            customViewModel.Status = CustomStatus.Revision; // на доработку
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        // POST: Custom/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,AttachFilePath,AttachFile,UserId,TypeTaskId,CategoryTaskId,EndingDate,Price")] CustomViewModel customViewModel)
        {
            if (ModelState.IsValid)
            {
                string fileName = Path.GetFileName(customViewModel.AttachFile.FileName);
                string path = Server.MapPath("~/Files/" + fileName);
                // сохраняем файл в папку Files в проекте
                customViewModel.AttachFile.SaveAs(path);
                customViewModel.AttachFilePath = "~/Files/" + fileName;
                customViewModel.Status = CustomStatus.Open; // открытая заявка
                customViewModel.UserId = User.Identity.GetUserId();
                customViewModel.StartDate = DateTime.Now;
                customViewModel.ExecutorStartDate = DateTime.Now;
                db.Customs.Add(customViewModel);
                await db.SaveChangesAsync();
                string uName = db.Users.Where(c => c.Id == customViewModel.UserId).FirstOrDefault().UserName;
                // SendMessage("Добавлен новый заказ", customViewModel.Id, uName);
                return RedirectToAction("Index");
            }

            return View(customViewModel);
        }

        // GET: Custom/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomViewModel customViewModel = await db.Customs.FindAsync(id);

            if (customViewModel == null)
            {
                return HttpNotFound();
            }

            if (customViewModel.UserId == User.Identity.GetUserId())
            {
                return View(customViewModel);
            }
            else { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }
        }

        // POST: Custom/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,AttachFilePath,AttachFile,UserId,TypeTaskId,CategoryTaskId,EndingDate,Price")] CustomViewModel customViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
            }

            return View(customViewModel);
        }

        // GET: Custom/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomViewModel customViewModel = await db.Customs.FindAsync(id);
            if (customViewModel == null)
            {
                return HttpNotFound();
            }

            if (customViewModel.UserId == User.Identity.GetUserId())
            {
                return View(customViewModel);
            }
            else { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }
        }

        // POST: Custom/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CustomViewModel customViewModel = await db.Customs.FindAsync(id);
            db.Customs.Remove(customViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
