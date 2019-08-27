using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using HelpMe.Models;
using HelpMe.Models.API;

namespace HelpMe.Controllers
{
    public class CustomsValuesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/CustomsValues
        public IQueryable<CustomApiModel> GetCustomViewModels()
        {
            IQueryable<CustomViewModel> customViewModels = db.Customs.Include(t => t.CategoryTask)
                                                    .Include(c => c.TypeTask)
                                                    .Include(c => c.Skill);



            var items = from u in customViewModels
                        select new CustomApiModel
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Description = u.Description,
                            Status = u.Status,
                            DoneInTime = u.DoneInTime,
                            StartDate = u.StartDate,
                            EndingDate = u.EndingDate,
                            ExecutorStartDate = u.ExecutorStartDate,
                            ExecutorName = u.Executor.UserName,
                            CategoryTaskName = u.CategoryTask.Name,
                            SkillName = u.Skill.Name,
                            TypeTaskName = u.TypeTask.Name,
                            Price = u.Price,
                            UserName = u.User.UserName,
                            ExecutorPrice = u.ExecutorPrice,
                            IsRevision = u.IsRevision
                        };

            return items;
        }

        // GET: api/CustomsValues/5
        [ResponseType(typeof(CustomApiModel))]
        public async Task<IHttpActionResult> GetCustomViewModel(int id)
        {
            CustomViewModel customViewModel = await db.Customs.Include(t => t.CategoryTask)
                                                    .Include(c => c.TypeTask)
                                                    .Include(c => c.Skill)
                                                    .Where(i => i.Id == id).SingleOrDefaultAsync();
                                                   

            if (customViewModel == null)
            {
                return NotFound();
            }

            CustomApiModel customApi = new CustomApiModel()
            {
                Id = customViewModel.Id,
                Name = customViewModel.Name,
                Description = customViewModel.Description,
                Status = customViewModel.Status,
                DoneInTime = customViewModel.DoneInTime,
                StartDate = customViewModel.StartDate,
                EndingDate = customViewModel.EndingDate,
                ExecutorStartDate = customViewModel.ExecutorStartDate,
                ExecutorName = customViewModel.Executor.UserName,
                CategoryTaskName = customViewModel.CategoryTask.Name,
                SkillName = customViewModel.Skill.Name,
                TypeTaskName = customViewModel.TypeTask.Name,
                Price = customViewModel.Price,
                UserName = customViewModel.User.UserName,
                ExecutorPrice = customViewModel.ExecutorPrice,
                IsRevision = customViewModel.IsRevision
            };

            return Ok(customApi);
        }

        // PUT: api/CustomsValues/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutCustomViewModel(int id, CustomViewModel customViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != customViewModel.Id)
            {
                return BadRequest();
            }

            db.Entry(customViewModel).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomViewModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/CustomsValues
        [ResponseType(typeof(CustomApiModel))]
        public async Task<IHttpActionResult> PostCustomViewModel(CustomApiModel customViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CustomViewModel custom = new CustomViewModel()
            {
               // Id = 1,
                Name = customViewModel.Name,
                Description = "",
                Status = CustomStatus.Open,
                DoneInTime = true,
                StartDate = new DateTime(2019, 8, 28),
                EndingDate = new DateTime(2019, 8, 28),
                ExecutorStartDate = new DateTime(2019, 8, 28),
                TypeTaskId = 1,
                IsRevision = false,
                CategoryTaskId = 1,
                Price = 100,
                UserId = "32418b6e-111e-47c5-8e1e-cc3ab59b2fe0"
            };

            db.Customs.Add(custom);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = custom.Id }, customViewModel);
        }

        // DELETE: api/CustomsValues/5
        [ResponseType(typeof(CustomViewModel))]
        public async Task<IHttpActionResult> DeleteCustomViewModel(int id)
        {
            CustomViewModel customViewModel = await db.Customs.FindAsync(id);
            if (customViewModel == null)
            {
                return NotFound();
            }

            db.Customs.Remove(customViewModel);
            await db.SaveChangesAsync();

            return Ok(customViewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CustomViewModelExists(int id)
        {
            return db.Customs.Count(e => e.Id == id) > 0;
        }
    }
}