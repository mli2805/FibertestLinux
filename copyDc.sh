#!/bin/bash
set -e

cd /home/leanid/FibertestLinux/DataCenter/DataCenter/bin/Release/net6.0/publish

sudo systemctl stop DataCenter.service
echo "сервис остановлен"
sleep 1s

cp * /var/fibertest/datacenter/
cd /var/fibertest/datacenter
sudo chmod a+rw dcconfig.json
echo "файлы скопированы"

sudo systemctl start DataCenter.service
echo "сервис запущен"
sleep 2s
sudo systemctl status DataCenter.service

read -p "нажми Enter для продолжения"

