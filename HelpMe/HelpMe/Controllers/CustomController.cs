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
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using HelpMe.Helpers;

namespace HelpMe.Controllers
{
    public class CustomController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        int pageSize = 20; // количество объектов на страницу

        // GET: Custom
        public ActionResult Index(int? id, string name, int? typeTaskId, int? taskCategoryId, int? skillId,
            int? minPrice, int? maxPrice, bool? customsWithoutOffers, int sortId = 1)
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

            if (minPrice != null && minPrice != 0)
            {
                customViewModels = customViewModels.Where(m => m.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                customViewModels = customViewModels.Where(m => m.Price <= maxPrice);
            }

            if (customsWithoutOffers == true)
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
            if (sortId == 1)
            {
                customs = customs.OrderByDescending(m => m.StartDate);
            }
            //by old customs
            if (sortId == 2)
            {
                customs = customs.OrderBy(m => m.StartDate);
            }
            //by price descending
            if (sortId == 3)
            {
                customs = customs.OrderByDescending(m => m.Price);
            }
            //by price ascending
            if (sortId == 4)
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
            string userId = User.Identity.GetUserId();

            var tasksModel = db.Customs.Include(c => c.TypeTask)
                .Include(c => c.CategoryTask)
                //.Include(c => c.Executor)
                .Include(c => c.Skill)
                .Include(c => c.User)
                .Include(c => c.MyAttachments)
                .Include(c => c.Comments)
                .Where(t => t.UserId == userId).OrderByDescending(m => m.Id).ToList();
            return View(tasksModel);
        }

        public ActionResult TasksExec()
        {
            string userId = User.Identity.GetUserId();

            var tasksModel = db.Customs.Include(c => c.TypeTask)
                .Include(c => c.CategoryTask)
                .Include(c => c.Executor)
                .Include(c => c.Skill)
                .Include(c => c.User).Include(c => c.MyAttachments)
                   .Include(c => c.Comments)
                .Where(t => t.ExecutorId == userId).ToList();

            return View(tasksModel);
        }

        public ActionResult Bidders(int? id)
        {
            var customViewModels = db.Customs.Include(c => c.Comments)
                                            .Include(c => c.User)
                                            //.Include(c => c.Executor)
                                            .Where(i => i.Id == id).FirstOrDefault();

            var comms = customViewModels.Comments.ToList();

            return View(comms);
        }

        public ActionResult MyActiveBids()
        {
            string userId = User.Identity.GetUserId();

            var comms = db.Comments.Include(u => u.User).Include(u => u.CustomViewModel).Where(u => u.UserId == userId);

            return View(comms);
        }

        public async Task<string> GetCustomStatus(int? id)
        {
            var status = await db.Customs.Where(c => c.Id == id).Select(c => c.Status).FirstOrDefaultAsync();
            return status.GetDisplayName();
        }

        public ActionResult LoadMyAttaches(int? id)
        {
            var allAttaches = db.MyAttachments.Include(c => c.CustomViewModel).Where(c => c.CustomViewModelId == id).ToList();
            return PartialView(allAttaches);
        }

        public ActionResult LoadMainAttaches(int? id)
        {
            var allMainAttaches = db.MainAttachments.Include(m => m.CustomViewModel).Where(c => c.CustomViewModelId == id).ToList();
            return PartialView(allMainAttaches);
        }

        public ActionResult LoadAttaches(int? id)
        {
            var allAttaches = db.Attachments.Include(c => c.CustomViewModel).Where(c => c.CustomViewModelId == id).ToList();
            return PartialView(allAttaches);
        }

        public ActionResult LoadButtons(int? id)
        {
            var customViewModel = db.Customs.Where(c => c.Id == id).SingleOrDefault();
            return PartialView(customViewModel);
        }

        public ActionResult LoadButtonArch(int? id)
        {
            var customViewModel = db.Customs.Where(c => c.Id == id).SingleOrDefault();
            return PartialView(customViewModel);
        }

