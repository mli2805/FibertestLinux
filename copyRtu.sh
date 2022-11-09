#!/bin/bash

cd /home/leanid/FibertestLinux/Deploy

sudo systemctl stop Rtu.service
cp rtu/* /var/Fibertest/rtu/

sudo systemctl start Rtu.service
sudo systemctl status Rtu.service
