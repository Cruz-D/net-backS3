# Usar la imagen base de .NET SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar los archivos de proyecto y restaurar las dependencias
COPY *.sln .
COPY S3.Demo.API/*.csproj ./S3.Demo.API/
RUN dotnet restore

# Copiar el resto de los archivos y compilar la aplicación
COPY . .
WORKDIR /app/S3.Demo.API
RUN dotnet publish -c Release -o out

# Usar la imagen base de .NET Runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/S3.Demo.API/out ./

# Exponer el puerto en el que la aplicación escuchará
EXPOSE 80

# Definir el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "S3.Demo.API.dll"]