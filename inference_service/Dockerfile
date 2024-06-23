FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine3.20 AS base
WORKDIR /app

#ENV ASPNETCORE_URLS=http://+:16500 (defined in .env file)

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" user && chown -R user /app
USER user

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20 AS build
WORKDIR /src
COPY ["inference_service/inference_service.csproj", "./"]
RUN dotnet restore "inference_service.csproj"
COPY inference_service/. .
RUN dotnet publish "inference_service.csproj" -c Release -o /app/publish \
    -r linux-musl-x64 \
    --self-contained true \
    -p:PublishSingeFile=true

FROM base AS final
USER root
WORKDIR /app
RUN mkdir /app/data
COPY --from=build /app/publish .
COPY inference_service/default_models /app/
RUN chown -R user:user /app
USER user
ENTRYPOINT ["./inference_service"]

