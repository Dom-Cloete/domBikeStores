using domBikeStores.Models;
using domBikeStores.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace domBikeStores.Controllers
{
    public class MaintainController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Maintain
        public async Task<ActionResult> Index(
        int staffPage = 1, string staffSearch = "",
        int custPage = 1, string custSearch = "",
        int prodPage = 1, string prodSearch = "")
            {
                const int pageSize = 3;

                // STAFF
                var staffsQuery = db.staffs.Include(s => s.stores)
                                            .Include(s => s.orders.Select(o => o.order_items.Select(oi => oi.products)))
                                            .AsQueryable();
                if (!string.IsNullOrEmpty(staffSearch))
                    staffsQuery = staffsQuery.Where(s => s.first_name.Contains(staffSearch) || s.last_name.Contains(staffSearch));

                var staffList = await staffsQuery
                    .OrderBy(s => s.staff_id)
                    .Skip((staffPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // CUSTOMERS
                var custQuery = db.customers.Include(c => c.orders.Select(o => o.order_items.Select(oi => oi.products))).AsQueryable();
                if (!string.IsNullOrEmpty(custSearch))
                    custQuery = custQuery.Where(c => c.first_name.Contains(custSearch) || c.last_name.Contains(custSearch));

                var custList = await custQuery
                    .OrderBy(c => c.customer_id)
                    .Skip((custPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // PRODUCTS
                var prodQuery = db.products.Include(p => p.brands).Include(p => p.categories).AsQueryable();
                if (!string.IsNullOrEmpty(prodSearch))
                    prodQuery = prodQuery.Where(p => p.product_name.Contains(prodSearch));

                var prodList = await prodQuery
                    .OrderBy(p => p.product_id)
                    .Skip((prodPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModel = new MaintainViewModel
                {
                    Staffs = staffList,
                    Customers = custList,
                    Products = prodList,
                    Stores = await db.stores.ToListAsync(),
                    Brands = await db.brands.ToListAsync(),
                    Categories = await db.categories.ToListAsync(),
                    StaffPage = staffPage,
                    CustPage = custPage,
                    ProdPage = prodPage
                };

                return View(viewModel);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStaff(staffs staff)
        {
            if (ModelState.IsValid)
            {
                db.staffs.Add(staff);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStaff(staffs staff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staff).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteStaff(int id)
        {
            var staff = await db.staffs.FindAsync(id);
            if (staff != null)
            {
                db.staffs.Remove(staff);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCustomer(customers customer)
        {
            if (ModelState.IsValid)
            {
                db.customers.Add(customer);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateCustomer(customers customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            var customer = await db.customers.FindAsync(id);
            if (customer != null)
            {
                db.customers.Remove(customer);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProduct(products product)
        {
            if (ModelState.IsValid)
            {
                db.products.Add(product);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProduct(products product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await db.products.FindAsync(id);
            if (product != null)
            {
                db.products.Remove(product);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}