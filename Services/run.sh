task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_services() { sh ./services/services.sh ${*}; }
task_test() { sh ./test/test.sh ${*}; }
task_unity() {
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseClientWebGL
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseServerLinuxHeadless
}
task_build() {
        task_down
        #task_unity
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        docker-compose run --rm --no-deps matching npm install
        docker-compose run --rm --no-deps api      npm install
}
task_publish() {
        echo "publish compose image to '${PUBLISH_TO?}' ..."
        task_build
        curl -sSL https://raw.githubusercontent.com/nunun/swarm-builder/master/push.sh \
                | sh -s docker-compose.* "${PUBLISH_TO?}"
}
. "`dirname ${0}`/.task.sh" up ${*}
