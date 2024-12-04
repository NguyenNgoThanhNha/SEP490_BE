using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class BlogRequest
    {
        [Required(ErrorMessage = "Title is required!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required!")]
        public string Content { get; set; }

        [Required(ErrorMessage = "AuthorId is required!")]
        public int AuthorId { get; set; }

        public string Status { get; set; }

        public string Note { get; set; }

        public DateTime CreatedDate { get; set; } 

        public DateTime UpdatedDate { get; set; } 
    }
}
