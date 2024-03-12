using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> EmpleadosOficio
            (int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            return View();
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
