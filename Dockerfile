FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update && apt-get install -y \
    chromium \
    chromium-driver \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY TestProject3.csproj ./
RUN dotnet restore

COPY . ./

CMD ["dotnet", "test", "--logger", "console;verbosity=normal"]
