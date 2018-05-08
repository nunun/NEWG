task_up() {
        echo "up stack '${ENV_STACK_NAME}' ..."
        task_init
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        docker stack deploy ${ENV_STACK_NAME} \
                --with-registry-auth --compose-file ${STACK_FILE}
}

task_down() {
        echo "down stack '${ENV_STACK_NAME}' ..."
        docker stack rm ${ENV_STACK_NAME}
}

task_build() {
        echo "building stack file ..."
        STACK_FILE=".stack${TASK_ENV_NAME_WITH_DOT}.yml"
        BUILD_YAMLS="-f docker-compose.yml -f docker-compose.stack.build.yml"
        CONFIG_YAMLS="-f docker-compose.yml -f docker-compose.stack.deploy.yml"
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        #(cd ${PROJECT_TASK_DIR}; sh run.sh unity)
        docker-compose ${BUILD_YAMLS} build --force-rm --no-cache
        docker-compose ${BUILD_YAMLS} push
        docker-compose ${CONFIG_YAMLS} config --resolve-image-digest > ${STACK_FILE}
        echo ""
        echo "successfully build stack file to '${STACK_FILE}'."
        echo "you can up stack locally and push stack file to docker image registry for deploy."
        echo "> sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} stack up    # up   stack locally"
        echo "> sh run.sh${TASK_ENV_NAME:+" "}${TASK_ENV_NAME} stack push  # push stack file to '${ENV_STACK_DEPLOY_TAG}' for deploy"
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
        echo "successfully push stack file to '${ENV_STACK_DEPLOY_TAG}'."
        echo "you can copy 'deploy.sh' to any host and deploy stack on the host."
        echo "> sh deploy.sh ${ENV_STACK_DEPLOY_TAG} stack up"
        echo "> sh deploy.sh ${ENV_STACK_DEPLOY_TAG} stack down"
        echo "> sh deploy.sh ${ENV_STACK_DEPLOY_TAG} stack init"
        echo "> sh deploy.sh ${ENV_STACK_DEPLOY_TAG} stack setup"
        echo "> sh deploy.sh ${ENV_STACK_DEPLOY_TAG} stack renew"
        echo "> sh deploy.sh ${ENV_STACK_DEPLOY_TAG} stack reset"
}

task_usage() {
        echo ""
        echo "sh run.sh <env> stack build"
        echo "sh run.sh <env> stack push"
        echo "sh run.sh <env> stack up"
        echo "sh run.sh <env> stack down"
        echo "sh run.sh <env> stack init"
        echo "sh run.sh <env> stack setup"
        echo "sh run.sh <env> stack renew"
        echo "sh run.sh <env> stack reset"
        echo "sh deploy.sh <deploy tag> stack up"
        echo "sh deploy.sh <deploy tag> stack down"
        echo "sh deploy.sh <deploy tag> stack init"
        echo "sh deploy.sh <deploy tag> stack setup"
        echo "sh deploy.sh <deploy tag> stack renew"
        echo "sh deploy.sh <deploy tag> stack reset"
        echo ""
        echo "ex) to up local stack on local host, write .task.env and do below:"
        echo "  sh run.sh stack build"
        echo "  sh run.sh stack up"
        echo "  sh run.sh stack down"
        echo ""
        echo "ex) to up develop stack on remote host, write .task.env.develop and do below:"
        echo "  sh run.sh develop stack build"
        echo "  sh run.sh develop stack push"
        echo "  sh deploy.sh registry:5000/myapp/stack stack up"
        echo "  sh deploy.sh registry:5000/myapp/stack stack down"
        echo ""
}

###############################################################################
###############################################################################
###############################################################################

task_init() {
        if [ "${ENV_SECRET_CERT}" ]; then
                if [    ! "`secret_exists ${ENV_SECRET_CERT_PEM}`" \
                     -o ! "`secret_exists ${ENV_SECRET_CHAIN_PEM}`" \
                     -o ! "`secret_exists ${ENV_SECRET_FULLCHAIN_PEM}`" \
                     -o ! "`secret_exists ${ENV_SECRET_PRIVKEY_PEM}`" ]; then
                        update_certs
                fi
        fi
        if [ "${ENV_SECRET_HTPASSWD}" ]; then
                if [ ! "`secret_exists ${ENV_SECRET_HTPASSWD}`" ]; then \
                        update_htpasswd
                fi
        fi
        if [ "${ENV_SECRET_APIKEY}" ]; then
                if [ ! "`secret_exists ${ENV_SECRET_APIKEY}`" ]; then \
                        update_apikey
                fi
        fi
}

task_setup() {
        task_down
        task_init
        if [ "${ENV_SECRET_HTPASSWD}" ]; then
                read  -p "htpasswd username: " USERNAME
                read -sp "htpasswd password: " PASSWORD
                echo ""
                update_htpasswd "${USERNAME}" "${PASSWORD}"
        fi
}

task_renew() {
        task_down
        task_init
        if [ "${ENV_SECRET_CERT}" ]; then
                update_certs
        fi
}

task_reset() {
        task_down
        remove_certs
        remove_htpasswd
        remove_apikey
}

###############################################################################
###############################################################################
###############################################################################

