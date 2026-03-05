using examenCrudMVC_NFramework.Models;
using examenCrudMVC_NFramework.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace examenCrudMVC_NFramework.Controllers
{
    public class EmpleadoController : Controller
    {
        private examenCrudMVC_NFrameworkEntities _dbContext = new examenCrudMVC_NFrameworkEntities();

        public async Task<ActionResult> Index(int? idDep)
        {
            var vm = new EmpleadoVM();
            ViewBag.IdSeleccionado = idDep;

            try
            {
                vm.ListaDepartamentos = await _dbContext.Departamentos.ToListAsync();
        
                if (idDep > 0)
                {
                    // 1. Primero ejecutamos el SP y guardamos el resultado en la lista del VM
                    vm.ListaEmpleados = await _dbContext.Empleados
                        .SqlQuery("exec sp_ListarEmpleadoPorIdDep @idDepartamento", new SqlParameter("@idDepartamento", idDep))
                        .ToListAsync();

                    // 2. Ahora que la lista NO es nula, hacemos el parche de las relaciones
                    foreach (var emp in vm.ListaEmpleados)
                    {
                        emp.IdDepartamentoNavigation = vm.ListaDepartamentos
                            .FirstOrDefault(d => d.idDepartamento == emp.idDepartamento);
                    }
                }
                else
                {
                    vm.ListaEmpleados = await _dbContext.Empleados
                        .Include(tD => tD.IdDepartamentoNavigation)
                        .OrderByDescending(a => a.activo)
                        .ToListAsync();
                }
                return View(vm);
            }
            catch (Exception ex)
            {
                vm.ListaEmpleados = new List<Empleado>();
                vm.ListaDepartamentos = new List<Departamento>();
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Guardar(EmpleadoVM vm)
        {
            try
            {
                _dbContext.Empleados.Add(vm.EmpleadoModelReference);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int? idEmp)
        {
            var vm = new EmpleadoVM();
            try
            {
                vm.ListaDepartamentos = await _dbContext.Departamentos.ToListAsync();
                if (idEmp == null) return RedirectToAction("Index");
                var empleado = await _dbContext.Empleados.FindAsync(idEmp);
                if (empleado == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    vm.EmpleadoModelReference = empleado;
                    return View(vm);
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(EmpleadoVM emp)
        {
            try
            {
                // 1. "Atachamos" la entidad y le decimos a EF: "Esto ya existe, solo cámbiale el estado"
                _dbContext.Entry(emp.EmpleadoModelReference).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Eliminar(int? idEmp)
        {

            try
            {
                var empleado = await _dbContext.Empleados.FindAsync(idEmp);
                if (empleado == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    _dbContext.Empleados.Remove(empleado);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Estado(int idEmp,int? idDep)
        {
            try
            {
                await _dbContext.Database.
                    ExecuteSqlCommandAsync("exec sp_EstadoEmpleado {0}", idEmp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index", new { idDep = idDep });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
