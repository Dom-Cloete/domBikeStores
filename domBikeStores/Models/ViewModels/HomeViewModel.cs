using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using domBikeStores.Models;

namespace domBikeStores.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<staffs> StaffList { get; set; }
        public IEnumerable<customers> CustomerList { get; set; }
        public IEnumerable<products> ProductList { get; set; }

        // filtering products
        public string SelectedBrand { get; set; }
        public string SelectedCategory { get; set; }
        public IEnumerable<brands> BrandList { get; set; }
        public IEnumerable<categories> CategoryList { get; set; }

        // pagination
        public int StaffPage { get; set; }
        public int CustPage { get; set; }
        public int ProdPage { get; set; }
    }
}