DEPLOY_TAG="${1?no deploy tag specified.}"
shift 1
set -e

MD5=`which md5 || which md5sum` || (echo "no md5 command." && exit 1)
HASH=`echo ${DEPLOY_TAG} | ${MD5} | cut -d" " -f1`
DEPLOY_DIR="/tmp/deploy/stacks/${HASH}"
VOLUME_DIR="${DEPLOY_DIR}"
[ "${OSTYPE}" = "cygwin" ] && VOLUME_DIR=`cygpath -w ${DEPLOY_DIR}`

mkdir -p "${DEPLOY_DIR}"
cd "${DEPLOY_DIR}"

echo "(in ${DEPLOY_DIR})"
echo "pull stack file from '${DEPLOY_TAG}' ..."
docker pull ${DEPLOY_TAG}
docker run --rm -v ${VOLUME_DIR}:/stack ${DEPLOY_TAG} \
        sh -c "cp -a . /stack/; chmod -R 777 /stack"
sh ./.deploy.sh ${*}
