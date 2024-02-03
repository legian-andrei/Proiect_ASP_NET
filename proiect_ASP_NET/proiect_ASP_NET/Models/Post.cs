using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect_ASP_NET.Models
{
	public class Post
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Title is required!")]
		[StringLength(100, ErrorMessage = "Title's maximum length is 100 chars!")]
		[MinLength(5, ErrorMessage = "Title's minimum lenth is 5 chars!")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Content is required!")]
		public string Content { get; set; }

		public DateTime Date { get; set; }

		[Required(ErrorMessage = "Category is required!")]
		public int? CategoryId { get; set; }
		public virtual Category? Category { get; set; }

		public virtual ICollection<Comment>? Comments { get; set; }

		public virtual ApplicationUser? User { get; set; }

		public string? UserId { get; set; }

		[NotMapped]
		public IEnumerable<SelectListItem>? Categ { get; set; }
	}
}
