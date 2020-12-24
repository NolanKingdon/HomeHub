using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HomeHub.Web.Dto
{
    public class DescriptiveGenresDto
    {
        public string Artist { get; set; }
        public string SongName { get; set; }
        public Collection<string> Genres { get; set; }
    }
}
