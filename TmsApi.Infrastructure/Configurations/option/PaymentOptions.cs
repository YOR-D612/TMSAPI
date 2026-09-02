using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using Microsoft.Extensions.Logging;

using TmsApi.Application.DTOs;

public class PaymentOptions
{
    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}