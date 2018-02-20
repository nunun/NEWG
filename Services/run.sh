CWD=$(cd `dirname ${0}`; pwd)
PUBLISH_TO="fu-n.net:5000/newg/compose"
TASK="${1:-"up"}"
shift
set -e
cd "${CWD}"

task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_api() { docker-compose run --rm --no-deps api node app.js; }
task_protocols() { sh ./protocols/protocols.sh ${*}; }
task_test() { sh ./test/test.sh ${*}; }
task_unity() {
        UNITY_PATH="/Applications/Unity5.6.0f3/Unity.app/Contents/MacOS/Unity"
        PROJECT_PATH=$(cd ..; pwd)
        OPTIONS="-batchmode -quit -logFile /dev/stdout -projectPath ${PROJECT_PATH}"
        ${UNITY_PATH} ${OPTIONS} -executeMethod GameBuildMenuItems.BuildReleaseClientWebGL \
        ${UNITY_PATH} ${OPTIONS} -executeMethod GameBuildMenuItems.BuildReleaseServerLinuxHeadless
}
task_build() {
        task_down
        task_unity
        docker-compose build --force-rm --pull
        git submodule update --init --recursive --remote
        docker pull nunun/mindlink
        docker-compose run --rm --no-deps matching sh -c \
                "(cd /usr/local/lib/node_modules/libservices && npm install)"
        docker-compose run --rm --no-deps matching npm update
        docker-compose run --rm --no-deps api      npm update
        docker-compose run --rm --no-deps matching npm link libservices
        docker-compose run --rm --no-deps api      npm link libservices
}
task_publish() {
        local publish_to="${1:-"${PUBLISH_TO}"}"
        echo "publish compose image to '${publish_to}' ..."
        task_build
        curl -sSL https://raw.githubusercontent.com/nunun/swarm-builder/master/push.sh \
                | sh -s docker-compose.* "${publish_to}"
}
task_${TASK} ${*}
