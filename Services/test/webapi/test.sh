task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_try() {
        docker-compose run --rm test-webapi-client mocha test.js
}
task_build() {
        task_down
        (cd ${RUN_ROOT_DIR}; sh run.sh services build)
        docker-compose build --force-rm
}
. "`dirname ${0}`/../../.run/task.sh" ${*}
