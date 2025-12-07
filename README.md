# GestionPlazasVacantes

Sistema de gestión de plazas vacantes desarrollado en ASP.NET Core MVC.

## 🛠️ Tecnologías

- **Framework**: ASP.NET Core 8.0 MVC
- **Lenguaje**: C#
- **Base de Datos**: SQL Server
- **ORM**: Entity Framework Core 9.0.10
- **Autenticación**: Cookie Authentication
- **PDFs**: QuestPDF 2025.7.3 + iText7 9.3.0
- **Frontend**: Razor Views + JavaScript + Bootstrap (CDN)

## 📁 Estructura del Proyecto

```
GestionPlazasVacantes/
├── Controllers/       # Controladores MVC
├── Data/             # DbContext y configuración de EF
├── Migrations/       # Migraciones de Entity Framework
├── Models/           # Modelos de datos
├── Services/         # Lógica de negocio
├── Views/            # Vistas Razor
├── wwwroot/          # Archivos estáticos (CSS, JS, imágenes)
├── .vscode/          # Configuración de VS Code
│   ├── extensions.json   # Extensiones recomendadas
│   ├── launch.json       # Configuración de depuración
│   └── tasks.json        # Tareas de compilación
├── appsettings.json  # Configuración de la aplicación
└── Program.cs        # Punto de entrada
```

## 🚀 Configuración Inicial

### 1. Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express, o completo)
- Visual Studio Code (recomendado) o Visual Studio

### 2. Extensiones Recomendadas para VS Code

Al abrir el proyecto en VS Code, se te sugerirán automáticamente las extensiones necesarias:
- C# Dev Kit
- C# (Microsoft)
- SQL Server (mssql)
- .NET Runtime Install Tool

### 3. Configurar Base de Datos

Revisa el archivo `database_setup_guide.md` para instrucciones detalladas sobre cómo configurar la base de datos.

**Resumen rápido** (si no tienes datos):
```powershell
# En SQL Server, elimina la BD si existe
DROP DATABASE GestionPlazasVacantesDB;

# Aplica las migraciones
dotnet ef database update
```

### 4. Ejecutar el Proyecto

```powershell
# Restaurar dependencias (si no lo has hecho)
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run
```

O simplemente presiona **F5** en VS Code para depurar.

## 🔐 Seguridad

El proyecto incluye:
- ✅ Autenticación con cookies seguras (HttpOnly, Secure, SameSite)
- ✅ Rate limiting en endpoints de login (10 intentos/minuto por IP)
- ✅ Cabeceras de seguridad (CSP, X-Frame-Options, etc.)
- ✅ Protección contra XSS y CSRF (Razor automático)

## 📝 Características

- Gestión de plazas vacantes
- Seguimiento de postulantes
- Generación de reportes en PDF
- Panel de administración con autenticación
- Interfaz responsiva con Bootstrap

## 🔧 Comandos Útiles

```powershell
# Ver migraciones aplicadas
dotnet ef migrations list

# Crear nueva migración
dotnet ef migrations add NombreMigracion

# Actualizar base de datos
dotnet ef database update

# Revertir a migración específica
dotnet ef database update NombreMigracion

# Ejecutar con hot reload
dotnet watch run
```

## 📞 Soporte

Para problemas con la configuración, revisa:
1. `database_setup_guide.md` - Guía de configuración de base de datos
2. Logs de la aplicación en la consola
3. Verifica que SQL Server esté corriendo

## 📄 Licencia

Proyecto privado de gestión interna.
