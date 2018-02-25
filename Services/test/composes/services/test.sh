task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() {
        docker-compose run --rm test-services-client mocha test.js
}
task_build() {
        task_down; docker-compose build --force-rm --pull
        git submodule update --init --recursive --remote
        docker-compose run --rm --no-deps test-services-client sh -c \
                "(cd /usr/local/lib/node_modules/services-library && npm install)"
        docker-compose run --rm --no-deps test-services-client npm update
        docker-compose run --rm --no-deps test-services-server npm update
        docker-compose run --rm --no-deps mindlink1            npm update
        docker-compose run --rm --no-deps test-services-client npm link services-library
        docker-compose run --rm --no-deps test-services-server npm link services-library
        docker-compose run --rm --no-deps mindlink1            npm link services-library
}
. "`dirname ${0}`/../../../.task.sh" test ${*}
