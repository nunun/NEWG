FROM node:9.3.0-alpine
WORKDIR /app
ADD app /app
RUN apk add --no-cache git
RUN npm install
RUN npm -g install mocha
CMD ["mocha", "test.js"]
