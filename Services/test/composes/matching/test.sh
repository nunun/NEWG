task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() {
        docker-compose run --rm test-matching-client mocha test.js
}
task_build() {
        task_down
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        docker-compose run --rm --no-deps test-matching-client npm update
        docker-compose run --rm --no-deps matching             npm update
        docker-compose run --rm --no-deps api                  npm update
        docker-compose run --rm --no-deps mindlink             npm update
}
task_clean() {
        docker-compose run --rm --no-deps test-matching-client rm -rf node_modules package-lock.json
        docker-compose run --rm --no-deps matching             rm -rf node_modules package-lock.json
        docker-compose run --rm --no-deps api                  rm -rf node_modules package-lock.json
        docker-compose run --rm --no-deps mindlink             rm -rf node_modules package-lock.json
}
. "`dirname ${0}`/../../../.task.sh" test ${*}
