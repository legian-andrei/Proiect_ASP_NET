using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using proiect_ASP_NET.Data;
using proiect_ASP_NET.Models;

namespace proiect_ASP_NET.Controllers
{
	public class CommentsController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public CommentsController(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager, 
			RoleManager<IdentityRole> roleManager 
			)
		{
			_db = context;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		[HttpPost]
		public IActionResult New(Comment comm)
		{
			comm.Date = DateTime.Now;

			if(ModelState.IsValid)
			{
				_db.Comments.Add( comm );
				_db.SaveChanges();

				return Redirect("/Posts/Show/" + comm.PostId );
			}
			else
			{
				return Redirect("/Posts/Show/" + comm.PostId);
			}
		}

		[Authorize(Roles = "User,Editor,Admin")]
		public IActionResult Edit(int id)
		{
			Comment comm = _db.Comments.Find(id);

			if(comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				return View(comm);
			}
			else
			{
				TempData["message"] = "Nu aveti dreptul sa editati comentariul";

				return RedirectToAction("Index", "Posts");
			}
		}

		[Authorize(Roles = "User,Editor,Admin")]
		[HttpPost]
		public IActionResult Edit(int id, Comment requestComment)
		{
			Comment comm = _db.Comments.Find(id);

			if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				if (ModelState.IsValid)
				{
					comm.Content = requestComment.Content;
					_db.SaveChanges();

					return Redirect("/Posts/Show/" + comm.PostId);
				}
				else
				{
					return View(requestComment);
				}
			}
			else
			{
				TempData["message"] = "Nu aveti dreptul sa editati comentariul";

				return RedirectToAction("Index", "Posts");
			}
		}

		[Authorize(Roles = "User,Editor,Admin")]
		[HttpPost]
		public IActionResult Delete(int id)
		{
			Comment comment = _db.Comments.Find(id);

			if(comment.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				_db.Comments.Remove(comment);
				_db.SaveChanges();

				return Redirect("/Posts/Show/" + comment.PostId);
			}
			else
			{
				TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";

				return RedirectToAction("Index", "Posts");
			}
		}
	}
}
