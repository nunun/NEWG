task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_protocols() { sh ./protocols/protocols.sh ${*}; }
task_test() { sh ./test/test.sh ${*}; }
task_unity() {
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseClientWebGL
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseServerLinuxHeadless
}
task_build() {
        task_down
        task_unity
        docker-compose build --force-rm --pull
        docker-compose run --rm --no-deps matching sh -c \
                "(cd /usr/local/lib/node_modules/services-library && npm install)"
        docker-compose run --rm --no-deps matching npm update
        docker-compose run --rm --no-deps api      npm update
        docker-compose run --rm --no-deps matching npm link services-library
        docker-compose run --rm --no-deps api      npm link services-library
}
task_publish() {
        echo "publish compose image to '${PUBLISH_TO?}' ..."
        task_build
        curl -sSL https://raw.githubusercontent.com/nunun/swarm-builder/master/push.sh \
                | sh -s docker-compose.* "${PUBLISH_TO?}"
}
. "`dirname ${0}`/.task.sh" up ${*}
