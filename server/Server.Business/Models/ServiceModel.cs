﻿using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Digests;

namespace Server.Business.Models
{
    public class ServiceModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public string? Status { get; set; }
    
        public string? Steps { get; set; }
        
        public int ServiceCategoryId  { get; set; }
        public virtual ServiceCategoryModel ServiceCategory { get; set; }
    
        public ICollection<Branch_ServiceModel> Branch_Services { get; set; }
        public string[] images { get; set; }
        public ICollection<ServiceImages> ServiceImages { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
