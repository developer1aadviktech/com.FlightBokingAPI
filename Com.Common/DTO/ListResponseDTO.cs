using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.DTO
{
    public class ListResponseDTO
    {
        public Metadata metaData { get; set; }
        public object items { get; set; }
    }

    public class Metadata
    {
        public int totalItems { get; set; }
        public int itemsPerPage { get; set; }
        public int totalPages { get; set; }
    }
}
