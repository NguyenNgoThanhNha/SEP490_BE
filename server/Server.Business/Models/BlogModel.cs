using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class BlogModel
    {
        public int BlogId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Thumbnail { get; set; }

        public int AuthorId { get; set; }

        public string AuthorName { get; set; }

        public string Status { get; set; }

        public string Note { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
