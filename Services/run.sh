if [ "${1}" = "task" ]; then
        DEFAULT_TASK="${2}"; shift 2; TASK="${1:-"${DEFAULT_TASK}"}"; shift
        cd "`dirname ${0}`"; set -e; [ -f ~/.newg ] && . ~/.newg
        task_${TASK} ${*}
        exit
fi

task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_protocols() { sh ./protocols/protocols.sh ${*}; }
task_test() { sh ./test/test.sh ${*}; }
task_unity() {
        PROJECT_PATH=$(cd ..; pwd)
        [ "${OSTYPE}" = "cygwin" ] && PROJECT_PATH=`cygpath -w ${PROJECT_PATH}`
        OPTIONS="-batchmode -quit -logFile /dev/stdout -projectPath ${PROJECT_PATH}"
        ${UNITY_PATH?} ${OPTIONS} -executeMethod GameBuildMenuItems.BuildReleaseClientWebGL
        ${UNITY_PATH?} ${OPTIONS} -executeMethod GameBuildMenuItems.BuildReleaseServerLinuxHeadless
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
        echo "publish compose image to '${PUBLISH_TO?}' ..."
        task_build
        curl -sSL https://raw.githubusercontent.com/nunun/swarm-builder/master/push.sh \
                | sh -s docker-compose.* "${PUBLISH_TO?}"
}
. "`dirname ${0}`/run.sh" task up ${*}
