
targetApp="HelloWorldMvc"
framework="netcoreapp1.0"
testName="test"

while getopts ":n:t:f:" opt; do
    case $opt in
        t) targetApp="$OPTARG"
        ;;
        f) framework="$OPTARG"
        ;;
        n) testName="$OPTARG"
        ;;
        \?) echo "Invalid option -$OPTARG" >&2
        ;;
    esac
done


./Archive.sh -n $testName -f $framework -t $targetApp

rm -f /etc/nginx/sites-enabled/nginx-coldstart-services
rm -f /etc/nginx/sites-available/nginx-coldstart-services
