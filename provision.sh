DEFAULT_CSX=

if ! echo "$@" | grep -q 'csx'
then
	DEFAULT_CSX=bot-provisioning/dependencies.csx
fi

bot-provisioning/provisionator-bootstrap.sh $DEFAULT_CSX $@
