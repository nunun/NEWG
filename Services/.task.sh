TASK="${2:-"${1?}"}"
shift 2
set -e
cd "`dirname ${0}`"

# load env file
[ -f ~/.env.newg ] && . ~/.env.newg

# execute unity
unity() {
        PROJECT_PATH="$(cd "${1?}"; pwd)"
        LOG_FILE="/tmp/unity.log"
        [ "${OSTYPE}" = "cygwin" ] \
                && PROJECT_PATH=`cygpath -w ${PROJECT_PATH}` \
                && LOG_FILE=`cygpath -w "${LOG_FILE}"`
        shift
        ${UNITY_PATH?} -batchmode -quit \
                -logFile ${LOG_FILE} -projectPath ${PROJECT_PATH} \
                ${*} & PID="${!}"
        sleep 1 && tail -F /tmp/unity.log --pid="${PID}" & wait ${PID}
}

# execute task
task_${TASK} ${*}
