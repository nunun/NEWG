FROM newg/node
WORKDIR /app
ADD app /app
RUN mv /services /app/services
RUN rm -rf /app/node_modules
RUN apk add --no-cache git
RUN npm install
#RUN npm -g install mocha
#CMD ["mocha", "test.js"]
CMD ["node", "app.js"]
