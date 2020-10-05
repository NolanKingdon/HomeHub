using System.Collections.Generic;

namespace HomeHub.Web.Dto
{
    public class DescriptiveGenresDto
    {
        public string Artist { get; set; }
        public string SongName { get; set; }
        public List<string> Genres { get; set; }
    }
}
