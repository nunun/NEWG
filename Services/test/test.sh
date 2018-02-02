CWD=$(cd `dirname ${0}`; pwd)
TASK="${1:-"test"}"
shift
set -e
cd "${CWD}"

task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_matching() { docker-compose run --rm matching node app.js; }
task_test() { docker-compose run --rm test_client mocha test.js; }
task_build() {
        task_down; docker-compose build --force-rm --pull
        git submodule update --init --recursive --remote
        docker-compose run --rm --no-deps test_client sh -c \
                "(cd /usr/local/lib/node_modules/libservices && npm install)"
        docker-compose run --rm --no-deps test_client npm update
        docker-compose run --rm --no-deps test_server npm update
        docker-compose run --rm --no-deps matching    npm update
        docker-compose run --rm --no-deps api         npm update
        docker-compose run --rm --no-deps test_client npm link libservices
        docker-compose run --rm --no-deps test_server npm link libservices
        docker-compose run --rm --no-deps matching    npm link libservices
        docker-compose run --rm --no-deps api         npm link libservices
}
task_${TASK} ${*}
