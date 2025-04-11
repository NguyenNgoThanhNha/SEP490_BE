﻿using Server.Data.Base;
using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data.Repositories
{
    public class ServiceRoutineRepository: GenericRepository<ServiceRoutine, int>
    {
        public ServiceRoutineRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
