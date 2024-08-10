using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class FilterRequestModel
    {
        public int[] ContentTypeId { get; set; }
        public int[] CategoryId { get; set; }
        public int[] ProducerId { get; set; }
        public int[] ArtistId { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string SortOrder { get; set; }
    }
}
