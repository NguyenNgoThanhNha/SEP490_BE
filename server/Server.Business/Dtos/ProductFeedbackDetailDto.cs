﻿using Server.Data.Entities;
using Server.Data.MongoDb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ProductFeedbackDetailDto
    {
        public int ProductFeedbackId { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public UserDTO Customer { get; set; }
        public string? Comment { get; set; }
        public int? Rating { get; set; }
        public string Status { get; set; }
        public string? ImageBefore { get; set; }
        public string? ImageAfter { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

}
