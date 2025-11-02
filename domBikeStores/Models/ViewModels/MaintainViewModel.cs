using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using domBikeStores.Models;

namespace domBikeStores.ViewModels
{
    public class MaintainViewModel
    {
        public IEnumerable<staffs> Staffs { get; set; }
        public IEnumerable<customers> Customers { get; set; }
        public IEnumerable<products> Products { get; set; }

        public IEnumerable<stores> Stores { get; set; }
        public IEnumerable<brands> Brands { get; set; }
        public IEnumerable<categories> Categories { get; set; }

        public int StaffPage { get; set; }
        public int CustPage { get; set; }
        public int ProdPage { get; set; }
    }
}