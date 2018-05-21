task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_all() {
        docker-compose run --rm test-matching-client mocha test.js
}
task_build() {
        task_down
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        docker-compose build --force-rm
}
. "`dirname ${0}`/../../.run/task.sh" ${*}