        public async Task<int> NotDownloadedMainAttaches(int? id)
        {
            var custom = await db.Customs.FirstOrDefaultAsync(c => c.Id == id);
            var count = await db.MainAttachments.Where(a => a.CustomViewModelId == id).CountAsync(m => m.IsDownloaded == false);
            if (count == 0)
            {
                custom.Status = CustomStatus.CheckCustom;
                db.Entry(custom).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return count;
        }

        public async Task<int> NotPurchasedAttaches(int? id)
        {
            return await db.Attachments.Where(a => a.CustomViewModelId == id).CountAsync(a => a.AttachStatus == AttachStatus.NotPurchased);
        }

        public async Task<JsonResult> DeleteMainAttach(int? id)
        {
            var mainAttach = await db.MainAttachments.FindAsync(id);
            var customId = mainAttach.CustomViewModelId;
            var customViewModel = await db.Customs.Include(c => c.MainAttachments).FirstOrDefaultAsync(c => c.Id == customId);
            db.MainAttachments.Remove(mainAttach);
            var mainAttachments = customViewModel.MainAttachments;

            if (mainAttachments.Where(m => m.IsDownloaded == true).Count() == 0)
            {
                db.Entry(customViewModel).State = EntityState.Modified;
                if (mainAttachments.Where(m => m.IsDownloaded == false).Count() == 0)
                {
                    customViewModel.Status = CustomStatus.Check;//выполняется исполнителем
                }
                else
                {
                    customViewModel.Status = CustomStatus.CheckCustom;//проверяется заказчиком
                }
            }
            await db.SaveChangesAsync();
            return Json(true);
        }

        public async Task<JsonResult> DeleteAttach(int? id)
        {
            AttachModel attach = await db.Attachments.FindAsync(id);
            var customId = attach.CustomViewModelId;

            var customViewModel = await db.Customs.Include(c => c.Attachments).FirstOrDefaultAsync(c => c.Id == customId);

            db.Attachments.Remove(attach);

            var attachments = customViewModel.Attachments;
            var revisions = attachments.Where(a => a.IsRevision == true);
            if (attachments.Where(a => a.AttachStatus == AttachStatus.NotPurchased).Count() == 0)
            {
                db.Entry(customViewModel).State = EntityState.Modified;
                if (attachments.Where(a => a.AttachStatus == AttachStatus.Purchased).Count() == 0)
                {
                    customViewModel.Status = CustomStatus.Check;//выполняется исполнителем
                }
                else
                {
                    if (!customViewModel.IsRevision)
                    {
                        customViewModel.Status = CustomStatus.CheckCustom;//проверяется заказчиком
                    }
                    else
                    {
                        if (revisions.Count() == 0)
                        {
                            customViewModel.Status = CustomStatus.Revision;//на доработке
                        }
                    }
                }
            }

            await db.SaveChangesAsync();
            return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> UploadAttach()
        {
            var idCustom = Convert.ToInt32(Request.Form["CustomViewModelId"]);
            CustomViewModel customViewModel = await db.Customs.Include(c => c.User).Include(a => a.Attachments).Include(a => a.MyAttachments).FirstOrDefaultAsync(c => c.Id == idCustom);
            var count = customViewModel.MyAttachments.Count;

            if (count < 10)
            {
                MyAttachModel attach = new MyAttachModel();

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

                        attach.CustomViewModelId = Convert.ToInt32(Request.Form["CustomViewModelId"]);
                        attach.AttachFilePath = path;

                        attach.UserId = User.Identity.GetUserId();
                        db.MyAttachments.Add(attach);
                        NotificationHubModel notificationHubModel = new NotificationHubModel()
                        {
                            UserFromId = User.Identity.GetUserId(),
                            UserToId = customViewModel.ExecutorId,
                            DescriptionFrom = "Вы загрузили решение к заказу",
                            DescriptionTo = "Исполнитель " + customViewModel.Executor.UserName + " загрузил решение"
                        };

                        SendM(notificationHubModel);
                        // SendMessage("Вы загрузили решение", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, "загрузил решение");
                        await db.SaveChangesAsync();
                    }
                }
            }
            return Json("Файл загружен");
        }

        [HttpPost]
        public async Task<JsonResult> UploadRevision()
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
                        FileInfo fil = new FileInfo(path);
                        var fileNameNew = fil.Name;
                        int index = fileNameNew.LastIndexOf(".");
                        if (index > 0)
                            fileNameNew = fileNameNew.Substring(0, index); // or index + 1 to keep slash

                        // сохраняем файл в папку Files в проекте
                        upload.SaveAs(path);
                        attach.Id = 1;
                        attach.ExecutorPrice = Convert.ToInt32(Request.Form["ExecutorPrice"]);
                        attach.CustomViewModelId = Convert.ToInt32(Request.Form["CustomViewModelId"]);
                        attach.AttachFilePath = path;
                        attach.AttachFileExtens = fil.Extension;
                        if (fileNameNew.Length > 20)
                        {
                            attach.AttachFileName = fileNameNew.Substring(0, 20);
                        }
                        else
                        {
                            attach.AttachFileName = fileNameNew;
                        }
                        attach.AttachStatus = AttachStatus.NotPurchased;

