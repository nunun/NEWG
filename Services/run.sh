task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_services() { sh ./services/services.sh ${*}; }
task_test() { sh ./test/test.sh ${*}; }
task_unity() {
        echo "building unity projects ..."
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseClientWebGL
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseServerLinuxHeadless
}
task_build() {
        echo "building docker images ..."
        task_down
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        #docker-compose run --rm --no-deps matching npm install
        #docker-compose run --rm --no-deps api      npm install
        #docker-compose run --rm --no-deps mindlink npm install
}
task_clean() {
        echo "cleaning build caches ..."
        find ${PROJECT_TASK_DIR} -name "node_modules"      -exec rm -rf '{}' +
        find ${PROJECT_TASK_DIR} -name "package-lock.json" -exec rm -rf '{}' +
}
task_stack() { sh .stack.sh ${*}; }
. "`dirname ${0}`/.task.sh" up ${*}
