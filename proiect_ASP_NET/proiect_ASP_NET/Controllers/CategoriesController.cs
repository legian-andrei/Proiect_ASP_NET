using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect_ASP_NET.Data;
using proiect_ASP_NET.Models;

namespace proiect_ASP_NET.Controllers
{
	public class CategoriesController : Controller 
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public CategoriesController(ApplicationDbContext context)
		{
			_db = context;
		}

		private void SetAccessRights()
		{
			ViewBag.AfisareButoane = false;

			if(User.IsInRole("Admin"))
			{
				ViewBag.AfisareButoane = true;
			}
		}

		// View
		public ActionResult Index()
		{
			if (TempData.ContainsKey("message"))
			{
				ViewBag.message = TempData["message"].ToString();
			}

			var categories = from category in _db.Categories.Include("Posts")
							 select new Category()
							 {
								 CategoryName = category.CategoryName,
								 Id = category.Id,
								 Descriere = category.Descriere,
								 Posts = (from p in category.Posts
										  select new Post()
										  {
											  Title = p.Title,
											  Content = p.Content,
											  Date = p.Date
										  }).ToList()
							 };
			ViewBag.Categories = categories;
			return View();
		}

		// Show
		public ActionResult Show(int id)
		{
			Category category = _db.Categories.Include("Posts")
								.Include("Posts.User")
								.Where(p => p.Id == id)
								.First();

			ViewBag.Category = category;
			SetAccessRights();

			return View();
		}

		// New
		[Authorize(Roles = "Admin")]
		public ActionResult New()
		{
			return View();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public ActionResult New(Category category)
		{
			try
			{
				_db.Categories.Add(category);
				_db.SaveChanges();

				TempData["message"] = "Category has been added!";

				return RedirectToAction("Index");
			}
			catch (Exception exc)
			{
				return View(category);
			}
		}

		// Edit
		[Authorize(Roles = "Admin")]
		public ActionResult Edit(int id)
		{
			Category category = _db.Categories.Find(id);
			ViewBag.Category = category;

			SetAccessRights();
			
			return View();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public ActionResult Edit(int id, Category requestCategory)
		{
			SetAccessRights();

			Category category = _db.Categories.Find(id);

			if(ModelState.IsValid)
			{
				category.CategoryName = requestCategory.CategoryName;
				category.Descriere = requestCategory.Descriere;
				_db.SaveChanges();

				TempData["message"] = "Category was modified!";

				return RedirectToAction("Index");
			}
			else
			{
				return View(requestCategory);
			}
		}

		// Delete
		[Authorize(Roles = "Admin")]
		[HttpPost]
		public ActionResult Delete(int id)
		{
			SetAccessRights();

			Category category = _db.Categories.Find(id);
			_db.Categories.Remove(category);
			_db.SaveChanges();

			TempData["message"] = "Category was deleted!";

			return RedirectToAction("Index");
		}
	}
}
