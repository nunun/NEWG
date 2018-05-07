task_deploy() {
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        docker stack deploy ${ENV_STACK_NAME} --compose-file ${STACK_FILE}
        echo "stack '${ENV_STACK_NAME}' created."
}

task_rm() {
        docker stack rm ${ENV_STACK_NAME}
        echo "stack '${ENV_STACK_NAME}' removed."
}

task_build() {
        echo "building stack file ..."
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        BUILD_YAMLS="-f docker-compose.yml -f docker-compose.stack.build.yml"
        CONFIG_YAMLS="-f docker-compose.yml -f docker-compose.stack.config.yml"
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        #(cd ${PROJECT_TASK_DIR}; sh run.sh unity)
        docker-compose ${BUILD_YAMLS} build --force-rm --no-cache
        docker-compose ${BUILD_YAMLS} push
        docker-compose ${CONFIG_YAMLS} config --resolve-image-digest > ${STACK_FILE}
        echo ""
        echo "successfully wrote to '${STACK_FILE}'."
        echo "* 'sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} stack deploy' to deploy stack locally."
        echo "* 'sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} stack push'   to upload stack to '${ENV_STACK_DEPLOY_TAG}'."
}

task_push() {
        echo "push stack file to ${ENV_STACK_DEPLOY_TAG} ..."
        BUNDLE_DIR="/tmp/stack"
        DOCKER_FILE="${BUNDLE_DIR}/Dockerfile"
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        mkdir -p ${BUNDLE_DIR}
        cp ${STACK_FILE}                 ${BUNDLE_DIR}/${STACK_FILE}
        cp ${TASK_ENV_FILE}              ${BUNDLE_DIR}/.task.env
        cp ${PROJECT_TASK_DIR}/.task.sh  ${BUNDLE_DIR}/.task.sh
        cp ${PROJECT_TASK_DIR}/.stack.sh ${BUNDLE_DIR}/.stack.sh
        echo "FROM alpine"           > "${DOCKER_FILE}"
        echo "WORKDIR /stack"       >> "${DOCKER_FILE}"
        echo "ADD ${STACK_FILE} ./" >> "${DOCKER_FILE}"
        echo "ADD .task.env     ./" >> "${DOCKER_FILE}"
        echo "ADD .task.sh      ./" >> "${DOCKER_FILE}"
        echo "ADD .stack.sh     ./" >> "${DOCKER_FILE}"
        (cd ${BUNDLE_DIR}; \
         docker build --no-cache -t ${ENV_STACK_DEPLOY_TAG} .; \
         docker push ${ENV_STACK_DEPLOY_TAG})
        echo ""
        echo "successfully upload stack to '${ENV_STACK_DEPLOY_TAG}'."
        echo "* 'sh services/stack.sh ${ENV_STACK_DEPLOY_TAG} deploy' to deploy stack to host."
        echo "* 'sh services/stack.sh ${ENV_STACK_DEPLOY_TAG} rm'     to remove stack from host."
}

task_usage() {
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
}

. "`dirname ${0}`/.task.sh" usage ${*}
