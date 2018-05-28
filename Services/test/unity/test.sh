task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_try() {
        local platform="StandaloneOSX"
        local xml="/tmp/result.xml"
        [ "${OSTYPE}" = "cygwin" ] \
                && platform="StandaloneWindows" \
                && xml=`cygpath -w "${xml}"`
        docker-compose up -d
        unity -runTests -testPlatform playmode -testResults "${xml}" \
                && echo_info "tests for services with unity are succeeded." \
                || echo_info "tests for services with unity are failed."
}
task_build() {
        task_down
        (cd ${RUN_ROOT_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        docker-compose run --rm --no-deps test-services-server npm install
        docker-compose run --rm --no-deps matching             npm install
        docker-compose run --rm --no-deps api                  npm install
        docker-compose run --rm --no-deps mindlink             npm install
}
. "`dirname ${0}`/../../.run/task.sh" ${*}
