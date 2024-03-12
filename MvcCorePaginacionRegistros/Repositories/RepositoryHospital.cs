using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;
using System.Data;

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
//create procedure SP_GRUPO_EMPLEADOS_OFICIO_OUT
//(@posicion int, @oficio nvarchar(50)
//, @registros int out)
//as
//select @registros = count(EMP_NO) from EMP
//where OFICIO=@oficio
//select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from 
//	(select cast(
//	ROW_NUMBER() OVER (ORDER BY APELLIDO) as int) AS POSICION
//	, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
//	from EMP
//	where OFICIO = @oficio) as QUERY
//	where QUERY.POSICION >= @posicion and QUERY.POSICION < (@posicion + 2)
//go
//create procedure SP_GRUPO_EMPLEADOS_DEPARTAMENTO
//(@posicion int, @departamento int
//, @registros int out)
//as
//select @registros = count(EMP_NO) from EMP
//where DEPT_NO=@departamento
//select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from 
//	(select cast(
//	ROW_NUMBER() OVER (ORDER BY APELLIDO) as int) AS POSICION
//	, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
//	from EMP
//	where DEPT_NO=@departamento) as QUERY
//	where QUERY.POSICION >= @posicion and QUERY.POSICION < (@posicion + 2)
//go
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

        //EL CONTROLLER NOS VA A DAR UNA POSICION Y UN OFICIO
        //DEBEMOS DEVOLVER LOS EMPLEADOS Y EL NUMERO DE REGISTROS
        public async Task<ModelEmpleadoPaginacion> 
            GetEmpleadoDepartamentoAsync
            (int posicion, int iddepartamento)
        {
            string sql = "SP_REGISTRO_EMPLEADO_DEPARTAMENTO @posicion, @departamento, "
                + " @registros out";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamDepartamento = 
                new SqlParameter("@departamento", iddepartamento);
            SqlParameter pamRegistros = new SqlParameter("@registros", -1);
            pamRegistros.Direction = ParameterDirection.Output;
            var consulta =
                this.context.Empleados.FromSqlRaw
                (sql, pamPosicion, pamDepartamento, pamRegistros);
            //PRIMERO DEBEMOS EJECUTAR LA CONSULTA PARA PODER RECUPERAR 
            //LOS PARAMETROS DE SALIDA
            var datos = await consulta.ToListAsync();
            Empleado empleado = datos.FirstOrDefault();
            int registros = (int)pamRegistros.Value;
            return new ModelEmpleadoPaginacion
            {
                Registros = registros,
                Empleado = empleado
            };
        }

        //EL CONTROLLER NOS VA A DAR UNA POSICION Y UN OFICIO
        //DEBEMOS DEVOLVER LOS EMPLEADOS Y EL NUMERO DE REGISTROS
        public async Task<ModelPaginacionEmpleados> GetGrupoEmpleadosOficioOutAsync
            (int posicion, string oficio)
        {
            string sql = "SP_GRUPO_EMPLEADOS_OFICIO_OUT @posicion, @oficio, "
                + " @registros out";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            SqlParameter pamRegistros = new SqlParameter("@registros", -1);
            pamRegistros.Direction = ParameterDirection.Output;
            var consulta =
                this.context.Empleados.FromSqlRaw
                (sql, pamPosicion, pamOficio, pamRegistros);
            //PRIMERO DEBEMOS EJECUTAR LA CONSULTA PARA PODER RECUPERAR 
            //LOS PARAMETROS DE SALIDA
            List<Empleado> empleados = await consulta.ToListAsync();
            int registros = (int)pamRegistros.Value;
            return new ModelPaginacionEmpleados
            {
                NumeroRegistros = registros, Empleados = empleados
            };
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

        public async Task<Departamento> FindDepartamentosAsync(int id)
        {
            return await this.context.Departamentos
                .FirstOrDefaultAsync(x => x.IdDepartamento == id);
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
