task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() {
        local platform="StandaloneOSX"
        local xml="/tmp/result.xml"
        [ "${OSTYPE}" = "cygwin" ] \
                && platform="StandaloneWindows" \
                && xml=`cygpath -w "${xml}"`
        docker-compose up -d
        unity -runTests -testPlatform playmode -testResults "${xml}" \
                && echo "tests for services with unity are succeeded." \
                || echo "tests for services with unity are failed."
}
task_build() {
        task_down; docker-compose build --force-rm --pull
        git submodule update --init --recursive --remote
        docker-compose run --rm --no-deps matching sh -c \
                "(cd /usr/local/lib/node_modules/services-library && npm install)"
        docker-compose run --rm --no-deps matching npm update
        docker-compose run --rm --no-deps api      npm update
        docker-compose run --rm --no-deps matching npm link services-library
        docker-compose run --rm --no-deps api      npm link services-library
}
. "`dirname ${0}`/../../../.task.sh" test ${*}
