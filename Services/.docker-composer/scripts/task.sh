# task example:
# task_example() {
#         echo "this is a task example.";
# }
# . "`dirname ${0}`/.docker-composer/scripts/task.sh" example ${*}
set -e
cd `dirname ${0}`

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
                [ -d "${d}/.docker-composer" ] && f="${d}" && break
                d=`dirname "${d}"`
        done
        echo "${f}"
}

# unity execute shorthand
unity() {
        local project_dir=`project_dir`
        local log_file="/dev/stdout"
        [ "${project_dir}" = "" -o ! -d "${project_dir}/Assets" ] \
                && "Unity ProjectPath could not detected." && exit 1
        [ "${OSTYPE}" = "cygwin" ] \
                && project_dir=`cygpath -w ${project_dir}` \
                && log_file=`cygpath -w "/tmp/unity.log"`
        [ ! -x ${UNITY_PATH?} ] && \
                echo "'${UNITY_PATH}' is not executable." && exit 1
        ${UNITY_PATH} \
                -logFile ${log_file} -projectPath ${project_dir} \
                ${*} & PID="${!}"
        [ "${OSTYPE}" = "cygwin" ] \
                && sleep 2 \
                && tail -n 1000 -F /tmp/unity.log --pid="${PID}" & wait ${PID}
        return ${?}
}

# ospath
ospath() {
        [ "${OSTYPE}" = "cygwin" ] && echo `cygpath -w ${1}` || echo "${1}"
}

# task help
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
PROJECT_NAME=`project_name`
PROJECT_DIR=`project_dir`
PROJECT_TASK_DIR=`project_task_dir`
TASK_DIR=`pwd`
DOCKER_COMPOSER_DIR="${PROJECT_TASK_DIR}/.docker-composer"
DOCKER_COMPOSER_CONFIGS_DIR="${DOCKER_COMPOSER_DIR}/configs"
DOCKER_COMPOSER_ENVIRONMENTS_DIR="${DOCKER_COMPOSER_DIR}/environments"
DOCKER_COMPOSER_SCRIPTS_DIR="${DOCKER_COMPOSER_DIR}/scripts"
DOCKER_COMPOSER_TASKS_DIR="${DOCKER_COMPOSER_DIR}/tasks"

# setup default task
DEFAULT_TASK="${1?no default task}"
shift 1

# load config file
DOCKER_COMPOSER_CONF_FILE="${DOCKER_COMPOSER_CONFIGS_DIR}/docker-composer.conf"
DOCKER_COMPOSER_CONF_EXAMPLE_FILE="${DOCKER_COMPOSER_CONFIGS_DIR}/docker-composer.conf.example"
if [ ! -f "${DOCKER_COMPOSER_CONF_FILE}" -a -f "${DOCKER_COMPOSER_CONF_EXAMPLE_FILE}" ]; then
        cp "${DOCKER_COMPOSER_CONF_EXAMPLE_FILE}" "${DOCKER_COMPOSER_CONF_FILE}"
fi
if [ -f "${DOCKER_COMPOSER_CONF_FILE}" ]; then
        . ${DOCKER_COMPOSER_CONF_FILE}
fi

# load environment file
DOCKER_COMPOSER_ENV_NAME="${DOCKER_COMPOSER_ENV_NAME_EXPORTED:-"${1}"}"
DOCKER_COMPOSER_ENV_NAME_WITH_DOT=".${DOCKER_COMPOSER_ENV_NAME}"
DOCKER_COMPOSER_ENV_NAME_WITH_SPACE=" ${DOCKER_COMPOSER_ENV_NAME}"
DOCKER_COMPOSER_ENV_FILE="${DOCKER_COMPOSER_ENVIRONMENTS_DIR}/${DOCKER_COMPOSER_ENV_NAME}"
if [ ! -f "${DOCKER_COMPOSER_ENV_FILE}" ]; then
        DOCKER_COMPOSER_ENV_NAME=""
        DOCKER_COMPOSER_ENV_NAME_WITH_DOT=""
        DOCKER_COMPOSER_ENV_NAME_WITH_SPACE=""
        DOCKER_COMPOSER_ENV_FILE="${DOCKER_COMPOSER_ENVIRONMENTS_DIR}/local"
else
        [ -z "${DOCKER_COMPOSER_ENV_NAME_EXPORTED}" ] && shift 1
        export DOCKER_COMPOSER_ENV_NAME_EXPORTED="${DOCKER_COMPOSER_ENV_NAME}"
fi
if [ -f "${DOCKER_COMPOSER_ENV_FILE}" ]; then
        . ${DOCKER_COMPOSER_ENV_FILE}
fi

# setup execute task
TASK="${1}"
if [ "${TASK}" = "" ]; then
        TASK="${DEFAULT_TASK}"
else
        shift 1
fi

# call task
task_${TASK} ${*}
