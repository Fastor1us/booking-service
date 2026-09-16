# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/UserService/UserService.Presentation/UserService.Presentation.csproj", "src/Services/UserService/UserService.Presentation/"]
COPY ["src/Aspire/ServiceDefaults/ServiceDefaults.csproj", "src/Aspire/ServiceDefaults/"]
COPY ["src/Shared/Security.Jwt/Security.Jwt.csproj", "src/Shared/Security.Jwt/"]
COPY ["src/Shared/Domain/Domain.csproj", "src/Shared/Domain/"]
COPY ["src/Services/UserService/UserService.Application/UserService.Application.csproj", "src/Services/UserService/UserService.Application/"]
COPY ["src/Services/UserService/UserService.Domain/UserService.Domain.csproj", "src/Services/UserService/UserService.Domain/"]
COPY ["src/Services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "src/Services/UserService/UserService.Infrastructure/"]
COPY ["src/Shared/Messaging.Persistence.EfCore/Messaging.Persistence.EfCore.csproj", "src/Shared/Messaging.Persistence.EfCore/"]
COPY ["src/Shared/Messaging.Abstractions/Messaging.Abstractions.csproj", "src/Shared/Messaging.Abstractions/"]
RUN dotnet restore "./src/Services/UserService/UserService.Presentation/UserService.Presentation.csproj"
COPY . .
WORKDIR "/src/src/Services/UserService/UserService.Presentation"
RUN dotnet build "./UserService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:EnableSourceControlManagerQueries=false

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false  /p:EnableSourceControlManagerQueries=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.Presentation.dll"]