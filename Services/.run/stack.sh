task_up() {
        local stack_name="${ENV_STACK_NAME}"
        local stack_file=`stack_file`
        echo_info "up stack '${stack_name}' ..."
        task_init
        docker stack deploy "${stack_name}" \
                --with-registry-auth --compose-file `ospath "${stack_file}"`
}

task_down() {
        local stack_name="${ENV_STACK_NAME}"
        echo_info "down stack '${stack_name}' ..."
        docker stack rm "${stack_name}"
}

task_build() {
        local stack_file=`stack_file`
        local env_name="${RUN_ENV_NAME_WITH_SPACE}"
        local deploy_tag="${ENV_STACK_DEPLOY_TAG}"
        cd "${RUN_ROOT_DIR}"
        mkdir -p `dirname ${stack_file}`
        sh run.sh build_stack_file "${stack_file}"
        echo ""
        echo "successfully build stack file to '${stack_file}'."
        echo "you can up stack locally and push stack file to docker image registry for deploy."
        echo "> sh run.sh${env_name} stack up    # up   stack locally"
        echo "> sh run.sh${env_name} stack push  # push stack file to '${deploy_tag}' for deploy"
}

task_push() {
        local stack_file=`stack_file`
        local bundle_dir="/tmp/stack"
        local dockerfile_path="${bundle_dir}/Dockerfile"
        local deploy_sh=".run/deploy.sh"
        local deploy_tag="${ENV_STACK_DEPLOY_TAG}"
        local env_file="${RUN_ENV_FILE}"
        echo_info "push stack file to '${deploy_tag}' ..."
        rm -rf "${bundle_dir}"
        mkdir -p "${bundle_dir}/.builds"
        cp    "${stack_file}"            "${bundle_dir}/.builds/stack.yml"
        cp    "${env_file}"              "${bundle_dir}/.run.env"
        cp -r "${RUN_ROOT_DIR}/.run" "${bundle_dir}/.run"
        echo "FROM alpine"              > "${dockerfile_path}"
        echo "WORKDIR /stack"          >> "${dockerfile_path}"
        echo "ADD .builds  ./.builds"  >> "${dockerfile_path}"
        echo "ADD .run.env ./.run.env" >> "${dockerfile_path}"
        echo "ADD .run     ./.run"     >> "${dockerfile_path}"
        (cd ${bundle_dir}; \
                docker build --no-cache -t "${deploy_tag}" .; \
                docker push "${deploy_tag}")
        echo ""
        echo "successfully push stack file to '${deploy_tag}'."
        echo "you can copy 'deploy.sh' to any host and deploy stack on the host."
        echo "> sh ${deploy_sh} ${deploy_tag} stack up"
        echo "> sh ${deploy_sh} ${deploy_tag} stack down"
        echo "> sh ${deploy_sh} ${deploy_tag} stack init"
        echo "> sh ${deploy_sh} ${deploy_tag} stack setup"
        echo "> sh ${deploy_sh} ${deploy_tag} stack renew"
        echo "> sh ${deploy_sh} ${deploy_tag} stack reset"
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
        echo "sh .run/scripts/deploy.sh <deploy tag> stack up"
        echo "sh .run/scripts/deploy.sh <deploy tag> stack down"
        echo "sh .run/scripts/deploy.sh <deploy tag> stack init"
        echo "sh .run/scripts/deploy.sh <deploy tag> stack setup"
        echo "sh .run/scripts/deploy.sh <deploy tag> stack renew"
        echo "sh .run/scripts/deploy.sh <deploy tag> stack reset"
        echo ""
        echo "ex) to up local stack on local host, write '.run/environments/local' and do below:"
        echo "  sh run.sh stack build"
        echo "  sh run.sh stack up"
        echo "  sh run.sh stack down"
        echo ""
        echo "ex) to up develop stack on remote host, write '.run/environments/develop' and do below:"
        echo "  sh run.sh develop stack build"
        echo "  sh run.sh develop stack push"
        echo "  sh .run/scripts/deploy.sh registry:5000/myapp/stack stack up"
        echo "  sh .run/scripts/deploy.sh registry:5000/myapp/stack stack down"
        echo ""
}

###############################################################################
###############################################################################
###############################################################################

