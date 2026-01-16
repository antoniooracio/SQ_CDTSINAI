# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os arquivos de projeto (Web e API) para restaurar dependências
COPY ["SQ.CDT_SINAI.Web/SQ.CDT_SINAI.Web.csproj", "SQ.CDT_SINAI.Web/"]
COPY ["SQ.CDT_SINAI.Shared/SQ.CDT_SINAI.Shared.csproj", "SQ.CDT_SINAI.Shared/"]

# Restaura as dependências do Web (que agora inclui a API)
RUN dotnet restore "SQ.CDT_SINAI.Web/SQ.CDT_SINAI.Web.csproj"

# Copia todo o código fonte da solução
COPY . .

# Define o diretório de trabalho para o Web e compila
WORKDIR "/src/SQ.CDT_SINAI.Web"
RUN dotnet build "SQ.CDT_SINAI.Web.csproj" -c Release -o /app/build

# Publica a aplicação
FROM build AS publish
RUN dotnet publish "SQ.CDT_SINAI.Web.csproj" -c Release -o /app/publish

# Estágio Final (Imagem de Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SQ.CDT_SINAI.Web.dll"]
