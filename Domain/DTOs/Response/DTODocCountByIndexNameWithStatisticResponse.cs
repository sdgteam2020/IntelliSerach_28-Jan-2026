using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTODocCountByIndexNameWithStatisticResponse
    {
        public DTODocCountByIndexNameResponse DTODocCountByIndexName { get; set; }
        public List<DTOStatisticsHit> DTOStatisticsHits { get; set; }
    }
}
