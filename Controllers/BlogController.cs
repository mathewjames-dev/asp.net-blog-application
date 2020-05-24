﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogApplication.Data;
using BlogApplication.Models.Posts;
using Microsoft.AspNetCore.Authorization;

namespace BlogApplication.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext db;

        public BlogController(ApplicationDbContext context)
        {
            db = context;
        }

        // GET: Blog
        public async Task<IActionResult> Index()
        {
            return View(await db.Categories.ToListAsync());
        }

        // GET: Blog/{slug}
        public async Task<IActionResult> Details(string slug)
        {
            if (slug == null)
            {
                return NotFound();
            }

            var post = await db.Posts
                .FirstOrDefaultAsync(m => m.UrlSlug == slug);

            if (post == null)
            {
                return NotFound();
            }

            DateTime Modified = new DateTime();      
            Modified = Convert.ToDateTime(post.Modified);
            ViewData["modified_date"] = Modified.ToLongDateString();

            var relatedPosts = await db.Posts.Where(m => m.Id != post.Id)
                .ToListAsync();
            ViewData["related_posts"] = relatedPosts;

            return View(post);
        }

        [Authorize(Policy = "RequireAdminRights")]
        // GET: Blog/Create
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "RequireAdminRights")]
        // POST: Blog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,UrlSlug,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Add(category);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [Authorize(Policy = "RequireAdminRights")]
        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Blog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Policy = "RequireAdminRights")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,UrlSlug,Description")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Update(category);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Blog/Delete/5
        [Authorize(Policy = "RequireAdminRights")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await db.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdminRights")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id);
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return db.Categories.Any(e => e.Id == id);
        }
    }
}
