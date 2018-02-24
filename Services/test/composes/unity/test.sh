task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_test() {
        local filter="${PROJECT_DIR}/Library/ScriptAssemblies/Assembly-CSharp.dll"
        local xml="/tmp/result.xml"
        [ "${OSTYPE}" = "cygwin" ] \
                && filter=`cygpath -w "${filter}" | sed -e "s/\\\\\\/\\//g"` \
                && xml=`cygpath -w "${xml}"`
        docker-compose up -d
        unity -runTests -testFilter ${filter} -testResults "${xml}"
        cat /tmp/result.xml
}
task_build() {
        task_down; docker-compose build --force-rm --pull
        docker pull nunun/mindlink
        docker-compose run --rm --no-deps matching sh -c \
                "(cd /usr/local/lib/node_modules/services-library && npm install)"
        docker-compose run --rm --no-deps matching npm update
        docker-compose run --rm --no-deps api      npm update
        docker-compose run --rm --no-deps matching npm link services-library
        docker-compose run --rm --no-deps api      npm link services-library
}
. "`dirname ${0}`/../../../.task.sh" test ${*}
