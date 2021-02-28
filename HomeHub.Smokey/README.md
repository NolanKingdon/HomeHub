# Smokey The Bear Module

Module for the arduino code to continuously poll the pi server and ensure temperatures never exceed dangerous levels.

This is included due to hardware related issues where the pi will hang and pin itself at max capacity, skyrocketing it's temperatures. Combined with the fact the pi will live in a fairly small space with poor ventilation, this is a failsafe that will add a layer of safety against overheating and damaging the hardware.

Also, it was a fun excuse to learn embedded programming with Arduino.