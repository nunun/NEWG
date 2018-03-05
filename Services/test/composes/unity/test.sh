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
        task_down
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        docker-compose run --rm --no-deps test-services-server npm update
        docker-compose run --rm --no-deps matching             npm update
        docker-compose run --rm --no-deps api                  npm update
        docker-compose run --rm --no-deps mindlink             npm update
}
task_clean() {
        docker-compose run --rm --no-deps test-services-server rm -rf node_modules package-lock.json
        docker-compose run --rm --no-deps matching             rm -rf node_modules package-lock.json
        docker-compose run --rm --no-deps api                  rm -rf node_modules package-lock.json
        docker-compose run --rm --no-deps mindlink             rm -rf node_modules package-lock.json
}
. "`dirname ${0}`/../../../.task.sh" test ${*}
