using Application.Interfaces.Repository;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        IRank Rank { get; }

        public Task<List<DTOMasterResponse>> GetAllMMaster(DTOMasterRequest Data);
    }
}
