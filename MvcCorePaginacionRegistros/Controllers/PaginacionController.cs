using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using MvcCorePaginacionRegistros.Models;
using MvcCorePaginacionRegistros.Repositories;

namespace MvcCorePaginacionRegistros.Controllers
{
    public class PaginacionController : Controller
    {
        private RepositoryHospital repo;

        public PaginacionController(RepositoryHospital repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Departamentos()
        {
            List<Departamento> departamentos = await this.repo.GetDepartamentosAsync();
            return View(departamentos);
        }

        public async Task<IActionResult> EmpleadosDepartamento
            (int? posicion, int iddepartamento)
        {
            if (posicion == null)
            {
                //POSICION PARA EL EMPLEADO
                posicion = 1;
            }
            ModelEmpleadoPaginacion model = await
                this.repo.GetEmpleadoDepartamentoAsync
                (posicion.Value, iddepartamento);
            Departamento departamento = 
                await this.repo.FindDepartamentosAsync(iddepartamento);
            ViewData["DEPARTAMENTOSELECCIONADO"] = departamento;
            ViewData["REGISTROS"] = model.Registros;
            ViewData["DEPARTAMENTO"] = iddepartamento;
            int siguiente = posicion.Value + 1;
            //DEBEMOS COMPROBAR QUE NO PASAMOS DEL NUMERO DE REGISTROS
            if (siguiente > model.Registros)
            {
                //EFECTO OPTICO
                siguiente = model.Registros;
            }
            int anterior = posicion.Value - 1;
            if (anterior < 1)
            {
                anterior = 1;
            }
            ViewData["ULTIMO"] = model.Registros;
            ViewData["SIGUIENTE"] = siguiente;
            ViewData["ANTERIOR"] = anterior;
            ViewData["POSICION"] = posicion;
            return View(model.Empleado);
        }


        public async Task<IActionResult> EmpleadosOficioOut
            (int? posicion, string oficio)
        {
            if (posicion == null)
            {
                posicion = 1;
                return View();
            }
            else
            {
                ModelPaginacionEmpleados model = await
                    this.repo.GetGrupoEmpleadosOficioOutAsync(posicion.Value, oficio);
                ViewData["REGISTROS"] = model.NumeroRegistros;
                ViewData["OFICIO"] = oficio;
                return View(model.Empleados);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadosOficioOut
            (string oficio)
        {
            ModelPaginacionEmpleados model = await
                this.repo.GetGrupoEmpleadosOficioOutAsync(1, oficio);
            ViewData["REGISTROS"] = model.NumeroRegistros;
            ViewData["OFICIO"] = oficio;
            return View(model.Empleados);
        }

        public async Task<IActionResult> EmpleadosOficio
            (int? posicion, string oficio)
        {
            if (posicion == null)
            {
                posicion = 1;
                return View();
            }
            else
            {
                List<Empleado> empleados = await
                    this.repo.GetGrupoEmpleadosOficioAsync(posicion.Value, oficio);
                int registros = await this.repo.GetNumeroEmpleadosOficioAsync(oficio);
                ViewData["REGISTROS"] = registros;
                ViewData["OFICIO"] = oficio;
                return View(empleados);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadosOficio
            (string oficio)
        {
            //CUANDO BUSCAMOS, NORMALMENTE, EN QUE POSICION COMIENZA TODO?
            List<Empleado> empleados = await
                this.repo.GetGrupoEmpleadosOficioAsync(1, oficio);
            int registros = await this.repo.GetNumeroEmpleadosOficioAsync(oficio);
            ViewData["REGISTROS"] = registros;
            ViewData["OFICIO"] = oficio;
            return View(empleados);
        }

        public async Task<IActionResult> 
            PaginarGrupoEmpleados(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            int registros = await this.repo.GetNumeroEmpleadosAsync();
            List<Empleado> empleados =
                await this.repo.GetGrupoEmpleadosAsync(posicion.Value);
            ViewData["REGISTROS"] = registros;
            return View(empleados);
        }


        public async Task<IActionResult>
            PaginarGrupoDepartamentos(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            int numeroRegistros = await
                this.repo.GetNumeroRegistrosVistaDepartamentos();
            ViewData["REGISTROS"] = numeroRegistros;
            List<Departamento> departamentos = await
                this.repo.GetGrupoDepartamentosAsync(posicion.Value);
            return View(departamentos);
        }

        public async Task<IActionResult>
            PaginarGrupoVistaDepartamento(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            int numeroRegistros = await 
                this.repo.GetNumeroRegistrosVistaDepartamentos();
            ViewData["REGISTROS"] = numeroRegistros;
            List<VistaDepartamento> departamentos =
                await this.repo.GetGrupoVistaDepartamentoAsync(posicion.Value);
            return View(departamentos);
        }

        public async Task<IActionResult>
            PaginarRegistroVistaDepartamento(int? posicion)
        {
            if (posicion == null)
            {
                //PONEMOS LA POSICION EN EL PRIMER REGISTRO
                posicion = 1;
            }
            //PRIMERO = 1
            //SIGUIENTE = 5
            //ANTERIOR = 4
            //ULTIMO = 5
            int numeroRegistros = 
                await this.repo.GetNumeroRegistrosVistaDepartamentos();
            int siguiente = posicion.Value + 1;
            //DEBEMOS COMPROBAR QUE NO PASAMOS DEL NUMERO DE REGISTROS
            if (siguiente > numeroRegistros)
            {
                //EFECTO OPTICO
                siguiente = numeroRegistros;
            }
            int anterior = posicion.Value - 1;
            if (anterior < 1)
            {
                anterior = 1;
            }
            VistaDepartamento vista = await
                this.repo.GetVistaDepartamentoAsync(posicion.Value);
            ViewData["ULTIMO"] = numeroRegistros;
            ViewData["SIGUIENTE"] = siguiente;
            ViewData["ANTERIOR"] = anterior;
            return View(vista);
        }
    }
}
