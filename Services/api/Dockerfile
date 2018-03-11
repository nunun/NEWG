FROM services/node:latest
ARG BUILD_CONFIGURATION
WORKDIR /app
ADD app /app
RUN mv /services /app/services
RUN rm -rf /app/node_modules
RUN apk add --no-cache git
RUN npm install
RUN if [ "${BUILD_CONFIGURATION}" = "tester" ]; then \
            npm -g install mocha; \
            echo "mocha ./test.js" > /start.sh; \
    elif [ "${BUILD_CONFIGURATION}" = "develop"  ]; then \
            npm -g install nodemon; \
            echo "nodemon ./app.js" > /start.sh; \
    else \
            echo "node ./app.js" > /start.sh; \
    fi
CMD [ "sh", "/start.sh" ]
