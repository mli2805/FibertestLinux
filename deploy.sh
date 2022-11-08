#!/bin/bash

sudo dotnet publish --configuration Release

rm -rf /home/leanid/FibertestLinux/Deploy/Files
mkdir -p /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine
mkdir -p /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine/etc_default
mkdir -p /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine/etc
mkdir -p /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine/share

echo ""
echo ""
echo "не забудь выкачать свежие исходники и сбилдить iit_otdr & Co "

# мой код c# без подкаталогов, чтобы не взять что-то измененное 
cp /home/leanid/FibertestLinux/Client/InteropExperiment/bin/Debug/net6.0/* /home/leanid/FibertestLinux/Deploy/Files/ 2>/dev/null
# сами либки сбилженные на этой машине
cp -a /home/leanid/Sources/OtdrMeasEngine/__Lin64Out/* /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine/
# остальные файлы без изменений из репозитория
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/ETC_default/* /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine/etc_default/
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/ETC_default/* /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine/etc/
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/SHARE/* /home/leanid/FibertestLinux/Deploy/Files/OtdrMeasEngine/share/

cd Deploy
tar -czf rtuarch.tar.gz Files

echo "Результат в архиве rtuarch.tar.gz"

