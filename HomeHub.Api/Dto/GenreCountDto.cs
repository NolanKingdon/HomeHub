using System.Collections.Generic;

namespace HomeHub.Api.Dto
{
    public class GenreCountDto
    {
        public int TotalCount { get; set; }
        public Dictionary<string, int> GenreCounts { get; set; } = new Dictionary<string, int>();
    }
}
