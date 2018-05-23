
* Windows: cygwin + docker for windows
* Mac: Terminal + docker for mac

````
sh run.sh [-v][-q] [<env>] [<command> [<command> ...]]
<env> = see '.env.<env>' files.

sh run.sh dotrun
sh run.sh env
sh run.sh help

sh run.sh build
sh run.sh up
sh run.sh down

sh run.sh stack build
sh run.sh stack up
sh run.sh stack down
sh run.sh stack push
sh run.sh stack usage

sh run.sh services build
sh run.sh services protocols

sh run.sh test services build
sh run.sh test services try
sh run.sh test services up
sh run.sh test services down

sh run.sh test matching build
sh run.sh test matching try
sh run.sh test matching up
sh run.sh test matching down

sh run.sh test unity build
sh run.sh test unity try
sh run.sh test unity up
sh run.sh test unity down

sh run.sh test webapi build
sh run.sh test webapi try
sh run.sh test webapi up
sh run.sh test webapi down
````
