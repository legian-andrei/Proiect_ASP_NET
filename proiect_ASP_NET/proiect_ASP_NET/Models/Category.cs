using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace proiect_ASP_NET.Models
{
	public class Category
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Name is required")]
		public string CategoryName { get; set; }

		[Required(ErrorMessage = "Description is required")]
		public string Descriere { get; set; }

		public virtual ICollection<Post>? Posts { get; set; }
	}
}
