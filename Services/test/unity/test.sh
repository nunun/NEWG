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
}
. "`dirname ${0}`/../../.docker-composer/scripts/task.sh" test ${*}
