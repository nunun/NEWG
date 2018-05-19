task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_api() {
        docker-compose run --rm api node app.js
}
task_test() {
        docker-compose run --rm test-webapi-client mocha test.js
}
task_build() {
        task_down
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        docker-compose build --force-rm
}
. "`dirname ${0}`/../../.docker-composer/scripts/task.sh" test ${*}
