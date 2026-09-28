using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTOGetDoc_WebCountResponse
    {
        public List<SearchResponseDto> Responses { get; set; }
    }

    public class SearchResponseDto
    {
        public HitsDto Hits { get; set; }
    }

    public class HitsDto
    {
        public TotalDto Total { get; set; }
    }

    public class TotalDto
    {
        public int Value { get; set; }
    }
}
