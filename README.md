Sistema de Gestión de Nóminas (Payroll System)
Descripción del Proyecto

Este es un sistema web integral diseñado para automatizar la gestión y el cálculo de nóminas de personal. Desarrollé este proyecto enfocándome en crear una arquitectura escalable y mantener un código limpio.

El objetivo principal de la aplicación es reducir la carga manual al momento de liquidar sueldos, asegurando que los cálculos de ingresos, descuentos y aportes de ley sean exactos mediante validaciones en el backend.

Características Principales

Gestión de Colaboradores: Permite administrar toda la información de los empleados (crear, editar, eliminar y listar).

Cálculo Automatizado: El sistema procesa ingresos y descuentos automáticamente según la normativa configurada.

Optimización con SQL Server: Implementé Procedimientos Almacenados (Stored Procedures) para manejar la lógica compleja y el procesamiento masivo de datos directamente en la base de datos, lo que hace que el sistema sea mucho más rápido.

Calidad de Código (SonarQube): Integré SonarQube durante el desarrollo para analizar el código, detectar errores potenciales ("code smells") y reducir la deuda técnica, garantizando un software más seguro y mantenible.

Diseño Responsivo: La interfaz fue construida con Bootstrap, por lo que se adapta correctamente a computadoras y dispositivos móviles.

Tecnologías Utilizadas

Backend: C# con ASP.NET Core (Arquitectura MVC)

Frontend: HTML5, CSS3, JavaScript y Bootstrap

Base de Datos: SQL Server

Calidad y Testing: SonarQube

Control de Versiones: Git

Instalación y Despliegue

Si deseas probar el proyecto en tu entorno local, sigue estos pasos:

Clona este repositorio en tu máquina.

Ve a la carpeta de base de datos, busca el archivo "script_db.sql" y ejecútalo en tu SQL Server Management Studio para crear las tablas y los procedimientos necesarios.

Abre el archivo de configuración (appsettings.json) y actualiza la cadena de conexión con tus credenciales locales de SQL.

Abre la solución en Visual Studio y ejecútala.

Autor

Ignacio Alejandro Llovera Amundaray Estudiante de Ingeniería de Sistemas
