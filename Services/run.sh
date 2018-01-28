CWD=$(cd `dirname ${0}`; pwd)
PUBLISH_TO="fu-n.net:5000/newg/compose"
TASK="${1:-"up"}"
shift
set -e
cd "${CWD}"

task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_matching() { docker-compose run --rm matching node app.js; }
task_build() {
        task_down; docker-compose build;
        git submodule init; git submodule update;
        docker-compose run --rm --no-deps matching npm update
}
task_publish() {
        local publish_to="${1:-"${PUBLISH_TO}"}"
        echo "publish compose image to '${publish_to}' ..."
        task_build
        curl -sSL https://raw.githubusercontent.com/nunun/swarm-builder/master/push.sh \
                | sh -s docker-compose.* "${publish_to}"
}
task_${TASK} ${*}
