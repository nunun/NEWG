task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() {
        docker-compose run --rm test-client mocha test.js
}
task_build() {
        task_down; docker-compose build --force-rm --pull
        docker-compose run --rm --no-deps test-client sh -c \
                "(cd /usr/local/lib/node_modules/services-library && npm install)"
        docker-compose run --rm --no-deps test-client npm update
        docker-compose run --rm --no-deps test-server npm update
        docker-compose run --rm --no-deps matching    npm update
        docker-compose run --rm --no-deps api         npm update
        docker-compose run --rm --no-deps mindlink    npm update
        docker-compose run --rm --no-deps test-client npm link services-library
        docker-compose run --rm --no-deps test-server npm link services-library
        docker-compose run --rm --no-deps matching    npm link services-library
        docker-compose run --rm --no-deps api         npm link services-library
        docker-compose run --rm --no-deps mindlink    npm link services-library
}
. "`dirname ${0}`/../../../.task.sh" test ${*}