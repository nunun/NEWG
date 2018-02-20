TASK="${2:-"${1?}"}"
shift 2
set -e
cd "`dirname ${0}`"

# project name
project_name() {
        local f="prj"
        local d=`pwd`
        while [ ! "${d}" = "/" ]; do
                [ -f "${d}/.task.sh" -o -d "${d}/Assets" ] && f=`basename ${d}`
                d=`dirname "${d}"`
        done
        echo "${f}"
}

# project dir
project_dir() {
        local f=`pwd`
        local d=`pwd`
        while [ ! "${d}" = "/" ]; do
                [ -f "${d}/.task.sh" ] && f="${d}" && break
                d=`dirname "${d}"`
        done
        echo "${f}"
}

# unity
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

# setup environment variables
PROJECT_NAME=`project_name`
PROJECT_DIR=`project_dir`

# load env file
[ -f ~/.env.${PROJECT_NAME} ] && . ~/.env.${PROJECT_NAME}

# debug message
#echo "PROJECT_NAME=${PROJECT_NAME}"
#echo "PROJECT_DIR=${PROJECT_DIR}"
#echo "TASK=${TASK}"
#echo "(on `pwd`)"
#echo ""

# execute task
task_${TASK} ${*}
