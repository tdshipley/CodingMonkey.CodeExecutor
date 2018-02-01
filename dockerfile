FROM microsoft/aspnetcore-build:2.0.3 AS builder
# Stage 1
    WORKDIR /source

    # caches restore result by copying csproj file separately
    COPY ./src/CodingMonkey.CodeExecutor/*.csproj .
    RUN dotnet restore

    # copies the rest of your code
    COPY ./src/CodingMonkey.CodeExecutor/ .
    RUN echo "Running unit tests"
    RUN dotnet test ./src/CodingMonkey.CodeExecutor.UnitTests/CodingMonkey.CodeExecutor.UnitTests.csproj
    RUN dotnet publish --output /app/ --configuration Release

# Stage 2
    FROM microsoft/aspnetcore
    WORKDIR /app
    COPY --from=builder /app .
    CMD ASPNETCORE_URLS=http://*:$PORT dotnet CodingMonkey.CodeExecutor.dll