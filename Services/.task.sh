# task example:
# task_example() {
#         echo "this is a task example.";
# }
# . "`dirname ${0}`/.task.sh" example ${*}
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

# task stack
task_stack() {
        case "${1}" in
        deploy)
                docker stack deploy ${ENV_STACK_NAME} --compose-file ${ENV_STACK_BUNDLE_FILE}
                echo "stack '${ENV_STACK_NAME}' created."
                ;;
        rm)
                docker stack rm ${ENV_STACK_NAME}
                echo "stack '${ENV_STACK_NAME}' removed."
                ;;
        build)
                echo "building stack file ..."
                (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
                BUILD_YAMLS="${ENV_STACK_BUNDLE_BUILD_YAMLS}"
                CONFIG_YAMLS="${ENV_STACK_BUNDLE_CONFIG_YAMLS}"
                if [ ! "${ENV_STACK_BUNDLE_PREPROCESS_TASKS}" = "" ]; then
                        for t in ${ENV_STACK_BUNDLE_PREPROCESS_TASKS}; do ${t}; done
                fi
                docker-compose ${BUILD_YAMLS} build --force-rm --no-cache
                if [ ! "${ENV_STACK_BUNDLE_POSTPROCESS_TASKS}" = "" ]; then
                        for t in ${ENV_STACK_BUNDLE_POSTPROCESS_TASKS}; do ${t}; done
                fi
                docker-compose ${BUILD_YAMLS} push
                docker-compose ${CONFIG_YAMLS} config --resolve-image-digest > ${ENV_STACK_BUNDLE_FILE}
                echo ""
                echo "successfully wrote to '${ENV_STACK_BUNDLE_FILE}'."
                echo "* 'sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} stack deploy' to deploy stack locally."
                echo "* 'sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} stack push'   to upload stack to '${ENV_STACK_DEPLOY_TAG}'."
                ;;
        push)
                echo "push stack file to ${ENV_STACK_DEPLOY_TAG} ..."
                BUNDLE_DIR="/tmp/stack"
                DOCKER_FILE="${BUNDLE_DIR}/Dockerfile"
                TASK_SH_FILE="${PROJECT_TASK_DIR}/.task.sh"
                mkdir -p ${BUNDLE_DIR}
                cp ${ENV_STACK_BUNDLE_FILE}     ${BUNDLE_DIR}/${ENV_STACK_BUNDLE_FILE}
                cp ${TASK_ENV_FILE}             ${BUNDLE_DIR}/.task.env
                cp ${PROJECT_TASK_DIR}/.task.sh ${BUNDLE_DIR}/.task.sh
                echo "FROM alpine"                      > "${DOCKER_FILE}"
                echo "WORKDIR /stack"                  >> "${DOCKER_FILE}"
                echo "ADD ${ENV_STACK_BUNDLE_FILE} ./" >> "${DOCKER_FILE}"
                echo "ADD .task.env                ./" >> "${DOCKER_FILE}"
                echo "ADD .task.sh                 ./" >> "${DOCKER_FILE}"
                (cd ${BUNDLE_DIR}; \
                 docker build --no-cache -t ${ENV_STACK_DEPLOY_TAG} .; \
                 docker push ${ENV_STACK_DEPLOY_TAG})
                echo ""
                echo "successfully upload stack to '${ENV_STACK_DEPLOY_TAG}'."
                echo "* 'sh services/stack.sh ${ENV_STACK_DEPLOY_TAG} deploy' to deploy stack to host."
                echo "* 'sh services/stack.sh ${ENV_STACK_DEPLOY_TAG} rm'     to remove stack from host."
                ;;
        *)
                echo ""
                echo "sh run.sh <env> stack build"
                echo "sh run.sh <env> stack push"
                echo "sh run.sh <env> stack deploy"
                echo "sh run.sh <env> stack rm"
                echo "sh services/stack.sh <deploy tag> deploy"
                echo "sh services/stack.sh <deploy tag> rm"
                echo ""
                echo "ex) to up local stack on local host, write .task.env and do below:"
                echo "  sh run.sh stack build"
                echo "  sh run.sh stack deploy"
                echo "  sh run.sh stack rm"
                echo ""
                echo "ex) to up develop stack on remote host, write .task.env.develop and do below:"
                echo "  sh run.sh develop stack build"
                echo "  sh run.sh develop stack push"
                echo "  sh services/stack.sh registry:5000/myapp/stack deploy"
                echo "  sh services/stack.sh registry:5000/myapp/stack rm"
                echo ""
                if [ ! "${1}" = "" ]; then
                        echo "invalid command (${1})."
                fi
                ;;
        esac
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

# setup default task
DEFAULT_TASK="${1?specify default task}"
shift 1

# load .task.conf file
TASK_CONF_FILE="${PROJECT_TASK_DIR}/.task.conf"
TASK_CONF_EXAMPLE_FILE="${PROJECT_TASK_DIR}/${TASK_CONF_FILE}.example"
if [ ! -f "${TASK_CONF_FILE}" -a -f "${TASK_CONF_EXAMPLE_FILE}" ]; then
        #echo "(copy ${TASK_CONF_FILE_EXAMPLE} to ${TASK_CONF_FILE})"
        cp ${TASK_CONF_FILE_EXAMPLE} ${TASK_CONF_FILE}
fi
if [ -f "${TASK_CONF_FILE}" ]; then
        #echo "(load ${TASK_CONF_FILE})"
        . ${TASK_CONF_FILE}
fi

# load .task.env file
TASK_ENV_NAME=""
TASK_ENV_FILE="${TASK_DIR}/.task.env.${1}"
TASK_ENV_DEFAULT_FILE="${TASK_DIR}/.task.env"
if [ -f "${TASK_ENV_FILE}" ]; then
        TASK_ENV_NAME="${1}"
        shift 1
else
        TASK_ENV_NAME=""
        TASK_ENV_FILE="${TASK_ENV_DEFAULT_FILE}"
fi
if [ -f "${TASK_ENV_FILE}" ]; then
        #echo "(load ${TASK_ENV_FILE})"
        . ${TASK_ENV_FILE}
fi

# setup execute task
EXECUTE_TASK="${1}"
if [   "${EXECUTE_TASK}" = "" ]; then
        EXECUTE_TASK="${DEFAULT_TASK}"
else
        shift 1
fi

# execute task
task_${EXECUTE_TASK} ${*}
