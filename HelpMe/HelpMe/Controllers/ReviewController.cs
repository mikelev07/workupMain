﻿using System;
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

namespace HelpMe.Controllers
{
    public class ReviewController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Review
        public async Task<ActionResult> Index()
        {
            var reviews = db.Reviews.Include(r => r.Owner).Include(r => r.User);
            return View(await reviews.ToListAsync());
        }

        // GET: Review/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = await db.Reviews.FindAsync(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // GET: Review/Create
        public ActionResult Create()
        {
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "Description");
            ViewBag.UserId = new SelectList(db.Users, "Id", "Description");
            return View();
        }

        // POST: Review/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Rating,Description,Date,UserId,OwnerId")] Review review)
        {
            if (ModelState.IsValid)
            {
                review.Date = DateTime.Now;
                review.UserId = User.Identity.GetUserId();
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                User user = await db.Users.FirstOrDefaultAsync(u => u.Id == review.OwnerId);
                return RedirectToAction("Details", "User", new { userName = user.UserName });
            }

           // ViewBag.OwnerId = new SelectList(db.Users, "Id", "Description", review.OwnerId);
            //ViewBag.UserId = new SelectList(db.Users, "Id", "Description", review.UserId);
            return View(review);
        }

        // GET: Review/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = await db.Reviews.FindAsync(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "Description", review.OwnerId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Description", review.UserId);
            return View(review);
        }

        // POST: Review/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,UserId,OwnerId")] Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "Description", review.OwnerId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Description", review.UserId);
            return View(review);
        }

        // GET: Review/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = await db.Reviews.FindAsync(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: Review/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Review review = await db.Reviews.FindAsync(id);
            db.Reviews.Remove(review);
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
