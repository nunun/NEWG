PWD=`pwd`
[ "${OSTYPE}" = "cygwin" ] && PWD=`cygpath -w "${PWD}"`
docker run --rm -w /work -e TERM=xterm \
        -v /var/run/docker.sock:/var/run/docker.sock \
        -v "${PWD}":/work \
        nunun/enforce ${*}
