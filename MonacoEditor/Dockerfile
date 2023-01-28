#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM node:lts-alpine AS build-spa
WORKDIR /src

COPY ["MonacoEditor/package.json", "/src"]
COPY ["MonacoEditor/package-lock.json", "/src"]
RUN npm ci

COPY ["MonacoEditor", "/src"]

RUN npm run build --prod

# stage 2

FROM nginx:alpine
COPY --from=build-spa /src/dist/monaco-editor /usr/share/nginx/html
EXPOSE 80