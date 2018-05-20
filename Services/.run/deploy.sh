DEPLOY_TAG="${1?no deploy tag specified.}"
shift 1
set -e

function ospath() {
        [ "${OSTYPE}" = "cygwin" ] && echo `cygpath -w ${1}` || echo "${1}"
}

MD5=`which md5sum || which md5` || (echo "no md5 command." && exit 1)
HASH=`echo "${DEPLOY_TAG}" | ${MD5} | cut -d" " -f1`
DEPLOY_DIR="/tmp/deploy/stacks/${HASH}"

echo "(in ${DEPLOY_DIR})"
mkdir -p "${DEPLOY_DIR}"
cd "${DEPLOY_DIR}"

echo "pull stack file from '${DEPLOY_TAG}' ..."
docker pull "${DEPLOY_TAG}"
docker run --rm -v `ospath ${DEPLOY_DIR}`:/deploy "${DEPLOY_TAG}" \
        sh -c "cp -a . /deploy/; chmod -R 777 /deploy"

function task_stack() {
        sh "${DEPLOY_DIR}/.run/stack.sh" ${*}
}
. "${DEPLOY_DIR}/.run/task.sh" ${*}
