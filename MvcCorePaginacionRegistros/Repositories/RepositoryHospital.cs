using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;

#region VISTAS Y PROCEDIMIENTOS
//create view V_DEPARTAMENTOS_INDIVIDUAL
//as
//	select CAST(
//	ROW_NUMBER() over (ORDER BY DEPT_NO) AS INT) AS POSICION,
//    ISNULL(DEPT_NO, 0) AS DEPT_NO, DNOMBRE, LOC FROM DEPT
//go
//create procedure SP_GRUPO_DEPARTAMENTOS
//(@posicion int)
//as
//	--
//	select DEPT_NO, DNOMBRE, LOC 
//	from V_DEPARTAMENTOS_INDIVIDUAL
//	where POSICION >= @posicion and POSICION < (@posicion + 2)
//go
//create view V_GRUPO_EMPLEADOS
//as
//	select cast(
//		row_number() over (order by apellido) as int) as posicion,
//        ISNULL(EMP_NO, 0) AS EMP_NO, APELLIDO
//        , OFICIO, SALARIO, DEPT_NO FROM EMP
//go
//create procedure SP_GRUPO_EMPLEADOS
//(@posicion int)
//as
//	select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
//	from V_GRUPO_EMPLEADOS
//	where posicion >= @posicion and posicion < (@posicion + 3)
//go
//exec SP_GRUPO_EMPLEADOS 4
//create procedure SP_GRUPO_EMPLEADOS_OFICIO
//(@posicion int, @oficio nvarchar(50))
//as
//select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from 
//	(select cast(
//	ROW_NUMBER() OVER (ORDER BY APELLIDO) as int) AS POSICION
//	, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
//	from EMP
//	where OFICIO = @oficio) as QUERY
//	where QUERY.POSICION >= @posicion and QUERY.POSICION < (@posicion + 2)
//go
//exec SP_GRUPO_EMPLEADOS_OFICIO 1, 'EMPLEADO'

#endregion

namespace MvcCorePaginacionRegistros.Repositories
{
    public class RepositoryHospital
    {
        private HospitalContext context;

        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }

        //METODO PARA SABER EL NUMERO DE EMPLEADOS POR OFICIO
        public async Task<int> GetNumeroEmpleadosOficioAsync(string oficio)
        {
            return await this.context.Empleados
                .Where(z => z.Oficio == oficio).CountAsync();
        }

        public async Task<List<Empleado>> GetGrupoEmpleadosOficioAsync
            (int posicion, string oficio)
        {
            string sql = "SP_GRUPO_EMPLEADOS_OFICIO @posicion, @oficio";
            SqlParameter pamPosicion =
                new SqlParameter("@posicion", posicion);
            SqlParameter pamOficio =
                new SqlParameter("@oficio", oficio);
            var consulta = this.context.Empleados.FromSqlRaw
                (sql, pamPosicion, pamOficio);
            return await consulta.ToListAsync();
        }

        public async Task<int> GetNumeroEmpleadosAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<List<Empleado>> 
            GetGrupoEmpleadosAsync(int posicion)
        {
            string sql = "SP_GRUPO_EMPLEADOS @posicion";
            SqlParameter pamPosicion =
                new SqlParameter("@posicion", posicion);
            var consulta = this.context.Empleados.FromSqlRaw(sql, pamPosicion);
            return await consulta.ToListAsync();
        }


        public async Task<List<Departamento>>
            GetGrupoDepartamentosAsync(int posicion)
        {
            string sql = "SP_GRUPO_DEPARTAMENTOS @posicion";
            SqlParameter pamPosicion =
                new SqlParameter("@posicion", posicion);
            var consulta =
                this.context.Departamentos.FromSqlRaw(sql, pamPosicion);
            return await consulta.ToListAsync();
        }

        public async Task<int> GetNumeroRegistrosVistaDepartamentos()
        {
            return await this.context.VistaDepartamentos.CountAsync();
        }

        public async Task<VistaDepartamento> 
            GetVistaDepartamentoAsync(int posicion)
        {
            VistaDepartamento vista = await
                this.context.VistaDepartamentos
                .Where(z => z.Posicion == posicion).FirstOrDefaultAsync();
            return vista;
        }

        public async Task<List<VistaDepartamento>>
            GetGrupoVistaDepartamentoAsync(int posicion)
        {
            //SELECT* FROM V_DEPARTAMENTOS_INDIVIDUAL
            //WHERE POSICION >= 1 AND POSICION< (1 +2)
            var consulta = from datos in this.context.VistaDepartamentos
                           where datos.Posicion >= posicion
                           && datos.Posicion < (posicion + 2)
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Departamento>> GetDepartamentosAsync()
        {
            return await this.context.Departamentos.ToListAsync();
        }

        public async Task<List<Empleado>> GetEmpleadosDepartamentoAsync
            (int idDepartamento)
        {
            var empleados = this.context.Empleados
                .Where(x => x.IdDepartamento == idDepartamento);
            if (empleados.Count() == 0)
            {
                return null;
            }
            else
            {
                return await empleados.ToListAsync();
            }
        }
    }
}
