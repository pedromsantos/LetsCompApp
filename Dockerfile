FROM microsoft/dotnet:2.0.3-sdk AS build-env
WORKDIR /sln

# Copy everything else and build
COPY ./src ./src
COPY ./tests ./tests
COPY ./*.sln ./

#Test and build
RUN dotnet restore
RUN dotnet build -c Release --no-restore
RUN dotnet test "./tests/" -c Release --no-build --no-restore
RUN dotnet publish -c Release -o "/dist"

# Build runtime image
FROM microsoft/aspnetcore:2.0.3
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Local
COPY --from=build-env /dist .
ENTRYPOINT ["dotnet", "LetsComp.dll"]