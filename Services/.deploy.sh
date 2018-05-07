task_up() {
        echo "deploy: starting '${ENV_STACK_NAME}' ..."
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        docker stack deploy ${ENV_STACK_NAME} \
                --with-registry-auth --compose-file ${STACK_FILE}
}

task_down() {
        echo "deploy: stopping '${ENV_STACK_NAME}' ..."
        docker stack rm ${ENV_STACK_NAME}
}

task_build() {
        echo "deploy: building stack file ..."
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        BUILD_YAMLS="-f docker-compose.yml -f docker-compose.deploy.build.yml"
        CONFIG_YAMLS="-f docker-compose.yml -f docker-compose.deploy.strategy.yml"
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        #(cd ${PROJECT_TASK_DIR}; sh run.sh unity)
        docker-compose ${BUILD_YAMLS} build --force-rm --no-cache
        docker-compose ${BUILD_YAMLS} push
        docker-compose ${CONFIG_YAMLS} config --resolve-image-digest > ${STACK_FILE}
        echo ""
        echo "successfully build stack file '${STACK_FILE}'."
        echo "* 'sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} deploy up'   to up   stack locally."
        echo "* 'sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} deploy push' to push stack file to '${ENV_STACK_DEPLOY_TAG}'."
}

task_push() {
        echo "deploy: push stack file to ${ENV_STACK_DEPLOY_TAG} ..."
        BUNDLE_DIR="/tmp/deploy"
        DOCKER_FILE="${BUNDLE_DIR}/Dockerfile"
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        mkdir -p ${BUNDLE_DIR}
        cp ${STACK_FILE}                  ${BUNDLE_DIR}/${STACK_FILE}
        cp ${TASK_ENV_FILE}               ${BUNDLE_DIR}/.task.env
        cp ${PROJECT_TASK_DIR}/.task.sh   ${BUNDLE_DIR}/.task.sh
        cp ${PROJECT_TASK_DIR}/.deploy.sh ${BUNDLE_DIR}/.deploy.sh
        echo "FROM alpine"           > "${DOCKER_FILE}"
        echo "WORKDIR /deploy"       >> "${DOCKER_FILE}"
        echo "ADD ${STACK_FILE} ./" >> "${DOCKER_FILE}"
        echo "ADD .task.env     ./" >> "${DOCKER_FILE}"
        echo "ADD .task.sh      ./" >> "${DOCKER_FILE}"
        echo "ADD .deploy.sh    ./" >> "${DOCKER_FILE}"
        (cd ${BUNDLE_DIR}; \
         docker build --no-cache -t ${ENV_STACK_DEPLOY_TAG} .; \
         docker push ${ENV_STACK_DEPLOY_TAG})
        echo ""
        echo "successfully push stack file to '${ENV_STACK_DEPLOY_TAG}'."
        echo "* 'sh deploy.sh ${ENV_STACK_DEPLOY_TAG} up'   to up   stack on host."
        echo "* 'sh deploy.sh ${ENV_STACK_DEPLOY_TAG} down' to down stack on host."
}

task_usage() {
        echo ""
        echo "sh run.sh <env> deploy build"
        echo "sh run.sh <env> deploy push"
        echo "sh run.sh <env> deploy up"
        echo "sh run.sh <env> deploy down"
        echo "sh deploy.sh <deploy tag> up"
        echo "sh deploy.sh <deploy tag> down"
        echo ""
        echo "ex) to up local stack on local host, write .task.env and do below:"
        echo "  sh run.sh deploy build"
        echo "  sh run.sh deploy up"
        echo "  sh run.sh deploy down"
        echo ""
        echo "ex) to up develop stack on remote host, write .task.env.develop and do below:"
        echo "  sh run.sh develop deploy build"
        echo "  sh run.sh develop deploy push"
        echo "  sh deploy.sh registry:5000/myapp/stack up"
        echo "  sh deploy.sh registry:5000/myapp/stack down"
        echo ""
}

. "`dirname ${0}`/.task.sh" usage ${*}
