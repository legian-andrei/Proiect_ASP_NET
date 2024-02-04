using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proiect_ASP_NET.Data;
using proiect_ASP_NET.Models;

namespace proiect_ASP_NET.Controllers
{
	public class PostsController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public PostsController(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager 
			)
		{
			_db = context;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		// Conditii de afisare a butoanelor de editare si stergere
		private void SetAccessRights()
		{
			ViewBag.AfisareButoane = false;

			if (User.IsInRole("Editor"))
			{
				ViewBag.AfisareButoane = true;
			}

			ViewBag.EsteAdmin = User.IsInRole("Admin");

			ViewBag.UserCurent = _userManager.GetUserId(User);
		}

		[NonAction]
		public IEnumerable<SelectListItem> GetAllCategories()
		{
			var selectList = new List<SelectListItem>();

			var categories = from cat in _db.Categories
							 select cat;

			foreach (var category in categories)
			{
				selectList.Add(new SelectListItem
				{
					Value = category.Id.ToString(),
					Text = category.CategoryName.ToString()
				});
			}

			return selectList;
		}

		// View
		public IActionResult Index(string sortBy)
		{
			ViewBag.SortByTitle = "title";
			ViewBag.SortByTitleDesc = "title_desc";
			ViewBag.SortByDate = "date";
			ViewBag.SortByDateDesc = "date_desc";

			var posts = _db.Posts.Include("Category").Include("User").AsQueryable();

			switch(sortBy)
			{
				case "title_desc": posts = posts.OrderByDescending(x => x.Title); break;
				case "title": posts = posts.OrderBy(x => x.Title); break;
				case "date": posts = posts.OrderBy(x => x.Date); break;

				default: posts = posts.OrderByDescending(x => x.Date); break;
			}

			ViewBag.Posts = posts;

			if (TempData.ContainsKey("message"))
			{
				ViewBag.Message = TempData["message"];
			}

			// Afisare paginata
			// Aleg sa afisez cate 5 postari/pagina
			int _perPage = 5;

			int totalItems = posts.Count();

			// Preiau nr paginii curente din view-ul asociat
			// Nr paginii este valoarea parametrului page din ruta /Articles/Index?page=valoare
			var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

			// Offset-ul va fi egal cu nr de postari care au fost deja afisate pe paginile anterioare
			var offset = 0;

			// Calculez offset-ul in functie de pagina la care sunt
			if(!currentPage.Equals(0))
			{
				offset = (currentPage - 1) * _perPage;
			}

			// Preiau postarile corespunzatoare paginii pe care ma aflu
			var paginatedPosts = posts.Skip(offset).Take(_perPage);

			// Preiau nr ultimei pagini
			ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

			ViewBag.Posts = paginatedPosts;

			return View();
		}

		// Show
		public IActionResult Show(int id)
		{
			Post post = _db.Posts.Include("Category").Include("User").Include("Comments").Include("Comments.User")
								.Where(x => x.Id == id)
								.First();

			SetAccessRights();

			return View(post);
		}

		// Adaugare comentarii unui articol
		[Authorize(Roles = "User,Editor,Admin")]
		[HttpPost]
		public IActionResult Show([FromForm] Comment comment)
		{
			SetAccessRights();
			comment.Date = DateTime.Now;
			comment.UserId = _userManager.GetUserId(User);

			if(ModelState.IsValid)
			{
				_db.Comments.Add(comment);
				_db.SaveChanges();
				return Redirect("/Posts/Show/" + comment.PostId);
			}
			else
			{
				Post p = _db.Posts.Include("Category").Include("User").Include("Comments").Include("Comments.User")
								.Where(p => p.Id == comment.Id)
								.First();

				return View(p);
			}
		}

		[Authorize(Roles = "User,Editor,Admin")]
		public IActionResult New()
		{
			Post post = new Post();

			post.Categ = GetAllCategories();

			return View(post);
		}

		[Authorize(Roles = "User,Editor,Admin")]
		[HttpPost]
		public IActionResult New(Post post)
		{
			post.Date = DateTime.Now;
			post.UserId = _userManager.GetUserId(User);

			if(ModelState.IsValid)
			{
				_db.Posts.Add(post);
				_db.SaveChanges();

				TempData["message"] = "Post has been saved.";

				return RedirectToAction("Index");
			}
			else
			{
				post.Categ = GetAllCategories();

				return View(post);
			}
		}

		[Authorize(Roles = "User,Editor,Admin")]
		public IActionResult Edit(int id)
		{
			Post post = _db.Posts.Include("Category")
								.Where(p => p.Id == id)
								.First();

			post.Categ = GetAllCategories();
			
			if(post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				return View(post);
			}
			else
			{
				TempData["message"] = "Nu puteti face modificari asupra unei postari care nu va apartine";

				return RedirectToAction("Index");
			}
		}

		[Authorize(Roles = "User,Editor,Admin")]
		[HttpPost]
		public IActionResult Edit(int id, Post requestPost)
		{
			Post post = _db.Posts.Find(id);

			if (ModelState.IsValid)
			{
				if(post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin") || User.IsInRole("Editor"))
				{
					post.Title = requestPost.Title;
					post.Content = requestPost.Content;
					post.Category = requestPost.Category;
					_db.SaveChanges();

					TempData["message"] = "Postarea a fost modificata cu succes.";

					return RedirectToAction("Index");

				}
				else
				{
					TempData["message"] = "Nu puteti face modificari asupra unei postari care nu va apartine";

					return RedirectToAction("Index");
				}
			}
			else
			{
				requestPost.Categ = GetAllCategories();

				return View(requestPost);
			}
		}

		[Authorize(Roles = "User,Editor,Admin")]
		[HttpPost]
		public IActionResult Delete(int id)
		{
			Post post = _db.Posts.Include("Comments")
								.Where(p => p.Id == id)
								.FirstOrDefault();

			if(post.UserId == _userManager.GetUserId(User) || User.IsInRole("Editor") || User.IsInRole("Admin"))
			{
				_db.Posts.Remove(post);
				_db.SaveChanges();

				TempData["message"] = "Postarea a fost stearsa";

				return RedirectToAction("Index");
			}
			else
			{
				TempData["message"] = "Nu puteti sterge o postare care nu va apartine";

				return RedirectToAction("Index");
			}
		}
	}
}
