# This script must run to keep the server alive

import urllib2
import time

while True:
    #query the server
    contents = urllib2.urlopen("http://128.223.6.20/api/intersection").read()
    #sleep for 15 minutes
    time.sleep(900)
