
* Windows: cygwin + docker for windows
* Mac: Terminal + docker for mac

````
sh run.sh help

sh run.sh build
sh run.sh [up]
sh run.sh down

sh run.sh stack build
sh run.sh stack up
sh run.sh stack down
sh run.sh stack push
sh run.sh stack [usage]

sh run.sh <env> build
sh run.sh <env> [up]
sh run.sh <env> down
sh run.sh <env> stack build
sh run.sh <env> stack up
sh run.sh <env> stack down
sh run.sh <env> stack push
sh run.sh <env> stack [usage]
<env> = env name referrence env file named '.task.env[.<env>]'.

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
