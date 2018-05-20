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
                [ -d "${d}/.run" ] && f="${d}" && break
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

# deploy tag
deploy_tag() {
        echo "fu-n.net:5000/services/dotrun:${1:-"lastet"}"
}

task_update() {
        local deploy_tag=`deploy_tag ${*}`
        echo "update current .run by '${deploy_tag}' ..."
        cd ${PROJECT_TASK_DIR}
        docker pull ${deploy_tag}
        docker run -v `cygpath -w "${PROJECT_TASK_DIR}/.run"`:/dotrun/run ${1} \
                rsync -ahv --delete .run/* run
}

task_diff() {
        local deploy_tag=`deploy_tag ${*}`
        echo "diff between current .run and '${deploy_tag}' ..."
        cd ${PROJECT_TASK_DIR}
        docker pull ${deploy_tag}
        docker run -v `ospath "${PROJECT_TASK_DIR}/.run"`:/dotrun/run \
                ${deploy_tag} diff -r .run run
}

task_push() {
        local deploy_tag=`deploy_tag ${*}`
        local bundle_dir="/tmp/dotrun"
        local dockerfile_path="${bundle_dir}/Dockerfile"
        echo "push current .run to '${deploy_tag}' ..."
        rm -rf "${bundle_dir}"
        mkdir -p "${bundle_dir}"
        cp -r "${PROJECT_TASK_DIR}/.run" "${bundle_dir}/.run"
        echo "FROM alpine"                   > "${dockerfile_path}"
        echo "RUN apk add --no-cache rsync" >> "${dockerfile_path}"
        echo "WORKDIR /dotrun"              >> "${dockerfile_path}"
        echo "ADD .run ./.run"              >> "${dockerfile_path}"
        (cd ${bundle_dir}; \
                docker build --no-cache -t "${deploy_tag}" .; \
                docker push "${deploy_tag}")
        echo "done."
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
TASK_DIR=`pwd`
PROJECT_NAME=`project_name`
PROJECT_DIR=`project_dir`
PROJECT_TASK_DIR=`project_task_dir`
RUN_DIR="${PROJECT_TASK_DIR}/.run"

# setup default task
DEFAULT_TASK="${1?no default task}"
shift 1

# load config file
RUN_CONF_FILE="${PROJECT_TASK_DIR}/.run.conf"
RUN_CONF_EXAMPLE_FILE="${RUN_DIR}/run.conf.example"
if [ ! -f "${RUN_CONF_FILE}" -a -f "${RUN_CONF_EXAMPLE_FILE}" ]; then
        cp "${RUN_CONF_EXAMPLE_FILE}" "${RUN_CONF_FILE}"
fi
if [ -f "${RUN_CONF_FILE}" ]; then
        . ${RUN_CONF_FILE}
fi

# load environment file
RUN_ENV_NAME="${RUN_ENV_NAME_EXPORTED:-"${1}"}"
RUN_ENV_NAME_WITH_DOT=".${RUN_ENV_NAME}"
RUN_ENV_NAME_WITH_SPACE=" ${RUN_ENV_NAME}"
RUN_ENV_FILE="${PROJECT_TASK_DIR}/.run.env${RUN_ENV_NAME_WITH_DOT}"
RUN_ENV_EXAMPLE_FILE="${RUN_DIR}/run.env.example"
if [ -f "${RUN_ENV_FILE}" ]; then
        if [ -z "${RUN_ENV_NAME_EXPORTED}" ]; then
                shift 1
                export RUN_ENV_NAME_EXPORTED="${RUN_ENV_NAME}"
        fi
else
        RUN_ENV_NAME="local"
        RUN_ENV_NAME_WITH_DOT=""
        RUN_ENV_NAME_WITH_SPACE=""
        RUN_ENV_FILE="${PROJECT_TASK_DIR}/.run.env${RUN_ENV_NAME_WITH_DOT}"
        if [ ! -f "${RUN_ENV_FILE}" -a -f "${RUN_ENV_EXAMPLE_FILE}" ]; then
                cp "${RUN_ENV_EXAMPLE_FILE}" "${RUN_ENV_FILE}"
        fi
fi
if [ -f "${RUN_ENV_FILE}" ]; then
        . ${RUN_ENV_FILE}
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
