CWD=$(cd `dirname ${0}`; pwd)
PUBLISH_TO="fu-n.net:5000/newg/compose"
TASK="${1:-"up"}"
shift
set -e
cd "${CWD}"

task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() { sh ./test/test.sh ${*}; }
task_build() {
        task_down; docker-compose build --force-rm --pull
        git submodule update --init --recursive --remote
        docker-compose run --rm --no-deps matching sh -c \
                "(cd /usr/local/lib/node_modules/libservices && npm install)"
        docker-compose run --rm --no-deps matching npm update
        docker-compose run --rm --no-deps matching npm link libservices
}
task_publish() {
        local publish_to="${1:-"${PUBLISH_TO}"}"
        echo "publish compose image to '${publish_to}' ..."
        task_build
        curl -sSL https://raw.githubusercontent.com/nunun/swarm-builder/master/push.sh \
                | sh -s docker-compose.* "${publish_to}"
}
task_${TASK} ${*}
