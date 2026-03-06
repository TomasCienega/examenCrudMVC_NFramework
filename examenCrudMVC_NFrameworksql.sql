 
-- DESKTOP-JJ9DM3F\SQLEXPRESS

create database examenCrudMVC_NFramework
use examenCrudMVC_NFramework

create table Departamento
(
	idDepartamento int identity(1,1) not null,
	nombreDepartamento varchar(50) not null,
	constraint PK_Departamento primary key (idDepartamento)
)
insert into Departamento(nombreDepartamento)values('Ventas'),('RH'),('IT')
select * from Departamento

create table Empleado
(
	idEmpleado int identity(1,1) not null,
	nombreEmpleado varchar(100) not null,
	idDepartamento int,
	activo bit default 1,
	constraint PK_Empleado primary key (idEmpleado),
	constraint FK_EmpleadoDepartamento foreign key (idDepartamento)
										references Departamento(idDepartamento)
)
insert into Empleado(nombreEmpleado,idDepartamento)values('Tomas',1),('Adrian',2),('Alan',3)
select * from Empleado

create procedure sp_ListarEmpleadoPorIdDep
(
	@idDepartamento int
)
as
begin
	select e.idEmpleado,e.nombreEmpleado,e.activo, d.idDepartamento, d.nombreDepartamento 
	from Empleado e
	inner join Departamento d
	on e.idDepartamento = d.idDepartamento
	where d.idDepartamento = @idDepartamento
	order by e.activo DESC
end

create procedure sp_EstadoEmpleado
(
	@idEmpleado int
)
as
begin
	update Empleado
	set activo = case when activo = 1 then 0 else 1 end 
	where idEmpleado = @idEmpleado
end

