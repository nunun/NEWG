
* Windows: cygwin + docker for windows
* Mac: Terminal + docker for mac

````
sh run.sh help
sh run.sh build
sh run.sh [up]
sh run.sh down
sh run.sh <env> bundle
<env> = local, develop ...

sh run.sh services protocols

sh run.sh test services build
sh run.sh test services [test]
sh run.sh test services up
sh run.sh test services down

sh run.sh test matching build
sh run.sh test matching [test]
sh run.sh test matching up
sh run.sh test matching down

sh run.sh test webapi build
sh run.sh test webapi [test]
sh run.sh test webapi up
sh run.sh test webapi down

sh run.sh test unity build
sh run.sh test unity [test]
sh run.sh test unity up
sh run.sh test unity down
````
