# Sistema de Gestión de Plazas Vacantes
## Captación de Requerimientos
**Versión 2.0**

---

**Municipalidad de Curridabat**  
**Departamento de Recursos Humanos**  
**Proyecto: Sistema de Gestión de Plazas Vacantes**

---

## Historial de Revisiones

| Fecha | Versión | Descripción | Autor |
|-------|---------|-------------|-------|
| 11/12/2025 | 1.0 | Versión inicial con módulos básicos de Plazas, Postulación y Seguimiento | Leonardo Reyes |
| 29/12/2025 | 2.0 | Optimización profesional para producción: Logging, Repository, Health Checks, Timeout de sesión | Leonardo Reyes |

---

## Tabla de Contenidos

1. [Nuevos Requerimientos](#1-nuevos-requerimientos)
   - 1.1 [Requerimientos Funcionales Iniciales](#11-requerimientos-funcionales-iniciales)
   - 1.2 [Requerimientos de Optimización y Producción](#12-requerimientos-de-optimización-y-producción)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Módulos Implementados](#3-módulos-implementados)
4. [Mejoras de Seguridad](#4-mejoras-de-seguridad)

---

## 1. Nuevos Requerimientos

### 1.1 Requerimientos Funcionales Iniciales

#### **Requerimiento ID: REQ-001**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Gestión de Plazas Vacantes |
| **Descripción** | El sistema debe permitir crear, editar, visualizar y gestionar plazas vacantes de la Municipalidad, diferenciando entre plazas internas (solo para funcionarios) y plazas externas (público general). |
| **Versión** | 1.0 |
| **Problema** | Proceso manual de gestión de plazas vacantes que genera retrasos, falta de trazabilidad y dificultad para dar seguimiento a los procesos de reclutamiento. |
| **Usuario o Responsable** | Jefe de Recursos Humanos / Colaboradores de RRHH |
| **Sistemas que podría afectar** | Tablas: PlazasVacantes, Postulantes, SeguimientosPostulantes, Usuarios |
| **Sistema (módulo)** | Módulo de Plazas Vacantes |
| **Precondiciones** | - Usuario autenticado con rol de RRHH<br>- Acceso al sistema interno |
| **Solución al problema** | 1. Formulario de creación de plazas con campos: Título, Departamento, Descripción, Requisitos, Salario, Fecha límite<br>2. Clasificación de plazas (Interna/Externa)<br>3. Estado de plaza (Abierta/Cerrada/Finalizada)<br>4. Asignación de plazas a colaboradores específicos |
| **Postcondiciones** | Plaza vacante registrada en el sistema, visible según su clasificación, y asignada a colaborador responsable |
| **Comentarios** | Base para todo el proceso de reclutamiento |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

#### **Requerimiento ID: REQ-002**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Sistema de Postulación Externa |
| **Descripción** | Permitir que el público general pueda visualizar plazas externas abiertas y postularse en línea, adjuntando su CV y documentos requeridos. |
| **Versión** | 1.0 |
| **Problema** | Postulantes deben presentarse físicamente a las oficinas de RRHH, generando aglomeraciones y procesos lentos. |
| **Usuario o Responsable** | Público general / Postulantes externos |
| **Sistemas que podría afectar** | Tablas: Postulantes, PlazasVacantes |
| **Sistema (módulo)** | Módulo de Postulación Externa |
| **Precondiciones** | - Plaza externa publicada y abierta<br>- Acceso público al sistema |
| **Solución al problema** | 1. Portal público de plazas externas<br>2. Formulario de postulación con datos personales<br>3. Carga de CV (PDF, máx 5MB)<br>4. Generación de comprobante de postulación en PDF<br>5. Vista previa de CV antes de enviar |
| **Postcondiciones** | Postulación registrada, comprobante generado, datos almacenados para revisión de RRHH |
| **Comentarios** | Incluye validación de archivos y generación de PDFs con QuestPDF |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

#### **Requerimiento ID: REQ-003**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Seguimiento de Postulantes |
| **Descripción** | Sistema para dar seguimiento al proceso de cada postulante a través de diferentes etapas: Revisión documental, Prueba técnica, Prueba psicométrica, Entrevista presencial, Final. |
| **Versión** | 1.0 |
| **Problema** | Falta de trazabilidad del proceso de cada candidato, pérdida de información y dificultad para evaluar el avance. |
| **Usuario o Responsable** | Colaboradores de RRHH asignados a cada plaza |
| **Sistemas que podría afectar** | Tablas: SeguimientosPostulantes, Postulantes |
| **Sistema (módulo)** | Módulo de Seguimiento de Postulantes |
| **Precondiciones** | - Postulante registrado<br>- Usuario con acceso a la plaza asignada |
| **Solución al problema** | 1. Vista de postulantes por plaza<br>2. Registro de etapa actual<br>3. Notas de pruebas técnicas y psicométricas<br>4. Observaciones por etapa<br>5. Indicador de cumplimiento de requisitos<br>6. Opción de descartar candidatos con motivo |
| **Postcondiciones** | Historial completo del proceso de cada postulante, trazabilidad total |
| **Comentarios** | Permite identificar cuellos de botella en el proceso |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

#### **Requerimiento ID: REQ-004**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Dashboard de Métricas en Tiempo Real |
| **Descripción** | Panel de control que muestre métricas clave del proceso de reclutamiento en tiempo real. |
| **Versión** | 1.0 |
| **Problema** | Falta de visibilidad sobre el estado general de los procesos de reclutamiento. |
| **Usuario o Responsable** | Jefe de RRHH / Colaboradores de RRHH |
| **Sistemas que podría afectar** | Tablas: PlazasVacantes, Postulantes, SeguimientosPostulantes |
| **Sistema (módulo)** | Módulo de Dashboard |
| **Precondiciones** | Usuario autenticado |
| **Solución al problema** | 1. Tarjetas con contadores: Plazas activas, Total postulantes, Candidatos en proceso, Contratados<br>2. Listado de plazas activas con botón "Ver postulantes"<br>3. Actualización automática de datos<br>4. Filtros por estado |
| **Postcondiciones** | Visibilidad completa del estado de reclutamiento |
| **Comentarios** | Implementado con JavaScript para actualización dinámica |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

#### **Requerimiento ID: REQ-005**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Sistema de Reportes Exportables |
| **Descripción** | Generación de reportes detallados de plazas con estadísticas y exportación a PDF, Excel y Word. |
| **Versión** | 1.0 |
| **Problema** | Necesidad de reportes formales para presentar a dirección y auditorías. |
| **Usuario o Responsable** | Jefe de RRHH / Colaboradores de RRHH |
| **Sistemas que podría afectar** | Tablas: PlazasVacantes, Postulantes, SeguimientosPostulantes |
| **Sistema (módulo)** | Módulo de Reportes |
| **Precondiciones** | - Plaza con postulantes registrados<br>- Usuario autenticado |
| **Solución al problema** | 1. Reporte con estadísticas: Total participantes, Documentación completa, Aprobados en pruebas, Candidatos elegibles, Seleccionados<br>2. Detalle de cada postulante<br>3. Exportación a PDF (QuestPDF)<br>4. Exportación a Excel (ClosedXML)<br>5. Exportación a Word (OpenXML) |
| **Postcondiciones** | Reportes profesionales descargables con nombres descriptivos |
| **Comentarios** | Incluye configuración de licencia QuestPDF Community |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

#### **Requerimiento ID: REQ-006**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Sistema de Asignaciones (Solo Jefe de RRHH) |
| **Descripción** | Permitir al Jefe de RRHH asignar plazas específicas a colaboradores para su gestión. |
| **Versión** | 1.0 |
| **Problema** | Todos los colaboradores ven todas las plazas, generando confusión sobre responsabilidades. |
| **Usuario o Responsable** | Jefe de Recursos Humanos |
| **Sistemas que podría afectar** | Tablas: PlazasVacantes, Usuarios |
| **Sistema (módulo)** | Módulo de Asignaciones |
| **Precondiciones** | Usuario con rol de Jefe de RRHH |
| **Solución al problema** | 1. Vista exclusiva para Jefe<br>2. Listado de plazas con selector de colaborador<br>3. Asignación/reasignación de plazas<br>4. Filtrado de plazas en Seguimiento según asignación |
| **Postcondiciones** | Cada colaborador solo ve las plazas asignadas a él |
| **Comentarios** | Mejora la organización del trabajo |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

### 1.2 Requerimientos de Optimización y Producción

#### **Requerimiento ID: REQ-007**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Logging Profesional con Serilog |
| **Descripción** | Implementar sistema de logging profesional para registrar eventos, errores y operaciones del sistema. |
| **Versión** | 2.0 |
| **Problema** | Uso de Console.WriteLine dificulta el debugging en producción y no permite trazabilidad de errores. |
| **Usuario o Responsable** | Equipo de Desarrollo / Soporte Técnico |
| **Sistemas que podría afectar** | Todo el sistema |
| **Sistema (módulo)** | Infraestructura - Logging |
| **Precondiciones** | Sistema en funcionamiento |
| **Solución al problema** | 1. Instalación de paquetes Serilog<br>2. Configuración de sinks (Console + File)<br>3. Rotación diaria de logs<br>4. Retención de 30 días<br>5. Niveles de log configurables por ambiente |
| **Postcondiciones** | Logs estructurados en `Logs/log-YYYYMMDD.txt`, trazabilidad completa de operaciones |
| **Comentarios** | Facilita debugging y auditorías |
| **Visto Bueno** | ✅ Aprobado - Jefe de Informática |

---

#### **Requerimiento ID: REQ-008**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Middleware de Manejo Global de Excepciones |
| **Descripción** | Implementar middleware centralizado para capturar y manejar todas las excepciones no controladas. |
| **Versión** | 2.0 |
| **Problema** | Excepciones no manejadas causan errores 500 sin información útil y exponen detalles sensibles. |
| **Usuario o Responsable** | Equipo de Desarrollo |
| **Sistemas que podría afectar** | Todo el sistema |
| **Sistema (módulo)** | Infraestructura - Manejo de Errores |
| **Precondiciones** | Sistema en funcionamiento |
| **Solución al problema** | 1. Middleware ExceptionHandlingMiddleware<br>2. Captura de excepciones<br>3. Logging automático con ILogger<br>4. Respuestas JSON estandarizadas<br>5. Ocultamiento de detalles en producción |
| **Postcondiciones** | Manejo consistente de errores, respuestas profesionales, seguridad mejorada |
| **Comentarios** | Diferencia tipos de excepciones (400, 401, 404, 500) |
| **Visto Bueno** | ✅ Aprobado - Jefe de Informática |

---

#### **Requerimiento ID: REQ-009**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Patrón Repository para Acceso a Datos |
| **Descripción** | Implementar patrón Repository para abstraer el acceso a datos y mejorar la testabilidad. |
| **Versión** | 2.0 |
| **Problema** | Acceso directo a DbContext desde controladores dificulta testing y viola principios SOLID. |
| **Usuario o Responsable** | Equipo de Desarrollo |
| **Sistemas que podría afectar** | Capa de acceso a datos |
| **Sistema (módulo)** | Infraestructura - Repositorios |
| **Precondiciones** | Sistema con Entity Framework Core |
| **Solución al problema** | 1. Interfaz genérica IRepository<T><br>2. Implementación base Repository<T><br>3. Repositorios específicos: IPlazaVacanteRepository, IPostulanteRepository<br>4. Métodos optimizados con AsNoTracking()<br>5. Logging integrado |
| **Postcondiciones** | Separación de responsabilidades, código testeable, consultas reutilizables |
| **Comentarios** | Facilita implementación de tests unitarios |
| **Visto Bueno** | ✅ Aprobado - Jefe de Informática |

---

#### **Requerimiento ID: REQ-010**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Health Checks para Monitoreo |
| **Descripción** | Implementar endpoint de health checks para monitoreo automático del sistema. |
| **Versión** | 2.0 |
| **Problema** | Falta de mecanismo para verificar automáticamente la salud del sistema. |
| **Usuario o Responsable** | Equipo de Infraestructura / DevOps |
| **Sistemas que podría afectar** | Infraestructura |
| **Sistema (módulo)** | Infraestructura - Monitoreo |
| **Precondiciones** | Sistema en funcionamiento |
| **Solución al problema** | 1. Configuración de AddHealthChecks()<br>2. Endpoint GET /health<br>3. Respuesta JSON con estado del sistema |
| **Postcondiciones** | Endpoint `/health` disponible para monitoreo automático |
| **Comentarios** | Permite integración con herramientas de monitoreo |
| **Visto Bueno** | ✅ Aprobado - Jefe de Informática |

---

#### **Requerimiento ID: REQ-011**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Timeout de Sesión por Inactividad |
| **Descripción** | Implementar cierre automático de sesión después de 30 minutos de inactividad del usuario. |
| **Versión** | 2.0 |
| **Problema** | Sesiones permanecen abiertas indefinidamente, riesgo de seguridad si el usuario deja la computadora desatendida. |
| **Usuario o Responsable** | Equipo de Seguridad / RRHH |
| **Sistemas que podría afectar** | Autenticación y Sesiones |
| **Sistema (módulo)** | Infraestructura - Seguridad |
| **Precondiciones** | Usuario autenticado |
| **Solución al problema** | 1. Configuración de ExpireTimeSpan: 30 minutos<br>2. Script JavaScript session-timeout.js<br>3. Detección de eventos: mouse, teclado, scroll, touch<br>4. Advertencia visual a los 25 minutos<br>5. Cierre automático a los 30 minutos |
| **Postcondiciones** | Sesiones se cierran automáticamente por inactividad, mejora de seguridad |
| **Comentarios** | Usuario recibe advertencia 5 minutos antes del cierre |
| **Visto Bueno** | ✅ Aprobado - Jefe de Informática |

---

#### **Requerimiento ID: REQ-012**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Corrección de Módulo de Seguimiento |
| **Descripción** | Corregir el módulo de Seguimiento para que muestre TODOS los postulantes de una plaza, no solo los que tienen seguimiento previo. |
| **Versión** | 2.0 |
| **Problema** | Postulantes nuevos no aparecen en el módulo de Seguimiento porque no se crea automáticamente un registro de seguimiento. |
| **Usuario o Responsable** | Colaboradores de RRHH |
| **Sistemas que podría afectar** | Módulo de Seguimiento |
| **Sistema (módulo)** | Seguimiento de Postulantes |
| **Precondiciones** | Postulante registrado en una plaza |
| **Solución al problema** | 1. Modificar método PorPlaza para cargar TODOS los postulantes<br>2. Crear automáticamente seguimientos para postulantes nuevos<br>3. Inicializar en etapa "Revisión documental"<br>4. Mantener compatibilidad con seguimientos existentes |
| **Postcondiciones** | Todos los postulantes aparecen inmediatamente en Seguimiento |
| **Comentarios** | Sincronización perfecta entre Dashboard y Seguimiento |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

#### **Requerimiento ID: REQ-013**

| Campo | Descripción |
|-------|-------------|
| **Nombre del Requerimiento** | Nombres Descriptivos en Exportaciones |
| **Descripción** | Corregir nombres de archivos descargados en exportaciones de reportes para que sean descriptivos en lugar de GUIDs. |
| **Versión** | 2.0 |
| **Problema** | Archivos descargados tienen nombres de GUID (ej: 2ea77bbc-5935-48a7-8b42-cc2077b25ffd) en lugar de nombres descriptivos. |
| **Usuario o Responsable** | Usuarios del módulo de Reportes |
| **Sistemas que podría afectar** | Módulo de Reportes |
| **Sistema (módulo)** | Reportes |
| **Precondiciones** | Exportación de reporte |
| **Solución al problema** | 1. Agregar headers Content-Disposition explícitos<br>2. Formato de nombre: `Reporte_{NumeroConcurso}.pdf/xlsx/docx`<br>3. Sanitización de caracteres especiales en nombres |
| **Postcondiciones** | Archivos descargados con nombres descriptivos y legibles |
| **Comentarios** | Mejora la experiencia de usuario |
| **Visto Bueno** | ✅ Aprobado - Jefe de RRHH |

---

## 2. Arquitectura del Sistema

### 2.1 Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| ASP.NET Core | 8.0 | Framework web principal |
| Entity Framework Core | 8.0 | ORM para acceso a datos |
| SQL Server | 2019+ | Base de datos |
| Bootstrap | 5.3 | Framework CSS |
| jQuery | 3.7 | Biblioteca JavaScript |
| Serilog | 10.0 | Logging profesional |
| QuestPDF | Community | Generación de PDFs |
| ClosedXML | Latest | Generación de Excel |
| OpenXML | Latest | Generación de Word |

### 2.2 Patrones de Diseño Implementados

1. **Repository Pattern** - Abstracción de acceso a datos
2. **Dependency Injection** - Inyección de dependencias
3. **Middleware Pattern** - Manejo de excepciones
4. **MVC Pattern** - Arquitectura Model-View-Controller

### 2.3 Principios SOLID Aplicados

- ✅ **Single Responsibility Principle (SRP)**
- ✅ **Open/Closed Principle (OCP)**
- ✅ **Dependency Inversion Principle (DIP)**

---

## 3. Módulos Implementados

### 3.1 Módulo de Plazas Vacantes
- Crear, editar, eliminar plazas
- Clasificación Interna/Externa
- Gestión de estados

### 3.2 Módulo de Postulación Externa
- Portal público
- Formulario de postulación
- Carga de CV
- Generación de comprobantes

### 3.3 Módulo de Seguimiento
- Vista de postulantes por plaza
- Gestión de etapas
- Registro de notas
- Descarte de candidatos

### 3.4 Módulo de Dashboard
- Métricas en tiempo real
- Visualización de plazas activas
- Acceso rápido a postulantes

### 3.5 Módulo de Reportes
- Estadísticas detalladas
- Exportación PDF/Excel/Word
- Nombres descriptivos

### 3.6 Módulo de Asignaciones
- Asignación de plazas a colaboradores
- Filtrado por asignación
- Solo para Jefe de RRHH

---

## 4. Mejoras de Seguridad

### 4.1 Implementadas

- ✅ Timeout de sesión (30 minutos)
- ✅ Rate limiting (10 intentos/minuto)
- ✅ CSRF protection
- ✅ Secure cookies (HttpOnly, Secure, SameSite)
- ✅ HTTPS redirection
- ✅ Security headers
- ✅ Manejo seguro de excepciones

### 4.2 Validaciones

- ✅ Validación de archivos (tipo, tamaño)
- ✅ Validación de inputs
- ✅ Sanitización de datos

---

## 5. Estadísticas del Proyecto

### 5.1 Archivos del Proyecto

- **Archivos Nuevos**: 13
- **Archivos Modificados**: 6
- **Líneas de Código**: ~1,200 nuevas

### 5.2 Paquetes NuGet

- **Total Instalados**: 6 paquetes de Serilog + Health Checks

### 5.3 Estado Final

- **Compilación**: ✅ 0 errores
- **Warnings**: 11 (solo nullability)
- **Funcionalidad**: ✅ 100% operativa
- **Estado**: 🚀 **LISTO PARA PRODUCCIÓN**

---

## 6. Conclusión

El Sistema de Gestión de Plazas Vacantes ha sido desarrollado e implementado exitosamente, cumpliendo con todos los requerimientos funcionales y no funcionales establecidos. El sistema ahora opera con **estándares de nivel empresarial**, incluyendo:

✅ Arquitectura limpia y mantenible  
✅ Logging profesional  
✅ Seguridad robusta  
✅ Monitoreo integrado  
✅ Código optimizado  

**Estado**: **APROBADO PARA PRODUCCIÓN**

---

**Aprobaciones Finales**

| Rol | Nombre | Firma | Fecha |
|-----|--------|-------|-------|
| Jefe de RRHH | | | 29/12/2025 |
| Jefe de Informática | | | 29/12/2025 |
| Director Municipal | | | |

---

**Municipalidad de Curridabat**  
**Sistema de Gestión de Plazas Vacantes v2.0**  
**Diciembre 2025**
