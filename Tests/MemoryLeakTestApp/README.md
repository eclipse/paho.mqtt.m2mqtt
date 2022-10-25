# MQTT test application

The goal of this application is to test all QoS in different scenarios and help making sure the library still works as expected with no memory leak after changes.

This application is complementary to the Unit Test and amin to check with a real server the behavior. See it as an end to end test application.

## What is needed

An MQTT broker like Mosquitto. To check the result on your MQTT broker, with Mosquitto, run the following command:

```shell
mosquitto_sub -h localhost -t "temp/#"
```

## Expected flow for basic tests

To get the expected flow, run the first basic QoS test from the application.

Output should looks like this i_n Debug:

```text
SENDRCV CONNECT(protocolName:MQTT,protocolVersion:4,clientId:8a63f8e2-fa63-4d88-b38d-1b3af3520ef9,willFlag:False,willRetain:False,willQosLevel:0,cleanSession:True,keepAlivePeriod:60)
RECV msgtype: 2
RECV CONNACK(returnCode:0)
_inflightQueue.Count  0
_internalQueue.Count  0
enqueued PUBLISH(messageId:1,topic:temp/test,message:4D65737361676520516F532030)
SEND PUBLISH(messageId:1,topic:temp/test,message:4D65737361676520516F532030)
processed PUBLISH(messageId:1,topic:temp/test,message:4D65737361676520516F532030)
_inflightQueue.Count  0
_internalQueue.Count  0
enqueued PUBLISH(messageId:2,topic:temp/test,message:4D65737361676520516F532031)
SEND PUBLISH(messageId:2,topic:temp/test,message:4D65737361676520516F532031)
RECV msgtype: 4
RECV PUBACK(messageId:2)
_inflightQueue.Count  1
_internalQueue.Count  0
enqueued PUBLISH(messageId:3,topic:temp/test,message:4D65737361676520516F532032)
SEND PUBLISH(messageId:3,topic:temp/test,message:4D65737361676520516F532032)
RECV msgtype: 5
RECV PUBREC(messageId:3)
enqueued PUBREC(messageId:3)
dequeued PUBREC(messageId:3)
SEND PUBREL(messageId:3)
RECV msgtype: 7
RECV PUBCOMP(messageId:3)
enqueued PUBCOMP(messageId:3)
dequeued PUBCOMP(messageId:3)
processed PUBLISH(messageId:3,topic:temp/test,message:4D65737361676520516F532032)
```

Expected output in your MQTT broker:

```text
Message QoS 0
Message QoS 1
Message QoS 2
```

## Running the advance QoS tests

Those tests consists of 4 different ones:

* Simple publication of the memory left with a delay of 1 second
* Same but with 2 messages in a raw with no delay between them and 1 second before sending the 2 next ones
* Same but with 2 messages in a raw with a small 50 ms delay between both and 1 second before sending the 2 next ones
* A stress test outputting all messages without any delay between them

What is important in this flow is to check that there is no memory leak from the beginning to the end. If any, you can only run the one to be able to debug. It is also recommended to run the test in Release mode, debug mode will output a lot of elements which may create artificial delays and can create some locks.

With a 5 run, here is how the output from the MQTT broker looks like. It is recommended to have batch of 20 or more to really check that everything behave properly:

```text
single message QoS 0
0-84300
1-84816
2-85248
3-85248
4-85248
two messages without delays QoS 0
0/0-84840
1/0-84840
0/1-85248
1/1-85248
0/2-85248
1/2-85248
0/3-85248
1/3-85248
0/4-85248
1/4-85248
two messages with delay QoS 0
0/0-84840
1/0-84840
0/1-85248
1/1-85248
0/2-85248
1/2-85248
0/3-85248
1/3-85248
0/4-85248
1/4-85248
Stress test with no delay QoS 0
0-84840
1-84840
2-84864
3-84432
4-84864
single message QoS 1
0-84828
1-84780
2-85224
3-85224
4-85224
two messages without delays QoS 1
0/0-84792
1/0-84792
0/1-84336
1/1-84336
0/2-84744
1/2-84744
0/3-84744
1/3-84744
0/4-84744
1/4-84744
two messages with delay QoS 1
0/0-84312
1/0-84312
0/1-84312
1/1-84312
0/2-84744
1/2-84744
0/3-84744
1/3-84744
0/4-84744
1/4-84744
Stress test with no delay QoS 1
0-84312
1-83856
2-83400
3-83928
4-83448
single message QoS 2
0-82572
1-85176
2-85176
3-85176
4-85176
two messages without delays QoS 2
0/0-84744
1/0-84744
0/1-85176
1/1-85176
0/2-85176
1/2-85176
0/3-85176
1/3-85176
0/4-85176
1/4-85176
two messages with delay QoS 2
0/0-84744
1/0-84744
0/1-85176
1/1-85176
0/2-85176
1/2-85176
0/3-85176
1/3-85176
0/4-85176
1/4-85176
Stress test with no delay QoS 2
0-84744
1-84720
2-84264
3-83928
4-83472
Memory left after all the test
85044
Memory left after all the test: 5s
85044
Memory left after all the test: 35s
84840
```

### Analyzing the results

From the previous table, what is important to check is the following:

* You should have almost the same amount of memory from the beginning up to the end (84300 with first message and 84840 with the last one in the example).
* You should see the messages in order (so from 0 to 4 in this example)
* You should have the 0/ before the 1/ messages. The rest of the message should be the same
* It is normal to see during the stress test a slight decrease in the memory left. It should then go back to normal at least at the end of the test
