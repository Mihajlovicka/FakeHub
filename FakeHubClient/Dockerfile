FROM node:20-alpine AS build

WORKDIR /usr/src/app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run docker:build -- --no-progress --configuration docker || true

FROM nginx:1.27.2-alpine-slim AS run
# FROM openresty/openresty:latest AS run
# RUN apt-get update && apt-get install -y \
#     bash \
#     curl \
#     nginx \
#     git \
#     make \
#     build-essential \
#     luarocks

# Install lua-resty-http using LuaRocks
# RUN luarocks install lua-resty-http
# RUN luarocks install lua-resty-jwt

COPY --from=build /usr/src/app/dist/fake-hub-client/browser /usr/share/nginx/html
COPY /nginx.conf /etc/nginx/nginx.conf
# COPY /nginx.conf /usr/local/openresty/nginx/conf/nginx.conf
COPY /ssl /etc/nginx/ssl
EXPOSE 80

# CMD ["/usr/local/openresty/bin/openresty", "-g", "daemon off;"]
