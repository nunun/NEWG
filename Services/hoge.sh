docker pull ${1}
docker run -v `cygpath -w "${2}/.run"`:/dotrun/run ${1} \
        rsync -ahv --delete .run/* run
