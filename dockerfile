FROM microsoft/aspnetcore-build:2.0.3 AS builder
# Stage 1
    WORKDIR /source

    # caches restore result by copying csproj file separately
    COPY ./src/CodingMonkey.CodeExecutor/CodingMonkey.CodeExecutor.csproj .
    COPY ./tests/CodingMonkey.CodeExecutor.UnitTests/CodingMonkey.CodeExecutor.UnitTests.csproj .
    RUN dotnet restore CodingMonkey.CodeExecutor.csproj
    RUN dotnet restore CodingMonkey.CodeExecutor.UnitTests.csproj

    # copies the rest of your code
    COPY . .
    RUN echo "Running unit tests"
    RUN ls
    RUN dotnet test ./tests/CodingMonkey.CodeExecutor.UnitTests/CodingMonkey.CodeExecutor.UnitTests.csproj
    RUN dotnet publish ./src/CodingMonkey.CodeExecutor/CodingMonkey.CodeExecutor.csproj --output /app/ --configuration Release

# Stage 2
    FROM microsoft/aspnetcore
    WORKDIR /app
    COPY --from=builder /app .
    CMD ASPNETCORE_URLS=http://*:$PORT dotnet CodingMonkey.CodeExecutor.dll