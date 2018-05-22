cd `dirname ${0}`/../
docker run --rm -w /work -e TERM=xterm \
        -v /var/run/docker.sock:/var/run/docker.sock \
        -v `pwd`:/work \
        nunun/runner sh ./run.sh ${*}
