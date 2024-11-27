# Define build arguments for GIT branch, GIT commit, and datetime
ARG GIT_BRANCH
ARG GIT_COMMIT
ARG GIT_COMMIT_DATE
ARG BUILD_DATETIME

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Set build arguments in the build stage
ARG GIT_BRANCH
ARG GIT_COMMIT
ARG GIT_COMMIT_DATE
ARG BUILD_DATETIME

# Copy the project files for all layers (1-web, 2-services, 3-data)
COPY ./ClearTheSky/ClearTheSky.csproj ./ClearTheSky/

# Restore dependencies for all layers
RUN dotnet restore ./ClearTheSky/ClearTheSky.csproj

# Copy the entire solution
COPY . .

# Build the solution
RUN dotnet build "./ClearTheSky/ClearTheSky.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
ARG GIT_BRANCH
ARG GIT_COMMIT
ARG GIT_COMMIT_DATE
ARG BUILD_DATETIME

RUN dotnet publish "./ClearTheSky/ClearTheSky.csproj" -c Release -o /app/publish

RUN apt-get update && apt-get install -y curl && curl -sL https://deb.nodesource.com/setup_22.x | bash - && apt-get install -y nodejs
WORKDIR /tmp/frontend-build
COPY ./frontend .
RUN npm install
RUN npm run build
RUN cp -r /tmp/frontend-build/dist /app/publish/frontend

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables in the final image
ARG GIT_BRANCH
ARG GIT_COMMIT
ARG GIT_COMMIT_DATE
ARG BUILD_DATETIME

ENV GIT_BRANCH=${GIT_BRANCH}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV GIT_COMMIT_DATE=${GIT_COMMIT_DATE}
ENV BUILD_DATETIME=${BUILD_DATETIME}
