FROM node:18.18.0
EXPOSE 8080

WORKDIR /app
COPY . .
RUN npm install
RUN npm run build
RUN npm install --global http-server
ENTRYPOINT [  "http-server", "/app/dist", "--port=8080"]