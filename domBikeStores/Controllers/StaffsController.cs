using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using domBikeStores.Models;

namespace domBikeStores.Controllers
{
    public class StaffsController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Staffs
        public async Task<ActionResult> Index()
        {
            var staffs = db.staffs.Include(s => s.staffs2).Include(s => s.stores);
            return View(await staffs.ToListAsync());
        }

        // GET: Staffs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            staffs staffs = await db.staffs.FindAsync(id);
            if (staffs == null)
            {
                return HttpNotFound();
            }
            return View(staffs);
        }

        // GET: Staffs/Create
        public ActionResult Create()
        {
            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name");
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name");
            return View();
        }

        // POST: Staffs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "staff_id,first_name,last_name,email,phone,active,store_id,manager_id")] staffs staffs)
        {
            if (ModelState.IsValid)
            {
                db.staffs.Add(staffs);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name", staffs.manager_id);
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name", staffs.store_id);
            return View(staffs);
        }

        // GET: Staffs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            staffs staffs = await db.staffs.FindAsync(id);
            if (staffs == null)
            {
                return HttpNotFound();
            }
            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name", staffs.manager_id);
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name", staffs.store_id);
            return View(staffs);
        }

        // POST: Staffs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "staff_id,first_name,last_name,email,phone,active,store_id,manager_id")] staffs staffs)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staffs).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name", staffs.manager_id);
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name", staffs.store_id);
            return View(staffs);
        }

        // GET: Staffs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            staffs staffs = await db.staffs.FindAsync(id);
            if (staffs == null)
            {
                return HttpNotFound();
            }
            return View(staffs);
        }

        // POST: Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            staffs staffs = await db.staffs.FindAsync(id);
            db.staffs.Remove(staffs);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
