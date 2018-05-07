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

# TODO
# renew + reset or setup + renew
task_setup() {
        # reset certs
        if [ -n "${ENV_CERT}" ]; then
                secret_rm "${ENV_CERT_PEM}"
                secret_rm "${ENV_CHAIN_PEM}"
                secret_rm "${ENV_FULLCHAIN_PEM}"
                secret_rm "${ENV_PRIVKEY_PEM}"
        fi

        # remove all cache
        CACHE_DIR="${PROJECT_TASK_DIR}/.cache/deploy"
        rm -rf "${CACHE_DIR}"
}

task_renew() {
        # renew certs
        if [ -n "${ENV_CERT}" ]; then
                CACHE_DIR="${PROJECT_TASK_DIR}/.cache/deploy/certs"
                RECREATE="NO"
                mkdir -p "${CACHE_DIR}"
                case "${ENV_CERT}" in
                selfsigned)
                        CRT_FILE="${CACHE_DIR}/domain.crt"
                        KEY_FILE="${CACHE_DIR}/domain.key"
                        if [ ! -f "${CRT_FILE}" -o ! -f "${KEY_FILE}" ]; then
                                openssl req -x509 -nodes -days 365 -newkey rsa:2048  \
                                        -subj "/CN=${ENV_FQDN} /O=${ENV_FQDN} /C=JP" \
                                        -out "${CRT_FILE}" -keyout "${KEY_FILE}"
                                RECREATE="YES"
                        fi
                        CERT_PEM_FILE="${CRT_FILE}"
                        CHAIN_PEM_FILE="${CRT_FILE}"
                        FULLCHAIN_PEM_FILE="${CRT_FILE}"
                        PRIVKEY_PEM_FILE="${KEY_FILE}"
                        ;;
                certbot)
                        # TODO check update time
                        docker run --rm -p "80:80" \
                                -v `ospath ${CACHE_DIR}`:/etc/letsencrypt \
                                deliverous/certbot certonly \
                                --standalone --renew-by-default --non-interactive \
                                --agree-tos --preferred-challenges http \
                                -d "${ENV_FQDN}" --email "${ENV_EMAIL}"
                        RECREATE="YES"                               # TODO
                        CERT_PEM_FILE="${CACHE_DIR}/domain.crt"      # TODO
                        CHAIN_PEM_FILE="${CACHE_DIR}/domain.crt"     # TODO
                        FULLCHAIN_PEM_FILE="${CACHE_DIR}/domain.crt" # TODO
                        PRIVKEY_PEM_FILE="${CACHE_DIR}/domain.key"   # TODO
                        ;;
                *)
                        echo "'${ENV_CERT}' is not supported."
                        ;;
                esac
                if [ "${RECREATE}" = "YES" ]; then
                        secret_rm     "${ENV_CERT_PEM}"
                        secret_rm     "${ENV_CHAIN_PEM}"
                        secret_rm     "${ENV_FULLCHAIN_PEM}"
                        secret_rm     "${ENV_PRIVKEY_PEM}"
                        secret_create "${ENV_CERT_PEM}"      "${CERT_PEM_FILE}"
                        secret_create "${ENV_CHAIN_PEM}"     "${CHAIN_PEM_FILE}"
                        secret_create "${ENV_FULLCHAIN_PEM}" "${FULLCHAIN_PEM_FILE}"
                        secret_create "${ENV_PRIVKEY_PEM}"   "${PRIVKEY_PEM_FILE}"
                fi
        fi
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
        echo "sh run.sh <env> deploy setup"
        echo "sh run.sh <env> deploy renew"
        echo "sh deploy.sh <deploy tag> up"
        echo "sh deploy.sh <deploy tag> down"
        echo "sh deploy.sh <deploy tag> setup"
        echo "sh deploy.sh <deploy tag> renew"
        echo ""
        echo "ex) to up local stack on local host, write .task.env and do below:"
        echo "  sh run.sh deploy build"
        echo "  sh run.sh deploy setup"
        echo "  sh run.sh deploy up"
        echo "  sh run.sh deploy down"
        echo "  sh run.sh deploy renew"
        echo ""
        echo "ex) to up develop stack on remote host, write .task.env.develop and do below:"
        echo "  sh run.sh develop deploy build"
        echo "  sh run.sh develop deploy push"
        echo "  sh deploy.sh registry:5000/myapp/stack setup"
        echo "  sh deploy.sh registry:5000/myapp/stack up"
        echo "  sh deploy.sh registry:5000/myapp/stack down"
        echo "  sh deploy.sh registry:5000/myapp/stack renew"
        echo ""
}

###############################################################################
###############################################################################
###############################################################################

secret_create() {
        SECRET_NAME="${1}"
        FILE_PATH="${2}"
        echo "create docker secret '${SECRET_NAME}' from ${FILE_PATH} ..."
        cat "${FILE_PATH}" | docker secret create "${SECRET_NAME}" -
}

secret_rm() {
        SECRET_NAME="${1}"
        FOUND="`docker secret ls -q -f name="${SECRET_NAME}"`"
        if [ -n "${FOUND}" ]; then
                echo "remove docker secret '${SECRET_NAME}' ..."
                docker secret rm "${SECRET_NAME}"
        fi
}

###############################################################################
###############################################################################
###############################################################################
. "`dirname ${0}`/.task.sh" usage ${*}
