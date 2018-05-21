# task example:
# task_example() {
#         echo "this is a task example.";
# }
# . "`dirname ${0}`/.docker-composer/scripts/task.sh" ${*}
set -e
cd `dirname ${0}`

# project task dir
project_task_dir() {
        local f="${RUN_PROJECT_TASK_DIR}"
        if [ ! "${f}" ]; then
                local f=`pwd`
                local d=`pwd`
                while [ ! "${d}" = "/" ]; do
                        [ -d "${d}/.run" ] && f="${d}" && break
                        d=`dirname "${d}"`
                done
                export RUN_PROJECT_TASK_DIR="${f}"
        fi
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

# manage dotrun
_task_dotrun() {
        local deploy_tag="fu-n.net:5000/services/dotrun:latest"
        local bundle_dir="/tmp/dotrun"
        local dockerfile_path="${bundle_dir}/Dockerfile"
        case ${1} in
        update)
                echo "update current .run by '${deploy_tag}' ..."
                cd ${PROJECT_TASK_DIR}
                docker pull ${deploy_tag}
                docker run -v `ospath "${PROJECT_TASK_DIR}/.run"`:/dotrun/run \
                ${deploy_tag} rsync -ahv --delete .run/* run
                ;;
        diff)
                echo "diff between current .run and '${deploy_tag}' ..."
                cd ${PROJECT_TASK_DIR}
                docker pull ${deploy_tag}
                docker run -v `ospath "${PROJECT_TASK_DIR}/.run"`:/dotrun/run \
                        ${deploy_tag} diff -r .run run
                ;;
        push)
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
                ;;
        *)
                echo " update, push, or diff"
                ;;
        esac
}

# display env
_task_env() {
        echo ""
        echo "run config file: ${RUN_CONF_FILE}"
        echo "-------------------------"
        cat ${RUN_CONF_FILE}
        echo "-------------------------"
        echo ""
        echo "run environment name: ${RUN_ENV_NAME}"
        echo "run environment file: ${RUN_ENV_FILE}"
        echo "-------------------------"
        cat ${RUN_ENV_FILE}
        echo "-------------------------"
        echo ""
}

# display help
_task_help() {
        local tasks=`declare -f | grep "^task_" | sed "s/^task_\(.*\) ()/\1/g"`
        printf ">"
        for task in ${tasks}; do
                printf " ${task}"
        done
        printf "\n"
}

# execute task
task() {
        local task=`declare -f | grep "^_\?task_${1} ()" | cut -d" " -f1`
        if [ ! "${task}" ]; then
                _task_help
                [ "${1}" ] && echo "unknown task '${1}'."
                return 0
        fi
        shift 1
        ${task} ${*}
}

# set environment variables
TASK_DIR=`pwd`
PROJECT_TASK_DIR=`project_task_dir`
RUN_DIR="${PROJECT_TASK_DIR}/.run"

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

# execute task
task ${*}
