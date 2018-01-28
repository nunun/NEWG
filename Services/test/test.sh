CWD=$(cd `dirname ${0}`; pwd)
TASK="${1:-"test"}"
shift
set -e
cd "${CWD}"

task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() { docker-compose run --rm test_client mocha test.js; }
task_build() {
        task_down; docker-compose build --force-rm --pull
        git submodule update --init --recursive --remote
        docker-compose run --rm --no-deps test_client sh -c \
                "(cd /usr/local/lib/node_modules/libmindlink && npm install)"
        docker-compose run --rm --no-deps test_server npm update
        docker-compose run --rm --no-deps test_client npm update
        docker-compose run --rm --no-deps test_server npm link libmindlink
        docker-compose run --rm --no-deps test_client npm link libmindlink
}
task_${TASK} ${*}