task_init() {
        if [ "${ENV_SECRET_CERT}" ]; then
                if [    ! `secret_exists ${ENV_SECRET_CERT_PEM}` \
                     -o ! `secret_exists ${ENV_SECRET_CHAIN_PEM}` \
                     -o ! `secret_exists ${ENV_SECRET_FULLCHAIN_PEM}` \
                     -o ! `secret_exists ${ENV_SECRET_PRIVKEY_PEM}` ]; then
                        update_certs
                fi
        fi
        if [ "${ENV_SECRET_HTPASSWD}" ]; then
                if [ ! `secret_exists ${ENV_SECRET_HTPASSWD}` ]; then \
                        update_htpasswd
                fi
        fi
        if [ "${ENV_SECRET_APIKEY}" ]; then
                if [ ! `secret_exists ${ENV_SECRET_APIKEY}` ]; then \
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
        local certs_dir=`certs_dir`
        local cert_pem_file="cert.pem"
        local chain_pem_file="chain.pem"
        local fullchain_pem_file="fullchain.pem"
        local privkey_pem_file="privkey.pem"
        mkdir -p "${certs_dir}"
        case "${ENV_SECRET_CERT}" in
        selfsigned)
                local crt_file="${certs_dir}/domain.crt"
                local key_file="${certs_dir}/domain.key"
                local subj="C=JP/CN=${ENV_SECRET_CERT_FQDN}"
                local subj="${subj}/O=${ENV_SECRET_CERT_FQDN}"
                local subj="${subj}/emailAddress=${ENV_SECRET_CERT_EMAIL}"
                openssl req -x509 -nodes -days 365 -newkey rsa:2048  \
                        -subj "${subj}" \
                        -out "${crt_file}" -keyout "${key_file}"
                cert_pem_file="${crt_file}"
                chain_pem_file="${crt_file}"
                fullchain_pem_file="${crt_file}"
                privkey_pem_file="${key_file}"
                ;;
        certbot)
                DRY_RUN="--dry-run" && echo_info "<<< DRY RUN >>>"
                docker run --rm -p "80:80" \
                        -v `ospath ${certs_dir}`:/etc/letsencrypt \
                        deliverous/certbot certonly ${DRY_RUN} \
                        --standalone --renew-by-default --non-interactive \
                        --agree-tos --preferred-challenges http \
                        -certs_dir "${ENV_SECRET_CERT_FQDN}" \
                        --email "${ENV_SECRET_CERT_EMAIL}"
                cert_pem_file="${certs_dir}/live/${ENV_FQDN}/cert.pem"
                chain_pem_file="${certs_dir}/live/${ENV_FQDN}/chain.pem"
                fullchain_pem_file="${certs_dir}/live/${ENV_FQDN}/fullchain.pem"
                privkey_pem_file="${certs_dir}/live/${ENV_FQDN}/privkey.pem"
                ;;
        external)
                if [    `secret_exists ${ENV_SECRET_CERT_PEM}` \
                     -a `secret_exists ${ENV_SECRET_CHAIN_PEM}` \
                     -a `secret_exists ${ENV_SECRET_FULLCHAIN_PEM}` \
                     -a `secret_exists ${ENV_SECRET_PRIVKEY_PEM}` ]; then
                        return # NOTE ok, external certs ready. nothing to do.
                fi
                abort "cert does not exist."
                ;;
        *)
                abort "'${ENV_SECRET_CERT}' is not supported."
                ;;
        esac
        secret_create_from_file "${ENV_SECRET_CERT_PEM}"      "${cert_pem_file}"
        secret_create_from_file "${ENV_SECRET_CHAIN_PEM}"     "${chain_pem_file}"
        secret_create_from_file "${ENV_SECRET_FULLCHAIN_PEM}" "${fullchain_pem_file}"
        secret_create_from_file "${ENV_SECRET_PRIVKEY_PEM}"   "${privkey_pem_file}"
}

remove_certs() {
        secret_rm "${ENV_SECRET_CERT_PEM}"
        secret_rm "${ENV_SECRET_CHAIN_PEM}"
        secret_rm "${ENV_SECRET_FULLCHAIN_PEM}"
        secret_rm "${ENV_SECRET_PRIVKEY_PEM}"
        local certs_dir=`certs_dir`
        rm -rf "${certs_dir}"
}

update_htpasswd() {
        remove_htpasswd
        local username="${1}"
        local password="${2}"
        [ -z "${username}" ] && username=`cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 16 | head -n 1`
        [ -z "${password}" ] && password=`cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 16 | head -n 1`
        secret_create_from_string "${ENV_SECRET_HTPASSWD}" \
                `docker run --rm --entrypoint htpasswd registry:2 -Bbn "${username}" "${password}"`
}

remove_htpasswd() {
        secret_rm "${ENV_SECRET_HTPASSWD}"
}

update_apikey() {
        remove_apikey
        local apikey=`cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 128 | head -n 1`
        secret_create_from_string "${ENV_SECRET_APIKEY}" "${apikey}"
}

remove_apikey() {
        secret_rm "${ENV_SECRET_APIKEY}"
}

###############################################################################
###############################################################################
###############################################################################

secret_create_from_string() {
        local secret_name="${1?empty secret name}"
        local secret_data="${2?empty secret data}"
        echo_info "create docker secret '${secret_name}' ..."
        echo "${secret_data}" | docker secret create "${secret_name}" -
}

secret_create_from_file() {
        local secret_name="${1?empty secret name}"
        local secret_file="${2?empty secret file}"
        echo_info "create docker secret '${secret_name}' from '${secret_file}' ..."
        cat "${secret_file}" | docker secret create "${secret_name}" -
}

secret_rm() {
        if [ "${1}" -a `secret_exists ${1}` ]; then
                echo_info "remove docker secret '${1}' ..."
                docker secret rm "${1}"
        fi
}

secret_exists() {
        if [ "${1}" ]; then
                echo `docker secret ls -q -f name="${1}"`
        fi
}

###############################################################################
###############################################################################
###############################################################################

stack_file() {
        local env_name="${RUN_ENV_NAME_WITH_DOT}"
        echo "${RUN_ROOT_DIR}/.builds/stack${env_name}.yml"
}

certs_dir() {
        echo "${RUN_ROOT_DIR}/.certs"
}

###############################################################################
###############################################################################
###############################################################################
. "`dirname ${0}`/task.sh" ${*}
