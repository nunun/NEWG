TAG="${1}"
TMP_DIR="/tmp/deploy/`echo ${TAG} | md5sum | cut -d" " -f1`"
VOL_DIR="${TMP_DIR}"
echo "pull stack file from ${TAG} ..."
[ "${OSTYPE}" = "cygwin" ] && VOL_DIR=`cygpath -w ${TMP_DIR}`
mkdir -p "${TMP_DIR}" && cd ${TMP_DIR}
docker pull ${TAG}
docker run --rm -v ${VOL_DIR}:/deploy ${TAG} \
sh -c "cp -a . /deploy/; chmod -R 777 /deploy"
shift 1 && sh ./.task.sh stack stack ${*}
