TASK="${2:-"${1?}"}"
shift 2
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
        ${UNITY_PATH} -batchmode -quit \
                -logFile ${log_file} -projectPath ${unity_project_path} \
                ${*} & PID="${!}"
        [ "${OSTYPE}" = "cygwin" ] \
                && sleep 1 \
                && tail -F /tmp/unity.log --pid="${PID}" & wait ${PID}
}

# setup environment variables
PROJECT_NAME=`project_name`
PROJECT_DIR=`project_dir`
PROJECT_TASK_DIR=`project_task_dir`
ENV_FILE="${HOME}/.env.${PROJECT_NAME}"

# load env file
[ -f "${ENV_FILE}" ] && . "${ENV_FILE}"

# execute task
task_${TASK} ${*}
