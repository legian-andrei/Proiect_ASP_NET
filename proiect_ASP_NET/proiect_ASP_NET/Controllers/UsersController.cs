using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proiect_ASP_NET.Data;
using proiect_ASP_NET.Models;

namespace proiect_ASP_NET.Controllers
{
	[Authorize(Roles = "Admin")]
	public class UsersController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public UsersController(
			ApplicationDbContext context, 
			UserManager<ApplicationUser> userManager, 
			RoleManager<IdentityRole> roleManager)
		{
			_db = context;

			_userManager = userManager;

			_roleManager = roleManager;
		}

		[NonAction]
		public IEnumerable<SelectListItem> GetAllRoles()
		{
			var selectList = new List<SelectListItem>();

			var roles = from role in _db.Roles
						select role;

			foreach ( var role in roles )
			{
				selectList.Add(new SelectListItem
				{
					Value = role.Id.ToString(),
					Text = role.Name.ToString()
				});
			}

			return selectList;
		}

		// View
		public IActionResult Index()
		{
			var users = from user in _db.Users
						orderby user.UserName
						select user;

			ViewBag.UsersList = users;

			return View();
		}

		// Show
		public async Task<ActionResult> Show(string id)
		{
			ApplicationUser user = _db.Users.Find(id);
			var roles = await _userManager.GetRolesAsync(user);

			ViewBag.RolesList = roles;

			return View(user);
		}

		// Edit
		public async Task<ActionResult> Edit(string id)
		{
			ApplicationUser user = _db.Users.Find(id);

			user.AllRoles = GetAllRoles();

			// Lista de nume de roluri
			var roleNames = await _userManager.GetRolesAsync(user);

			// Caut Id-ul rolului in baza de date
			var currentUserRole = _roleManager.Roles
											.Where(x => roleNames.Contains(x.Name))
											.Select(x => x.Id)
											.First();
			ViewBag.UserRole = currentUserRole;

			return View(user);
		}

		[HttpPost]
		public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
		{
			ApplicationUser user = _db.Users.Find(id);

			user.AllRoles = GetAllRoles();

			if (ModelState.IsValid)
			{
				user.UserName = newData.UserName;
				user.Email = newData.Email;
				user.FirstName = newData.FirstName;
				user.LastName = newData.LastName;
				user.PhoneNumber = newData.PhoneNumber;

				// Caut toate rolurile din baza de date
				var roles = _db.Roles.ToList();

				foreach (var role in roles)
				{
					// Scot userul din rolurile vechi
					await _userManager.RemoveFromRoleAsync(user, role.Name);
				}

				// Adaug noul rol selectat
				var roleName = await _roleManager.FindByIdAsync(newRole);
				await _userManager.AddToRoleAsync(user, roleName.ToString());

				_db.SaveChanges();
			}

			return RedirectToAction("Index");
		}

		// Delete
		[HttpPost]
		public IActionResult Delete(string id)
		{
			var user = _db.Users
						.Include("Posts")
						.Include("Comments")
						.Where(x => x.Id == id)
						.First();

			// Sterg postarile userului
			if(user.Posts.Count > 0)
			{
				foreach (var post in user.Posts)
				{
					_db.Posts.Remove(post);
				}
			}

			// Sterg comentariile userului
			if(user.Comments.Count > 0)
			{
				foreach (var comment in user.Comments)
				{
					_db.Comments.Remove(comment);
				}
			}

			// Sterg userul
			_db.ApplicationUsers.Remove(user);
			_db.SaveChanges();

			return RedirectToAction("Index");
		}
	}
}
