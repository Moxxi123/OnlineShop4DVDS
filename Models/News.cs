using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models
{
	public class News
	{
		public int Id { get; set; }

		public string Title { get; set; }

		public string Summary { get; set; }

		public string NewsContent { get; set; }

		public string NewsCategory { get; set; }

		public DateTime PublishDate { get; set; }

        [ValidateNever]
        public string? ImageUrl { get; set; }

        public bool Status { get; set; }
	}
}
