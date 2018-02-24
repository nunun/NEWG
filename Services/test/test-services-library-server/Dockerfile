FROM node:9.3.0-alpine
WORKDIR /app
ADD app /app
RUN rm -rf /app/node_modules
RUN apk add --no-cache git
RUN npm install
CMD ["node", "app.js"]
