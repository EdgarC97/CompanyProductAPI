# CompanyProductAPI

Una API RESTful construida con .NET 8 y ADO.NET para gestionar Compañías y sus Productos.

## 📅 Requisitos

* .NET 8 SDK
* SQL Server (local o remoto)
* Visual Studio o Visual Studio Code
* Postman (para pruebas de API)

---

## 📊 Arquitectura

* **Controladores**: gestionan las solicitudes HTTP
* **Servicios**: aplican la lógica de negocio y usan LINQ para procesar datos
* **Repositorios**: acceden directamente a la base de datos con ADO.NET
* **Modelos**: clases que representan las entidades `Company` y `Product`

---

## 🔧 Instalación y ejecución

1. **Clona el repositorio**

```bash
git clone https://github.com/EdgarC97/CompanyProductAPI
cd CompanyProductAPI
```

2. **Agrega el archivo `.env` en la raíz del proyecto**

```env
DB_CONNECTION_STRING=Server=localhost;Database=CompanyProductsDb;User Id=sa;Password=TuPassword;Trusted_Connection=True;TrustServerCertificate=True;
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5041
SWAGGER_ENABLED=true
```

3. **Restaurar paquetes NuGet**

```bash
dotnet restore
```

4. **Ejecutar migraciones manualmente (si aplica)**

Este proyecto usa scripts manuales. Ejecuta el script SQL incluido para crear las tablas `Companies` y `Products`.

5. **Ejecuta el proyecto**

```bash
dotnet run
```

La API estará disponible en: `http://localhost:5041`

---

## 🔍 Endpoints disponibles

### Companies

* `GET /api/companies` - Lista todas las compañías
* `GET /api/companies/{id}` - Detalles de una compañía
* `POST /api/companies` - Crear una nueva compañía
* `PUT /api/companies/{id}` - Actualizar compañía
* `DELETE /api/companies/{id}` - Eliminar compañía (solo si no tiene productos)

### Products

* `GET /api/companies/{companyId}/products` - Productos de una compañía
* `POST /api/companies/{companyId}/products` - Crear producto asociado a una compañía
* `PUT /api/products/{id}` - Actualizar producto
* `DELETE /api/products/{id}` - Eliminar producto

---

## 🔹 NuGet packages usados

* `Microsoft.Data.SqlClient`
* `dotenv.net`
* `Swashbuckle.AspNetCore`

---

## 🎓 Pruebas con Postman

Importa el archivo `CompanyProductsApi.postman_collection.json` incluido en la raíz del proyecto en Postman para probar todos los endpoints.

---

## ✅ Notas finales

* Todas las operaciones con base de datos usan ADO.NET (SqlConnection, SqlCommand).
* LINQ se usa para ordenar, filtrar y transformar los datos una vez cargados en memoria.
* La relación entre Company y Product es 1\:N.

---
