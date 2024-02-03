using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace proiect_ASP_NET.Models
{
	public class Comment
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Content is required")]
		public string Content { get; set; }

		public DateTime Date { get; set; }

		public int? PostId { get; set; }
		public virtual Post? Post { get; set; }

		public string? UserId { get; set; }

		public virtual ApplicationUser? User { get; set; }
	}
}
