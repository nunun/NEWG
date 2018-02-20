task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() {
        docker-compose run --rm test_client mocha test.js
}
task_build() {
        task_down; docker-compose build --force-rm --pull
        git submodule update --init --recursive --remote
        docker pull nunun/mindlink
        docker-compose run --rm --no-deps test_client sh -c \
                "(cd /usr/local/lib/node_modules/libservices && npm install)"
        docker-compose run --rm --no-deps test_client npm update
        docker-compose run --rm --no-deps matching    npm update
        docker-compose run --rm --no-deps api         npm update
        docker-compose run --rm --no-deps test_client npm link libservices
        docker-compose run --rm --no-deps matching    npm link libservices
        docker-compose run --rm --no-deps api         npm link libservices
}
. "`dirname ${0}`/../../../.task.sh" test ${*}
