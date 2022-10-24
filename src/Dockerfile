FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV CONNECTION_STRING="Server={{ Server_ip }};Database=Chess;MultipleActiveResultSets=true;UID=chess;PWD={{ user_chess_password }}"

# You can also specify the connection string as env variable
# ENV CONNECTION_STRING= 

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Web/Chess.Web/Chess.Web.csproj", "Web/Chess.Web/"]
RUN dotnet restore "Web/Chess.Web/Chess.Web.csproj"
COPY . .
WORKDIR "/src/Web/Chess.Web"
RUN dotnet build "Chess.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chess.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chess.Web.dll"]
