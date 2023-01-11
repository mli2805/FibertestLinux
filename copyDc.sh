#!/bin/bash
set -e

cd /home/leanid/FibertestLinux/DataCenter/DataCenter/bin/Release/net6.0/publish

sudo systemctl stop Dc.service
echo "сервис остановлен"
sleep 1s

cp -r * /var/fibertest/datacenter/
cd /var/fibertest/datacenter
sudo chmod a+rw dc.json
echo "файлы скопированы"

sudo systemctl start Dc.service
echo "сервис запущен"
sleep 2s
sudo systemctl status Dc.service

read -p "нажми Enter для продолжения"