                        if (customViewModel.IsRevision)//см. коммент в методе Accept
                        {
                            attach.IsRevision = true;
                        }

                        attach.UserId = User.Identity.GetUserId();
                        db.Attachments.Add(attach);

                        NotificationHubModel notificationHubModel = new NotificationHubModel()
                        {
                            UserFromId = User.Identity.GetUserId(),
                            UserToId = customViewModel.ExecutorId,
                            DescriptionFrom = "Вы загрузили решение к заказу",
                            DescriptionTo = "Исполнитель " + customViewModel.Executor.UserName + " загрузил решение"
                        };

                        SendM(notificationHubModel);

                        //SendMessage("Вы загрузили решение", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, "загрузил решение");
                        db.Entry(customViewModel).State = EntityState.Modified;
                        customViewModel.Status = CustomStatus.NeedBuy;//ожидает покупки
                        await db.SaveChangesAsync();
                    }
                }
            }
            return Json("Файл загружен");
        }

        public async Task<bool> EnoughMoneyForBuying(int? id)
        {
            var attachment = await db.Attachments.Include(a => a.CustomViewModel).SingleOrDefaultAsync(a => a.Id == id);
            var attachPrice = attachment.ExecutorPrice;
            var userId = attachment.CustomViewModel.UserId;
            var wallet = await db.Wallets.SingleOrDefaultAsync(w => w.UserId == userId);
            var walletSum = wallet.Summ;

            return walletSum >= attachPrice;
        }

        public async Task<ActionResult> Buy(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // optimaize
            AttachModel attachViewModel = await db.Attachments
                .Include(c => c.User)
                .Include(c => c.CustomViewModel)
                .Include(c => c.CustomViewModel.Attachments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (attachViewModel.AttachStatus != AttachStatus.Purchased)
            {

                string myId = User.Identity.GetUserId();
                Wallet wallet = db.Wallets.Where(x => x.UserId == myId).FirstOrDefault();
                if (wallet.Summ - attachViewModel.ExecutorPrice >= 0)
                {
                    Transaction transaction = new Transaction()
                    {
                        Date = DateTime.Now,
                        CustomId = (int)attachViewModel.CustomViewModelId,
                        AttachId = attachViewModel.Id,
                        Status = TransactionStatus.Waiting
                    };
                    transaction.Id = 1;

                    wallet.Summ -= attachViewModel.ExecutorPrice;

                    transaction.Price = attachViewModel.ExecutorPrice;
                    transaction.FromUserId = myId;
                    transaction.ToUserId = attachViewModel.CustomViewModel.ExecutorId;
                    transaction.TimeBlock = attachViewModel.CustomViewModel.TimeBlock;
                    transaction.DateBlockEnd = transaction.Date.AddDays(transaction.TimeBlock);

                    db.Transactions.Add(transaction);
                    await db.SaveChangesAsync();
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                attachViewModel.AttachStatus = AttachStatus.Purchased;
                if (attachViewModel.CustomViewModel.Attachments.Where(c => c.AttachStatus == AttachStatus.NotPurchased).Count() == 0)
                {
                    attachViewModel.CustomViewModel.Status = CustomStatus.CheckCustom;//проверяется заказчиком
                }
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
        [HttpPost]
        public async Task<ActionResult> ChooseExecutor(int? CustId)
        {
            if (CustId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CommentViewModel commentViewModel = await db.Comments.Include(c => c.CustomViewModel).FirstOrDefaultAsync(c => c.Id == CustId);
            CustomViewModel customViewModel = db.Customs.Where(c => c.Id == commentViewModel.CustomViewModelId).FirstOrDefault();

            string myId = User.Identity.GetUserId();
            Wallet wallet = db.Wallets.Where(x => x.UserId == myId).FirstOrDefault();
            if (customViewModel.UserId != customViewModel.ExecutorId)
            {
                customViewModel.ExecutorId = commentViewModel.UserId;

                db.Entry(customViewModel).State = EntityState.Modified;
                customViewModel.ExecutorPrice = commentViewModel.OfferPrice;
                customViewModel.Status = CustomStatus.Check; // заявка выполняется

                if (wallet.Summ - commentViewModel.OfferPrice >= 0)
                {

                    Transaction transaction = new Transaction()
                    {
                        Date = DateTime.Now,
                        CustomId = customViewModel.Id,
                        Status = TransactionStatus.Waiting
                    };
                    transaction.Id = 1;
                    wallet.Summ -= commentViewModel.OfferPrice;
                    transaction.Price = commentViewModel.OfferPrice;
                    transaction.FromUserId = myId;
                    transaction.ToUserId = customViewModel.ExecutorId;
                    transaction.TimeBlock = 1;
                    transaction.DateBlockEnd = transaction.Date.AddDays(transaction.TimeBlock);
                    db.Transactions.Add(transaction);
                    await db.SaveChangesAsync();
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                db.Entry(wallet).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            string uName = db.Users.Where(c => c.Id == customViewModel.UserId).FirstOrDefault().UserName;
            string exName = db.Users.Where(c => c.Id == customViewModel.ExecutorId).FirstOrDefault().UserName;
           // SendMessage();

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

        private void SendM(NotificationHubModel notif)
        {
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            Notification notificationFrom = new Notification
            {
                Id = 1,
                UserFromId = notif.UserFromId,
                UserToId = notif.UserToId,
                UserId = notif.UserFromId,
                DescriptionFrom = notif.DescriptionFrom,
                DescriptionTo = notif.DescriptionTo,
                StartDate = DateTime.Now
            };

            Notification notificationTo = new Notification
            {
                Id = 2,
                UserFromId = notif.UserFromId,
                UserToId = notif.UserToId,
                UserId = notif.UserFromId,
                DescriptionFrom = notif.DescriptionFrom,
                DescriptionTo = notif.DescriptionTo,
                StartDate = DateTime.Now
            };

            db.Notifications.Add(notificationFrom);
            db.Notifications.Add(notificationTo);
            db.SaveChanges();

            context.Clients.User(notif.UserFromId).displayMessage(notif.DescriptionFrom);
            context.Clients.User(notif.UserToId).displayMessage(notif.DescriptionTo);
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

        //[AcceptVerbs(HttpVerbs.Get)]
        //public JsonResult GetMyAttachments(int id)
        //{
        //    var model = db.Customs.Where(i => i.Id == id).Include(c=>c.MyAttachments).SingleOrDefault();
        //    var results = model.MyAttachments.Select(e=> e.AttachFilePath).ToList();

        //    return Json(results, JsonRequestBehavior.AllowGet);
        //}

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetArchive(int id)
        {
            var model = db.Customs.Where(i => i.Id == id).Include(c => c.MyAttachments).SingleOrDefault();
            var files = model.MyAttachments.Select(e => e.AttachFilePath).ToList();
            List<string> filenames = files;

            string filename = Guid.NewGuid().ToString() + ".zip";

            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

            zipStream.SetLevel(3); // уровень сжатия от 0 до 9

            foreach (string file in filenames)
            {
                FileInfo fil = new FileInfo(file);
                var fileName = fil.Name;
                FileInfo fi = new FileInfo(Server.MapPath("~/Files/" + fileName));

                string entryName = ZipEntry.CleanName(fi.Name);
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime;
                newEntry.Size = fi.Length;
                zipStream.PutNextEntry(newEntry);

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = System.IO.File.OpenRead(fi.FullName))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            zipStream.IsStreamOwner = false;
            zipStream.Close();

            outputMemStream.Position = 0;

            string file_type = "application/zip";
            return File(outputMemStream, file_type, filename);
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
        public JsonResult ShowExecutors(string Description, int CustomViewModelId, int OfferPrice, int Days, int Hours)
        {
            var comment = new CommentViewModel
            {
                Description = Description,
                CreationDate = DateTime.Now,
                OfferPrice = OfferPrice,
                Days = Days,
                Hours = Hours,
                UserId = User.Identity.GetUserId(),
                CustomViewModelId = CustomViewModelId
            };
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
                    CreationDate = comment.CreationDate.ToString("dd MMMM yyyy HH:mm:ss"),
                    OfferPrice = comment.OfferPrice,
                    CustomViewModelId = comment.CustomViewModelId,
                    UserId = comment.UserId,
                    ExecutorRating = FindExecutorRating(user),//for js method 
                    ExecutorImagePath = imagePath//for js method 
                }, JsonRequestBehavior.AllowGet);
                // return new JsonResult() { Data = comment, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditComment([Bind(Include = "Id,Name,Description,UserId,OfferPrice, CreationDate, Days,Hours,CustomViewModelId")] CommentViewModel commentViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(commentViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                string htmlResult = "<div class=\"rate\">"
                    + commentViewModel.OfferPrice
                    + " рублей</div>";


                if (commentViewModel.Days > 0)
                {
                    htmlResult += "<span> за " + commentViewModel.Days + " дней</span>";
                }
                else
                {
                    htmlResult += "<span> за " + commentViewModel.Hours + " часов</span>";
                }
                return Json(htmlResult);
                // return RedirectToAction("Details", "Custom", new { id = commentViewModel.CustomViewModelId }); ;
            }
            //ViewBag.CustomViewModelId = new SelectList(db.Customs, "Id", "Name", commentViewModel.CustomViewModelId);
            return Json(true);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<bool> DeleteComment(int? id)
        {
            var commentViewModel = await db.Comments.FindAsync(id);
            //var customId = commentViewModel.CustomViewModelId;
            db.Comments.Remove(commentViewModel);
            await db.SaveChangesAsync();
            return true;
            //var currentCustom = await db.Customs.FindAsync(customId);
            //var executorId = currentCustom.ExecutorId;
            //bool isExec = executorId == User.Identity.GetUserId();
            //if (!isExec)
            //{
            //    db.Comments.Remove(commentViewModel);
            //    await db.SaveChangesAsync();
            //    return 
            //}
            //return;
            //return RedirectToAction("Index");
        }

        public async Task<bool> EditMyBid(int commentId, int price, int period, string timeUnit, string description)
        {
            var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            db.Entry(comment).State = EntityState.Modified;
            comment.OfferPrice = price;
            if (timeUnit == "дней")
            {
                comment.Days = period;
                comment.Hours = 0;
            }
            if (timeUnit == "часов")
            {
                comment.Days = 0;
                comment.Hours = period;
            }
            comment.Description = description;
            await db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> RemoveSpecialization(int? id)
        {
            string uId = User.Identity.GetUserId();

            User user = await db.Users.Where(u => u.Id == uId)
                                      .Include(t => t.TaskCategories)
                                      .Include(s => s.Skills)
                                      .FirstOrDefaultAsync();

            var skill = await db.Skills.FindAsync(id);

            user.Skills.Remove(skill);
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<JsonResult> HasComments(int id)
        {
            CustomViewModel currentCustom = await db.Customs.FindAsync(id);
            bool hasComments = currentCustom.Comments.Where(c => c.UserId == User.Identity.GetUserId()).Count() >= 1;
            return Json(hasComments);
        }


        public async Task<JsonResult> ChooseSpecialization(int? id)
        {
            string uId = User.Identity.GetUserId();

            User user = await db.Users.Where(u => u.Id == uId)
                                      .Include(t => t.TaskCategories)
                                      .Include(s => s.Skills)
                                      .FirstOrDefaultAsync();

            Skill skill = db.Skills.Where(i => i.Id == id).FirstOrDefault();



            user.Skills.Add(skill);

            await db.SaveChangesAsync();

            //bool hasComments = currentCustom.Comments.Where(c => c.UserId == User.Identity.GetUserId()).Count() >= 1;
            return Json(true);
        }


        [HttpPost]
        public async Task<JsonResult> UploadTestMultiMyAttach(int? id)
        {
            CustomViewModel customViewModel = await db.Customs.Include(c => c.User).Include(a => a.MyAttachments).Include(a => a.Attachments).FirstOrDefaultAsync(c => c.Id == id);
            var count = customViewModel.MyAttachments.Count;
            if (customViewModel.UserId == User.Identity.GetUserId() && count < 10)
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    if (count < 10)
                    {
                        MyAttachModel attach = new MyAttachModel();

                        var upload = Request.Files.Get(i);

                        if (Request.Files.Get(i).FileName == "") continue;

                        if (upload != null)
                        {
                            // получаем имя файла
                            string fileName = System.IO.Path.GetFileName(upload.FileName);
                            // сохраняем файл в папку Files в проекте
                            upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                            string path = Server.MapPath("~/Files/" + fileName);

                            FileInfo fil = new FileInfo(path);
                            var fileNameNew = fil.Name;
                            int index = fileNameNew.LastIndexOf(".");
                            if (index > 0)
                                fileNameNew = fileNameNew.Substring(0, index);
                            // сохраняем файл в папку Files в проекте
                            upload.SaveAs(path);
                            attach.Id = 1;
                            attach.CustomViewModelId = id;
                            attach.AttachFilePath = path;
                            attach.AttachFileExtens = fil.Extension;
                            if (fileNameNew.Length > 20)
                            {
                                attach.AttachFileName = fileNameNew.Substring(0, 20);
                            }
                            else
                            {
                                attach.AttachFileName = fileNameNew;
                            }

                            attach.UserId = User.Identity.GetUserId();
                            db.MyAttachments.Add(attach);
                            // SendMessage("Вы загрузили решение", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, "загрузил решение");
                            await db.SaveChangesAsync();
                            count = customViewModel.MyAttachments.Count;
                        }
                    }
                }
            }
            return Json("Файл загружен");
        }


        [HttpPost]
        public async Task<JsonResult> UploadTestMultiAttach(int? id)
        {
            CustomViewModel customViewModel = await db.Customs.Include(c => c.User).Include(a => a.MainAttachments).Include(a => a.Attachments).FirstOrDefaultAsync(c => c.Id == id);
            var count = customViewModel.MainAttachments.Count;
            if (customViewModel.ExecutorId == User.Identity.GetUserId() && count < 10)
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    if (count < 10)
                    {
                        MainAttachModel attach = new MainAttachModel();

                        var upload = Request.Files.Get(i);

                        if (Request.Files.Get(i).FileName == "") continue;

                        if (upload != null)
                        {
                            // получаем имя файла
                            string fileName = System.IO.Path.GetFileName(upload.FileName);
                            // сохраняем файл в папку Files в проекте
                            upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                            string path = Server.MapPath("~/Files/" + fileName);

                            FileInfo fil = new FileInfo(path);
                            var fileNameNew = fil.Name;
                            int index = fileNameNew.LastIndexOf(".");
                            if (index > 0)
                                fileNameNew = fileNameNew.Substring(0, index);
                            // сохраняем файл в папку Files в проекте
                            upload.SaveAs(path);
                            attach.Id = 1;
                            attach.CustomViewModelId = id;
                            attach.AttachFilePath = path;
                            attach.AttachFileExtens = fil.Extension;
                            if (fileNameNew.Length > 20)
                            {
                                attach.AttachFileName = fileNameNew.Substring(0, 20);
                            }
                            else
                            {
                                attach.AttachFileName = fileNameNew;
                            }

                            attach.UserId = User.Identity.GetUserId();
                            db.MainAttachments.Add(attach);

                            NotificationHubModel notificationHubModel = new NotificationHubModel()
                            {
                                UserFromId = User.Identity.GetUserId(),
                                UserToId = customViewModel.ExecutorId,
                                DescriptionFrom = "Вы загрузили решение к заказу",
                                DescriptionTo = "Исполнитель " + customViewModel.Executor.UserName + " загрузил решение"
                            };

                            SendM(notificationHubModel);

                            // SendMessage("Вы загрузили решение", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, "загрузил решение");
                            await db.SaveChangesAsync();
                            count = customViewModel.MainAttachments.Count;
                        }
                    }
                }
            }
            return Json("Файл загружен");
        }

        public async Task<bool> IsExecutor(int id)
        {
            var commentViewModel = await db.Comments.FindAsync(id);
            var customId = commentViewModel.CustomViewModelId;
            var currentCustom = await db.Customs.FindAsync(customId);
            var executorId = currentCustom.ExecutorId;
            bool isExec = executorId == User.Identity.GetUserId();
            return isExec;
        }


        public FileResult GetFileAttach(string path, int? id)
        {
            AttachModel attachViewModel = db.Attachments.Include(c => c.User).Include(c => c.CustomViewModel)
                                                            .FirstOrDefault(c => c.Id == id);

            // Тип файла - content-type
            if (
                (attachViewModel.CustomViewModel.UserId == User.Identity.GetUserId() && attachViewModel.AttachStatus == AttachStatus.Purchased) ||
                attachViewModel.CustomViewModel.ExecutorId == User.Identity.GetUserId()
                )
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

        public FileResult GetFileNew(string path)
        {
            string file_type = "application/" + Path.GetExtension(path);
            string file_name = Path.GetFileName(path);

            return File(path, file_type, file_name);
        }

        //действие для скачки готового решения заказчиком 
        //осуществляется следующая проверка: остались ли еще решения для скачивания
        public async Task<bool> DownloadMainAttachment(int fileId, int customId)
        {
            var currentCustom = await db.Customs.Include(c => c.MainAttachments).FirstOrDefaultAsync(c => c.Id == customId);
            var mainAttachments = currentCustom.MainAttachments;

            string currentFilePath = "";
            //bool allFilesDownloaded = true;
            foreach (var attach in mainAttachments)
            {
                if (attach.Id == fileId)
                {
                    currentFilePath = attach.AttachFilePath;
                    if (!attach.IsDownloaded)
                    {
                        db.MainAttachments.Attach(attach);
                        //для скачиваемого в данный момент готового решения (которое ранее не скачивалось) проставляем необходимые атрибуты
                        attach.IsDownloaded = true;
                        attach.DownloadDate = DateTime.Now;
                        db.Entry(attach).State = EntityState.Modified;
                        //await db.SaveChangesAsync();
                    }

                }
            }

            await db.SaveChangesAsync();
            return true;
        }

        // GET: Custom/Details/5
        public async Task<ActionResult> Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var reqId = User.Identity.GetUserId();
            // optimaize
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                              .Include(c => c.CategoryTask)
                                                              .Include(c => c.Skill)
                                                              .Include(c => c.TypeTask)
                                                              .Include(c => c.User)
                                                              .Include(c => c.Attachments)
                                                              .Include(c => c.MyAttachments)
                                                              .Include(c => c.MainAttachments)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            var wallet = db.Wallets.Where(w => w.UserId == reqId).FirstOrDefault();
            ViewBag.WalletSumm = wallet?.Summ;

            if (customViewModel == null)
            {
                return HttpNotFound();
            }

            //пока еще не выбран исполнитель, подгружаем коллекцию дисциплин и типов задач (для возможности редактирования заказа)
            if (customViewModel.ExecutorId == null)
            {
                ViewBag.Types = new SelectList(db.CustomTypes, "Id", "Name"); // выбор типа задачи
                ViewBag.Tasks = new SelectList(db.TaskCategories, "Id", "Name"); // выбор дисциплины
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
            ViewBag.Tasks = new SelectList(db.TaskCategories, "Id", "Name");
            return View();
        }


        // POST: Custom/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,AttachFilePath,AttachFile,UserId,TypeTaskId,CategoryTaskId,SkillId,TimeBlock,PlagiarismPercentage,EndingDate,Price")] CustomViewModel customViewModel)
        {
            if (ModelState.IsValid)
            {
                customViewModel.Status = CustomStatus.Open; // открытая заявка
                customViewModel.UserId = User.Identity.GetUserId();
                customViewModel.StartDate = DateTime.Now;
                customViewModel.ExecutorStartDate = DateTime.Now;
                db.Customs.Add(customViewModel);
                await db.SaveChangesAsync();

                var count = customViewModel.MyAttachments.Count;
                if (count < 10)
                {
                    for (var i = 0; i < Request.Files.Count; i++)
                    {
                        if (count < 10)
                        {
                            MyAttachModel attach = new MyAttachModel();

                            var upload = Request.Files.Get(i);

                            if (Request.Files.Get(i).FileName == "") continue;

                            if (upload != null)
                            {
                                // получаем имя файла
                                string fileName = System.IO.Path.GetFileName(upload.FileName);
                                // сохраняем файл в папку Files в проекте
                                upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                                string path = Server.MapPath("~/Files/" + fileName);

                                FileInfo fil = new FileInfo(path);
                                var fileNameNew = fil.Name;
                                int index = fileNameNew.LastIndexOf(".");
                                if (index > 0)
                                    fileNameNew = fileNameNew.Substring(0, index);
                                // сохраняем файл в папку Files в проекте
                                upload.SaveAs(path);
                                attach.Id = 1;
                                attach.CustomViewModelId = customViewModel.Id;
                                attach.AttachFilePath = path;
                                attach.AttachFileExtens = fil.Extension;
                                if (fileNameNew.Length > 20)
                                {
                                    attach.AttachFileName = fileNameNew.Substring(0, 20);
                                }
                                else
                                {
                                    attach.AttachFileName = fileNameNew;
                                }

                                attach.UserId = User.Identity.GetUserId();
                                db.MyAttachments.Add(attach);
                                // SendMessage("Вы загрузили решение", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, "загрузил решение");
                                await db.SaveChangesAsync();
                                count = customViewModel.MyAttachments.Count;
                            }
                        }
                    }
                }
                //string uName = db.Users.Where(c => c.Id == customViewModel.UserId).FirstOrDefault().UserName;
                // SendMessage("Добавлен новый заказ", customViewModel.Id, uName);
                return RedirectToAction("Index");
            }

            return View(customViewModel);
        }

        //для отклонения исполнителя. 
        //Но сперва нужен функционал для рассмотрения заявки, а потом уже будет вызываться этот метод.
        public async Task<ActionResult> RejectExecutor(int? customId)
        {
            if (customId == null)
            {
                return new HttpNotFoundResult("CustomId is null!");
            }
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Executor).FirstOrDefaultAsync(c => c.Id == customId);
            if (customViewModel == null)
            {
                return new HttpNotFoundResult("Custom #" + customId + " wasn't found!");
            }
            db.Entry(customViewModel).State = EntityState.Modified;
            customViewModel.ExecutorId = null;
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        // GET: Custom/Create
        public async Task<ActionResult> Accept(int? id, [Bind(Include = "Id,Rating,Description,Date,UserId,OwnerId")] Review review)
        {
            if (ModelState.IsValid)
            {
                //Make a review
                review.Date = DateTime.Now;
                review.UserId = User.Identity.GetUserId();
                db.Reviews.Add(review);
                User user = await db.Users.FirstOrDefaultAsync(u => u.Id == review.OwnerId);

                //make close-custom's logics
                var transactions = await db.Transactions.Where(t => t.CustomId == id).ToListAsync();
                var executorId = await db.Customs.Where(c => c.Id == id).Select(c => c.ExecutorId).FirstOrDefaultAsync();
                var wallet = await db.Wallets.Where(w => w.UserId == executorId).FirstOrDefaultAsync();

                foreach (var t in transactions.Where(t => t.Status == TransactionStatus.Waiting))
                {
                    t.Status = TransactionStatus.Success;
                    wallet.Summ += t.Price;
                }

                // optimaize
                CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                                  .Include(c => c.CategoryTask)
                                                                  .Include(c => c.TypeTask)
                                                                  .Include(c => c.User)
                                                                  .Include(c => c.Attachments)
                                                                  .FirstOrDefaultAsync(c => c.Id == id);

                /*
                 Мысль такая: когда мы отправили заказ на доработку (метод Revision), то:
                        -в заказ проставляется атрибут IsRevision = true
                        -все новые прикрепленные решения (в заказе с таким флагом) также получают атрибут IsRevision = true
                 Когда мы закрываем заказ (метод Accept), то:
                        -снимаем атрибут IsRevision в заказе
                        -снимаем атрибут IsRevision со всех решений, являвшихся доработками
                 Логика в следующем: чтобы отличать новую доработку от старой. То есть когда цикл доработки завершен, 
                 то считаем все купленные доработки уже обычными решениями. А если откроется еще одна доработка, то мы 
                 сразу сможем отличить новые прикрепленные доработки от старых доработок. Как-то так. 
                 */
                if (customViewModel.IsRevision)
                {
                    var revisions = customViewModel.Attachments.Where(a => a.IsRevision);
                    foreach (var rev in revisions)
                    {
                        rev.IsRevision = false;
                    }
                }

                db.Entry(customViewModel).State = EntityState.Modified;
                customViewModel.Status = CustomStatus.Close;
                customViewModel.IsRevision = false;
                if (customViewModel.EndingDate != null)
                {
                    customViewModel.DoneInTime = DateTime.Now < customViewModel.EndingDate;
                }
                else
                {
                    customViewModel.DoneInTime = true;
                }
                await db.SaveChangesAsync();
                //SendMessage("Вы закрыли заказ", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, customViewModel.User.UserName + "подтвердил выполнение заказа");
                return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
            }
            return new HttpNotFoundResult("Model of review is not valid!");
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

            customViewModel.IsRevision = true;//см. коммент в методе Accept

            customViewModel.Status = CustomStatus.Revision; // на доработку
            await db.SaveChangesAsync();
            //SendMessage("Вы отправили заказ на доработку", customViewModel.Id, customViewModel.Executor.UserName, customViewModel.User.UserName, customViewModel.User.UserName + "отправил заказ на доработку");
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        public async Task<ActionResult> Cancel(int? id)
        {
            var customViewModel = await db.Customs.Include(c => c.Comments).Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            var usersWithOffers = customViewModel.Comments.Select(c => c.User);

            db.Entry(customViewModel).State = EntityState.Modified;

            var previousStatus = customViewModel.Status;
            if (previousStatus == CustomStatus.Open)
            {
                customViewModel.Status = CustomStatus.Cancelled; // Отменён
                //SendMessage("Вы отменили заказ", customViewModel.Id, customViewModel.User.UserName);
                if (usersWithOffers != null)
                {
                    foreach (var user in usersWithOffers)
                    {
                        //SendMessage(null, customViewModel.Id, null, user.UserName, customViewModel.User.UserName + " отменил заказ");
                    }
                }
            }
            if (previousStatus == CustomStatus.Cancelled)
            {
                customViewModel.Status = CustomStatus.Open; // Открыт
               // SendMessage("Вы переоткрыли заказ", customViewModel.Id, customViewModel.User.UserName);
                if (usersWithOffers != null)
                {
                    foreach (var user in usersWithOffers)
                    {
                        //SendMessage(null, customViewModel.Id, null, user.UserName, customViewModel.User.UserName + " переоткрыл заказ");
                    }
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
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
        public async Task<ActionResult> Edit([Bind(Include = "Id, Name, Description, TypeTaskId, CategoryTaskId, SkillId, UserId, StartDate, EndingDate, ExecutorStartDate, Price")] CustomViewModel customViewModel)
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
