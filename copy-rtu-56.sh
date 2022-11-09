#!/bin/bash

cd /home/leanid/FibertestLinux/Deploy

pscp rtu.tar.gz leanid@192.168.96.56:/var/tmp
pscp install.sh leanid@192.168.96.56:/var/tmp
