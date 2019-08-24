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
        public IQueryable<CustomViewModel> GetCustomViewModels()
        {
            return db.Customs;
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
        [ResponseType(typeof(CustomViewModel))]
        public async Task<IHttpActionResult> PostCustomViewModel(CustomViewModel customViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Customs.Add(customViewModel);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = customViewModel.Id }, customViewModel);
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