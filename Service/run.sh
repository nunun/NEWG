CWD=$(cd `dirname ${0}`; pwd)
PUBLISH_TO="fu-n.net:5000/NEWG/compose"
set -e
cd "${CWD}"

task_up() { task_down; task_build; docker-compose up; }
task_down() { docker-compose down; }
task_build() { task_down; docker-compose build; }
task_publish() {
        local publish_to="${1:-"${PUBLISH_TO}"}"
        echo "publish compose image to '${publish_to}' ..."
        task_build
        curl -sSL https://raw.githubusercontent.com/nunun/swarm-builder/master/push.sh \
                | sh -s docker-compose.* "${publish_to}"
}
task() {
        local task_name="${1:-up}"
        shift
        task_${task_name} ${*}
}
task ${*}
