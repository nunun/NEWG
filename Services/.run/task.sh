# task example:
# task_example() {
#         echo "this is a task example.";
# }
# . "`dirname ${0}`/.docker-composer/scripts/task.sh" ${*}
set -e
cd `dirname ${0}`

# run_root_dir
run_root_dir() {
        local root_dir="${RUN_ROOT_DIR_EXPORTED}"
        if [ ! "${root_dir}" ]; then
                root_dir=`pwd`
                while :; do
                        [ "${root_dir}" = "/" ] && \
                                abort "no run root directory."
                        [ -d "${root_dir}/.run" ] && \
                                break
                        root_dir=`dirname "${root_dir}"`
                done
                export RUN_ROOT_DIR_EXPORTED="${root_dir}"
        fi
        echo "${root_dir}"
}

# manage .run
_task_dotrun() {
        local deploy_tag="fu-n.net:5000/services/dotrun:latest"
        local bundle_dir="/tmp/dotrun"
        local dockerfile_path="${bundle_dir}/Dockerfile"
        case ${1} in
        push)
                echo_info "push current .run to '${deploy_tag}' ..."
                rm -rf "${bundle_dir}"
                mkdir -p "${bundle_dir}"
                cp -r "${RUN_ROOT_DIR}/.run" "${bundle_dir}/.run"
                echo "FROM alpine"                   > "${dockerfile_path}"
                echo "RUN apk add --no-cache rsync" >> "${dockerfile_path}"
                echo "WORKDIR /dotrun"              >> "${dockerfile_path}"
                echo "ADD .run ./.run"              >> "${dockerfile_path}"
                (cd ${bundle_dir}; \
                        docker build --no-cache -t "${deploy_tag}" .; \
                        docker push "${deploy_tag}")
                echo_info "done."
                ;;
        pull)
                echo_info "pull current .run from '${deploy_tag}' ..."
                cd ${RUN_ROOT_DIR}
                docker pull ${deploy_tag}
                docker run -v `ospath "${RUN_ROOT_DIR}/.run"`:/dotrun/run \
                ${deploy_tag} rsync -ahv --delete .run/* run
                echo_info "done."
                ;;
        diff)
                echo_info "diff between current .run and '${deploy_tag}' ..."
                cd ${RUN_ROOT_DIR}
                docker pull ${deploy_tag}
                docker run -v `ospath "${RUN_ROOT_DIR}/.run"`:/dotrun/run \
                        ${deploy_tag} diff -r .run run || :
                echo_info "done."
                ;;
        *)
                echo " push, pull, or diff"
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
        local line=">"; for task in ${tasks}; do line="${line} ${task}"; done
        echo "${line}"
        if [ `is_debug` ]; then
                tasks=`declare -f | grep "^_task_" | sed "s/^_task_\(.*\) ()/\1/g"`
                line=">"; for task in ${tasks}; do line="${line} ${task}"; done
                echo_debug "${line}"
        fi
}

# execute task
task() {
        local task=`declare -f | grep "^_\?task_${1} ()" | cut -d" " -f1`
        if [ ! "${task}" ]; then
                _task_help
                [ "${1}" ] && abort "unknown task '${1}'."
                return 0
        fi
        shift 1
        ${task} ${*}
}

# unity execute shorthand
unity() {
        local project_dir=`pwd`
        while :; do
                [ "${project_dir}" = "/" ] && \
                        abort "no unity project directory"
                [ -d "${project_dir}/Assets" ] && \
                        break
                project_dir=`dirname "${project_dir}"`
        done
        local log_file="/dev/stdout"
        [ "${project_dir}" = "" -o ! -d "${project_dir}/Assets" ] \
                && abort "Unity ProjectPath could not detected."
        [ "${OSTYPE}" = "cygwin" ] \
                && project_dir=`cygpath -w ${project_dir}` \
                && log_file=`cygpath -w "/tmp/unity.log"`
        [ ! -x ${UNITY_PATH?} ] && \
                abort "'${UNITY_PATH}' is not executable."
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

# echo fatal output
echo_fatal() {
        if [ ${RUN_OUTPUT_LEVEL} -ge 0 ]; then echo_err "fatal: ${*}"; exit 1; fi
}

# echo error output
echo_err() {
        if [ ${RUN_OUTPUT_LEVEL} -ge 1 ]; then echo_output 1 ${*}; fi
}

# echo warning output
echo_warn() {
        if [ ${RUN_OUTPUT_LEVEL} -ge 2 ]; then echo_output 3 ${*}; fi
}

# echo info output
echo_info() {
        if [ ${RUN_OUTPUT_LEVEL} -ge 3 ]; then echo_output 2 ${*}; fi
}

# echo debug output
echo_debug() {
        if [ ${RUN_OUTPUT_LEVEL} -ge 4 ]; then echo_output 4 ${*}; fi
}

# echo develop output
echo_dev() {
        if [ ${RUN_OUTPUT_LEVEL} -ge 5 ]; then echo_output 4 ${*}; fi
}

# echo output
echo_output() {
        local color="${1}"
        shift 1
        tput setaf "${color}"
        echo ${*}
        tput sgr0
}

# check debug level
is_debug() {
        if [ ${RUN_OUTPUT_LEVEL} -ge 4 ]; then echo "yes"; fi
}

# error abort
abort() {
        echo_err "abort: ${1}"
        exit ${2:-1} # exit immediately
}

# set environment variables
RUN_DIR=`pwd`
RUN_ROOT_DIR=`run_root_dir`
RUN_DOTRUN_DIR="${RUN_ROOT_DIR}/.run"
RUN_OUTPUT_LEVEL=3 # fatal=0, error=1, warn=2, info=3, debug=4, develop=5

# parse options
while getopts "vqb" OPT; do
        case $OPT in
        v) RUN_OUTPUT_LEVEL=`expr ${RUN_OUTPUT_LEVEL} + 1`;;
        q) RUN_OUTPUT_LEVEL=`expr ${RUN_OUTPUT_LEVEL} - 1`;;
        esac
done
shift `expr ${OPTIND} - 1`

# load config file
RUN_CONF_FILE="${RUN_ROOT_DIR}/.run.conf"
RUN_CONF_EXAMPLE_FILE="${RUN_DOTRUN_DIR}/run.conf.example"
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
RUN_ENV_FILE="${RUN_ROOT_DIR}/.env${RUN_ENV_NAME_WITH_DOT}"
RUN_ENV_EXAMPLE_FILE="${RUN_DOTRUN_DIR}/env.example"
if [ -f "${RUN_ENV_FILE}" ]; then
        if [ -z "${RUN_ENV_NAME_EXPORTED}" ]; then
                shift 1
                export RUN_ENV_NAME_EXPORTED="${RUN_ENV_NAME}"
        fi
else
        RUN_ENV_NAME="local"
        RUN_ENV_NAME_WITH_DOT=""
        RUN_ENV_NAME_WITH_SPACE=""
        RUN_ENV_FILE="${RUN_ROOT_DIR}/.env${RUN_ENV_NAME_WITH_DOT}"
        if [ ! -f "${RUN_ENV_FILE}" -a -f "${RUN_ENV_EXAMPLE_FILE}" ]; then
                cp "${RUN_ENV_EXAMPLE_FILE}" "${RUN_ENV_FILE}"
        fi
fi
if [ -f "${RUN_ENV_FILE}" ]; then
        set -a && . ${RUN_ENV_FILE} && set +a
fi

# execute task
task ${*}
