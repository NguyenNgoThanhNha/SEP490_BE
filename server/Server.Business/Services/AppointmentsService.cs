using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

public class AppointmentsService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
        private readonly AppDbContext _context;

    {
        _mapper = mapper;
            _context = context;
    }

    {
        {
        }
        var totalCount = listAppointments.Count();
        
        {
            }
        };
    }

    {
        }


    {
        }
        {
        
        }
        
        
        {
        };
        var appointmentsEntity = await _unitOfWorks.AppointmentsRepository.AddAsync(_mapper.Map<Appointments>(createNewAppointments));
        var result = await _unitOfWorks.AppointmentsRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<AppointmentsModel>(appointmentsEntity);
        }
        return null;
    }
    
    {
        {
        }
        
        {
        }
        {
    }
        {
    }

        {
        }
            {
    }
        }

        return null;
    }
}