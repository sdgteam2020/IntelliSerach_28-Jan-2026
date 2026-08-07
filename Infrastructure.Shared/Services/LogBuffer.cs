using Domain.DTOs.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Shared.Services
{
    public class LogBuffer
    {
        private readonly ConcurrentQueue<DTOLogEntryRequest> _queue = new();

        public void Enqueue(DTOLogEntryRequest log)
        {
            _queue.Enqueue(log);
        }

        public List<DTOLogEntryRequest> DequeueBatch(int batchSize)
        {
            var list = new List<DTOLogEntryRequest>();

            while (list.Count < batchSize && _queue.TryDequeue(out var log))
            {
                list.Add(log);
            }

            return list;
        }

        public int Count => _queue.Count;
    }
}
