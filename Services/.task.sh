# task example:
# task_example() { echo "this is a task example."; }
# . "`dirname ${0}`/.task.sh" up ${*}
set -e
cd "`dirname ${0}`"

# project name
project_name() {
        local d=`project_dir`
        d=`basename "${d}"`
        echo "${d}"
}

# project dir
project_dir() {
        local d=`project_task_dir`
        [ -d "${d}/../Assets" ] && d=`dirname "${d}"`
        echo "${d}"
}

# project task dir
project_task_dir() {
        local f=`pwd`
        local d=`pwd`
        while [ ! "${d}" = "/" ]; do
                [ -f "${d}/.task.sh" ] && f="${d}" && break
                d=`dirname "${d}"`
        done
        echo "${f}"
}

# unity project path
unity_project_path() {
        local f=""
        local d=`pwd`
        while [ ! "${d}" = "/" ]; do
                [ -d "${d}/Assets" ] && f="${d}" && break
                d=`dirname "${d}"`
        done
        echo "${f}"
}

# unity execute shorthand
unity() {
        local unity_project_path=`unity_project_path`
        local log_file="/dev/stdout"
        [ "${unity_project_path}" = "" ] \
                && "Unity ProjectPath could not detected." && exit 1
        [ "${OSTYPE}" = "cygwin" ] \
                && unity_project_path=`cygpath -w ${unity_project_path}` \
                && log_file=`cygpath -w "/tmp/unity.log"`
        [ ! -x ${UNITY_PATH?} ] && \
                echo "'${UNITY_PATH}' is not executable." && exit 1
        ${UNITY_PATH} \
                -logFile ${log_file} -projectPath ${unity_project_path} \
                ${*} & PID="${!}"
        [ "${OSTYPE}" = "cygwin" ] \
                && sleep 2 \
                && tail -n 1000 -F /tmp/unity.log --pid="${PID}" & wait ${PID}
        return ${?}
}

# help
task_help() {
        local tasks=`declare -f | grep "^task_" | sed "s/^task_\(.*\) ()/\1/g"`
        for t in ${tasks}; do
                [ "${t}" = "${DEFAULT_TASK}" ] \
                        && printf " [${t}]" \
                        || printf " ${t}"
        done
        echo ""
}

# setup environment variables
TASK_DIR=`pwd`
PROJECT_NAME=`project_name`
PROJECT_DIR=`project_dir`
PROJECT_TASK_DIR=`project_task_dir`

# setup default task
DEFAULT_TASK="${1?specify default task}"
shift 1

# load .task.conf file
TASK_CONF_FILE="${PROJECT_TASK_DIR}/.task.conf"
TASK_CONF_EXAMPLE_FILE="${PROJECT_TASK_DIR}/${TASK_CONF_FILE}.example"
if [ ! -f "${TASK_CONF_FILE}" ]; then
        #echo "(copy ${TASK_CONF_FILE_EXAMPLE} to ${TASK_CONF_FILE})"
        cp ${TASK_CONF_FILE_EXAMPLE} ${TASK_CONF_FILE}
fi
if [   -f "${TASK_CONF_FILE}" ]; then
        #echo "(load ${TASK_CONF_FILE})"
        . ${TASK_CONF_FILE}
fi

# load .task.env file
TASK_ENV_FILE="${TASK_DIR}/.task.env.${1}"
TASK_ENV_DEFAULT_FILE="${TASK_DIR}/.task.env"
if [ -f "${TASK_ENV_FILE}" ]; then
        #echo "(load ${TASK_ENV_FILE})"
        . ${TASK_ENV_FILE}
        shift 1
elif [ -f "${TASK_ENV_DEFAULT_FILE}" ]; then
        #echo "(load ${TASK_ENV_DEFAULT_FILE})"
        . ${TASK_ENV_DEFAULT_FILE}
fi

# setup execute task
EXECUTE_TASK=${1:-"${DEFAULT_TASK}"}
shift 1

# execute task
task_${EXECUTE_TASK} ${*}
