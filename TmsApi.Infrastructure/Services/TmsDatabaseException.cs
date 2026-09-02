
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using Microsoft.Extensions.Logging;

using TmsApi.Application.DTOs;
namespace TmsApi.Application.Interfaces;
public class TmsDatabaseException : Exception
{
    public TmsDatabaseException(string message)
        : base(message)
    {
    }
}