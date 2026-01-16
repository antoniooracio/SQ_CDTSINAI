# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
COPY ["SQ.CDT_SINAI.API/SQ.CDT_SINAI.API.csproj", "SQ.CDT_SINAI.API/"]
COPY ["SQ.CDT_SINAI.Shared/SQ.CDT_SINAI.Shared.csproj", "SQ.CDT_SINAI.Shared/"]
RUN dotnet restore "SQ.CDT_SINAI.API/SQ.CDT_SINAI.API.csproj"

# Copia o restante dos arquivos e compila
COPY . .
WORKDIR "/src/SQ.CDT_SINAI.API"
RUN dotnet build "SQ.CDT_SINAI.API.csproj" -c Release -o /app/build

# Publica a aplicação
FROM build AS publish
RUN dotnet publish "SQ.CDT_SINAI.API.csproj" -c Release -o /app/publish

# Estágio Final (Imagem de Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
# Instalar dependências para o QuestPDF (SkiaSharp) no Linux
RUN apt-get update && apt-get install -y libfontconfig1 libfreetype6 libexpat1 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SQ.CDT_SINAI.API.dll"]