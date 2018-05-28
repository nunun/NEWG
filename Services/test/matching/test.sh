task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_try() {
        docker-compose run --rm test-matching-client mocha test.js
}
task_build() {
        task_down
        (cd ${RUN_ROOT_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        docker-compose run --rm --no-deps test-matching-client npm install
        docker-compose run --rm --no-deps matching             npm install
        docker-compose run --rm --no-deps api                  npm install
        docker-compose run --rm --no-deps mindlink             npm install
}
. "`dirname ${0}`/../../.run/task.sh" ${*}
