docker run --privileged --rm -it -w /work -e TERM=xterm ^
        -v /var/run/docker.sock:/var/run/docker.sock ^
        -v %cd%:/work ^
        nunun/enforce %1 %2 %3 %4 %5 %6 %7 %8 %9
