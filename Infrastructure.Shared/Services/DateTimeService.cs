using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Shared.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime NowUtc => DateTime.UtcNow;

        public string NowUtcString => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
