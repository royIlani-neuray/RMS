### STAGE 1: Build ###
FROM node:16.18-alpine AS build
WORKDIR /src
COPY web_app/package.json web_app/package-lock.json ./
RUN npm install
COPY web_app/. .
RUN npm run build

## STAGE 2: Run ###
FROM nginx:1.23.2-alpine
COPY web_app/nginx.conf.template /etc/nginx/nginx.conf.template
COPY --from=build /src/dist/web_app /usr/share/nginx/html

