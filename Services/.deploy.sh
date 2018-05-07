task_up() {
        echo "deploy: starting '${ENV_STACK_NAME}' ..."
        task_init # NOTE init automatically
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
        echo "sh run.sh <env> deploy init"
        echo "sh run.sh <env> deploy setup"
        echo "sh run.sh <env> deploy renew"
        echo "sh run.sh <env> deploy reset"
        echo "sh deploy.sh <deploy tag> up"
        echo "sh deploy.sh <deploy tag> down"
        echo "sh deploy.sh <deploy tag> init"
        echo "sh deploy.sh <deploy tag> setup"
        echo "sh deploy.sh <deploy tag> renew"
        echo "sh deploy.sh <deploy tag> reset"
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

###############################################################################
###############################################################################
###############################################################################

task_init() {
        if [ "${ENV_CERT}" ]; then
                if [    ! "`secret_exists ${ENV_CERT_PEM}`" \
                     -o ! "`secret_exists ${ENV_CHAIN_PEM}`" \
                     -o ! "`secret_exists ${ENV_FULLCHAIN_PEM}`" \
                     -o ! "`secret_exists ${ENV_PRIVKEY_PEM}`" ]; then
                        update_certs
                fi
        fi
        if [ "${ENV_HTPASSWD}" ]; then
                if [ ! "`secret_exists ${ENV_HTPASSWD}`" ]; then \
                        update_htpasswd
                fi
        fi
        if [ "${ENV_APIKEY}" ]; then
                if [ ! "`secret_exists ${ENV_APIKEY}`" ]; then \
                        update_apikey
                fi
        fi
}

task_setup() {
        task_down
        task_init
        if [ "${ENV_HTPASSWD}" ]; then
                read  -p "username: " USERNAME
                read -sp "password: " PASSWORD
                update_htpasswd "${USERNAME}" "${PASSWORD}"
        fi
}

task_renew() {
        task_down
        task_init
        if [ "${ENV_CERT}" ]; then
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
        case "${ENV_CERT}" in
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
        rent)
                if [    "`secret_exists ${ENV_CERT_PEM}`" \
                     -a "`secret_exists ${ENV_CHAIN_PEM}`" \
                     -a "`secret_exists ${ENV_FULLCHAIN_PEM}`" \
                     -a "`secret_exists ${ENV_PRIVKEY_PEM}`" ]; then
                        return
                fi
                echo "cert does not exist."
                exit 1
                ;;
        *)
                echo "'${ENV_CERT}' is not supported."
                exit 1
                ;;
        esac
        secret_create "${ENV_CERT_PEM}"      "${CERT_PEM_FILE}"
        secret_create "${ENV_CHAIN_PEM}"     "${CHAIN_PEM_FILE}"
        secret_create "${ENV_FULLCHAIN_PEM}" "${FULLCHAIN_PEM_FILE}"
        secret_create "${ENV_PRIVKEY_PEM}"   "${PRIVKEY_PEM_FILE}"
}

remove_certs() {
        secret_rm "${ENV_CERT_PEM}"
        secret_rm "${ENV_CHAIN_PEM}"
        secret_rm "${ENV_FULLCHAIN_PEM}"
        secret_rm "${ENV_PRIVKEY_PEM}"
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
        docker run --rm --entrypoint htpasswd registry:2 \
                -Bbn "${USERNAME}" "${PASSWORD}" \
                | docker secret create "${ENV_HTPASSWD}" -
}

remove_htpasswd() {
        secret_rm "${ENV_HTPASSWD}"
}

update_apikey() {
        remove_apikey
        APIKEY="`cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 128 | head -n 1`"
        echo "${APIKEY}" | docker secret create "${ENV_APIKEY}" -
}

remove_apikey() {
        secret_rm "${ENV_APIKEY}"
}

###############################################################################
###############################################################################
###############################################################################

secret_create() {
        SECRET_NAME="${1?empty secret name}"
        FILE_PATH="${2?empty file path}"
        echo "create docker secret '${SECRET_NAME}' from ${FILE_PATH} ..."
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