update_certs() {
        remove_certs
        CACHE_DIR="${PROJECT_TASK_DIR}/.certs"
        mkdir -p "${CACHE_DIR}"
        case "${ENV_SECRET_CERT}" in
        selfsigned)
                CRT_FILE="${CACHE_DIR}/domain.crt"
                KEY_FILE="${CACHE_DIR}/domain.key"
                openssl req -x509 -nodes -days 365 -newkey rsa:2048  \
                        -subj "/CN=${ENV_FQDN} /O=${ENV_FQDN} /C=JP" \
                        -out "${CRT_FILE}" -keyout "${KEY_FILE}"
                CERT_PEM_FILE="${CRT_FILE}"
                CHAIN_PEM_FILE="${CRT_FILE}"
                FULLCHAIN_PEM_FILE="${CRT_FILE}"
                PRIVKEY_PEM_FILE="${KEY_FILE}"
                ;;
        certbot)
                docker run --rm -p "80:80" \
                        -v `ospath ${CACHE_DIR}`:/etc/letsencrypt \
                        deliverous/certbot certonly --dry-run \
                        --standalone --renew-by-default --non-interactive \
                        --agree-tos --preferred-challenges http \
                        -d "${ENV_FQDN}" --email "${ENV_EMAIL}"
                CERT_PEM_FILE="${CACHE_DIR}/live/${ENV_FQDN}/cert.pem"
                CHAIN_PEM_FILE="${CACHE_DIR}/live/${ENV_FQDN}/chain.pem"
                FULLCHAIN_PEM_FILE="${CACHE_DIR}/live/${ENV_FQDN}/fullchain.pem"
                PRIVKEY_PEM_FILE="${CACHE_DIR}/live/${ENV_FQDN}/privkey.pem"
                ;;
        external)
                if [    "`secret_exists ${ENV_SECRET_CERT_PEM}`" \
                     -a "`secret_exists ${ENV_SECRET_CHAIN_PEM}`" \
                     -a "`secret_exists ${ENV_SECRET_FULLCHAIN_PEM}`" \
                     -a "`secret_exists ${ENV_SECRET_PRIVKEY_PEM}`" ]; then
                        return
                fi
                echo "cert does not exist."
                exit 1
                ;;
        *)
                echo "'${ENV_SECRET_CERT}' is not supported."
                exit 1
                ;;
        esac
        secret_create_from_file "${ENV_SECRET_CERT_PEM}"      "${CERT_PEM_FILE}"
        secret_create_from_file "${ENV_SECRET_CHAIN_PEM}"     "${CHAIN_PEM_FILE}"
        secret_create_from_file "${ENV_SECRET_FULLCHAIN_PEM}" "${FULLCHAIN_PEM_FILE}"
        secret_create_from_file "${ENV_SECRET_PRIVKEY_PEM}"   "${PRIVKEY_PEM_FILE}"
}

remove_certs() {
        secret_rm "${ENV_SECRET_CERT_PEM}"
        secret_rm "${ENV_SECRET_CHAIN_PEM}"
        secret_rm "${ENV_SECRET_FULLCHAIN_PEM}"
        secret_rm "${ENV_SECRET_PRIVKEY_PEM}"
        CACHE_DIR="${PROJECT_TASK_DIR}/.certs"
        rm -rf "${CACHE_DIR}"
}

update_htpasswd() {
        remove_htpasswd
        if [ "${1}" -o "${2}" ]; then
                USERNAME="${1}"
                PASSWORD="${2}"
        else
                USERNAME="`cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 16 | head -n 1`"
                PASSWORD="`cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 16 | head -n 1`"
        fi
        secret_create_from_string "${ENV_SECRET_HTPASSWD}" \
                "`docker run --rm --entrypoint htpasswd registry:2 -Bbn "${USERNAME}" "${PASSWORD}"`"
}

remove_htpasswd() {
        secret_rm "${ENV_SECRET_HTPASSWD}"
}

update_apikey() {
        remove_apikey
        APIKEY="`cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 128 | head -n 1`"
        secret_create_from_string "${ENV_SECRET_APIKEY}" "${APIKEY}"
}

remove_apikey() {
        secret_rm "${ENV_SECRET_APIKEY}"
}

###############################################################################
###############################################################################
###############################################################################

secret_create_from_string() {
        SECRET_NAME="${1?empty secret name}"
        SECRET_DATA="${2?empty secret data}"
        echo "create docker secret '${SECRET_NAME}' ..."
        echo "${SECRET_DATA}" | docker secret create "${SECRET_NAME}" -
}

secret_create_from_file() {
        SECRET_NAME="${1?empty secret name}"
        FILE_PATH="${2?empty file path}"
        echo "create docker secret '${SECRET_NAME}' from '${FILE_PATH}' ..."
        cat "${FILE_PATH}" | docker secret create "${SECRET_NAME}" -
}

secret_rm() {
        if [ "${1}" -a "`secret_exists ${1}`" ]; then
                echo "remove docker secret '${1}' ..."
                docker secret rm "${1}"
        fi
}

secret_exists() {
        if [ "${1}" ]; then
                echo "`docker secret ls -q -f name="${1}"`"
        fi
}

###############################################################################
###############################################################################
###############################################################################
. "`dirname ${0}`/.task.sh" usage ${*}
